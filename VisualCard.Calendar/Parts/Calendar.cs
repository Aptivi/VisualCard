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
    [DebuggerDisplay("vCalendar version {CalendarVersion.ToString()}, parts: (A [{partsArray.Count}] | S [{strings.Count} | I [{integers.Count}])")]
    public class Calendar : IEquatable<Calendar>
    {
        internal readonly List<CalendarEvent> events = [];
        internal readonly List<CalendarTodo> todos = [];
        internal readonly List<CalendarJournal> journals = [];
        internal readonly List<CalendarFreeBusy> freeBusyList = [];
        internal readonly List<CalendarTimeZone> timeZones = [];
        internal readonly List<CalendarOtherComponent> others = [];
        private readonly Version version;
        private readonly Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> partsArray = [];
        private readonly Dictionary<CalendarStringsEnum, List<CalendarValueInfo<string>>> strings = [];
        private readonly Dictionary<CalendarIntegersEnum, List<CalendarValueInfo<double>>> integers = [];

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
        /// Gets a part array from a specified key
        /// </summary>
        /// <returns>An array of values or an empty part array []</returns>
        public virtual TPart[] GetPartsArray<TPart>() where TPart : BaseCalendarPartInfo
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
        public virtual TPart[] GetPartsArray<TPart>(CalendarPartsArrayEnum key) where TPart : BaseCalendarPartInfo =>
            GetPartsArray<TPart>(key, version, partsArray);

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="partType">Target part type that derives from <see cref="BaseCalendarPartInfo"/></param>
        /// <returns>An array of values or an empty part array []</returns>
        public virtual BaseCalendarPartInfo[] GetPartsArray(Type partType)
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
        public virtual BaseCalendarPartInfo[] GetPartsArray(Type partType, CalendarPartsArrayEnum key)
        {
            // Check the base type
            if (partType.BaseType != typeof(BaseCalendarPartInfo) && partType != typeof(BaseCalendarPartInfo))
                throw new InvalidOperationException($"Base type is not BaseCalendarPartInfo [{partType.BaseType.Name}] and the part type is [{partType.Name}] that doesn't represent calendar part.");

            return GetPartsArray(partType, key, version, partsArray);
        }

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>An array of values or an empty part array []</returns>
        public virtual BaseCalendarPartInfo[] GetPartsArray(CalendarPartsArrayEnum key)
        {
            string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum(key);
            var (_, _, enumType, _, _, _, _, _, _) = VCalendarParserTools.GetPartType(prefix, "", CalendarVersion);
            if (enumType is null)
                throw new ArgumentException($"Enumeration type is not found for {key}");
            return GetPartsArray(enumType, key, version, partsArray);
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
        /// <returns>A value or null if any other type either doesn't exist or the type is not supported by the card version</returns>
        public virtual CalendarValueInfo<string>[] GetString(CalendarStringsEnum key) =>
            GetString(key, version, strings);

        internal CalendarValueInfo<string>[] GetString(CalendarStringsEnum key, Version version, Dictionary<CalendarStringsEnum, List<CalendarValueInfo<string>>> strings)
        {
            // Check for version support
            if (!VCalendarParserTools.StringSupported(key, version, GetType()))
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
        public virtual CalendarValueInfo<double>[] GetInteger(CalendarIntegersEnum key) =>
            GetInteger(key, version, integers);

        internal CalendarValueInfo<double>[] GetInteger(CalendarIntegersEnum key, Version version, Dictionary<CalendarIntegersEnum, List<CalendarValueInfo<double>>> integers)
        {
            // Check for version support
            if (!VCalendarParserTools.IntegerSupported(key, version, GetType()))
                return [];

            // Check to see if the integer has a value or not
            bool hasValue = integers.TryGetValue(key, out var values);
            if (!hasValue)
                return [];

            // Return the list
            return [.. values];
        }

        /// <summary>
        /// Saves this parsed card to the string
        /// </summary>
        public virtual string SaveToString() =>
            SaveToString(version, partsArray, strings, integers, VCalendarConstants._objectVCalendarSpecifier);

        internal string SaveToString(Version version, Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> partsArray, Dictionary<CalendarStringsEnum, List<CalendarValueInfo<string>>> strings, Dictionary<CalendarIntegersEnum, List<CalendarValueInfo<double>>> integers, string objectType)
        {
            // Initialize the card builder
            var cardBuilder = new StringBuilder();

            // First, write the header
            cardBuilder.AppendLine($"{VcardConstants._beginSpecifier}:{objectType}");
            if (objectType == VCalendarConstants._objectVCalendarSpecifier)
                cardBuilder.AppendLine($"{VcardConstants._versionSpecifier}:{version}");

            // Then, enumerate all the strings
            CalendarStringsEnum[] stringEnums = (CalendarStringsEnum[])Enum.GetValues(typeof(CalendarStringsEnum));
            foreach (CalendarStringsEnum stringEnum in stringEnums)
            {
                // Get the string values
                var array = GetString(stringEnum, version, strings);
                if (array is null || array.Length == 0)
                    continue;

                // Get the prefix
                string prefix = VCalendarParserTools.GetPrefixFromStringsEnum(stringEnum);
                var type = VCalendarParserTools.GetPartType(prefix, objectType, version);
                string defaultType = type.defaultType;
                string defaultValueType = type.defaultValueType;

                // Now, assemble the line
                foreach (var part in array)
                {
                    var partBuilder = new StringBuilder();
                    string partArguments = CalendarBuilderTools.BuildArguments(part, defaultType, defaultValueType);
                    string[] partArgumentsLines = partArguments.SplitNewLines();
                    string group = part.Group;
                    if (!string.IsNullOrEmpty(group))
                        cardBuilder.Append($"{group}.");
                    partBuilder.Append($"{prefix}");
                    partBuilder.Append($"{partArguments}");
                    partBuilder.Append($"{VcardCommonTools.MakeStringBlock(part.Value, partArgumentsLines[partArgumentsLines.Length - 1].Length + prefix.Length)}");
                    cardBuilder.AppendLine($"{partBuilder}");
                }
            }

            // Then, enumerate all the integers
            CalendarIntegersEnum[] integerEnums = (CalendarIntegersEnum[])Enum.GetValues(typeof(CalendarIntegersEnum));
            foreach (CalendarIntegersEnum integerEnum in integerEnums)
            {
                // Get the string value
                var array = GetInteger(integerEnum, version, integers);
                if (array is null || array.Length == 0)
                    continue;

                // Get the prefix
                string prefix = VCalendarParserTools.GetPrefixFromIntegersEnum(integerEnum);
                var type = VCalendarParserTools.GetPartType(prefix, objectType, version);
                string defaultType = type.defaultType;
                string defaultValueType = type.defaultValueType;

                // Now, assemble the line
                foreach (var part in array)
                {
                    var partBuilder = new StringBuilder();
                    string partArguments = CalendarBuilderTools.BuildArguments(part, defaultType, defaultValueType);
                    string[] partArgumentsLines = partArguments.SplitNewLines();
                    string group = part.Group;
                    if (!string.IsNullOrEmpty(group))
                        cardBuilder.Append($"{group}.");
                    partBuilder.Append($"{prefix}");
                    partBuilder.Append($"{partArguments}");
                    partBuilder.Append($"{VcardCommonTools.MakeStringBlock($"{part.Value}", partArgumentsLines[partArgumentsLines.Length - 1].Length + prefix.Length)}");
                    cardBuilder.AppendLine($"{partBuilder}");
                }
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
                var type = VCalendarParserTools.GetPartType(prefix, objectType, version);
                string defaultType = type.defaultType;
                string defaultValueType = type.defaultValueType;

                // Now, assemble the line
                foreach (var part in array)
                {
                    var partBuilder = new StringBuilder();
                    string partRepresentation = part.ToStringVcalendarInternal(version);
                    string partArguments = CalendarBuilderTools.BuildArguments(part, defaultType, defaultValueType);
                    string[] partArgumentsLines = partArguments.SplitNewLines();
                    string group = part.Group;
                    if (!string.IsNullOrEmpty(group))
                        cardBuilder.Append($"{group}.");
                    partBuilder.Append($"{prefix}");
                    partBuilder.Append($"{partArguments}");
                    partBuilder.Append($"{VcardCommonTools.MakeStringBlock(partRepresentation, partArgumentsLines[partArgumentsLines.Length - 1].Length + prefix.Length)}");
                    cardBuilder.AppendLine($"{partBuilder}");
                }
            }

            // Then, the components
            if (objectType == VCalendarConstants._objectVCalendarSpecifier)
            {
                foreach (var calendarEvent in events)
                    cardBuilder.Append(calendarEvent.SaveToString());
                foreach (var calendarTodo in todos)
                    cardBuilder.Append(calendarTodo.SaveToString());
                foreach (var calendarJournal in journals)
                    cardBuilder.Append(calendarJournal.SaveToString());
                foreach (var calendarFreeBusy in freeBusyList)
                    cardBuilder.Append(calendarFreeBusy.SaveToString());
                foreach (var calendarTimeZone in timeZones)
                    cardBuilder.Append(calendarTimeZone.SaveToString());
                foreach (var calendarOther in others)
                    cardBuilder.Append(calendarOther.SaveToString());
            }
            else if (objectType == VCalendarConstants._objectVEventSpecifier)
            {
                foreach (var calendarAlarm in ((CalendarEvent)this).alarms)
                    cardBuilder.Append(calendarAlarm.SaveToString());
            }
            else if (objectType == VCalendarConstants._objectVTodoSpecifier)
            {
                foreach (var calendarAlarm in ((CalendarTodo)this).alarms)
                    cardBuilder.Append(calendarAlarm.SaveToString());
            }
            else if (objectType == VCalendarConstants._objectVTimeZoneSpecifier)
            {
                foreach (var calendarStandard in ((CalendarTimeZone)this).standards)
                    cardBuilder.Append(calendarStandard.SaveToString());
                foreach (var calendarDaylight in ((CalendarTimeZone)this).daylights)
                    cardBuilder.Append(calendarDaylight.SaveToString());
            }

            // End the card and return it
            cardBuilder.AppendLine($"{VcardConstants._endSpecifier}:{objectType}");
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
                PartComparison.CompareCalendarComponents(source.timeZones, target.timeZones) &&
                PartComparison.CompareCalendarComponents(source.others, target.others)
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
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>>>.Default.GetHashCode(partsArray);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CalendarStringsEnum, List<CalendarValueInfo<string>>>>.Default.GetHashCode(strings);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CalendarIntegersEnum, List<CalendarValueInfo<double>>>>.Default.GetHashCode(integers);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(Calendar a, Calendar b)
            => a.Equals(b);

        /// <inheritdoc/>
        public static bool operator !=(Calendar a, Calendar b)
            => !a.Equals(b);

        internal virtual void AddPartToArray(CalendarPartsArrayEnum key, BaseCalendarPartInfo value) =>
            AddPartToArray(key, value, version, partsArray, VCalendarConstants._objectVCalendarSpecifier);

        internal virtual void AddPartToArray(CalendarPartsArrayEnum key, BaseCalendarPartInfo value, Version version, Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> partsArray, string objectType)
        {
            if (value is null)
                return;

            // Get the appropriate type and check it
            string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum(key);
            var enumType = VCalendarParserTools.GetPartType(prefix, objectType, version).enumType;
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

        internal virtual void AddString(CalendarStringsEnum key, CalendarValueInfo<string> value) =>
            AddString(key, value, strings);

        internal virtual void AddString(CalendarStringsEnum key, CalendarValueInfo<string> value, Dictionary<CalendarStringsEnum, List<CalendarValueInfo<string>>> strings)
        {
            if (value is null || string.IsNullOrEmpty(value.Value))
                return;

            // If we don't have this key yet, add it.
            if (!strings.ContainsKey(key))
                strings.Add(key, [value]);
            else
            {
                // We need to check the cardinality.
                var cardinality = VCalendarParserTools.GetStringsEnumFromType(key);
                bool onlyOne =
                    cardinality == PartCardinality.ShouldBeOne ||
                    cardinality == PartCardinality.MayBeOne;
                if (onlyOne)
                    throw new InvalidOperationException($"Can't add part array {key}, because cardinality is {cardinality}.");
                strings[key].Add(value);
            }
        }

        internal virtual void AddInteger(CalendarIntegersEnum key, CalendarValueInfo<double> value) =>
            AddInteger(key, value, integers);

        internal virtual void AddInteger(CalendarIntegersEnum key, CalendarValueInfo<double> value, Dictionary<CalendarIntegersEnum, List<CalendarValueInfo<double>>> integers)
        {
            if (value is null || value.Value < 0)
                return;

            // If we don't have this key yet, add it.
            if (!integers.ContainsKey(key))
                integers.Add(key, [value]);
            else
            {
                // We need to check the cardinality.
                var cardinality = VCalendarParserTools.GetIntegersEnumFromType(key);
                bool onlyOne =
                    cardinality == PartCardinality.ShouldBeOne ||
                    cardinality == PartCardinality.MayBeOne;
                if (onlyOne)
                    throw new InvalidOperationException($"Can't add part array {key}, because cardinality is {cardinality}.");
                integers[key].Add(value);
            }
        }

        internal Calendar(Version version) =>
            this.version = version;
    }
}
