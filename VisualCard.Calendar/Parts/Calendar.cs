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
using Textify.General;
using VisualCard.Calendar.Parsers;
using VisualCard.Calendar.Parts.Comparers;
using VisualCard.Calendar.Parts.Enums;
using VisualCard.Parsers;
using VisualCard.Parts.Enums;

namespace VisualCard.Calendar.Parts
{
    /// <summary>
    /// A vCalendar card instance
    /// </summary>
    [DebuggerDisplay("vCalendar version {CalendarVersion.ToString()}, parts: (A [{partsArray.Count}] | S [{strings.Count}])")]
    public class Calendar : IEquatable<Calendar>
    {
        internal readonly List<CalendarEvent> events = [];
        internal readonly List<CalendarTodo> todos = [];
        internal readonly List<CalendarJournal> journals = [];
        internal readonly List<CalendarFreeBusy> freeBusyList = [];
        internal readonly List<CalendarTimeZone> timeZones = [];
        private readonly Version version;
        private readonly Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> partsArray = [];
        private readonly Dictionary<CalendarStringsEnum, string> strings = [];
        private readonly Dictionary<CalendarIntegersEnum, int> integers = [];

        /// <summary>
        /// The vCalendar version
        /// </summary>
        public Version CalendarVersion =>
            version;

        /// <summary>
        /// Unique ID for this card
        /// </summary>
        public string UniqueId =>
            GetString(CalendarStringsEnum.Uid);

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
        /// Gets a part array from a specified key
        /// </summary>
        /// <returns>An array of values or an empty part array []</returns>
        public TPart[] GetPartsArray<TPart>() where TPart : BaseCalendarPartInfo
        {
            // Get the parts enumeration according to the type
            var key = VCalendarParserTools.GetPartsArrayEnumFromType(typeof(TPart), CalendarVersion);

            // Now, return the value
            return GetPartsArray<TPart>(key.Item1);
        }

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>An array of values or an empty part array []</returns>
        public TPart[] GetPartsArray<TPart>(CalendarPartsArrayEnum key) where TPart : BaseCalendarPartInfo =>
            GetPartsArray<TPart>(key, version, partsArray);

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="partType">Target part type that derives from <see cref="BaseCalendarPartInfo"/></param>
        /// <returns>An array of values or an empty part array []</returns>
        public BaseCalendarPartInfo[] GetPartsArray(Type partType)
        {
            // Check the base type
            if (partType.BaseType != typeof(BaseCalendarPartInfo))
                throw new InvalidOperationException($"Base type is not BaseCalendarPartInfo [{partType.BaseType.Name}] and the part type is [{partType.Name}] that doesn't represent calendar part.");

            // Get the parts enumeration according to the type
            var key = VCalendarParserTools.GetPartsArrayEnumFromType(partType, CalendarVersion);

            // Now, return the value
            return GetPartsArray(partType, key.Item1);
        }

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="partType">Target part type that derives from <see cref="BaseCalendarPartInfo"/></param>
        /// <param name="key">A key to use</param>
        /// <returns>An array of values or an empty part array []</returns>
        public BaseCalendarPartInfo[] GetPartsArray(Type partType, CalendarPartsArrayEnum key)
        {
            // Check the base type
            if (partType.BaseType != typeof(BaseCalendarPartInfo) && partType != typeof(BaseCalendarPartInfo))
                throw new InvalidOperationException($"Base type is not BaseCalendarPartInfo [{partType.BaseType.Name}] and the part type is [{partType.Name}] that doesn't represent calendar part.");

            return GetPartsArray(partType, key, version, partsArray);
        }

        internal TPart[] GetPartsArray<TPart>(Version version, Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> partsArray)
            where TPart : BaseCalendarPartInfo
        {
            // Get the parts enumeration according to the type
            var key = VCalendarParserTools.GetPartsArrayEnumFromType(typeof(TPart), version);

            // Now, return the value
            return GetPartsArray<TPart>(key.Item1, version, partsArray);
        }

        internal TPart[] GetPartsArray<TPart>(CalendarPartsArrayEnum key, Version version, Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> partsArray)
            where TPart : BaseCalendarPartInfo =>
            GetPartsArray(typeof(TPart), key, version, partsArray).Cast<TPart>().ToArray();

        internal BaseCalendarPartInfo[] GetPartsArray(Type partType, Version version, Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> partsArray)
        {
            // Get the parts enumeration according to the type
            var key = VCalendarParserTools.GetPartsArrayEnumFromType(typeof(BaseCalendarPartInfo), version);

            // Now, return the value
            return GetPartsArray(partType, key.Item1, version, partsArray);
        }

