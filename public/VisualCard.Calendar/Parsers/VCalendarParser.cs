//
// VisualCard  Copyright (C) 2021-2025  Aptivi
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
using VisualCard.Calendar.Parts;
using VisualCard.Calendar.Parts.Enums;
using VisualCard.Calendar.Exceptions;
using VisualCard.Parsers;
using VisualCard.Parsers.Arguments;
using VisualCard.Calendar.Parts.Implementations.Event;
using VisualCard.Calendar.Parts.Implementations.Todo;
using VisualCard.Parts;
using VisualCard.Common.Parts.Enums;

namespace VisualCard.Calendar.Parsers
{
    /// <summary>
    /// The base vCalendar parser
    /// </summary>
    [DebuggerDisplay("vCalendar parser, version {CalendarVersion.ToString()}, {CalendarContent.Length} lines")]
    internal class VCalendarParser
    {
        private readonly Version calendarVersion = new();
        private readonly (int, string)[] calendarContent = [];

        /// <summary>
        /// vCalendar calendar content
        /// </summary>
        public (int, string)[] CalendarContent =>
            calendarContent;
        /// <summary>
        /// vCalendar calendar version
        /// </summary>
        public Version CalendarVersion =>
            calendarVersion;

        /// <summary>
        /// Parses a vCalendar representation
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
            List<(string, Parts.Calendar)> begins = [];
            for (int i = 0; i < CalendarContent.Length; i++)
            {
                // Get line
                var contentLines = CalendarContent.Select((tuple) => tuple.Item2).ToArray();
                var content = CalendarContent[i];
                string _value = VcardCommonTools.ConstructBlocks(contentLines, ref i);
                int lineNumber = content.Item1;
                if (string.IsNullOrEmpty(_value))
                    continue;

                // Take the last sub-part if available
                Parts.Calendar? subPart = null;
                if (begins.Count > 0)
                    subPart = begins[begins.Count - 1].Item2;

                try
                {
                    // Check to see if we have a BEGIN or an END prefix
                    var info = new PropertyInfo(_value);
                    if (info.Prefix == VcardConstants._beginSpecifier)
                    {
                        string finalType = info.Value.ToUpper();
                        begins.Add((finalType, GetCalendarInheritedInstance(finalType)));
                        continue;
                    }
                    else if (info.Prefix == VcardConstants._endSpecifier)
                    {
                        string expectedType = begins[begins.Count - 1].Item1;
                        if (info.Value == expectedType)
                        {
                            if (begins.Count == 1)
                                SaveLastSubPart(subPart, ref calendar);
                            else
                            {
                                var lastSubPart = begins[begins.Count - 2].Item2;
                                SaveLastSubPart(subPart, ref lastSubPart);
                            }
                            begins.RemoveAt(begins.Count - 1);
                        }
                        else
                            throw new ArgumentException($"Ending mismatch: Expected {expectedType} vs. actual {info.Value}");
                        continue;
                    }

                    // Process this line
                    Process(subPart, _value, calendar, CalendarVersion);
                }
                catch (Exception ex)
                {
                    throw new VCalendarParseException(ex.Message, _value, lineNumber, ex);
                }
            }

            // Validate this calendar before returning it.
            calendar.Validate();
            return calendar;
        }

        internal static void Process(Parts.Calendar? subPart, string _value, Parts.Calendar calendar, Version version)
        {
            // Now, parse a property
            var info = new PropertyInfo(_value);

            // Get the type and its properties
            Type calendarType = subPart is not null ? subPart.GetType() : calendar.GetType();
            var partType = VCalendarParserTools.GetPartType(info.Prefix, version, calendarType);

            // Check the type for allowed types
            string[] elementTypes = VcardCommonTools.GetTypes(info.Arguments, partType.defaultType);
            foreach (string elementType in elementTypes)
            {
                string elementTypeUpper = elementType.ToUpper();
                if (!partType.allowedExtraTypes.Contains(elementTypeUpper) && !elementTypeUpper.StartsWith("X-"))
                {
                    if (partType.type == PartType.PartsArray &&
                        ((CalendarPartsArrayEnum)partType.enumeration == CalendarPartsArrayEnum.IanaNames ||
                         (CalendarPartsArrayEnum)partType.enumeration == CalendarPartsArrayEnum.NonstandardNames))
                        continue;
                    throw new InvalidDataException($"Part info type {partType.enumType?.Name ?? "<null>"} doesn't support property type {elementTypeUpper} because the following types are supported: [{string.Join(", ", partType.allowedExtraTypes)}]");
                }
            }

            // Handle the part type, and extract the value
            string valueType = VcardCommonTools.GetFirstValue(info.Arguments, partType.defaultValueType, VcardConstants._valueArgumentSpecifier);
            string finalValue = VcardCommonTools.ProcessStringValue(info.Value, valueType, version.Major == 1 ? ';' : ',');

            // Check for allowed values
            if (partType.allowedValues.Length != 0)
            {
                bool found = false;
                foreach (string allowedValue in partType.allowedValues)
                {
                    if (finalValue == allowedValue)
                        found = true;
                }
                if (!found)
                    throw new InvalidDataException($"Value {finalValue} not in the list of allowed values [{string.Join(", ", partType.allowedValues)}]");
            }

            // Check for support
            bool supported = partType.minimumVersionCondition(version);
            if (!supported)
                return;

            // Process the value
            switch (partType.type)
            {
                case PartType.Strings:
                    {
                        CalendarStringsEnum stringType = (CalendarStringsEnum)partType.enumeration;

                        // Set the string for real
                        var stringValueInfo = new ValueInfo<string>(info, -1, elementTypes, valueType, finalValue);
                        if (subPart is not null)
                            subPart.AddString(stringType, stringValueInfo);
                        else
                            calendar.AddString(stringType, stringValueInfo);
                    }
                    break;
                case PartType.Integers:
                    {
                        CalendarIntegersEnum integerType = (CalendarIntegersEnum)partType.enumeration;

                        // Get the final value
                        double finalDouble = double.Parse(finalValue);

                        // Set the integer for real
                        var stringValueInfo = new ValueInfo<double>(info, -1, elementTypes, valueType, finalDouble);
                        if (subPart is not null)
                            subPart.AddInteger(integerType, stringValueInfo);
                        else
                            calendar.AddInteger(integerType, stringValueInfo);
                    }
                    break;
                case PartType.PartsArray:
                    {
                        CalendarPartsArrayEnum partsArrayType = (CalendarPartsArrayEnum)partType.enumeration;
                        if (partType.fromStringFunc is null)
                            return;

                        // Now, get the part info
                        finalValue = partsArrayType is CalendarPartsArrayEnum.IanaNames or CalendarPartsArrayEnum.NonstandardNames ? _value : info.Value;
                        var partInfo = partType.fromStringFunc(finalValue, info, elementTypes, info.Group, valueType, version);

                        // Set the array for real
                        if (subPart is not null)
                            subPart.AddPartToArray(partsArrayType, partInfo);
                        else
                            calendar.AddPartToArray(partsArrayType, partInfo);
                    }
                    break;
                default:
                    throw new InvalidDataException($"The type {partType.type} is invalid. Are you sure that you've specified the correct type in your vCalendar representation?");
            }
        }

