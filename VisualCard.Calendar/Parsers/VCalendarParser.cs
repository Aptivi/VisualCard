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
using VisualCard.Calendar.Exceptions;
using VisualCard.Parts.Enums;
using VisualCard.Parsers;

namespace VisualCard.Calendar.Parsers
{
    /// <summary>
    /// The base vCalendar parser
    /// </summary>
    [DebuggerDisplay("vCalendar parser, version {CalendarVersion.ToString()}, {CalendarContent.Length} lines")]
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
            bool constructing = false;
            StringBuilder valueBuilder = new();
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
                                var lastSubPart = begins[begins.Count - 2].Item2;
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
                        if (!extraAllowedTypes.Contains(elementTypeUpper) && !elementTypeUpper.StartsWith("X-"))
                            throw new InvalidDataException($"Part info type {classType?.Name ?? "<null>"} doesn't support property type {elementTypeUpper} because the following types are supported: [{string.Join(", ", extraAllowedTypes)}]");
                    }

                    // Handle the part type
                    Type calendarType = subPart is not null ? subPart.GetType() : calendar.GetType();
                    string values = VcardParserTools.GetValuesString(splitArgs, defaultValue, VCalendarConstants._valueArgumentSpecifier);
                    switch (type)
                    {
                        case PartType.Strings:
                            {
                                CalendarStringsEnum stringType = (CalendarStringsEnum)enumeration;
                                string finalValue = value;
                                bool supported = VCalendarParserTools.StringSupported(stringType, CalendarVersion, calendarType);
                                if (!supported)
                                    continue;

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
                                    case CalendarStringsEnum.Trigger:
                                    case CalendarStringsEnum.TimeZoneId:
                                    case CalendarStringsEnum.Recursion:
                                        // Unescape the value
                                        finalValue = Regex.Unescape(value);
                                        break;
                                    case CalendarStringsEnum.Action:
                                        // Unescape the value
                                        finalValue = Regex.Unescape(value);
                                        if (finalValue != "AUDIO" && finalValue != "DISPLAY" && finalValue != "EMAIL")
                                            throw new ArgumentException($"Invalid status {finalValue}");
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
                                    case CalendarStringsEnum.TimeZoneUrl:
                                        // Unescape the value
                                        finalValue = Regex.Unescape(value);
                                        if (!Uri.TryCreate(finalValue, UriKind.Absolute, out Uri zoneUri))
                                            throw new InvalidDataException($"Time zone URL {finalValue} is invalid");
                                        finalValue = zoneUri.ToString();
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
                                bool supported = VCalendarParserTools.IntegerSupported(integerType, CalendarVersion, calendarType);
                                if (!supported)
                                    continue;

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
                                    case CalendarIntegersEnum.TimeZoneOffsetFrom:
                                    case CalendarIntegersEnum.TimeZoneOffsetTo:
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
                                bool supported = VCalendarParserTools.EnumArrayTypeSupported(partsArrayType, CalendarVersion, calendarType);
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
            // Track the required root fields
            string[] expectedFields = [VCalendarConstants._productIdSpecifier];
            if (!ValidateComponent(ref expectedFields, out string[] actualFields, calendar))
                throw new InvalidDataException($"The following keys [{string.Join(", ", expectedFields)}] are required in the root calendar. Got [{string.Join(", ", actualFields)}].");

            // Now, track the individual components starting from events
            string[] expectedEventFields =
                calendar.CalendarVersion.Major == 2 ?
                [VCalendarConstants._uidSpecifier, VCalendarConstants._dateStampSpecifier] :
                [VCalendarConstants._uidSpecifier];
            string[] expectedTodoFields = expectedEventFields;
            string[] expectedAlarmFields = [VCalendarConstants._actionSpecifier, VCalendarConstants._triggerSpecifier];
            foreach (var eventInfo in calendar.events)
            {
                if (!ValidateComponent(ref expectedEventFields, out string[] actualEventFields, eventInfo))
                    throw new InvalidDataException($"The following keys [{string.Join(", ", expectedEventFields)}] are required in the event representation. Got [{string.Join(", ", actualEventFields)}].");
                foreach (var alarmInfo in eventInfo.Alarms)
                {
                    if (!ValidateComponent(ref expectedAlarmFields, out string[] actualAlarmFields, alarmInfo))
                        throw new InvalidDataException($"The following keys [{string.Join(", ", expectedAlarmFields)}] are required in the alarm representation. Got [{string.Join(", ", actualAlarmFields)}].");
                }
            }
            foreach (var todoInfo in calendar.Todos)
            {
                if (!ValidateComponent(ref expectedTodoFields, out string[] actualTodoFields, todoInfo))
                    throw new InvalidDataException($"The following keys [{string.Join(", ", expectedTodoFields)}] are required in the todo representation. Got [{string.Join(", ", actualTodoFields)}].");
                foreach (var alarmInfo in todoInfo.Alarms)
                {
                    if (!ValidateComponent(ref expectedAlarmFields, out string[] actualAlarmFields, alarmInfo))
                        throw new InvalidDataException($"The following keys [{string.Join(", ", expectedAlarmFields)}] are required in the alarm representation. Got [{string.Join(", ", actualAlarmFields)}].");
                }
            }

            // Continue if we have a calendar with version 2.0
            if (calendar.CalendarVersion.Major < 2)
                return;
            string[] expectedJournalFields = expectedEventFields;
            string[] expectedFreeBusyFields = expectedEventFields;
            string[] expectedTimeZoneFields = [VCalendarConstants._tzidSpecifier];
            string[] expectedStandardFields = [VCalendarConstants._dateStartSpecifier, VCalendarConstants._tzOffsetFromSpecifier, VCalendarConstants._tzOffsetToSpecifier];
            string[] expectedDaylightFields = expectedStandardFields;
            foreach (var journalInfo in calendar.Journals)
            {
                if (!ValidateComponent(ref expectedJournalFields, out string[] actualJournalFields, journalInfo))
                    throw new InvalidDataException($"The following keys [{string.Join(", ", expectedJournalFields)}] are required in the journal representation. Got [{string.Join(", ", actualJournalFields)}].");
            }
            foreach (var freebusyInfo in calendar.FreeBusyList)
            {
                if (!ValidateComponent(ref expectedFreeBusyFields, out string[] actualFreeBusyFields, freebusyInfo))
                    throw new InvalidDataException($"The following keys [{string.Join(", ", expectedFreeBusyFields)}] are required in the freebusy representation. Got [{string.Join(", ", actualFreeBusyFields)}].");
            }
            foreach (var timezoneInfo in calendar.TimeZones)
            {
                if (!ValidateComponent(ref expectedTimeZoneFields, out string[] actualTimeZoneFields, timezoneInfo))
                    throw new InvalidDataException($"The following keys [{string.Join(", ", expectedTimeZoneFields)}] are required in the timezone representation. Got [{string.Join(", ", actualTimeZoneFields)}].");
                foreach (var standardInfo in timezoneInfo.StandardTimeList)
                {
                    if (!ValidateComponent(ref expectedStandardFields, out string[] actualStandardFields, standardInfo))
                        throw new InvalidDataException($"The following keys [{string.Join(", ", expectedStandardFields)}] are required in the standard representation. Got [{string.Join(", ", actualStandardFields)}].");
                }
                foreach (var daylightInfo in timezoneInfo.DaylightTimeList)
                {
                    if (!ValidateComponent(ref expectedDaylightFields, out string[] actualDaylightFields, daylightInfo))
                        throw new InvalidDataException($"The following keys [{string.Join(", ", expectedDaylightFields)}] are required in the daylight representation. Got [{string.Join(", ", actualDaylightFields)}].");
                }
            }
        }

        private bool ValidateComponent<TComponent>(ref string[] expectedFields, out string[] actualFields, TComponent component)
            where TComponent : Parts.Calendar
        {
            // Track the required fields
            List<string> actualFieldList = [];

            // Requirement checks
            foreach (string expectedFieldName in expectedFields)
            {
                var (type, enumeration, enumType, _, _, _, _) = VCalendarParserTools.GetPartType(expectedFieldName);
                switch (type)
                {
                    case PartType.Strings:
                        {
                            string value = component.GetString((CalendarStringsEnum)enumeration);
                            bool exists = !string.IsNullOrEmpty(value);
                            if (exists)
                                actualFieldList.Add(expectedFieldName);
                        }
                        break;
                    case PartType.PartsArray:
                        {
                            if (enumType is null)
                                continue;
                            var values = component.GetPartsArray(enumType, (CalendarPartsArrayEnum)enumeration);
                            bool exists = values.Length > 0;
                            if (exists)
                                actualFieldList.Add(expectedFieldName);
                        }
                        break;
                    case PartType.Integers:
                        {
                            int value = component.GetInteger((CalendarIntegersEnum)enumeration);
                            bool exists = value != -1;
                            if (exists)
                                actualFieldList.Add(expectedFieldName);
                        }
                        break;
                }
            }
            Array.Sort(expectedFields);
            actualFieldList.Sort();
            actualFields = [.. actualFieldList];
            return actualFields.SequenceEqual(expectedFields);
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
                _ => throw new ArgumentException($"Invalid type {type}"),
            };
        }

        private void SaveLastSubPart(Parts.Calendar? subpart, ref Parts.Calendar part)
        {
            if (subpart is null)
                return;
            switch (part.GetType().Name)
            {
                case nameof(Parts.Calendar):
                    switch (subpart.GetType().Name)
                    {
                        case nameof(Parts.Calendar):
                            throw new ArgumentException("Can't nest calendar inside calendar");
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
                        case nameof(CalendarStandard):
                            throw new ArgumentException("Can't nest standard time info inside calendar");
                        case nameof(CalendarDaylight):
                            throw new ArgumentException("Can't nest daylight time info inside calendar");
                        case nameof(CalendarAlarm):
                            throw new ArgumentException("Can't nest alarm inside calendar");
                    }
                    break;
                case nameof(CalendarEvent):
                    switch (subpart.GetType().Name)
                    {
                        case nameof(Parts.Calendar):
                            throw new ArgumentException("Can't nest calendar inside event");
                        case nameof(CalendarEvent):
                            throw new ArgumentException("Can't nest event inside event");
                        case nameof(CalendarTodo):
                            throw new ArgumentException("Can't nest to-do inside event");
                        case nameof(CalendarJournal):
                            throw new ArgumentException("Can't nest journal inside event");
                        case nameof(CalendarFreeBusy):
                            throw new ArgumentException("Can't nest free/busy inside event");
                        case nameof(CalendarTimeZone):
                            throw new ArgumentException("Can't nest time zone info inside event");
                        case nameof(CalendarStandard):
                            throw new ArgumentException("Can't nest standard time info inside event");
                        case nameof(CalendarDaylight):
                            throw new ArgumentException("Can't nest daylight time info inside event");
                        case nameof(CalendarAlarm):
                            ((CalendarEvent)part).alarms.Add((CalendarAlarm)subpart);
                            break;
                    }
                    break;
                case nameof(CalendarTodo):
                    switch (subpart.GetType().Name)
                    {
                        case nameof(Parts.Calendar):
                            throw new ArgumentException("Can't nest calendar inside to-do");
                        case nameof(CalendarEvent):
                            throw new ArgumentException("Can't nest event inside to-do");
                        case nameof(CalendarTodo):
                            throw new ArgumentException("Can't nest to-do inside to-do");
                        case nameof(CalendarJournal):
                            throw new ArgumentException("Can't nest journal inside to-do");
                        case nameof(CalendarFreeBusy):
                            throw new ArgumentException("Can't nest free/busy inside to-do");
                        case nameof(CalendarTimeZone):
                            throw new ArgumentException("Can't nest time zone info inside to-do");
                        case nameof(CalendarStandard):
                            throw new ArgumentException("Can't nest standard time info inside to-do");
                        case nameof(CalendarDaylight):
                            throw new ArgumentException("Can't nest daylight time info inside to-do");
                        case nameof(CalendarAlarm):
                            ((CalendarTodo)part).alarms.Add((CalendarAlarm)subpart);
                            break;
                    }
                    break;
                case nameof(CalendarJournal):
                    switch (subpart.GetType().Name)
                    {
                        case nameof(Parts.Calendar):
                            throw new ArgumentException("Can't nest calendar inside journal");
                        case nameof(CalendarEvent):
                            throw new ArgumentException("Can't nest event inside journal");
                        case nameof(CalendarTodo):
                            throw new ArgumentException("Can't nest to-do inside journal");
                        case nameof(CalendarJournal):
                            throw new ArgumentException("Can't nest journal inside journal");
                        case nameof(CalendarFreeBusy):
                            throw new ArgumentException("Can't nest free/busy inside journal");
                        case nameof(CalendarTimeZone):
                            throw new ArgumentException("Can't nest time zone info inside journal");
                        case nameof(CalendarStandard):
                            throw new ArgumentException("Can't nest standard time info inside journal");
                        case nameof(CalendarDaylight):
                            throw new ArgumentException("Can't nest daylight time info inside journal");
                        case nameof(CalendarAlarm):
                            throw new ArgumentException("Can't nest alarm info inside journal");
                    }
                    break;
                case nameof(CalendarFreeBusy):
                    switch (subpart.GetType().Name)
                    {
                        case nameof(Parts.Calendar):
                            throw new ArgumentException("Can't nest calendar inside free/busy info");
                        case nameof(CalendarEvent):
                            throw new ArgumentException("Can't nest event inside free/busy info");
                        case nameof(CalendarTodo):
                            throw new ArgumentException("Can't nest to-do inside free/busy info");
                        case nameof(CalendarJournal):
                            throw new ArgumentException("Can't nest journal inside free/busy info");
                        case nameof(CalendarFreeBusy):
                            throw new ArgumentException("Can't nest free/busy inside free/busy info");
                        case nameof(CalendarTimeZone):
                            throw new ArgumentException("Can't nest time zone info inside free/busy info");
                        case nameof(CalendarStandard):
                            throw new ArgumentException("Can't nest standard time info inside free/busy info");
                        case nameof(CalendarDaylight):
                            throw new ArgumentException("Can't nest daylight time info inside free/busy info");
                        case nameof(CalendarAlarm):
                            throw new ArgumentException("Can't nest alarm info inside free/busy info");
                    }
                    break;
                case nameof(CalendarTimeZone):
                    switch (subpart.GetType().Name)
                    {
                        case nameof(Parts.Calendar):
                            throw new ArgumentException("Can't nest calendar inside time zone");
                        case nameof(CalendarEvent):
                            throw new ArgumentException("Can't nest event inside time zone");
                        case nameof(CalendarTodo):
                            throw new ArgumentException("Can't nest to-do inside time zone");
                        case nameof(CalendarJournal):
                            throw new ArgumentException("Can't nest journal inside time zone");
                        case nameof(CalendarFreeBusy):
                            throw new ArgumentException("Can't nest free/busy inside time zone");
                        case nameof(CalendarTimeZone):
                            throw new ArgumentException("Can't nest time zone info inside time zone");
                        case nameof(CalendarStandard):
                            ((CalendarTimeZone)part).standards.Add((CalendarStandard)subpart);
                            break;
                        case nameof(CalendarDaylight):
                            ((CalendarTimeZone)part).daylights.Add((CalendarDaylight)subpart);
                            break;
                        case nameof(CalendarAlarm):
                            throw new ArgumentException("Can't nest alarm info inside time zone");
                    }
                    break;
                case nameof(CalendarStandard):
                    switch (subpart.GetType().Name)
                    {
                        case nameof(Parts.Calendar):
                            throw new ArgumentException("Can't nest calendar inside standard time info");
                        case nameof(CalendarEvent):
                            throw new ArgumentException("Can't nest event inside standard time info");
                        case nameof(CalendarTodo):
                            throw new ArgumentException("Can't nest to-do inside standard time info");
                        case nameof(CalendarJournal):
                            throw new ArgumentException("Can't nest journal inside standard time info");
                        case nameof(CalendarFreeBusy):
                            throw new ArgumentException("Can't nest free/busy inside standard time info");
                        case nameof(CalendarTimeZone):
                            throw new ArgumentException("Can't nest time zone info inside standard time info");
                        case nameof(CalendarStandard):
                            throw new ArgumentException("Can't nest standard time info inside standard time info");
                        case nameof(CalendarDaylight):
                            throw new ArgumentException("Can't nest daylight time info inside standard time info");
                        case nameof(CalendarAlarm):
                            throw new ArgumentException("Can't nest alarm info inside standard time info");
                    }
                    break;
                case nameof(CalendarDaylight):
                    switch (subpart.GetType().Name)
                    {
                        case nameof(Parts.Calendar):
                            throw new ArgumentException("Can't nest calendar inside daylight time info");
                        case nameof(CalendarEvent):
                            throw new ArgumentException("Can't nest event inside daylight time info");
                        case nameof(CalendarTodo):
                            throw new ArgumentException("Can't nest to-do inside daylight time info");
                        case nameof(CalendarJournal):
                            throw new ArgumentException("Can't nest journal inside daylight time info");
                        case nameof(CalendarFreeBusy):
                            throw new ArgumentException("Can't nest free/busy inside daylight time info");
                        case nameof(CalendarTimeZone):
                            throw new ArgumentException("Can't nest time zone info inside daylight time info");
                        case nameof(CalendarStandard):
                            throw new ArgumentException("Can't nest standard time info inside daylight time info");
                        case nameof(CalendarDaylight):
                            throw new ArgumentException("Can't nest daylight time info inside daylight time info");
                        case nameof(CalendarAlarm):
                            throw new ArgumentException("Can't nest alarm info inside daylight time info");
                    }
                    break;
                case nameof(CalendarAlarm):
                    switch (subpart.GetType().Name)
                    {
                        case nameof(Parts.Calendar):
                            throw new ArgumentException("Can't nest calendar inside alarm info");
                        case nameof(CalendarEvent):
                            throw new ArgumentException("Can't nest event inside alarm info");
                        case nameof(CalendarTodo):
                            throw new ArgumentException("Can't nest to-do inside alarm info");
                        case nameof(CalendarJournal):
                            throw new ArgumentException("Can't nest journal inside alarm info");
                        case nameof(CalendarFreeBusy):
                            throw new ArgumentException("Can't nest free/busy inside alarm info");
                        case nameof(CalendarTimeZone):
                            throw new ArgumentException("Can't nest time zone info inside alarm info");
                        case nameof(CalendarStandard):
                            throw new ArgumentException("Can't nest standard time info inside alarm info");
                        case nameof(CalendarDaylight):
                            throw new ArgumentException("Can't nest daylight time info inside alarm info");
                        case nameof(CalendarAlarm):
                            throw new ArgumentException("Can't nest alarm info inside alarm info");
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