        internal BaseCalendarPartInfo[] GetPartsArray(Type partType, CalendarPartsArrayEnum key, Version version, Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> partsArray)
        {
            // Check for version support
            if (!VCalendarParserTools.EnumArrayTypeSupported(key, version, GetType()))
                return [];

            // Get the parts enumeration according to the type
            if (partType != typeof(BaseCalendarPartInfo))
            {
                // We don't need the base, but a derivative of it. Check it.
                var partsArrayEnum = VCalendarParserTools.GetPartsArrayEnumFromType(partType, version).Item1;
                if (key != partsArrayEnum)
                    throw new InvalidOperationException($"Parts array enumeration [{key}] is different from the expected one [{partsArrayEnum}] according to type {partType.Name}.");
            }

            // Get the fallback value
            BaseCalendarPartInfo[] fallback = [];

            // Check to see if the partarray has a value or not
            bool hasValue = partsArray.ContainsKey(key);
            if (!hasValue)
                return fallback;

            // Cast the values
            var value = partsArray[key];
            BaseCalendarPartInfo[] parts = value.ToArray();

            // Now, return the value
            return parts;
        }

        /// <summary>
        /// Gets a string from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>A value, or "individual" if the kind doesn't exist, or an empty string ("") if any other type either doesn't exist or the type is not supported by the card version</returns>
        public string GetString(CalendarStringsEnum key) =>
            GetString(key, version, strings);

        internal string GetString(CalendarStringsEnum key, Version version, Dictionary<CalendarStringsEnum, string> strings)
        {
            // Check for version support
            if (!VCalendarParserTools.StringSupported(key, version, GetType()))
                return "";

            // Get the fallback value
            string fallback = "";

            // Check to see if the string has a value or not
            bool hasValue = strings.TryGetValue(key, out string value);
            if (!hasValue)
                return fallback;

            // Now, verify that the string is not empty
            hasValue = !string.IsNullOrEmpty(value);
            return hasValue ? value : fallback;
        }

        /// <summary>
        /// Gets a integer from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>A value or -1 if any other type either doesn't exist or the type is not supported by the card version</returns>
        public int GetInteger(CalendarIntegersEnum key) =>
            GetInteger(key, version, integers);

        internal int GetInteger(CalendarIntegersEnum key, Version version, Dictionary<CalendarIntegersEnum, int> integers)
        {
            // Check for version support
            if (!VCalendarParserTools.IntegerSupported(key, version, GetType()))
                return -1;

            // Get the fallback value
            int fallback = -1;

            // Check to see if the integer has a value or not
            bool hasValue = integers.TryGetValue(key, out int value);
            if (!hasValue)
                return fallback;
            return value;
        }

        /// <summary>
        /// Saves this parsed card to the string
        /// </summary>
        public string SaveToString() =>
            SaveToString(version, partsArray, strings, integers, VCalendarConstants._objectVCalendarSpecifier);