        private Parts.Calendar GetCalendarInheritedInstance(string type)
        {
            return type switch
            {
                VCalendarConstants._objectVEventSpecifier => new CalendarEvent(CalendarVersion),
                VCalendarConstants._objectVTodoSpecifier => new CalendarTodo(CalendarVersion),
                VCalendarConstants._objectVJournalSpecifier => new CalendarJournal(CalendarVersion),
                VCalendarConstants._objectVFreeBusySpecifier => new CalendarFreeBusy(CalendarVersion),
                VCalendarConstants._objectVTimeZoneSpecifier => new CalendarTimeZone(CalendarVersion),
                VCalendarConstants._objectVStandardSpecifier => new CalendarStandard(CalendarVersion),
                VCalendarConstants._objectVDaylightSpecifier => new CalendarDaylight(CalendarVersion),
                VCalendarConstants._objectVAlarmSpecifier => new CalendarAlarm(CalendarVersion),
                _ =>
                    CalendarVersion.Major == 2 ?
                    new CalendarOtherComponent(CalendarVersion, type) :
                    throw new ArgumentException($"Invalid type {type}"),
            };
        }

        private void SaveLastSubPart(Parts.Calendar? subpart, ref Parts.Calendar part)
        {
            if (subpart is null)
                return;
            bool nestable = true;
            switch (part.GetType().Name)
            {
                case nameof(Parts.Calendar):
                    switch (subpart.GetType().Name)
                    {
                        case nameof(CalendarEvent):
                            part.events.Add((CalendarEvent)subpart);
                            break;
                        case nameof(CalendarTodo):
                            part.todos.Add((CalendarTodo)subpart);
                            break;
                        case nameof(CalendarJournal):
                            part.journals.Add((CalendarJournal)subpart);
                            break;
                        case nameof(CalendarFreeBusy):
                            part.freeBusyList.Add((CalendarFreeBusy)subpart);
                            break;
                        case nameof(CalendarTimeZone):
                            part.timeZones.Add((CalendarTimeZone)subpart);
                            break;
                        case nameof(CalendarOtherComponent):
                            part.others.Add((CalendarOtherComponent)subpart);
                            break;
                        default:
                            nestable = false;
                            break;
                    }
                    break;
                case nameof(CalendarEvent):
                    switch (subpart.GetType().Name)
                    {
                        case nameof(CalendarAlarm):
                            ((CalendarEvent)part).alarms.Add((CalendarAlarm)subpart);
                            break;
                        default:
                            nestable = false;
                            break;
                    }
                    break;
                case nameof(CalendarTodo):
                    switch (subpart.GetType().Name)
                    {
                        case nameof(CalendarAlarm):
                            ((CalendarTodo)part).alarms.Add((CalendarAlarm)subpart);
                            break;
                        default:
                            nestable = false;
                            break;
                    }
                    break;
                case nameof(CalendarTimeZone):
                    switch (subpart.GetType().Name)
                    {
                        case nameof(CalendarStandard):
                            ((CalendarTimeZone)part).standards.Add((CalendarStandard)subpart);
                            break;
                        case nameof(CalendarDaylight):
                            ((CalendarTimeZone)part).daylights.Add((CalendarDaylight)subpart);
                            break;
                        default:
                            nestable = false;
                            break;
                    }
                    break;
                default:
                    nestable = false;
                    break;
            }
            if (!nestable)
                throw new ArgumentException($"Can't place {subpart.GetType().Name} inside {part.GetType().Name}");
        }

        internal VCalendarParser((int, string)[] calendarContent, Version calendarVersion)
        {
            this.calendarContent = calendarContent;
            this.calendarVersion = calendarVersion;
        }
    }
}
