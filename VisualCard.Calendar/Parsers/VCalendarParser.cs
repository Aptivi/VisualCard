//
// VisualCard  Copyright (C) 2021-2024  Aptivi
//
// This file is part of VisualCard
//
// VisualCard is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// VisualCard is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Textify.General;
using VisualCard.Calendar.Parts;
using VisualCard.Calendar.Parts.Enums;
using VisualCard.Calendar.Parts.Implementations;
using VisualCard.Calendar.Exceptions;
using VisualCard.Parts.Enums;
using VisualCard.Parsers;

namespace VisualCard.Calendar.Parsers
{
    /// <summary>
    /// The base vCalendar parser
    /// </summary>
    [DebuggerDisplay("vCalendar contact, version {CalendarVersion.ToString()}, {CalendarContent.Length} lines")]
    internal class VCalendarParser
    {
        private readonly Version calendarVersion = new();
        private readonly string[] calendarContent = [];

        /// <summary>
        /// vCalendar calendar content
        /// </summary>
        public string[] CalendarContent =>
            calendarContent;
        /// <summary>
        /// vCalendar calendar version
        /// </summary>
        public Version CalendarVersion =>
            calendarVersion;

        /// <summary>
        /// Parses a vCalendar contact
        /// </summary>
        /// <returns>A strongly-typed <see cref="Parts.Calendar"/> instance holding information about the calendar</returns>
        public Parts.Calendar Parse()
        {
            // Check the content to ensure that we really have data
            if (CalendarContent.Length == 0)
                throw new InvalidDataException($"Calendar content is empty.");

            // Make a new vcalendar
            var calendar = new Parts.Calendar(CalendarVersion);

            // Iterate through all the lines
            bool constructing = false;
            StringBuilder valueBuilder = new();
            string[] allowedTypes = ["HOME", "WORK", "PREF"];
            List<(string, Parts.Calendar)> begins = [];
            for (int i = 0; i < CalendarContent.Length; i++)
            {
                // Get line
                string _value = CalendarContent[i];
                int lineNumber = i + 1;
                if (string.IsNullOrEmpty(_value))
                    continue;

                // Take the last sub-part if available
                Parts.Calendar? subPart = null;
                if (begins.Count > 0)
                    subPart = begins[begins.Count - 1].Item2;

                // First, check to see if we need to construct blocks
                string secondLine = i + 1 < CalendarContent.Length ? CalendarContent[i + 1] : "";
                bool firstConstructedLine = !_value.StartsWith(VCalendarConstants._spaceBreak) && !_value.StartsWith(VCalendarConstants._tabBreak);
                constructing = secondLine.StartsWithAnyOf([VCalendarConstants._spaceBreak, VCalendarConstants._tabBreak]);
                secondLine = secondLine.Length > 1 ? secondLine.Substring(1) : "";
                if (constructing)
                {
                    if (firstConstructedLine)
                        valueBuilder.Append(_value);
                    valueBuilder.Append(secondLine);
                    continue;
                }
                else if (!firstConstructedLine)
                {
                    _value = valueBuilder.ToString();
                    valueBuilder.Clear();
                }

                try
                {
                    // Now, parse a line
                    if (!_value.Contains(VCalendarConstants._argumentDelimiter))
                        throw new ArgumentException("The line must contain an argument delimiter.");
                    string value = _value.Substring(_value.IndexOf(VCalendarConstants._argumentDelimiter) + 1);
                    string prefixWithArgs = _value.Substring(0, _value.IndexOf(VCalendarConstants._argumentDelimiter));
                    string prefix = prefixWithArgs.Contains(';') ? prefixWithArgs.Substring(0, prefixWithArgs.IndexOf(';')) : prefixWithArgs;
                    string args = prefixWithArgs.Contains(';') ? prefixWithArgs.Substring(prefix.Length + 1) : "";
                    string[] splitArgs = args.Split([VCalendarConstants._fieldDelimiter], StringSplitOptions.RemoveEmptyEntries);
                    string[] splitValues = value.Split([VCalendarConstants._fieldDelimiter], StringSplitOptions.RemoveEmptyEntries);
                    bool isWithType = splitArgs.Length > 0;
                    List<string> finalArgs = [];

                    // Check to see if we have a BEGIN or an END prefix
                    if (prefix == VCalendarConstants._beginSpecifier)
                    {
                        begins.Add((value, GetCalendarInheritedInstance(value)));
                        continue;
                    }
                    else if (prefix == VCalendarConstants._endSpecifier)
                    {
                        string expectedType = begins[begins.Count - 1].Item1;
                        if (value == expectedType)
                        {
                            if (begins.Count == 1)
                                SaveLastSubPart(subPart, ref calendar);
                            else
                            {
                                var lastSubPart = begins[begins.Count - 1].Item2;
                                SaveLastSubPart(subPart, ref lastSubPart);
                            }
                            begins.RemoveAt(begins.Count - 1);
                        }
                        else
                            throw new ArgumentException($"Ending mismatch: Expected {expectedType} vs. actual {value}");
                        continue;
                    }

                    // Get the part type
                    bool xNonstandard = prefix.StartsWith(VCalendarConstants._xSpecifier);
                    bool specifierRequired = CalendarVersion.Major >= 3;
                    var (type, enumeration, classType, fromString, defaultType, defaultValue, extraAllowedTypes) = VCalendarParserTools.GetPartType(xNonstandard ? VCalendarConstants._xSpecifier : prefix);

                    // Handle arguments
                    if (isWithType)
                    {
                        // Finalize the arguments
                        finalArgs.AddRange(splitArgs.Except(
                            splitArgs.Where((arg) =>
                                arg.StartsWith(VCalendarConstants._valueArgumentSpecifier) ||
                                arg.StartsWith(VCalendarConstants._typeArgumentSpecifier) ||
                                CalendarVersion.Major == 2 && !arg.Contains(VCalendarConstants._argumentValueDelimiter)
                            )
                        ));
                    }

                    // Check the type for allowed types
                    string[] elementTypes = VcardParserTools.GetTypes(splitArgs, defaultType, specifierRequired);
                    foreach (string elementType in elementTypes)
                    {
                        string elementTypeUpper = elementType.ToUpper();
                        if (!allowedTypes.Contains(elementTypeUpper) && !extraAllowedTypes.Contains(elementTypeUpper) && !elementTypeUpper.StartsWith("X-"))
                            throw new InvalidDataException($"Part info type {classType?.Name ?? "<null>"} doesn't support property type {elementTypeUpper} because the following base types are supported: [{string.Join(", ", allowedTypes)}] and the extra types are supported: [{string.Join(", ", extraAllowedTypes)}]");
                    }

                    // Handle the part type
                    string values = VcardParserTools.GetValuesString(splitArgs, defaultValue, VCalendarConstants._valueArgumentSpecifier);
                    switch (type)
                    {
                        case PartType.Strings:
                            {
                                CalendarStringsEnum stringType = (CalendarStringsEnum)enumeration;
                                string finalValue = value;

                                // Now, handle each type individually
                                switch (stringType)
                                {
                                    case CalendarStringsEnum.ProductId:
                                    case CalendarStringsEnum.Organizer:
                                    case CalendarStringsEnum.Summary:
                                    case CalendarStringsEnum.Description:
                                    case CalendarStringsEnum.CalScale:
                                    case CalendarStringsEnum.Method:
                                    case CalendarStringsEnum.Class:
                                        // Unescape the value
                                        finalValue = Regex.Unescape(value);
                                        break;
                                    case CalendarStringsEnum.Status:
                                        // Unescape the value
                                        finalValue = Regex.Unescape(value);
                                        if (finalValue != "TENTATIVE" && finalValue != "CONFIRMED" && finalValue != "CANCELLED")
                                            throw new ArgumentException($"Invalid status {finalValue}");
                                        break;
                                    case CalendarStringsEnum.Transparency:
                                        // Unescape the value
                                        finalValue = Regex.Unescape(value);
                                        if (CalendarVersion.Major == 1 && !int.TryParse(finalValue, out _))
                                            throw new ArgumentException($"Invalid transparency number {finalValue}");
                                        else if (CalendarVersion.Major == 2 && finalValue != "TRANSPARENT" && finalValue != "OPAQUE")
                                            throw new ArgumentException($"Invalid transparency {finalValue}");
                                        break;
                                    case CalendarStringsEnum.Uid:
                                        // Unescape the value
                                        finalValue = Regex.Unescape(value);
                                        if (!Uri.TryCreate(finalValue, UriKind.Absolute, out Uri uri) && values.Equals("uri", StringComparison.OrdinalIgnoreCase))
                                            throw new InvalidDataException($"URL {finalValue} is invalid");
                                        finalValue = uri is not null ? uri.ToString() : finalValue;
                                        break;
                                    default:
                                        throw new InvalidDataException($"The string enum type {stringType} is invalid. Are you sure that you've specified the correct type in your vCalendar representation?");
                                }

                                // Set the string for real
                                if (subPart is not null)
                                    subPart.SetString(stringType, finalValue);
                                else
                                    calendar.SetString(stringType, finalValue);
                            }
                            break;
                        case PartType.Integers:
                            {
                                CalendarIntegersEnum integerType = (CalendarIntegersEnum)enumeration;
                                int primaryValue = int.Parse(value);
                                int finalValue = 0;

                                // Now, handle each type individually
                                switch (integerType)
                                {
                                    case CalendarIntegersEnum.Priority:
                                        // Check the range
                                        if (primaryValue < 0 || primaryValue > 9)
                                            throw new ArgumentOutOfRangeException(nameof(primaryValue), primaryValue, "Priority may not be less than zero or greater than 9");
                                        finalValue = primaryValue;
                                        break;
                                    case CalendarIntegersEnum.Sequence:
                                        // Check the range
                                        if (primaryValue < 0)
                                            throw new ArgumentOutOfRangeException(nameof(primaryValue), primaryValue, "Value may not be less than zero");
                                        finalValue = primaryValue;
                                        break;
                                    default:
                                        throw new InvalidDataException($"The integer enum type {integerType} is invalid. Are you sure that you've specified the correct type in your vCalendar representation?");
                                }

                                // Set the integer for real
                                if (subPart is not null)
                                    subPart.SetInteger(integerType, finalValue);
                                else
                                    calendar.SetInteger(integerType, finalValue);
                            }
                            break;
                        case PartType.PartsArray:
                            {
                                CalendarPartsArrayEnum partsArrayType = (CalendarPartsArrayEnum)enumeration;
                                Type? partsArrayClass = classType;
                                bool supported = VCalendarParserTools.EnumArrayTypeSupported(partsArrayType, CalendarVersion);
                                if (!supported)
                                    continue;
                                if (fromString is null)
                                    continue;

                                // Now, get the part info
                                string finalValue = partsArrayType == CalendarPartsArrayEnum.NonstandardNames ? _value : value;
                                var partInfo = fromString(finalValue, [.. finalArgs], elementTypes, values, CalendarVersion);
                                if (subPart is not null)
                                    subPart.AddPartToArray(partsArrayType, partInfo);
                                else
                                    calendar.AddPartToArray(partsArrayType, partInfo);
                            }
                            break;
                        default:
                            throw new InvalidDataException($"The type {type} is invalid. Are you sure that you've specified the correct type in your vcalendar representation?");
                    }
                }
                catch (Exception ex)
                {
                    throw new VCalendarParseException(ex.Message, _value, lineNumber, ex);
                }
            }

            // Validate this calendar before returning it.
            ValidateCalendar(calendar);
            return calendar;
        }