        internal string SaveToString(Version version, Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> partsArray, Dictionary<CalendarStringsEnum, string> strings, Dictionary<CalendarIntegersEnum, int> integers, string objectType)
        {
            // Initialize the card builder
            var cardBuilder = new StringBuilder();

            // First, write the header
            cardBuilder.AppendLine($"{VCalendarConstants._beginSpecifier}:{objectType}");
            if (objectType == VCalendarConstants._objectVCalendarSpecifier)
                cardBuilder.AppendLine($"{VCalendarConstants._versionSpecifier}:{version}");

            // Then, enumerate all the strings
            CalendarStringsEnum[] stringEnums = (CalendarStringsEnum[])Enum.GetValues(typeof(CalendarStringsEnum));
            foreach (CalendarStringsEnum stringEnum in stringEnums)
            {
                // Get the string value
                string stringValue = GetString(stringEnum, version, strings);
                if (string.IsNullOrEmpty(stringValue))
                    continue;

                // Now, locate the prefix and assemble the line
                string prefix = VCalendarParserTools.GetPrefixFromStringsEnum(stringEnum);
                cardBuilder.Append($"{prefix}{VCalendarConstants._argumentDelimiter}");
                cardBuilder.AppendLine($"{VcardParserTools.MakeStringBlock(stringValue, prefix.Length)}");
            }

            // Then, enumerate all the integers
            CalendarIntegersEnum[] integerEnums = (CalendarIntegersEnum[])Enum.GetValues(typeof(CalendarIntegersEnum));
            foreach (CalendarIntegersEnum integerEnum in integerEnums)
            {
                // Get the integer value
                int integerValue = GetInteger(integerEnum, version, integers);
                if (integerValue == -1)
                    continue;

                // Now, locate the prefix and assemble the line
                string prefix = VCalendarParserTools.GetPrefixFromIntegersEnum(integerEnum);
                cardBuilder.AppendLine($"{prefix}{VCalendarConstants._argumentDelimiter}{integerValue}");
            }

            // Then, enumerate all the arrays
            CalendarPartsArrayEnum[] partsArrayEnums = (CalendarPartsArrayEnum[])Enum.GetValues(typeof(CalendarPartsArrayEnum));
            foreach (CalendarPartsArrayEnum partsArrayEnum in partsArrayEnums)
            {
                // Get the array value
                var array = GetPartsArray<BaseCalendarPartInfo>(partsArrayEnum, version, partsArray);
                if (array is null || array.Length == 0)
                    continue;

                // Get the prefix
                string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum(partsArrayEnum);
                var type = VCalendarParserTools.GetPartType(prefix);
                string defaultType = type.defaultType;
                string defaultValue = type.defaultValue;

                // Now, assemble the line
                foreach (var part in array)
                {
                    var partBuilder = new StringBuilder();
                    string partRepresentation = part.ToStringVcalendarInternal(version);
                    string partArguments = CalendarBuilderTools.BuildArguments(part, defaultType, defaultValue);
                    string[] partArgumentsLines = partArguments.SplitNewLines();
                    partBuilder.Append($"{prefix}");
                    partBuilder.Append($"{partArguments}");
                    partBuilder.Append($"{VcardParserTools.MakeStringBlock(partRepresentation, partArgumentsLines[partArgumentsLines.Length - 1].Length + prefix.Length)}");
                    cardBuilder.AppendLine($"{partBuilder}");
                }
            }

            // Then, the components
            if (objectType == VCalendarConstants._objectVCalendarSpecifier)
            {
                foreach (var calendarEvent in events)
                    cardBuilder.Append(calendarEvent.SaveToString(version, calendarEvent.partsArray, calendarEvent.strings, calendarEvent.integers, VCalendarConstants._objectVEventSpecifier));
                foreach (var calendarTodo in todos)
                    cardBuilder.Append(calendarTodo.SaveToString(version, calendarTodo.partsArray, calendarTodo.strings, calendarTodo.integers, VCalendarConstants._objectVTodoSpecifier));
                foreach (var calendarJournal in journals)
                    cardBuilder.Append(calendarJournal.SaveToString(version, calendarJournal.partsArray, calendarJournal.strings, calendarJournal.integers, VCalendarConstants._objectVJournalSpecifier));
                foreach (var calendarFreeBusy in freeBusyList)
                    cardBuilder.Append(calendarFreeBusy.SaveToString(version, calendarFreeBusy.partsArray, calendarFreeBusy.strings, calendarFreeBusy.integers, VCalendarConstants._objectVFreeBusySpecifier));
                foreach (var calendarTimeZone in timeZones)
                    cardBuilder.Append(calendarTimeZone.SaveToString(version, calendarTimeZone.partsArray, calendarTimeZone.strings, calendarTimeZone.integers, VCalendarConstants._objectVTimeZoneSpecifier));
            }
            else if (objectType == VCalendarConstants._objectVEventSpecifier)
            {
                foreach (var calendarAlarm in ((CalendarEvent)this).alarms)
                    cardBuilder.Append(calendarAlarm.SaveToString(version, calendarAlarm.partsArray, calendarAlarm.strings, calendarAlarm.integers, VCalendarConstants._objectVAlarmSpecifier));
            }
            else if (objectType == VCalendarConstants._objectVTodoSpecifier)
            {
                foreach (var calendarAlarm in ((CalendarTodo)this).alarms)
                    cardBuilder.Append(calendarAlarm.SaveToString(version, calendarAlarm.partsArray, calendarAlarm.strings, calendarAlarm.integers, VCalendarConstants._objectVAlarmSpecifier));
            }
            else if (objectType == VCalendarConstants._objectVTimeZoneSpecifier)
            {
                foreach (var calendarStandard in ((CalendarTimeZone)this).standards)
                    cardBuilder.Append(calendarStandard.SaveToString(version, calendarStandard.partsArray, calendarStandard.strings, calendarStandard.integers, VCalendarConstants._objectVStandardSpecifier));
                foreach (var calendarDaylight in ((CalendarTimeZone)this).daylights)
                    cardBuilder.Append(calendarDaylight.SaveToString(version, calendarDaylight.partsArray, calendarDaylight.strings, calendarDaylight.integers, VCalendarConstants._objectVDaylightSpecifier));
            }

            // End the card and return it
            cardBuilder.AppendLine($"{VCalendarConstants._endSpecifier}:{objectType}");
            return cardBuilder.ToString();
        }

