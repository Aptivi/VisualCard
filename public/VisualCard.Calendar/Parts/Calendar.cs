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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Textify.General;
using VisualCard.Calendar.Languages;
using VisualCard.Calendar.Parsers;
using VisualCard.Calendar.Parts.Comparers;
using VisualCard.Calendar.Parts.Enums;
using VisualCard.Calendar.Parts.Implementations.Event;
using VisualCard.Calendar.Parts.Implementations.Todo;
using VisualCard.Common.Diagnostics;
using VisualCard.Common.Parsers;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts;
using VisualCard.Common.Parts.Comparers;
using VisualCard.Common.Parts.Enums;
using VisualCard.Common.Parts.Implementations;
using VisualCard.Parsers;

namespace VisualCard.Calendar.Parts
{
    /// <summary>
    /// A vCalendar card instance
    /// </summary>
    [DebuggerDisplay("vCalendar version {CalendarVersion.ToString()}, parts: (A [{partsArray.Count}] | S [{strings.Count}] | I [{integers.Count}] | E [{extraParts.Count}])")]
    public class Calendar : IEquatable<Calendar>
    {
        internal readonly List<CalendarEvent> events = [];
        internal readonly List<CalendarTodo> todos = [];
        internal readonly List<CalendarJournal> journals = [];
        internal readonly List<CalendarFreeBusy> freeBusyList = [];
        internal readonly List<CalendarTimeZone> timeZones = [];
        internal readonly List<CalendarOtherComponent> others = [];
        internal readonly Dictionary<PartsArrayEnum, List<BasePartInfo>> extraParts = [];
        internal readonly Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> partsArray = [];
        internal readonly Dictionary<CalendarStringsEnum, List<ValueInfo<string>>> strings = [];
        internal readonly Dictionary<CalendarIntegersEnum, List<ValueInfo<double>>> integers = [];
        private readonly Version version;

        /// <summary>
        /// The vCalendar version
        /// </summary>
        public Version CalendarVersion =>
            version;

        /// <summary>
        /// Unique ID for this card
        /// </summary>
        public string UniqueId =>
            GetString(CalendarStringsEnum.Uid).Length > 0 ? GetString(CalendarStringsEnum.Uid)[0].Value : "";

        /// <summary>
        /// Event list
        /// </summary>
        public CalendarEvent[] Events =>
            [.. events];

        /// <summary>
        /// To-do list
        /// </summary>
        public CalendarTodo[] Todos =>
            [.. todos];

        /// <summary>
        /// Journal list
        /// </summary>
        public CalendarJournal[] Journals =>
            [.. journals];

        /// <summary>
        /// Free/busy list
        /// </summary>
        public CalendarFreeBusy[] FreeBusyList =>
            [.. freeBusyList];

        /// <summary>
        /// Time zone list
        /// </summary>
        public CalendarTimeZone[] TimeZones =>
            [.. timeZones];

        /// <summary>
        /// Other component list
        /// </summary>
        public CalendarOtherComponent[] Others =>
            [.. others];

        /// <summary>
        /// Extra part array list in a dictionary (for enumeration operations)
        /// </summary>
        public ReadOnlyDictionary<PartsArrayEnum, ReadOnlyCollection<BasePartInfo>> ExtraParts =>
            new(extraParts.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.AsReadOnly()));

