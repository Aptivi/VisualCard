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
using VisualCard.Calendar.Parts;
using VisualCard.Calendar.Parts.Enums;
using VisualCard.Calendar.Exceptions;
using VisualCard.Parts.Enums;
using VisualCard.Parsers;
using VisualCard.Parsers.Arguments;
using VisualCard.Calendar.Parts.Implementations.Event;
using VisualCard.Calendar.Parts.Implementations.Todo;

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
                var content = CalendarContent[i];
                string _value = VcardCommonTools.ConstructBlocks(CalendarContent, ref i);
                int lineNumber = content.Item1;
                if (string.IsNullOrEmpty(_value))
                    continue;

                // Take the last sub-part if available
                Parts.Calendar? subPart = null;
                if (begins.Count > 0)
                    subPart = begins[begins.Count - 1].Item2;

                try
                {
                    // Now, parse a property
                    var info = new PropertyInfo(_value);

                    // Check to see if we have a BEGIN or an END prefix
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

                    // Get the type and its properties
                    Type calendarType = subPart is not null ? subPart.GetType() : calendar.GetType();
                    var (type, enumeration, classType, fromString, defaultType, defaultValue, defaultValueType, extraAllowedTypes, allowedValues) = VCalendarParserTools.GetPartType(info.Prefix, VCalendarParserTools.GetObjectTypeFromType(calendarType), CalendarVersion);

                    // Check the type for allowed types
                    string[] elementTypes = VcardCommonTools.GetTypes(info.Arguments, defaultType);
                    foreach (string elementType in elementTypes)
                    {
                        string elementTypeUpper = elementType.ToUpper();
                        if (!extraAllowedTypes.Contains(elementTypeUpper) && !elementTypeUpper.StartsWith("X-"))
                        {
                            if (type == PartType.PartsArray && ((CalendarPartsArrayEnum)enumeration == CalendarPartsArrayEnum.IanaNames || (CalendarPartsArrayEnum)enumeration == CalendarPartsArrayEnum.NonstandardNames))
                                continue;
                            throw new InvalidDataException($"Part info type {classType?.Name ?? "<null>"} doesn't support property type {elementTypeUpper} because the following types are supported: [{string.Join(", ", extraAllowedTypes)}]");
                        }
                    }

                    // Handle the part type, and extract the value
                    string valueType = VcardCommonTools.GetFirstValue(info.Arguments, defaultValueType, VcardConstants._valueArgumentSpecifier);
                    string finalValue = VcardCommonTools.ProcessStringValue(info.Value, valueType, calendarVersion.Major == 1 ? ';' : ',');

                    // Check for allowed values
                    if (allowedValues.Length != 0)
                    {
                        bool found = false;
                        foreach (string allowedValue in allowedValues)
                        {
                            if (finalValue == allowedValue)
                                found = true;
                        }
                        if (!found)
                            throw new InvalidDataException($"Value {finalValue} not in the list of allowed values [{string.Join(", ", allowedValues)}]");
                    }

                    // Process the value
                    switch (type)
                    {
                        case PartType.Strings:
                            {
                                CalendarStringsEnum stringType = (CalendarStringsEnum)enumeration;
                                bool supported = VCalendarParserTools.StringSupported(stringType, CalendarVersion, calendarType);
                                if (!supported)
                                    continue;

                                // Set the string for real
                                var stringValueInfo = new CalendarValueInfo<string>(info.Arguments, elementTypes, info.Group, valueType, finalValue);
                                if (subPart is not null)
                                    subPart.AddString(stringType, stringValueInfo);
                                else
                                    calendar.AddString(stringType, stringValueInfo);
                            }
                            break;
                        case PartType.Integers:
                            {
                                CalendarIntegersEnum integerType = (CalendarIntegersEnum)enumeration;
                                bool supported = VCalendarParserTools.IntegerSupported(integerType, CalendarVersion, calendarType);
                                if (!supported)
                                    continue;

                                // Get the final value
                                double finalDouble = double.Parse(finalValue);

                                // Set the integer for real
                                var stringValueInfo = new CalendarValueInfo<double>(info.Arguments, elementTypes, info.Group, valueType, finalDouble);
                                if (subPart is not null)
                                    subPart.AddInteger(integerType, stringValueInfo);
                                else
                                    calendar.AddInteger(integerType, stringValueInfo);
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
                                finalValue = partsArrayType is CalendarPartsArrayEnum.IanaNames or CalendarPartsArrayEnum.NonstandardNames ? _value : info.Value;
                                var partInfo = fromString(finalValue, info.Arguments, elementTypes, info.Group, valueType, CalendarVersion);

                                // Set the array for real
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
            string[] expectedFields =
                calendar.CalendarVersion.Major == 2 ? [VCalendarConstants._productIdSpecifier] : [];
            if (!ValidateComponent(ref expectedFields, out string[] actualFields, calendar))
                throw new InvalidDataException($"The following keys [{string.Join(", ", expectedFields)}] are required in the root calendar. Got [{string.Join(", ", actualFields)}].");

            // Now, track the individual components starting from events
            string[] expectedEventFields =
                calendar.CalendarVersion.Major == 2 ?
                [VCalendarConstants._uidSpecifier, VCalendarConstants._dateStampSpecifier] : [];
            string[] expectedTodoFields = expectedEventFields;
            expectedEventFields =
                calendar.CalendarVersion.Major == 2 && calendar.GetString(CalendarStringsEnum.Method).Length == 0 ?
                [VCalendarConstants._dateStartSpecifier, .. expectedEventFields] :
                expectedEventFields;
            foreach (var eventInfo in calendar.events)
            {
                if (!ValidateComponent(ref expectedEventFields, out string[] actualEventFields, eventInfo))
                    throw new InvalidDataException($"The following keys [{string.Join(", ", expectedEventFields)}] are required in the event representation. Got [{string.Join(", ", actualEventFields)}].");
                foreach (var alarmInfo in eventInfo.Alarms)
                    ValidateAlarm(alarmInfo);

                // Check the priority
                var priorities = eventInfo.GetInteger(CalendarIntegersEnum.PercentComplete);
                foreach (var priority in priorities)
                {
                    if (priority.Value < 0 || priority.Value > 9)
                        throw new ArgumentOutOfRangeException(nameof(CalendarIntegersEnum.PercentComplete), priority.Value, "Percent completion may not be less than zero or greater than 100");
                }

                // Check for conflicts
                var dtends = eventInfo.GetPartsArray<DateEndInfo>();
                var durations = eventInfo.GetString(CalendarStringsEnum.Duration);
                if (dtends.Length > 0 && durations.Length > 0)
                    throw new InvalidDataException("Date end and duration conflict found.");
            }
            foreach (var todoInfo in calendar.Todos)
            {
                if (!ValidateComponent(ref expectedTodoFields, out string[] actualTodoFields, todoInfo))
                    throw new InvalidDataException($"The following keys [{string.Join(", ", expectedTodoFields)}] are required in the todo representation. Got [{string.Join(", ", actualTodoFields)}].");
                foreach (var alarmInfo in todoInfo.Alarms)
                    ValidateAlarm(alarmInfo);

                // Check the percentage
                var percentages = todoInfo.GetInteger(CalendarIntegersEnum.PercentComplete);
                foreach (var percentage in percentages)
                {
                    if (percentage.Value < 0 || percentage.Value > 100)
                        throw new ArgumentOutOfRangeException(nameof(CalendarIntegersEnum.PercentComplete), percentage.Value, "Percent completion may not be less than zero or greater than 100");
                }

                // Check the priority
                var priorities = todoInfo.GetInteger(CalendarIntegersEnum.PercentComplete);
                foreach (var priority in priorities)
                {
                    if (priority.Value < 0 || priority.Value > 9)
                        throw new ArgumentOutOfRangeException(nameof(CalendarIntegersEnum.PercentComplete), priority.Value, "Percent completion may not be less than zero or greater than 100");
                }

                // Check for conflicts
                var dtstarts = todoInfo.GetPartsArray<DateStartInfo>();
                var dues = todoInfo.GetPartsArray<DueDateInfo>();
                var durations = todoInfo.GetString(CalendarStringsEnum.Duration);
                if (dues.Length > 0 && durations.Length > 0)
                    throw new InvalidDataException("Due date and duration conflict found.");
                if (durations.Length > 0 && dtstarts.Length == 0)
                    throw new InvalidDataException("There is no date start to add to the duration.");
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

                // Check for standard and/or daylight
                if (timezoneInfo.StandardTimeList.Length == 0 && timezoneInfo.DaylightTimeList.Length == 0)
                    throw new InvalidDataException("One of the standard/daylight components is required.");

                // Verify the standard and/or daylight components
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
                if (HasComponent(expectedFieldName, component))
                    actualFieldList.Add(expectedFieldName);
            }
            Array.Sort(expectedFields);
            actualFieldList.Sort();
            actualFields = [.. actualFieldList];
            return actualFields.SequenceEqual(expectedFields);
        }

        private bool HasComponent<TComponent>(string expectedFieldName, TComponent component)
            where TComponent : Parts.Calendar
        {
            // Requirement checks
            var (type, enumeration, enumType, _, _, _, _, _, _) = VCalendarParserTools.GetPartType(expectedFieldName, VCalendarParserTools.GetObjectTypeFromComponent(component), CalendarVersion);
            bool exists = false;
            switch (type)
            {
                case PartType.Strings:
                    {
                        var values = component.GetString((CalendarStringsEnum)enumeration);
                        exists = values.Length > 0;
                    }
                    break;
                case PartType.PartsArray:
                    {
                        if (enumType is null)
                            return false;
                        var values = component.GetPartsArray((CalendarPartsArrayEnum)enumeration);
                        exists = values.Length > 0;
                    }
                    break;
                case PartType.Integers:
                    {
                        var values = component.GetInteger((CalendarIntegersEnum)enumeration);
                        exists = values.Length > 0;
                    }
                    break;
            }
            return exists;
        }

        private void ValidateAlarm(CalendarAlarm alarmInfo)
        {
            string[] expectedAlarmFields = [VCalendarConstants._actionSpecifier, VCalendarConstants._triggerSpecifier];
            if (!ValidateComponent(ref expectedAlarmFields, out string[] actualAlarmFields, alarmInfo))
                throw new InvalidDataException($"The following keys [{string.Join(", ", expectedAlarmFields)}] are required in the alarm representation. Got [{string.Join(", ", actualAlarmFields)}].");

            // Check the alarm action
            string[] expectedAudioAlarmFields = [VCalendarConstants._attachSpecifier];
            string[] expectedDisplayAlarmFields = [VCalendarConstants._descriptionSpecifier];
            string[] expectedMailAlarmFields = [VCalendarConstants._descriptionSpecifier, VCalendarConstants._summarySpecifier, VCalendarConstants._attendeeSpecifier];
            var actionList = alarmInfo.GetString(CalendarStringsEnum.Action);
            string type = actionList.Length > 0 ? actionList[0].Value : "";
            switch (type)
            {
                case "AUDIO":
                    if (!ValidateComponent(ref expectedAudioAlarmFields, out string[] actualAudioAlarmFields, alarmInfo))
                        throw new InvalidDataException($"The following keys [{string.Join(", ", expectedAudioAlarmFields)}] are required in the audio alarm representation. Got [{string.Join(", ", actualAudioAlarmFields)}].");
                    break;
                case "DISPLAY":
                    if (!ValidateComponent(ref expectedDisplayAlarmFields, out string[] actualDisplayAlarmFields, alarmInfo))
                        throw new InvalidDataException($"The following keys [{string.Join(", ", expectedDisplayAlarmFields)}] are required in the display alarm representation. Got [{string.Join(", ", actualDisplayAlarmFields)}].");
                    break;
                case "EMAIL":
                    if (!ValidateComponent(ref expectedMailAlarmFields, out string[] actualMailAlarmFields, alarmInfo))
                        throw new InvalidDataException($"The following keys [{string.Join(", ", expectedMailAlarmFields)}] are required in the mail alarm representation. Got [{string.Join(", ", actualMailAlarmFields)}].");
                    break;
            }

            // Check to see if there is a repeat property
            var repeatList = alarmInfo.GetInteger(CalendarIntegersEnum.Repeat);
            int repeat = (int)(repeatList.Length > 0 ? repeatList[0].Value : -1);
            string[] expectedRepeatedAlarmFields = [VCalendarConstants._durationSpecifier];
            if (repeat >= 1)
            {
                if (!ValidateComponent(ref expectedRepeatedAlarmFields, out string[] actualRepeatedAlarmFields, alarmInfo))
                    throw new InvalidDataException($"The following keys [{string.Join(", ", expectedRepeatedAlarmFields)}] are required in the repeated alarm representation. Got [{string.Join(", ", actualRepeatedAlarmFields)}].");
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