        /// <summary>
        /// Saves this parsed card to a file path
        /// </summary>
        /// <param name="path">File path to save this card to</param>
        public void SaveTo(string path)
        {
            // Save all the changes to the file
            var cardString = SaveToString();
            File.WriteAllText(path, cardString);
        }

        /// <summary>
        /// Saves the contact to the returned string
        /// </summary>
        public override string ToString() =>
            SaveToString();

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
                PartComparison.PartsArrayEnumEqual(source.partsArray, target.partsArray) &&
                PartComparison.StringsEqual(source.strings, target.strings) &&
                PartComparison.IntegersEqual(source.integers, target.integers) &&
                PartComparison.CompareCalendarComponents(source.events, target.events) &&
                PartComparison.CompareCalendarComponents(source.todos, target.todos) &&
                PartComparison.CompareCalendarComponents(source.journals, target.journals) &&
                PartComparison.CompareCalendarComponents(source.freeBusyList, target.freeBusyList) &&
                PartComparison.CompareCalendarComponents(source.timeZones, target.timeZones)
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -1266621595;
            hashCode = hashCode * -1521134295 + EqualityComparer<List<CalendarEvent>>.Default.GetHashCode(events);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<CalendarTodo>>.Default.GetHashCode(todos);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<CalendarJournal>>.Default.GetHashCode(journals);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<CalendarFreeBusy>>.Default.GetHashCode(freeBusyList);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<CalendarTimeZone>>.Default.GetHashCode(timeZones);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>>>.Default.GetHashCode(partsArray);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CalendarStringsEnum, string>>.Default.GetHashCode(strings);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CalendarIntegersEnum, int>>.Default.GetHashCode(integers);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(Calendar a, Calendar b)
            => a.Equals(b);

        /// <inheritdoc/>
        public static bool operator !=(Calendar a, Calendar b)
            => !a.Equals(b);

        internal void AddPartToArray(CalendarPartsArrayEnum key, BaseCalendarPartInfo value) =>
            AddPartToArray(key, value, version, partsArray);

        internal void AddPartToArray(CalendarPartsArrayEnum key, BaseCalendarPartInfo value, Version version, Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> partsArray)
        {
            if (value is null)
                return;

            // Get the appropriate type and check it
            string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum(key);
            var enumType = VCalendarParserTools.GetPartType(prefix).enumType;
            if (value.GetType() != enumType)
                return;

            // If we don't have this key yet, add it.
            if (!partsArray.ContainsKey(key))
                partsArray.Add(key, [value]);
            else
            {
                // We need to check the cardinality.
                var cardinality = VCalendarParserTools.GetPartsArrayEnumFromType(enumType, version).Item2;
                bool onlyOne =
                    cardinality == PartCardinality.ShouldBeOne ||
                    cardinality == PartCardinality.MayBeOne;
                if (onlyOne)
                    throw new InvalidOperationException($"Can't add part array {key}, because cardinality is {cardinality}.");
                partsArray[key].Add(value);
            }
        }

        internal void SetString(CalendarStringsEnum key, string value) =>
            SetString(key, value, strings);

        internal void SetString(CalendarStringsEnum key, string value, Dictionary<CalendarStringsEnum, string> strings)
        {
            if (string.IsNullOrEmpty(value))
                return;

            // If we don't have this key yet, add it.
            if (!strings.ContainsKey(key))
                strings.Add(key, value);
            else
                throw new InvalidOperationException($"Can't overwrite string {key}.");
        }

        internal void SetInteger(CalendarIntegersEnum key, int value) =>
            SetInteger(key, value, integers);

        internal void SetInteger(CalendarIntegersEnum key, int value, Dictionary<CalendarIntegersEnum, int> integers)
        {
            if (value == -1)
                return;

            // If we don't have this key yet, add it.
            if (!integers.ContainsKey(key))
                integers.Add(key, value);
            else
                throw new InvalidOperationException($"Can't overwrite integer {key}.");
        }

        internal Calendar(Version version) =>
            this.version = version;
    }
}