        /// <summary>
        /// Part array list in a dictionary (for enumeration operations)
        /// </summary>
        public ReadOnlyDictionary<CalendarPartsArrayEnum, ReadOnlyCollection<BaseCalendarPartInfo>> PartsArray =>
            new(partsArray.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.AsReadOnly()));

        /// <summary>
        /// String list in a dictionary (for enumeration operations)
        /// </summary>
        public ReadOnlyDictionary<CalendarStringsEnum, ReadOnlyCollection<ValueInfo<string>>> Strings =>
            new(strings.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.AsReadOnly()));

        /// <summary>
        /// Integer list in a dictionary (for enumeration operations)
        /// </summary>
        public ReadOnlyDictionary<CalendarIntegersEnum, ReadOnlyCollection<ValueInfo<double>>> Integers =>
            new(integers.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.AsReadOnly()));

        /// <summary>
        /// Adds an event to the calendar (To validate, you'll need to call <see cref="Validate"/>).
        /// </summary>
        /// <param name="eventInstance">Instance of a calendar event</param>
        public void AddEvent(CalendarEvent eventInstance) =>
            events.Add(eventInstance);

        /// <summary>
        /// Deletes an event from the calendar (To validate, you'll need to call <see cref="Validate"/>).
        /// </summary>
        /// <param name="eventInstance">Instance of a calendar event</param>
        public void DeleteEvent(CalendarEvent eventInstance) =>
            events.Remove(eventInstance);

        /// <summary>
        /// Adds a todo to the calendar (To validate, you'll need to call <see cref="Validate"/>).
        /// </summary>
        /// <param name="todoInstance">Instance of a calendar todo</param>
        public void AddTodo(CalendarTodo todoInstance) =>
            todos.Add(todoInstance);

        /// <summary>
        /// Deletes a todo to the calendar (To validate, you'll need to call <see cref="Validate"/>).
        /// </summary>
        /// <param name="todoInstance">Instance of a calendar todo</param>
        public void DeleteTodo(CalendarTodo todoInstance) =>
            todos.Remove(todoInstance);

        /// <summary>
        /// Adds a journal to the calendar (To validate, you'll need to call <see cref="Validate"/>).
        /// </summary>
        /// <param name="journalInstance">Instance of a calendar journal</param>
        public void AddJournal(CalendarJournal journalInstance) =>
            journals.Add(journalInstance);

        /// <summary>
        /// Deletes a journal to the calendar (To validate, you'll need to call <see cref="Validate"/>).
        /// </summary>
        /// <param name="journalInstance">Instance of a calendar journal</param>
        public void DeleteJournal(CalendarJournal journalInstance) =>
            journals.Remove(journalInstance);

        /// <summary>
        /// Adds a free/busy info to the calendar (To validate, you'll need to call <see cref="Validate"/>).
        /// </summary>
        /// <param name="freeBusyInstance">Instance of a calendar free/busy info</param>
        public void AddFreeBusy(CalendarFreeBusy freeBusyInstance) =>
            freeBusyList.Add(freeBusyInstance);

        /// <summary>
        /// Deletes a free/busy info to the calendar (To validate, you'll need to call <see cref="Validate"/>).
        /// </summary>
        /// <param name="freeBusyInstance">Instance of a calendar free/busy info</param>
        public void DeleteFreeBusy(CalendarFreeBusy freeBusyInstance) =>
            freeBusyList.Remove(freeBusyInstance);

        /// <summary>
        /// Adds a timezone to the calendar (To validate, you'll need to call <see cref="Validate"/>).
        /// </summary>
        /// <param name="timeZoneInstance">Instance of a calendar timezone</param>
        public void AddTimeZone(CalendarTimeZone timeZoneInstance) =>
            timeZones.Add(timeZoneInstance);

        /// <summary>
        /// Deletes a timezone to the calendar (To validate, you'll need to call <see cref="Validate"/>).
        /// </summary>
        /// <param name="timeZoneInstance">Instance of a calendar timezone</param>
        public void DeleteTimeZone(CalendarTimeZone timeZoneInstance) =>
            timeZones.Remove(timeZoneInstance);

        /// <summary>
        /// Adds a custom component to the calendar (To validate, you'll need to call <see cref="Validate"/>).
        /// </summary>
        /// <param name="otherInstance">Instance of a calendar custom component</param>
        public void AddOther(CalendarOtherComponent otherInstance) =>
            others.Add(otherInstance);

        /// <summary>
        /// Deletes a custom component to the calendar (To validate, you'll need to call <see cref="Validate"/>).
        /// </summary>
        /// <param name="otherInstance">Instance of a calendar custom component</param>
        public void DeleteOther(CalendarOtherComponent otherInstance) =>
            others.Remove(otherInstance);

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <returns>An array of values or an empty part array []</returns>
        public TPart[] GetExtraPartsArray<TPart>() where TPart : BasePartInfo
        {
            // Get the parts enumeration according to the type
            var key = VCalendarParserTools.GetPartsArrayEnumFromType(typeof(TPart), CalendarVersion, GetType());

            // Now, return the value
            return GetExtraPartsArray<TPart>((PartsArrayEnum)key);
        }

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>An array of values or an empty part array []</returns>
        public TPart[] GetExtraPartsArray<TPart>(PartsArrayEnum key) where TPart : BasePartInfo =>
            GetExtraPartsArray(typeof(TPart), key).Cast<TPart>().ToArray();

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="partType">Target part type that derives from <see cref="BasePartInfo"/></param>
        /// <returns>An array of values or an empty part array []</returns>
        public BasePartInfo[] GetExtraPartsArray(Type partType)
        {
            // Check the base type
            CommonTools.VerifyBasePartType(partType);

            // Get the parts enumeration according to the type
            var key = VCalendarParserTools.GetPartsArrayEnumFromType(partType, CalendarVersion, GetType());

            // Now, return the value
            return GetExtraPartsArray(partType, (PartsArrayEnum)key);
        }

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>An array of values or an empty part array []</returns>
        public BasePartInfo[] GetExtraPartsArray(PartsArrayEnum key)
        {
            string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum((CalendarPartsArrayEnum)key);
            var partType = VCalendarParserTools.GetPartType(prefix, CalendarVersion, typeof(Calendar));
            if (partType.enumType is null)
                throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_ENUMTYPENOTFOUND").FormatString(key));
            return GetExtraPartsArray(partType.enumType, key);
        }

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="partType">Target part type that derives from <see cref="BasePartInfo"/></param>
        /// <param name="key">A key to use</param>
        /// <returns>An array of values or an empty part array []</returns>
        public BasePartInfo[] GetExtraPartsArray(Type partType, PartsArrayEnum key)
        {
            VerifyPartsArrayType((CalendarPartsArrayEnum)key, partType, GetType());

            // Check for version support
            string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum((CalendarPartsArrayEnum)key);
            var type = VCalendarParserTools.GetPartType(prefix, version, GetType());
            if (!type.minimumVersionCondition(version))
                return [];

            // Check to see if the part array has a value or not
            bool hasValue = extraParts.ContainsKey(key);
            if (!hasValue)
                return [];

            // Cast the values
            var value = extraParts[key];
            BasePartInfo[] parts = [.. value];

            // Now, return the value
            return parts;
        }

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <returns>An array of values or an empty part array []</returns>
        public TPart[] GetPartsArray<TPart>() where TPart : BaseCalendarPartInfo
        {
            // Get the parts enumeration according to the type
            var key = VCalendarParserTools.GetPartsArrayEnumFromType(typeof(TPart), CalendarVersion, GetType());

            // Now, return the value
            return GetPartsArray<TPart>(key);
        }

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>An array of values or an empty part array []</returns>
        public TPart[] GetPartsArray<TPart>(CalendarPartsArrayEnum key) where TPart : BaseCalendarPartInfo =>
            GetPartsArray(typeof(TPart), key).Cast<TPart>().ToArray();

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="partType">Target part type that derives from <see cref="BaseCalendarPartInfo"/></param>
        /// <returns>An array of values or an empty part array []</returns>
        public BaseCalendarPartInfo[] GetPartsArray(Type partType)
        {
            // Check the base type
            VerifyPartType(partType);

            // Get the parts enumeration according to the type
            var key = VCalendarParserTools.GetPartsArrayEnumFromType(partType, CalendarVersion, GetType());

            // Now, return the value
            return GetPartsArray(partType, key);
        }

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>An array of values or an empty part array []</returns>
        public BaseCalendarPartInfo[] GetPartsArray(CalendarPartsArrayEnum key)
        {
            string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum(key);
            var partType = VCalendarParserTools.GetPartType(prefix, CalendarVersion, typeof(Calendar));
            if (partType.enumType is null)
                throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_ENUMTYPENOTFOUND").FormatString(key));
            return GetPartsArray(partType.enumType, key);
        }

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="partType">Target part type that derives from <see cref="BaseCalendarPartInfo"/></param>
        /// <param name="key">A key to use</param>
        /// <returns>An array of values or an empty part array []</returns>
        public BaseCalendarPartInfo[] GetPartsArray(Type partType, CalendarPartsArrayEnum key)
        {
            VerifyPartsArrayType(key, partType, GetType());

            // Check for version support
            string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum(key);
            var type = VCalendarParserTools.GetPartType(prefix, version, GetType());
            if (!type.minimumVersionCondition(version))
                return [];

            // Check to see if the part array has a value or not
            bool hasValue = partsArray.ContainsKey(key);
            if (!hasValue)
                return [];

            // Cast the values
            var value = partsArray[key];
            BaseCalendarPartInfo[] parts = [.. value];

            // Now, return the value
            return parts;
        }

        /// <summary>
        /// Gets a string from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>A value or null if any other type either doesn't exist or the type is not supported by the card version</returns>
        public ValueInfo<string>[] GetString(CalendarStringsEnum key)
        {
            // Check for version support
            string prefix = VCalendarParserTools.GetPrefixFromStringsEnum(key);
            var partType = VCalendarParserTools.GetPartType(prefix, version, GetType());
            if (!partType.minimumVersionCondition(version))
                return [];

            // Check to see if the string has a value or not
            bool hasValue = strings.TryGetValue(key, out var values);
            if (!hasValue)
                return [];

            // Return the list
            return [.. values];
        }

        /// <summary>
        /// Gets a integer from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>A value or null if any other type either doesn't exist or the type is not supported by the card version</returns>
        public ValueInfo<double>[] GetInteger(CalendarIntegersEnum key)
        {
            // Check for version support
            string prefix = VCalendarParserTools.GetPrefixFromIntegersEnum(key);
            var partType = VCalendarParserTools.GetPartType(prefix, version, GetType());
            if (!partType.minimumVersionCondition(version))
                return [];

            // Check to see if the integer has a value or not
            bool hasValue = integers.TryGetValue(key, out var values);
            if (!hasValue)
                return [];

            // Return the list
            return [.. values];
        }

        /// <summary>
        /// Finds a part array from a specified key
        /// </summary>
        /// <param name="prefixToFind">Part of prefix to find (case-insensitive)</param>
        /// <returns>An array of values or an empty part array []</returns>
        public TPart[] FindExtraPartsArray<TPart>(string prefixToFind)
            where TPart : BasePartInfo
        {
            // Get the extra part enumeration according to the type
            var key = (PartsArrayEnum)VCalendarParserTools.GetPartsArrayEnumFromType(typeof(TPart), version, GetType());

            // Now, return the value
            return FindExtraPartsArray<TPart>(key, prefixToFind);
        }

        /// <summary>
        /// Finds a part array from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <param name="prefixToFind">Part of prefix to find (case-insensitive)</param>
        /// <returns>An array of values or an empty part array []</returns>
        public TPart[] FindExtraPartsArray<TPart>(PartsArrayEnum key, string prefixToFind)
            where TPart : BasePartInfo =>
            FindExtraPartsArray(typeof(TPart), key, prefixToFind).Cast<TPart>().ToArray();

        /// <summary>
        /// Finds a part array from a specified key
        /// </summary>
        /// <param name="partType">Target part type that derives from <see cref="BasePartInfo"/></param>
        /// <param name="prefixToFind">Part of prefix to find (case-insensitive)</param>
        /// <returns>An array of values or an empty part array []</returns>
        public BasePartInfo[] FindExtraPartsArray(Type partType, string prefixToFind)
        {
            // Check the base type
            CommonTools.VerifyBasePartType(partType);

            // Get the extra part enumeration according to the type
            var key = (PartsArrayEnum)VCalendarParserTools.GetPartsArrayEnumFromType(partType, version, GetType());

            // Now, return the value
            return FindExtraPartsArray(partType, key, prefixToFind);
        }

        /// <summary>
        /// Finds a part array from a specified key
        /// </summary>
        /// <param name="partType">Target part type that derives from <see cref="BasePartInfo"/></param>
        /// <param name="key">A key to use</param>
        /// <param name="prefixToFind">Part of prefix to find (case-insensitive)</param>
        /// <returns>An array of values or an empty part array []</returns>
        public BasePartInfo[] FindExtraPartsArray(Type partType, PartsArrayEnum key, string prefixToFind) =>
            GetExtraPartsArray(partType, key).Where((bpi) =>
            {
                if (bpi is XNameInfo xNameInfo)
                    return xNameInfo.XKeyName?.ContainsWithNoCase(prefixToFind) ?? false;
                else if (bpi is ExtraInfo extraInfo)
                    return extraInfo.KeyName?.ContainsWithNoCase(prefixToFind) ?? false;
                return false;
            }).ToArray();

        /// <summary>
        /// Saves this parsed calendar to the string
        /// </summary>
        /// <param name="validate">Whether to validate all the fields or not</param>
        public virtual string SaveToString(bool validate = false) =>
            SaveToString(version, VCalendarConstants._objectVCalendarSpecifier, validate);

        internal string SaveToString(Version version, string objectType, bool validate)
        {
            // Check to see if we need to validate
            if (validate)
            {
                LoggingTools.Info("Validation requested before saving");
                Validate();
            }

            // Initialize the calendar builder
            var calendarBuilder = new StringBuilder();

            // First, write the header
            LoggingTools.Debug("Writing header for {0}", objectType);
            calendarBuilder.AppendLine($"{CommonConstants._beginSpecifier}:{objectType}");
            if (objectType == VCalendarConstants._objectVCalendarSpecifier)
                calendarBuilder.AppendLine($"{CommonConstants._versionSpecifier}:{version}");

            // Then, enumerate all the strings
            foreach (CalendarStringsEnum stringEnum in strings.Keys)
            {
                // Get the string values
                var array = GetString(stringEnum);
                if (array is null || array.Length == 0)
                    continue;
                LoggingTools.Debug("Installing {0} strings to calendar text...", array.Length);

                // Get the prefix
                string prefix = VCalendarParserTools.GetPrefixFromStringsEnum(stringEnum);
                var type = VCalendarParserTools.GetPartType(prefix, version, GetType());
                string defaultType = type.defaultType;
                string defaultValueType = type.defaultValueType;

                // Now, assemble the line
                foreach (var part in array)
                {
                    var partBuilder = new StringBuilder();
                    string partArguments = BuilderTools.BuildArguments(part, defaultType, defaultValueType);
                    string[] partArgumentsLines = partArguments.SplitNewLines();
                    string group = part.Group;
                    if (!string.IsNullOrEmpty(group))
                        partBuilder.Append($"{group}.");
                    partBuilder.Append($"{prefix}");
                    partBuilder.Append($"{partArguments}");
                    partBuilder.Append($"{CommonTools.MakeStringBlock(part.Value, partArgumentsLines[partArgumentsLines.Length - 1].Length + prefix.Length, encoding: part.Encoding ?? "")}");
                    LoggingTools.Debug("Adding string to line with length {0} [prefix: {1}, {2}]", partBuilder.Length, prefix, partArguments);
                    calendarBuilder.AppendLine($"{partBuilder}");
                }
            }

            // Then, enumerate all the integers
            foreach (CalendarIntegersEnum integerEnum in integers.Keys)
            {
                // Get the string value
                var array = GetInteger(integerEnum);
                if (array is null || array.Length == 0)
                    continue;
                LoggingTools.Debug("Installing {0} integers to calendar text...", array.Length);

                // Get the prefix
                string prefix = VCalendarParserTools.GetPrefixFromIntegersEnum(integerEnum);
                var type = VCalendarParserTools.GetPartType(prefix, version, GetType());
                string defaultType = type.defaultType;
                string defaultValueType = type.defaultValueType;

                // Now, assemble the line
                foreach (var part in array)
                {
                    var partBuilder = new StringBuilder();
                    string partArguments = BuilderTools.BuildArguments(part, defaultType, defaultValueType);
                    string[] partArgumentsLines = partArguments.SplitNewLines();
                    string group = part.Group;
                    if (!string.IsNullOrEmpty(group))
                        partBuilder.Append($"{group}.");
                    partBuilder.Append($"{prefix}");
                    partBuilder.Append($"{partArguments}");
                    partBuilder.Append($"{CommonTools.MakeStringBlock($"{part.Value}", partArgumentsLines[partArgumentsLines.Length - 1].Length + prefix.Length, encoding: part.Encoding ?? "")}");
                    LoggingTools.Debug("Adding integer to line with length {0} [prefix: {1}, {2}]", partBuilder.Length, prefix, partArguments);
                    calendarBuilder.AppendLine($"{partBuilder}");
                }
            }

            // Then, enumerate all the arrays
            foreach (CalendarPartsArrayEnum partsArrayEnum in partsArray.Keys)
            {
                // Get the array value
                var array = GetPartsArray<BaseCalendarPartInfo>(partsArrayEnum);
                if (array is null || array.Length == 0)
                    continue;
                LoggingTools.Debug("Installing {0} parts to calendar text...", array.Length);

                // Get the prefix
                string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum(partsArrayEnum);
                var type = VCalendarParserTools.GetPartType(prefix, version, GetType());
                string defaultType = type.defaultType;
                string defaultValueType = type.defaultValueType;

                // Now, assemble the line
                foreach (var part in array)
                {
                    var partBuilder = new StringBuilder();
                    string partRepresentation = part.ToStringInternal(version);
                    string partArguments = BuilderTools.BuildArguments(part, defaultType, defaultValueType);
                    string[] partArgumentsLines = partArguments.SplitNewLines();
                    string group = part.Group;
                    if (!string.IsNullOrEmpty(group))
                        partBuilder.Append($"{group}.");
                    partBuilder.Append($"{prefix}");
                    partBuilder.Append($"{partArguments}");
                    partBuilder.Append($"{CommonTools.MakeStringBlock(partRepresentation, partArgumentsLines[partArgumentsLines.Length - 1].Length + prefix.Length, encoding: part.Encoding ?? "")}");
                    LoggingTools.Debug("Adding part to line with length {0} [prefix: {1}, {2}]", partBuilder.Length, prefix, partArguments);
                    calendarBuilder.AppendLine($"{partBuilder}");
                }
            }

            // Then, enumerate all the extra arrays
            foreach (PartsArrayEnum extraPartEnum in extraParts.Keys)
            {
                // Get the array value
                var array = GetExtraPartsArray<BasePartInfo>(extraPartEnum);
                if (array is null || array.Length == 0)
                    continue;
                LoggingTools.Debug("Installing {0} extra parts to calendar text...", array.Length);

                // Get the prefix
                string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum((CalendarPartsArrayEnum)extraPartEnum);
                var type = VCalendarParserTools.GetPartType(prefix, version, GetType());
                string defaultType = type.defaultType;
                string defaultValueType = type.defaultValueType;

                // Now, assemble the line
                foreach (var part in array)
                {
                    var partBuilder = new StringBuilder();
                    string partRepresentation = part.ToStringInternal(version);
                    string partArguments = BuilderTools.BuildArguments(part, defaultType, defaultValueType);
                    string[] partArgumentsLines = partArguments.SplitNewLines();
                    string group = part.Group;
                    if (!string.IsNullOrEmpty(group))
                        partBuilder.Append($"{group}.");
                    partBuilder.Append($"{prefix}");
                    partBuilder.Append($"{partArguments}");
                    partBuilder.Append($"{CommonTools.MakeStringBlock(partRepresentation, partArgumentsLines[partArgumentsLines.Length - 1].Length + prefix.Length, encoding: part.Encoding ?? "")}");
                    LoggingTools.Debug("Adding extra part to line with length {0} [prefix: {1}, {2}]", partBuilder.Length, prefix, partArguments);
                    calendarBuilder.AppendLine($"{partBuilder}");
                }
            }

            // Then, the components
            if (objectType == VCalendarConstants._objectVCalendarSpecifier)
            {
                LoggingTools.Debug("Installing {0} events to calendar text...", events.Count);
                foreach (var calendarEvent in events)
                    calendarBuilder.Append(calendarEvent.SaveToString());

                LoggingTools.Debug("Installing {0} todos to calendar text...", todos.Count);
                foreach (var calendarTodo in todos)
                    calendarBuilder.Append(calendarTodo.SaveToString());

                LoggingTools.Debug("Installing {0} journals to calendar text...", journals.Count);
                foreach (var calendarJournal in journals)
                    calendarBuilder.Append(calendarJournal.SaveToString());

                LoggingTools.Debug("Installing {0} free/busy info to calendar text...", freeBusyList.Count);
                foreach (var calendarFreeBusy in freeBusyList)
                    calendarBuilder.Append(calendarFreeBusy.SaveToString());

                LoggingTools.Debug("Installing {0} time zones to calendar text...", timeZones.Count);
                foreach (var calendarTimeZone in timeZones)
                    calendarBuilder.Append(calendarTimeZone.SaveToString());

                LoggingTools.Debug("Installing {0} extra components to calendar text...", others.Count);
                foreach (var calendarOther in others)
                    calendarBuilder.Append(calendarOther.SaveToString());
            }
            else if (objectType == VCalendarConstants._objectVEventSpecifier)
            {
                var alarms = ((CalendarEvent)this).alarms;
                LoggingTools.Debug("Installing {0} alarms to calendar event text...", alarms.Count);
                foreach (var calendarAlarm in alarms)
                    calendarBuilder.Append(calendarAlarm.SaveToString());
            }
            else if (objectType == VCalendarConstants._objectVTodoSpecifier)
            {
                var alarms = ((CalendarTodo)this).alarms;
                LoggingTools.Debug("Installing {0} alarms to calendar todo text...", alarms.Count);
                foreach (var calendarAlarm in alarms)
                    calendarBuilder.Append(calendarAlarm.SaveToString());
            }
            else if (objectType == VCalendarConstants._objectVTimeZoneSpecifier)
            {
                var standards = ((CalendarTimeZone)this).standards;
                LoggingTools.Debug("Installing {0} daylight time zone info to calendar time zone text...", standards.Count);
                foreach (var calendarStandard in standards)
                    calendarBuilder.Append(calendarStandard.SaveToString());

                var daylights = ((CalendarTimeZone)this).daylights;
                LoggingTools.Debug("Installing {0} daylight time zone info to calendar time zone text...", daylights.Count);
                foreach (var calendarDaylight in daylights)
                    calendarBuilder.Append(calendarDaylight.SaveToString());
            }

            // End the calendar and return it
            LoggingTools.Debug("Writing footer...");
            calendarBuilder.AppendLine($"{CommonConstants._endSpecifier}:{objectType}");
            LoggingTools.Info("Returning calendar text with length {0}...", calendarBuilder.Length);
            return calendarBuilder.ToString();
        }

        /// <summary>
        /// Saves this parsed calendar to a file path
        /// </summary>
        /// <param name="path">File path to save this calendar to</param>
        public void SaveTo(string path)
        {
            // Save all the changes to the file
            var calendarString = SaveToString();
            File.WriteAllText(path, calendarString);
        }

        /// <summary>
        /// Deletes a string from the list of string values
        /// </summary>
        /// <param name="stringsEnum">String type</param>
        /// <param name="idx">Index of a string value</param>
        /// <returns>True if successful; false otherwise</returns>
        public bool DeleteString(CalendarStringsEnum stringsEnum, int idx)
        {
            // Get the string values
            var stringValues = GetString(stringsEnum);

            // Check the index
            if (idx >= stringValues.Length)
                return false;

            // Remove the string value
            var stringValue = strings[stringsEnum][idx];
            bool result = strings[stringsEnum].Remove(stringValue);
            LoggingTools.Debug("Removal of {0} from {1} result: {2}", idx, stringsEnum, result);

            // Delete section if needed
            if (strings[stringsEnum].Count == 0)
            {
                LoggingTools.Warning("Deleting dangling section {0}...", stringsEnum);
                strings.Remove(stringsEnum);
            }
            return result;
        }

        /// <summary>
        /// Deletes an integer from the list of integer values
        /// </summary>
        /// <param name="integersEnum">Integer type</param>
        /// <param name="idx">Index of a integer value</param>
        /// <returns>True if successful; false otherwise</returns>
        public bool DeleteInteger(CalendarIntegersEnum integersEnum, int idx)
        {
            // Get the integer values
            var integerValues = GetInteger(integersEnum);

            // Check the index
            if (idx >= integerValues.Length)
                return false;

            // Remove the integer value
            var integerValue = integers[integersEnum][idx];
            bool result = integers[integersEnum].Remove(integerValue);
            LoggingTools.Debug("Removal of {0} from {1} result: {2}", idx, integersEnum, result);

            // Delete section if needed
            if (integers[integersEnum].Count == 0)
            {
                LoggingTools.Warning("Deleting dangling section {0}...", integersEnum);
                integers.Remove(integersEnum);
            }
            return result;
        }

        /// <summary>
        /// Deletes a part from the list of parts
        /// </summary>
        /// <param name="partsArrayEnum">Part array type</param>
        /// <param name="idx">Index of a part</param>
        /// <returns>True if successful; false otherwise</returns>
        public bool DeletePartsArray(CalendarPartsArrayEnum partsArrayEnum, int idx)
        {
            // Get the part type
            string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum(partsArrayEnum);
            var type = VCalendarParserTools.GetPartType(prefix, CalendarVersion, GetType());
            var partType = type.enumType ??
                throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_ENUMTYPEUNDETERMINABLE_DELETE"));

            // Remove the string value
            return DeletePartsArray(partsArrayEnum, partType, idx);
        }

        /// <summary>
        /// Deletes a part from the list of parts
        /// </summary>
        /// <param name="partsArrayEnum">Part array type</param>
        /// <param name="enumType">Enumeration type</param>
        /// <param name="idx">Index of a part</param>
        /// <returns>True if successful; false otherwise</returns>
        public bool DeletePartsArray(CalendarPartsArrayEnum partsArrayEnum, Type enumType, int idx)
        {
            // Get the string values
            var parts = GetPartsArray(enumType, partsArrayEnum);

            // Check the index
            if (idx >= parts.Length)
                return false;

            // Remove the string value
            return DeletePartsArrayInternal(partsArrayEnum, idx);
        }

        /// <summary>
        /// Deletes an extra part from the list of parts
        /// </summary>
        /// <param name="idx">Index of an extra part</param>
        /// <returns>True if successful; false otherwise</returns>
        public bool DeleteExtraPartsArray<TPart>(int idx)
            where TPart : BasePartInfo
        {
            // Get the parts enumeration according to the type
            var key = VCalendarParserTools.GetPartsArrayEnumFromType(typeof(TPart), CalendarVersion, GetType());

            // Remove the part
            return DeleteExtraPartsArray<TPart>((PartsArrayEnum)key, idx);
        }

        /// <summary>
        /// Deletes an extra part from the list of parts
        /// </summary>
        /// <param name="partsArrayEnum">Part array type</param>
        /// <param name="idx">Index of an extra part</param>
        /// <returns>True if successful; false otherwise</returns>
        public bool DeleteExtraPartsArray<TPart>(PartsArrayEnum partsArrayEnum, int idx)
            where TPart : BasePartInfo
        {
            // Get the parts
            var parts = GetExtraPartsArray(typeof(TPart), partsArrayEnum);

            // Check the index
            if (idx >= parts.Length)
                return false;

            // Remove the part
            return DeletePartsArrayInternal((CalendarPartsArrayEnum)partsArrayEnum, idx);
        }

        /// <summary>
        /// Deletes an extra part from the list of parts
        /// </summary>
        /// <param name="partsArrayEnum">Part array type</param>
        /// <param name="idx">Index of an extra part</param>
        /// <returns>True if successful; false otherwise</returns>
        public bool DeleteExtraPartsArray(PartsArrayEnum partsArrayEnum, int idx)
        {
            // Get the part type
            string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum((CalendarPartsArrayEnum)partsArrayEnum);
            var type = VCalendarParserTools.GetPartType(prefix, CalendarVersion, GetType());
            var partType = type.enumType ??
                throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_ENUMTYPEUNDETERMINABLE_DELETE"));

            // Remove the string value
            return DeleteExtraPartsArray(partsArrayEnum, partType, idx);
        }

        /// <summary>
        /// Deletes an extra part from the list of parts
        /// </summary>
        /// <param name="partsArrayEnum">Part array type</param>
        /// <param name="enumType">Enumeration type</param>
        /// <param name="idx">Index of an extra part</param>
        /// <returns>True if successful; false otherwise</returns>
        public bool DeleteExtraPartsArray(PartsArrayEnum partsArrayEnum, Type enumType, int idx)
        {
            // Get the string values
            var parts = GetExtraPartsArray(enumType, partsArrayEnum);

            // Check the index
            if (idx >= parts.Length)
                return false;

            // Remove the string value
            return DeletePartsArrayInternal((CalendarPartsArrayEnum)partsArrayEnum, idx);
        }

        /// <summary>
        /// Deletes a part from the list of parts
        /// </summary>
        /// <param name="idx">Index of a part</param>
        /// <returns>True if successful; false otherwise</returns>
        public bool DeletePartsArray<TPart>(int idx)
            where TPart : BaseCalendarPartInfo
        {
            // Get the parts enumeration according to the type
            var key = VCalendarParserTools.GetPartsArrayEnumFromType(typeof(TPart), CalendarVersion, GetType());

            // Remove the part
            return DeletePartsArray<TPart>(key, idx);
        }

        /// <summary>
        /// Deletes a part from the list of parts
        /// </summary>
        /// <param name="partsArrayEnum">Part array type</param>
        /// <param name="idx">Index of a part</param>
        /// <returns>True if successful; false otherwise</returns>
        public bool DeletePartsArray<TPart>(CalendarPartsArrayEnum partsArrayEnum, int idx)
            where TPart : BaseCalendarPartInfo
        {
            // Get the parts
            var parts = GetPartsArray(typeof(TPart), partsArrayEnum);

            // Check the index
            if (idx >= parts.Length)
                return false;

            // Remove the part
            return DeletePartsArrayInternal(partsArrayEnum, idx);
        }

        internal bool DeletePartsArrayInternal(CalendarPartsArrayEnum partsArrayEnum, int idx)
        {
            if (partsArrayEnum is CalendarPartsArrayEnum.IanaNames or CalendarPartsArrayEnum.NonstandardNames)
            {
                // Remove the extra part
                var extraPartEnum = (PartsArrayEnum)partsArrayEnum;
                var part = extraParts[extraPartEnum][idx];
                bool result = extraParts[extraPartEnum].Remove(part);
                LoggingTools.Debug("Removal of {0} from {1} result: {2}", idx, partsArrayEnum, result);
                if (extraParts[extraPartEnum].Count == 0)
                {
                    LoggingTools.Warning("Deleting dangling section {0}...", extraPartEnum);
                    extraParts.Remove(extraPartEnum);
                }
                return result;
            }
            else
            {
                // Remove the part
                var part = partsArray[partsArrayEnum][idx];
                bool result = partsArray[partsArrayEnum].Remove(part);
                LoggingTools.Debug("Removal of {0} from {1} result: {2}", idx, partsArrayEnum, result);
                if (partsArray[partsArrayEnum].Count == 0)
                {
                    LoggingTools.Warning("Deleting dangling section {0}...", partsArrayEnum);
                    partsArray.Remove(partsArrayEnum);
                }
                return result;
            }
        }

        /// <summary>
        /// Adds a part to the array
        /// </summary>
        /// <typeparam name="TPart">Part type to add</typeparam>
        /// <param name="rawValue">Raw value representing a group of values delimited by the semicolon</param>
        /// <param name="group">Property group (can be nested with a dot)</param>
        /// <param name="extraPrefix">Extra prefix to append (only for <see cref="PartsArrayEnum.NonstandardNames"/> and <see cref="PartsArrayEnum.IanaNames"/>)</param>
        /// <param name="args">Argument list to be added</param>
        public void AddPartToArray<TPart>(string rawValue, string group = "", string extraPrefix = "", params ArgumentInfo[] args)
            where TPart : BaseCalendarPartInfo
        {
            // Get the parts enumeration according to the type
            var key = VCalendarParserTools.GetPartsArrayEnumFromType(typeof(TPart), CalendarVersion, GetType());

            // Now, add the part
            AddPartToArray<TPart>(key, rawValue, group, extraPrefix, args);
        }

        /// <summary>
        /// Adds a part to the array
        /// </summary>
        /// <typeparam name="TPart">Part type to add</typeparam>
        /// <param name="key">Part array type</param>
        /// <param name="rawValue">Raw value representing a group of values delimited by the semicolon</param>
        /// <param name="group">Property group (can be nested with a dot)</param>
        /// <param name="extraPrefix">Extra prefix to append (only for <see cref="PartsArrayEnum.NonstandardNames"/> and <see cref="PartsArrayEnum.IanaNames"/>)</param>
        /// <param name="args">Argument list to be added</param>
        public void AddPartToArray<TPart>(CalendarPartsArrayEnum key, string rawValue, string group = "", string extraPrefix = "", params ArgumentInfo[] args)
            where TPart : BaseCalendarPartInfo =>
            AddPartToArray(key, typeof(TPart), rawValue, group, extraPrefix, args);

        /// <summary>
        /// Adds a part to the array
        /// </summary>
        /// <param name="key">Part array type</param>
        /// <param name="rawValue">Raw value representing a group of values delimited by the semicolon</param>
        /// <param name="group">Property group (can be nested with a dot)</param>
        /// <param name="extraPrefix">Extra prefix to append (only for <see cref="PartsArrayEnum.NonstandardNames"/> and <see cref="PartsArrayEnum.IanaNames"/>)</param>
        /// <param name="args">Argument list to be added</param>
        public void AddPartToArray(CalendarPartsArrayEnum key, string rawValue, string group = "", string extraPrefix = "", params ArgumentInfo[] args)
        {
            // Get the part type
            string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum(key);
            var type = VCalendarParserTools.GetPartType(prefix, CalendarVersion, GetType());
            var partType = type.enumType ??
                throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_ENUMTYPEUNDETERMINABLE_ADD"));

            // Now, add the part
            AddPartToArray(key, partType, rawValue, group, extraPrefix, args);
        }

        /// <summary>
        /// Adds a part to the array
        /// </summary>
        /// <param name="key">Part array type</param>
        /// <param name="partType">Enumeration type</param>
        /// <param name="rawValue">Raw value representing a group of values delimited by the semicolon</param>
        /// <param name="group">Property group (can be nested with a dot)</param>
        /// <param name="extraPrefix">Extra prefix to append (only for <see cref="PartsArrayEnum.NonstandardNames"/> and <see cref="PartsArrayEnum.IanaNames"/>)</param>
        /// <param name="args">Argument list to be added</param>
        public void AddPartToArray(CalendarPartsArrayEnum key, Type partType, string rawValue, string group = "", string extraPrefix = "", params ArgumentInfo[] args)
        {
            VerifyPartsArrayType(key, partType, GetType());

            // Get the part type and build the line
            string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum(key);
            if (key == CalendarPartsArrayEnum.IanaNames || key == CalendarPartsArrayEnum.NonstandardNames)
                prefix += extraPrefix.ToUpper();
            string line = CommonTools.BuildRawValue(prefix, rawValue, group, args);

            // Process the value
            VCalendarParser.Process(line, this, version);
        }

        internal void AddPartToArray(CalendarPartsArrayEnum key, BaseCalendarPartInfo value)
        {
            if (value is null)
                return;

            // Get the appropriate type and check it
            string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum(key);
            var partType = VCalendarParserTools.GetPartType(prefix, version, GetType());
            var enumType = partType.enumType;
            if (value.GetType() != enumType)
                return;

            // If we don't have this key yet, add it.
            if (!partsArray.ContainsKey(key))
            {
                LoggingTools.Debug("Adding part storage: {0}", key);
                partsArray.Add(key, []);
            }

            // We need to check the cardinality.
            var cardinality = partType.cardinality;
            bool onlyOne =
                cardinality == PartCardinality.ShouldBeOne ||
                cardinality == PartCardinality.MayBeOne;
            LoggingTools.Debug("Checking cardinality {0} [{1}]", cardinality, onlyOne);
            if (onlyOne && partsArray[key].Count > 0)
                throw new InvalidOperationException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_CANTADDCARDINALITY_PARTS").FormatString(key, cardinality));

            // Add this value info!
            LoggingTools.Debug("Adding value to storage: {0}", key);
            partsArray[key].Add(value);
        }

        internal void AddExtraPartToArray(PartsArrayEnum key, BasePartInfo value)
        {
            if (value is null)
                return;

            // Get the appropriate type and check it
            string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum((CalendarPartsArrayEnum)key);
            var partType = VCalendarParserTools.GetPartType(prefix, version, GetType());
            var enumType = partType.enumType;
            if (value.GetType() != enumType)
                return;

            // If we don't have this key yet, add it.
            if (!extraParts.ContainsKey(key))
            {
                LoggingTools.Debug("Adding part storage: {0}", key);
                extraParts.Add(key, []);
            }

            // We need to check the cardinality.
            var cardinality = partType.cardinality;
            bool onlyOne =
                cardinality == PartCardinality.ShouldBeOne ||
                cardinality == PartCardinality.MayBeOne;
            LoggingTools.Debug("Checking cardinality {0} [{1}]", cardinality, onlyOne);
            if (onlyOne && extraParts[key].Count > 0)
                throw new InvalidOperationException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_CANTADDCARDINALITY_PARTS").FormatString(key, cardinality));

            // Add this value info!
            LoggingTools.Debug("Adding value to storage: {0}", key);
            extraParts[key].Add(value);
        }

        /// <summary>
        /// Adds a string to the array
        /// </summary>
        /// <param name="key">String type</param>
        /// <param name="rawValue">Raw value representing a group of values delimited by the semicolon</param>
        /// <param name="group">Property group (can be nested with a dot)</param>
        /// <param name="args">Argument list to be added</param>
        public void AddString(CalendarStringsEnum key, string rawValue, string group = "", params ArgumentInfo[] args)
        {
            // Get the part type and build the line
            string prefix = VCalendarParserTools.GetPrefixFromStringsEnum(key);
            string line = CommonTools.BuildRawValue(prefix, rawValue, group, args);

            // Process the value
            VCalendarParser.Process(line, this, version);
        }

        internal void AddString(CalendarStringsEnum key, ValueInfo<string> value)
        {
            if (value is null || string.IsNullOrEmpty(value.Value))
                return;

            // Get the appropriate type
            string prefix = VCalendarParserTools.GetPrefixFromStringsEnum(key);
            var partType = VCalendarParserTools.GetPartType(prefix, version, GetType());

            // If we don't have this key yet, add it.
            if (!strings.ContainsKey(key))
            {
                LoggingTools.Debug("Adding string storage: {0}", key);
                strings.Add(key, []);
            }

            // We need to check the cardinality.
            var cardinality = partType.cardinality;
            bool onlyOne =
                cardinality == PartCardinality.ShouldBeOne ||
                cardinality == PartCardinality.MayBeOne;
            LoggingTools.Debug("Checking cardinality {0} [{1}]", cardinality, onlyOne);
            if (onlyOne && strings[key].Count > 0)
                throw new InvalidOperationException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_CANTADDCARDINALITY_STRING").FormatString(key, cardinality));

            // Add this value info!
            LoggingTools.Debug("Adding value to storage: {0}", key);
            strings[key].Add(value);
        }

        /// <summary>
        /// Adds an integer to the array
        /// </summary>
        /// <param name="key">Integer type</param>
        /// <param name="rawValue">Raw value representing a group of values delimited by the semicolon</param>
        /// <param name="group">Property group (can be nested with a dot)</param>
        /// <param name="args">Argument list to be added</param>
        public void AddInteger(CalendarIntegersEnum key, string rawValue, string group = "", params ArgumentInfo[] args)
        {
            // Get the part type and build the line
            string prefix = VCalendarParserTools.GetPrefixFromIntegersEnum(key);
            string line = CommonTools.BuildRawValue(prefix, rawValue, group, args);

            // Process the value
            VCalendarParser.Process(line, this, version);
        }

        internal void AddInteger(CalendarIntegersEnum key, ValueInfo<double> value)
        {
            if (value is null || value.Value < 0)
                return;

            // Get the appropriate type
            string prefix = VCalendarParserTools.GetPrefixFromIntegersEnum(key);
            var partType = VCalendarParserTools.GetPartType(prefix, version, GetType());

            // If we don't have this key yet, add it.
            if (!integers.ContainsKey(key))
            {
                LoggingTools.Debug("Adding integer storage: {0}", key);
                integers.Add(key, []);
            }

            // We need to check the cardinality.
            var cardinality = partType.cardinality;
            bool onlyOne =
                cardinality == PartCardinality.ShouldBeOne ||
                cardinality == PartCardinality.MayBeOne;
            LoggingTools.Debug("Checking cardinality {0} [{1}]", cardinality, onlyOne);
            if (onlyOne && integers[key].Count > 0)
                throw new InvalidOperationException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_CANTADDCARDINALITY_INTEGER").FormatString(key, cardinality));

            // Add this value info!
            LoggingTools.Debug("Adding value to storage: {0}", key);
            integers[key].Add(value);
        }

        /// <summary>
        /// Validates the calendar
        /// </summary>
        /// <exception cref="InvalidDataException"></exception>
        public void Validate()
        {
            // Track the required root fields
            string[] expectedFields =
                CalendarVersion.Major == 2 ? [VCalendarConstants._productIdSpecifier] : [];
            LoggingTools.Debug("Expected fields: {0} [{1}]", expectedFields.Length, string.Join(", ", expectedFields));
            if (!ValidateComponent(ref expectedFields, out string[] actualFields, this))
                throw new InvalidDataException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_NEEDSKEYS_ROOT").FormatString(string.Join(", ", expectedFields), string.Join(", ", actualFields)));

            // Now, track the individual components starting from events
            string[] expectedEventFields =
                CalendarVersion.Major == 2 ?
                [VCalendarConstants._uidSpecifier, VCalendarConstants._dateStampSpecifier] : [];
            string[] expectedTodoFields = expectedEventFields;
            expectedEventFields =
                CalendarVersion.Major == 2 && GetString(CalendarStringsEnum.Method).Length == 0 ?
                [VCalendarConstants._dateStartSpecifier, .. expectedEventFields] :
                expectedEventFields;
            LoggingTools.Debug("Expected event fields: {0} [{1}]", expectedEventFields.Length, string.Join(", ", expectedEventFields));
            LoggingTools.Debug("Expected todo fields: {0} [{1}]", expectedTodoFields.Length, string.Join(", ", expectedTodoFields));
            foreach (var eventInfo in events)
            {
                if (!ValidateComponent(ref expectedEventFields, out string[] actualEventFields, eventInfo))
                    throw new InvalidDataException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_NEEDSKEYS_EVENT").FormatString(string.Join(", ", expectedEventFields), string.Join(", ", actualEventFields)));
                foreach (var alarmInfo in eventInfo.Alarms)
                    alarmInfo.ValidateAlarm();

                // Check the priority
                var priorities = eventInfo.GetInteger(CalendarIntegersEnum.Priority);
                foreach (var priority in priorities)
                {
                    LoggingTools.Debug("Checking priority value: {0}", priority.Value);
                    if (priority.Value < 0 || priority.Value > 9)
                        throw new ArgumentOutOfRangeException(nameof(CalendarIntegersEnum.Priority), priority.Value, LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_PRIORITY"));
                }

                // Check for conflicts
                var dtends = eventInfo.GetPartsArray<DateEndInfo>();
                var durations = eventInfo.GetString(CalendarStringsEnum.Duration);
                LoggingTools.Debug("dtends: {0}, durations: {1}", dtends.Length, durations.Length);
                if (dtends.Length > 0 && durations.Length > 0)
                    throw new InvalidDataException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_DATEENDCONFLICT_EVENT"));
            }
            foreach (var todoInfo in Todos)
            {
                if (!ValidateComponent(ref expectedTodoFields, out string[] actualTodoFields, todoInfo))
                    throw new InvalidDataException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_NEEDSKEYS_TODO").FormatString(string.Join(", ", expectedTodoFields), string.Join(", ", actualTodoFields)));
                foreach (var alarmInfo in todoInfo.Alarms)
                    alarmInfo.ValidateAlarm();

                // Check the percentage
                var percentages = todoInfo.GetInteger(CalendarIntegersEnum.PercentComplete);
                foreach (var percentage in percentages)
                {
                    LoggingTools.Debug("Checking percent value: {0}", percentage.Value);
                    if (percentage.Value < 0 || percentage.Value > 100)
                        throw new ArgumentOutOfRangeException(nameof(CalendarIntegersEnum.PercentComplete), percentage.Value, LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_PERCENTCOMPLETION"));
                }

                // Check the priority
                var priorities = todoInfo.GetInteger(CalendarIntegersEnum.Priority);
                foreach (var priority in priorities)
                {
                    LoggingTools.Debug("Checking priority value: {0}", priority.Value);
                    if (priority.Value < 0 || priority.Value > 9)
                        throw new ArgumentOutOfRangeException(nameof(CalendarIntegersEnum.Priority), priority.Value, LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_PRIORITY"));
                }

                // Check for conflicts
                var dtstarts = todoInfo.GetPartsArray<DateStartInfo>();
                var dues = todoInfo.GetPartsArray<DueDateInfo>();
                var durations = todoInfo.GetString(CalendarStringsEnum.Duration);
                LoggingTools.Debug("dtstarts: {0}, dues: {1}, durations: {2}", dtstarts.Length, dues.Length, durations.Length);
                if (dues.Length > 0 && durations.Length > 0)
                    throw new InvalidDataException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_DUEDATECONFLICT_TODO"));
                if (durations.Length > 0 && dtstarts.Length == 0)
                    throw new InvalidDataException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_NODATESTARTDURATION_TODO"));
            }

            // Continue if we have a calendar with version 2.0
            if (CalendarVersion.Major < 2)
                return;
            string[] expectedJournalFields = expectedEventFields;
            string[] expectedFreeBusyFields = expectedEventFields;
            string[] expectedTimeZoneFields = [VCalendarConstants._tzidSpecifier];
            string[] expectedStandardFields = [VCalendarConstants._dateStartSpecifier, VCalendarConstants._tzOffsetFromSpecifier, VCalendarConstants._tzOffsetToSpecifier];
            string[] expectedDaylightFields = expectedStandardFields;
            LoggingTools.Debug("Expected journal fields: {0} [{1}]", expectedJournalFields.Length, string.Join(", ", expectedJournalFields));
            LoggingTools.Debug("Expected free/busy fields: {0} [{1}]", expectedFreeBusyFields.Length, string.Join(", ", expectedFreeBusyFields));
            LoggingTools.Debug("Expected timezone fields: {0} [{1}]", expectedTimeZoneFields.Length, string.Join(", ", expectedTimeZoneFields));
            LoggingTools.Debug("Expected standard fields: {0} [{1}]", expectedStandardFields.Length, string.Join(", ", expectedStandardFields));
            LoggingTools.Debug("Expected daylight fields: {0} [{1}]", expectedDaylightFields.Length, string.Join(", ", expectedDaylightFields));
            foreach (var journalInfo in Journals)
            {
                if (!ValidateComponent(ref expectedJournalFields, out string[] actualJournalFields, journalInfo))
                    throw new InvalidDataException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_NEEDSKEYS_JOURNAL").FormatString(string.Join(", ", expectedJournalFields), string.Join(", ", actualJournalFields)));
            }
            foreach (var freebusyInfo in FreeBusyList)
            {
                if (!ValidateComponent(ref expectedFreeBusyFields, out string[] actualFreeBusyFields, freebusyInfo))
                    throw new InvalidDataException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_NEEDSKEYS_FREEBUSY").FormatString(string.Join(", ", expectedFreeBusyFields), string.Join(", ", actualFreeBusyFields)));
            }
            foreach (var timezoneInfo in TimeZones)
            {
                if (!ValidateComponent(ref expectedTimeZoneFields, out string[] actualTimeZoneFields, timezoneInfo))
                    throw new InvalidDataException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_NEEDSKEYS_TIMEZONE").FormatString(string.Join(", ", expectedTimeZoneFields), string.Join(", ", actualTimeZoneFields)));

                // Check for standard and/or daylight
                LoggingTools.Debug("st: {0}, dl: {1}", timezoneInfo.StandardTimeList.Length, timezoneInfo.DaylightTimeList.Length);
                if (timezoneInfo.StandardTimeList.Length == 0 && timezoneInfo.DaylightTimeList.Length == 0)
                    throw new InvalidDataException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_NEEDSCOMPONENTS_TIMEZONE"));

                // Verify the standard and/or daylight components
                foreach (var standardInfo in timezoneInfo.StandardTimeList)
                {
                    if (!ValidateComponent(ref expectedStandardFields, out string[] actualStandardFields, standardInfo))
                        throw new InvalidDataException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_NEEDSKEYS_TIMEZONE_STANDARD").FormatString(string.Join(", ", expectedStandardFields), string.Join(", ", actualStandardFields)));
                }
                foreach (var daylightInfo in timezoneInfo.DaylightTimeList)
                {
                    if (!ValidateComponent(ref expectedDaylightFields, out string[] actualDaylightFields, daylightInfo))
                        throw new InvalidDataException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_NEEDSKEYS_TIMEZONE_DAYLIGHT").FormatString(string.Join(", ", expectedDaylightFields), string.Join(", ", actualDaylightFields)));
                }
            }
        }

        internal bool ValidateComponent<TComponent>(ref string[] expectedFields, out string[] actualFields, TComponent component)
            where TComponent : Parts.Calendar
        {
            // Track the required fields
            List<string> actualFieldList = [];

            // Requirement checks
            foreach (string expectedFieldName in expectedFields)
            {
                if (HasComponent(expectedFieldName, component))
                {
                    LoggingTools.Debug("Added {0} to actual field list", expectedFieldName);
                    actualFieldList.Add(expectedFieldName);
                }
            }
            Array.Sort(expectedFields);
            actualFieldList.Sort();
            actualFields = [.. actualFieldList];
            LoggingTools.Debug("Field count: {0}", actualFields.Length);
            return actualFields.SequenceEqual(expectedFields);
        }

        private bool HasComponent<TComponent>(string expectedFieldName, TComponent component)
            where TComponent : Parts.Calendar
        {
            // Requirement checks
            var partType = VCalendarParserTools.GetPartType(expectedFieldName, CalendarVersion, component.GetType());
            bool exists = false;
            switch (partType.type)
            {
                case PartType.Strings:
                    {
                        var values = component.GetString((CalendarStringsEnum)partType.enumeration);
                        exists = values.Length > 0;
                    }
                    break;
                case PartType.PartsArray:
                    {
                        if (partType.enumType is null)
                            return false;
                        var values = component.GetPartsArray((CalendarPartsArrayEnum)partType.enumeration);
                        exists = values.Length > 0;
                    }
                    break;
                case PartType.Integers:
                    {
                        var values = component.GetInteger((CalendarIntegersEnum)partType.enumeration);
                        exists = values.Length > 0;
                    }
                    break;
            }
            return exists;
        }

        internal void VerifyPartType(Type partType)
        {
            // Check the base type
            if (partType.BaseType != typeof(BaseCalendarPartInfo) && partType != typeof(BaseCalendarPartInfo))
                throw new InvalidOperationException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_INVALIDBASETYPE").FormatString(nameof(BaseCalendarPartInfo), partType.BaseType.Name, partType.Name));
        }

        internal void VerifyPartsArrayType(CalendarPartsArrayEnum key, Type partType, Type componentType)
        {
            // Check the base type
            if (key == CalendarPartsArrayEnum.IanaNames || key == CalendarPartsArrayEnum.NonstandardNames)
                CommonTools.VerifyBasePartType(partType);
            else
                VerifyPartType(partType);

            // Get the type
            string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum(key);
            var type = VCalendarParserTools.GetPartType(prefix, CalendarVersion, componentType);

            // Get the parts enumeration according to the type
            if (partType != typeof(BaseCalendarPartInfo))
            {
                // We don't need the base, but a derivative of it. Check it.
                var partsArrayEnum = (CalendarPartsArrayEnum)type.enumeration;
                LoggingTools.Debug("Comparing {0} and {1}", key, partsArrayEnum);
                if (key != partsArrayEnum)
                    throw new InvalidOperationException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_PARTSTYPEMISMATCH").FormatString(key, partsArrayEnum, partType.Name));
            }
        }

        /// <summary>
        /// Saves the calendar to the returned string
        /// </summary>
        public override string ToString() =>
            SaveToString();

        /// <summary>
        /// Saves the calendar to the returned string
        /// </summary>
        /// <param name="validate">Whether to validate all the fields or not</param>
        public virtual string ToString(bool validate) =>
            SaveToString(validate);

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((Calendar)obj);

        /// <summary>
        /// Checks to see if both the cards are equal
        /// </summary>
        /// <param name="other">The target <see cref="Calendar"/> instance to check to see if they equal</param>
        /// <returns>True if all the card elements are equal. Otherwise, false.</returns>
        public bool Equals(Calendar other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the cards are equal
        /// </summary>
        /// <param name="source">The source <see cref="Calendar"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="Calendar"/> instance to check to see if they equal</param>
        /// <returns>True if all the card elements are equal. Otherwise, false.</returns>
        public bool Equals(Calendar source, Calendar target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                CommonComparison.ExtraPartsEnumEqual(source.extraParts, target.extraParts) &&
                CalendarPartComparison.PartsArrayEnumEqual(source.partsArray, target.partsArray) &&
                CalendarPartComparison.StringsEqual(source.strings, target.strings) &&
                CalendarPartComparison.IntegersEqual(source.integers, target.integers) &&
                CalendarPartComparison.CompareCalendarComponents(source.events, target.events) &&
                CalendarPartComparison.CompareCalendarComponents(source.todos, target.todos) &&
                CalendarPartComparison.CompareCalendarComponents(source.journals, target.journals) &&
                CalendarPartComparison.CompareCalendarComponents(source.freeBusyList, target.freeBusyList) &&
                CalendarPartComparison.CompareCalendarComponents(source.timeZones, target.timeZones) &&
                CalendarPartComparison.CompareCalendarComponents(source.others, target.others)
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 797403623;
            hashCode = hashCode * -1521134295 + EqualityComparer<List<CalendarEvent>>.Default.GetHashCode(events);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<CalendarTodo>>.Default.GetHashCode(todos);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<CalendarJournal>>.Default.GetHashCode(journals);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<CalendarFreeBusy>>.Default.GetHashCode(freeBusyList);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<CalendarTimeZone>>.Default.GetHashCode(timeZones);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<CalendarOtherComponent>>.Default.GetHashCode(others);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<PartsArrayEnum, List<BasePartInfo>>>.Default.GetHashCode(extraParts);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>>>.Default.GetHashCode(partsArray);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CalendarStringsEnum, List<ValueInfo<string>>>>.Default.GetHashCode(strings);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CalendarIntegersEnum, List<ValueInfo<double>>>>.Default.GetHashCode(integers);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(Calendar a, Calendar b)
            => a.Equals(b);

        /// <inheritdoc/>
        public static bool operator !=(Calendar a, Calendar b)
            => !a.Equals(b);

        /// <summary>
        /// Makes an empty calendar
        /// </summary>
        /// <param name="version">vCalendar version to use</param>
        /// <exception cref="ArgumentException"></exception>
        public Calendar(Version version)
        {
            if (!VCalendarParserTools.VerifySupportedVersion(version))
                throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_CALENDAR_INVALIDVERSION").FormatString(version));
            this.version = version;
        }
    }
}