        internal void ValidateCalendar(Parts.Calendar calendar)
        {
            // Track the required fields
            List<string> expectedFields =
            [
                VCalendarConstants._productIdSpecifier,
            ];
            List<string> actualFields = [];

            // Requirement checks
            if (expectedFields.Contains(VCalendarConstants._productIdSpecifier))
            {
                var prodId = calendar.GetString(CalendarStringsEnum.ProductId);
                bool exists = !string.IsNullOrEmpty(prodId);
                if (exists)
                    actualFields.Add(VcardConstants._productIdSpecifier);
            }
            expectedFields.Sort();
            actualFields.Sort();
            if (!actualFields.SequenceEqual(expectedFields))
                throw new InvalidDataException($"The following keys [{string.Join(", ", expectedFields)}] are required. Got [{string.Join(", ", actualFields)}].");
        }

        private Parts.Calendar GetCalendarInheritedInstance(string type)
        {
            return type switch
            {
                VCalendarConstants._objectVEventSpecifier => new CalendarEvent(CalendarVersion),
                _ => throw new ArgumentException($"Invalid type {type}"),
            };
        }

        private void SaveLastSubPart(Parts.Calendar? subpart, ref Parts.Calendar part)
        {
            if (subpart is null)
                return;
            switch (part.GetType().Name)
            {
                case nameof(Calendar):
                    switch (subpart.GetType().Name)
                    {
                        case nameof(Calendar):
                            throw new ArgumentException("Can't nest calendar inside calendar");
                        case nameof(CalendarEvent):
                            part.events.Add((CalendarEvent)subpart);
                            break;
                    }
                    break;
                case nameof(CalendarEvent):
                    switch (subpart.GetType().Name)
                    {
                        case nameof(Calendar):
                            throw new ArgumentException("Can't nest calendar inside event");
                        case nameof(CalendarEvent):
                            throw new ArgumentException("Can't nest event inside event");
                    }
                    break;
            }
        }

        internal VCalendarParser(string[] calendarContent, Version calendarVersion)
        {
            this.calendarContent = calendarContent;
            this.calendarVersion = calendarVersion;
        }
    }
}
