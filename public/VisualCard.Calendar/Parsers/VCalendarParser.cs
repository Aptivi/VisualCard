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
using VisualCard.Common.Parts.Enums;
using VisualCard.Common.Parsers;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts;
using VisualCard.Common.Parts.Implementations;
using VisualCard.Common.Diagnostics;
using Textify.General;

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
            LoggingTools.Debug("Content lines is {0}...", CalendarContent.Length);
            if (CalendarContent.Length == 0)
                throw new InvalidDataException("Calendar content is empty.");

            // Make a new vcalendar
            var calendar = new Parts.Calendar(CalendarVersion);
            LoggingTools.Info("Made a new calendar instance with version {0}...", CalendarVersion.ToString());

            // Iterate through all the lines
            List<(string, Parts.Calendar)> begins = [];
            for (int i = 0; i < CalendarContent.Length; i++)
            {
                // Get line
                var contentLines = CalendarContent.Select((tuple) => tuple.Item2).ToArray();
                var content = CalendarContent[i];
                string _value = CommonTools.ConstructBlocks(contentLines, ref i);
                int lineNumber = content.Item1;
                LoggingTools.Debug("Content number {0} [idx: {1}] constructed to {2}...", lineNumber, i, _value);
                if (string.IsNullOrEmpty(_value))
                    continue;

                // Take the last sub-part if available
                Parts.Calendar? subPart = null;
                if (begins.Count > 0)
                {
                    LoggingTools.Debug("Setting last sub-part [idx: {0}] as the target with type {1}", begins.Count - 1, subPart?.GetType().Name ?? "<null>");
                    subPart = begins[begins.Count - 1].Item2;
                }

                try
                {
                    // Check to see if we have a BEGIN or an END prefix
                    var info = new PropertyInfo(_value);
                    if (info.Prefix == CommonConstants._beginSpecifier)
                    {
                        string finalType = info.Value.ToUpper();
                        LoggingTools.Debug("Trying to add sub-part from inherited instance of {0}...", finalType);
                        begins.Add((finalType, GetCalendarInheritedInstance(finalType)));
                        continue;
                    }
                    else if (info.Prefix == CommonConstants._endSpecifier)
                    {
                        string expectedType = begins[begins.Count - 1].Item1;
                        LoggingTools.Debug("Expected type is {0}.", expectedType);
                        if (info.Value == expectedType)
                        {
                            LoggingTools.Debug("Saving sub-part...");
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
                        {
                            LoggingTools.Error("Expected type {0} for ending, got {1}", expectedType, info.Value);
                            throw new ArgumentException("Ending mismatch: Expected {0} vs. actual {1}".FormatString(expectedType, info.Value));
                        }
                        continue;
                    }

                    // Process this line
                    Process(_value, subPart ?? calendar, CalendarVersion);
                }
                catch (Exception ex)
                {
                    throw new VCalendarParseException(ex.Message, _value, lineNumber, ex);
                }
            }

            // Validate this calendar before returning it.
            calendar.Validate();
            LoggingTools.Info("Returning valid calendar...");
            return calendar;
        }

        internal static void Process(string _value, Parts.Calendar component, Version version)
        {
            LoggingTools.Debug("Processing value with component type {0} in version {1}: {2}", component.GetType().Name, version.ToString(), _value);

            // Now, parse a property
            var info = new PropertyInfo(_value);

            // Get the type and its properties
            Type calendarType = component.GetType();
            var partType = VCalendarParserTools.GetPartType(info.Prefix, version, calendarType);

            // Check the type for allowed types
            string[] elementTypes = CommonTools.GetTypes(info.Arguments, partType.defaultType);
            LoggingTools.Debug("Got {0} element types [{1}]", elementTypes.Length, string.Join(", ", elementTypes));
            foreach (string elementType in elementTypes)
            {
                string elementTypeUpper = elementType.ToUpper();
                LoggingTools.Debug("Processing element type [{0}, resolved to {1}]", elementType, elementTypeUpper);
                if (!partType.allowedExtraTypes.Contains(elementTypeUpper) && !elementTypeUpper.StartsWith("X-"))
                {
                    if (partType.type == PartType.PartsArray &&
                        ((CalendarPartsArrayEnum)partType.enumeration == CalendarPartsArrayEnum.IanaNames ||
                         (CalendarPartsArrayEnum)partType.enumeration == CalendarPartsArrayEnum.NonstandardNames))
                        continue;
                    LoggingTools.Error("Element type {0} is not in the list of allowed types", elementTypeUpper);
                    throw new InvalidDataException("Part info type {0} doesn't support property type {1} because the following types are supported: [{2}]".FormatString(partType.enumType?.Name ?? "<null>", elementTypeUpper, string.Join(", ", partType.allowedExtraTypes)));
                }
            }

            // Handle the part type, and extract the value
            string valueType = CommonTools.GetFirstValue(info.Arguments, partType.defaultValueType, CommonConstants._valueArgumentSpecifier);
            string finalValue = CommonTools.ProcessStringValue(info.Value, valueType, version.Major == 1 ? ';' : ',');
            info.ValueType = valueType;
            LoggingTools.Debug("Got value [type: {0}]: {1}", valueType, finalValue);

            // Check for allowed values
            if (partType.allowedValues.Length != 0)
            {
                bool found = false;
                LoggingTools.Debug("Comparing value against {0} allowed values [{1}] [type: {2}]: {3}", partType.allowedValues.Length, string.Join(", ", partType.allowedValues), valueType, finalValue);
                foreach (string allowedValue in partType.allowedValues)
                {
                    if (finalValue == allowedValue)
                        found = true;
                }
                if (!found)
                    throw new InvalidDataException("Value {0} not in the list of allowed values [{1}]".FormatString(finalValue, string.Join(", ", partType.allowedValues)));
                LoggingTools.Debug("Found allowed value [type: {0}]: {1}", valueType, finalValue);
            }

            // Check for support
            bool supported = partType.minimumVersionCondition(version);
            if (!supported)
            {
                LoggingTools.Warning("Part {0} doesn't support version {1}", partType.enumeration, version.ToString());
                return;
            }

            // Process the value
            LoggingTools.Debug("Part {0} is {1}", partType.enumeration, partType.type);
            switch (partType.type)
            {
                case PartType.Strings:
                    {
                        CalendarStringsEnum stringType = (CalendarStringsEnum)partType.enumeration;

                        // Set the string for real
                        var stringValueInfo = new ValueInfo<string>(info, -1, elementTypes, finalValue);
                        component.AddString(stringType, stringValueInfo);
                        LoggingTools.Debug("Added string {0} with value {1}", stringType, finalValue);
                    }
                    break;
                case PartType.Integers:
                    {
                        CalendarIntegersEnum integerType = (CalendarIntegersEnum)partType.enumeration;

                        // Get the final value
                        double finalDouble = double.Parse(finalValue);

                        // Set the integer for real
                        var stringValueInfo = new ValueInfo<double>(info, -1, elementTypes, finalDouble);
                        component.AddInteger(integerType, stringValueInfo);
                        LoggingTools.Debug("Added integer {0} with value {1}", integerType, finalValue);
                    }
                    break;
                case PartType.PartsArray:
                    {
                        CalendarPartsArrayEnum partsArrayType = (CalendarPartsArrayEnum)partType.enumeration;
                        bool isExtra = partsArrayType is CalendarPartsArrayEnum.NonstandardNames or CalendarPartsArrayEnum.IanaNames;

                        // Now, get the part info
                        if (isExtra)
                        {
                            // Get the base part info from the nonstandard value, stripping the group part from the initial value.
                            if (info.Group.Length > 0)
                                _value = _value.Substring(info.Group.Length + 1);
                            var partInfo =
                                partsArrayType == CalendarPartsArrayEnum.NonstandardNames ?
                                XNameInfo.FromStringStatic(_value, info, -1, elementTypes, version) :
                                ExtraInfo.FromStringStatic(_value, info, -1, elementTypes, version);
                            component.AddExtraPartToArray((PartsArrayEnum)partsArrayType, partInfo);
                            LoggingTools.Debug("Added extra part {0} with value {1}", partsArrayType, _value);
                        }
                        else
                        {
                            if (partType.fromStringFunc is null)
                                return;

                            // Get the part info from the part type and add it to the part array
                            var partInfo = partType.fromStringFunc(finalValue, info, -1, elementTypes, version);
                            component.AddPartToArray(partsArrayType, partInfo);
                            LoggingTools.Debug("Added part {0} to array with value {1}", partsArrayType, finalValue);
                        }
                    }
                    break;
                default:
                    LoggingTools.Error("Unknown part {0}", partType.type);
                    throw new InvalidDataException("The type {0} is invalid. Are you sure that you've specified the correct type in your vCalendar representation?".FormatString(partType.type));
            }
        }

        private Parts.Calendar GetCalendarInheritedInstance(string type)
        {
            LoggingTools.Debug("Trying to get inherited instance of type {0}", type);
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
                    throw new ArgumentException("Invalid type {0}".FormatString(type)),
            };
        }

        private void SaveLastSubPart(Parts.Calendar? subpart, ref Parts.Calendar part)
        {
            if (subpart is null)
                return;
            bool nestable = true;
            LoggingTools.Debug("Part type: {0}, sub-part type: {1}", part.GetType().Name, subpart.GetType().Name);
            switch (part.GetType().Name)
            {
                case nameof(Parts.Calendar):
                    switch (subpart.GetType().Name)
                    {
                        case nameof(CalendarEvent):
                            part.AddEvent((CalendarEvent)subpart);
                            break;
                        case nameof(CalendarTodo):
                            part.AddTodo((CalendarTodo)subpart);
                            break;
                        case nameof(CalendarJournal):
                            part.AddJournal((CalendarJournal)subpart);
                            break;
                        case nameof(CalendarFreeBusy):
                            part.AddFreeBusy((CalendarFreeBusy)subpart);
                            break;
                        case nameof(CalendarTimeZone):
                            part.AddTimeZone((CalendarTimeZone)subpart);
                            break;
                        case nameof(CalendarOtherComponent):
                            part.AddOther((CalendarOtherComponent)subpart);
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
            {
                LoggingTools.Warning("Part type {0} can't hold parts of type {1}", part.GetType().Name, subpart.GetType().Name);
                throw new ArgumentException("Can't place {0} inside {1}".FormatString(subpart.GetType().Name, part.GetType().Name));
            }
        }

        internal VCalendarParser((int, string)[] calendarContent, Version calendarVersion)
        {
            this.calendarContent = calendarContent;
            this.calendarVersion = calendarVersion;
        }
    }
}
