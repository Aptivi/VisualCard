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
using VisualCard.Calendar.Parsers;
using VisualCard.Calendar.Parts.Comparers;
using VisualCard.Calendar.Parts.Enums;
using VisualCard.Calendar.Parts.Implementations.Event;
using VisualCard.Calendar.Parts.Implementations.Todo;
using VisualCard.Common.Parsers;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts;
using VisualCard.Common.Parts.Enums;
using VisualCard.Common.Parts.Implementations;
using VisualCard.Parsers;

namespace VisualCard.Calendar.Parts
{
    /// <summary>
    /// A vCalendar card instance
    /// </summary>
    [DebuggerDisplay("vCalendar version {CalendarVersion.ToString()}, parts: (A [{partsArray.Count}] | S [{strings.Count}] | I [{integers.Count} | E [{extraParts.Count}])")]
    public class Calendar : IEquatable<Calendar>
    {
        internal readonly List<CalendarEvent> events = [];
        internal readonly List<CalendarTodo> todos = [];
        internal readonly List<CalendarJournal> journals = [];
        internal readonly List<CalendarFreeBusy> freeBusyList = [];
        internal readonly List<CalendarTimeZone> timeZones = [];
        internal readonly List<CalendarOtherComponent> others = [];
        private readonly Version version;
        private readonly Dictionary<PartsArrayEnum, List<BasePartInfo>> extraParts = [];
        private readonly Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> partsArray = [];
        private readonly Dictionary<CalendarStringsEnum, List<ValueInfo<string>>> strings = [];
        private readonly Dictionary<CalendarIntegersEnum, List<ValueInfo<double>>> integers = [];

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
        public virtual ReadOnlyDictionary<PartsArrayEnum, ReadOnlyCollection<BasePartInfo>> ExtraParts =>
            new(extraParts.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.AsReadOnly()));

        /// <summary>
        /// Part array list in a dictionary (for enumeration operations)
        /// </summary>
        public virtual ReadOnlyDictionary<CalendarPartsArrayEnum, ReadOnlyCollection<BaseCalendarPartInfo>> PartsArray =>
            new(partsArray.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.AsReadOnly()));

        /// <summary>
        /// String list in a dictionary (for enumeration operations)
        /// </summary>
        public virtual ReadOnlyDictionary<CalendarStringsEnum, ReadOnlyCollection<ValueInfo<string>>> Strings =>
            new(strings.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.AsReadOnly()));

        /// <summary>
        /// Integer list in a dictionary (for enumeration operations)
        /// </summary>
        public virtual ReadOnlyDictionary<CalendarIntegersEnum, ReadOnlyCollection<ValueInfo<double>>> Integers =>
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
        public virtual TPart[] GetExtraPartsArray<TPart>() where TPart : BasePartInfo
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
        public virtual TPart[] GetExtraPartsArray<TPart>(PartsArrayEnum key) where TPart : BasePartInfo =>
            GetExtraPartsArray<TPart>(key, version, extraParts);

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="partType">Target part type that derives from <see cref="BasePartInfo"/></param>
        /// <returns>An array of values or an empty part array []</returns>
        public virtual BasePartInfo[] GetExtraPartsArray(Type partType)
        {
            // Check the base type
            VerifyBasePartType(partType);

            // Get the parts enumeration according to the type
            var key = VCalendarParserTools.GetPartsArrayEnumFromType(partType, CalendarVersion, GetType());

            // Now, return the value
            return GetExtraPartsArray(partType, (PartsArrayEnum)key);
        }

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="partType">Target part type that derives from <see cref="BasePartInfo"/></param>
        /// <param name="key">A key to use</param>
        /// <returns>An array of values or an empty part array []</returns>
        public virtual BasePartInfo[] GetExtraPartsArray(Type partType, PartsArrayEnum key)
        {
            // Check the base type
            VerifyBasePartType(partType);
            return GetExtraPartsArray(partType, key, version, extraParts);
        }

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>An array of values or an empty part array []</returns>
        public virtual BasePartInfo[] GetExtraPartsArray(PartsArrayEnum key)
        {
            string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum((CalendarPartsArrayEnum)key);
            var partType = VCalendarParserTools.GetPartType(prefix, CalendarVersion, typeof(Calendar));
            if (partType.enumType is null)
                throw new ArgumentException($"Enumeration type is not found for {key}");
            return GetExtraPartsArray(partType.enumType, key, CalendarVersion, extraParts);
        }

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <returns>An array of values or an empty part array []</returns>
        public virtual TPart[] GetPartsArray<TPart>() where TPart : BaseCalendarPartInfo
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
            VerifyPartType(partType);

            // Get the parts enumeration according to the type
            var key = VCalendarParserTools.GetPartsArrayEnumFromType(partType, CalendarVersion, GetType());

            // Now, return the value
            return GetPartsArray(partType, key);
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
            VerifyPartType(partType);
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
            var partType = VCalendarParserTools.GetPartType(prefix, CalendarVersion, typeof(Calendar));
            if (partType.enumType is null)
                throw new ArgumentException($"Enumeration type is not found for {key}");
            return GetPartsArray(partType.enumType, key, CalendarVersion, partsArray);
        }

        internal TPart[] GetExtraPartsArray<TPart>(Version version, Dictionary<PartsArrayEnum, List<BasePartInfo>> extraPartsArray)
            where TPart : BasePartInfo
        {
            // Get the parts enumeration according to the type
            var key = (PartsArrayEnum)VCalendarParserTools.GetPartsArrayEnumFromType(typeof(TPart), version, GetType());

            // Now, return the value
            return GetExtraPartsArray<TPart>(key, version, extraPartsArray);
        }

        internal TPart[] GetExtraPartsArray<TPart>(PartsArrayEnum key, Version version, Dictionary<PartsArrayEnum, List<BasePartInfo>> extraPartsArray)
            where TPart : BasePartInfo =>
            GetExtraPartsArray(typeof(TPart), key, version, extraPartsArray).Cast<TPart>().ToArray();

        internal BasePartInfo[] GetExtraPartsArray(Type partType, Version version, Dictionary<PartsArrayEnum, List<BasePartInfo>> extraPartsArray)
        {
            // Get the parts enumeration according to the type
            var key = (PartsArrayEnum)VCalendarParserTools.GetPartsArrayEnumFromType(typeof(BasePartInfo), version, GetType());

            // Now, return the value
            return GetExtraPartsArray(partType, key, version, extraPartsArray);
        }

        internal BasePartInfo[] GetExtraPartsArray(Type partType, PartsArrayEnum key, Version version, Dictionary<PartsArrayEnum, List<BasePartInfo>> extraPartsArray)
        {
            VerifyPartsArrayType((CalendarPartsArrayEnum)key, partType, GetType());

            // Check for version support
            string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum((CalendarPartsArrayEnum)key);
            var type = VCalendarParserTools.GetPartType(prefix, version, GetType());
            if (!type.minimumVersionCondition(version))
                return [];

            // Get the fallback value
            BasePartInfo[] fallback = [];

            // Check to see if the part array has a value or not
            bool hasValue = extraPartsArray.ContainsKey(key);
            if (!hasValue)
                return fallback;

            // Cast the values
            var value = extraPartsArray[key];
            BasePartInfo[] parts = [.. value];

            // Now, return the value
            return parts;
        }

        internal TPart[] GetPartsArray<TPart>(Version version, Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> partsArray)
            where TPart : BaseCalendarPartInfo
        {
            // Get the parts enumeration according to the type
            var key = VCalendarParserTools.GetPartsArrayEnumFromType(typeof(TPart), version, GetType());

            // Now, return the value
            return GetPartsArray<TPart>(key, version, partsArray);
        }

        internal TPart[] GetPartsArray<TPart>(CalendarPartsArrayEnum key, Version version, Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> partsArray)
            where TPart : BaseCalendarPartInfo =>
            GetPartsArray(typeof(TPart), key, version, partsArray).Cast<TPart>().ToArray();

        internal BaseCalendarPartInfo[] GetPartsArray(Type partType, Version version, Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> partsArray)
        {
            // Get the parts enumeration according to the type
            var key = VCalendarParserTools.GetPartsArrayEnumFromType(typeof(BaseCalendarPartInfo), version, GetType());

            // Now, return the value
            return GetPartsArray(partType, key, version, partsArray);
        }

        internal BaseCalendarPartInfo[] GetPartsArray(Type partType, CalendarPartsArrayEnum key, Version version, Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> partsArray)
        {
            VerifyPartsArrayType(key, partType, GetType());

            // Check for version support
            string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum(key);
            var type = VCalendarParserTools.GetPartType(prefix, version, GetType());
            if (!type.minimumVersionCondition(version))
                return [];

            // Get the fallback value
            BaseCalendarPartInfo[] fallback = [];

            // Check to see if the part array has a value or not
            bool hasValue = partsArray.ContainsKey(key);
            if (!hasValue)
                return fallback;

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
        public virtual ValueInfo<string>[] GetString(CalendarStringsEnum key) =>
            GetString(key, version, strings);

        internal ValueInfo<string>[] GetString(CalendarStringsEnum key, Version version, Dictionary<CalendarStringsEnum, List<ValueInfo<string>>> strings)
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
        public virtual ValueInfo<double>[] GetInteger(CalendarIntegersEnum key) =>
            GetInteger(key, version, integers);

        internal ValueInfo<double>[] GetInteger(CalendarIntegersEnum key, Version version, Dictionary<CalendarIntegersEnum, List<ValueInfo<double>>> integers)
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
        public virtual TPart[] FindExtraPartsArray<TPart>(string prefixToFind)
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
        public virtual TPart[] FindExtraPartsArray<TPart>(PartsArrayEnum key, string prefixToFind)
            where TPart : BasePartInfo =>
            FindExtraPartsArray(typeof(TPart), key, prefixToFind).Cast<TPart>().ToArray();

        /// <summary>
        /// Finds a part array from a specified key
        /// </summary>
        /// <param name="partType">Target part type that derives from <see cref="BasePartInfo"/></param>
        /// <param name="prefixToFind">Part of prefix to find (case-insensitive)</param>
        /// <returns>An array of values or an empty part array []</returns>
        public virtual BasePartInfo[] FindExtraPartsArray(Type partType, string prefixToFind)
        {
            // Check the base type
            VerifyBasePartType(partType);

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
        public virtual BasePartInfo[] FindExtraPartsArray(Type partType, PartsArrayEnum key, string prefixToFind) =>
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
            SaveToString(version, extraParts, partsArray, strings, integers, VCalendarConstants._objectVCalendarSpecifier, validate);

        internal string SaveToString(Version version, Dictionary<PartsArrayEnum, List<BasePartInfo>> extraParts, Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> partsArray, Dictionary<CalendarStringsEnum, List<ValueInfo<string>>> strings, Dictionary<CalendarIntegersEnum, List<ValueInfo<double>>> integers, string objectType, bool validate)
        {
            // Check to see if we need to validate
            if (validate)
                Validate();

            // Initialize the card builder
            var cardBuilder = new StringBuilder();

            // First, write the header
            cardBuilder.AppendLine($"{CommonConstants._beginSpecifier}:{objectType}");
            if (objectType == VCalendarConstants._objectVCalendarSpecifier)
                cardBuilder.AppendLine($"{CommonConstants._versionSpecifier}:{version}");

            // Then, enumerate all the strings
            foreach (CalendarStringsEnum stringEnum in strings.Keys)
            {
                // Get the string values
                var array = GetString(stringEnum, version, strings);
                if (array is null || array.Length == 0)
                    continue;

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
                        cardBuilder.Append($"{group}.");
                    partBuilder.Append($"{prefix}");
                    partBuilder.Append($"{partArguments}");
                    partBuilder.Append($"{CommonTools.MakeStringBlock(part.Value, partArgumentsLines[partArgumentsLines.Length - 1].Length + prefix.Length, encoding: part.Property?.Encoding ?? "")}");
                    cardBuilder.AppendLine($"{partBuilder}");
                }
            }

            // Then, enumerate all the integers
            foreach (CalendarIntegersEnum integerEnum in integers.Keys)
            {
                // Get the string value
                var array = GetInteger(integerEnum, version, integers);
                if (array is null || array.Length == 0)
                    continue;

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
                        cardBuilder.Append($"{group}.");
                    partBuilder.Append($"{prefix}");
                    partBuilder.Append($"{partArguments}");
                    partBuilder.Append($"{CommonTools.MakeStringBlock($"{part.Value}", partArgumentsLines[partArgumentsLines.Length - 1].Length + prefix.Length, encoding: part.Property?.Encoding ?? "")}");
                    cardBuilder.AppendLine($"{partBuilder}");
                }
            }

            // Then, enumerate all the arrays
            foreach (CalendarPartsArrayEnum partsArrayEnum in partsArray.Keys)
            {
                // Get the array value
                var array = GetPartsArray<BaseCalendarPartInfo>(partsArrayEnum, version, partsArray);
                if (array is null || array.Length == 0)
                    continue;

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
                        cardBuilder.Append($"{group}.");
                    partBuilder.Append($"{prefix}");
                    partBuilder.Append($"{partArguments}");
                    partBuilder.Append($"{CommonTools.MakeStringBlock(partRepresentation, partArgumentsLines[partArgumentsLines.Length - 1].Length + prefix.Length, encoding: part.Property?.Encoding ?? "")}");
                    cardBuilder.AppendLine($"{partBuilder}");
                }
            }

            // Then, enumerate all the extra arrays
            foreach (PartsArrayEnum extraPartEnum in extraParts.Keys)
            {
                // Get the array value
                var array = GetExtraPartsArray<BasePartInfo>(extraPartEnum, version, extraParts);
                if (array is null || array.Length == 0)
                    continue;

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
                        cardBuilder.Append($"{group}.");
                    partBuilder.Append($"{prefix}");
                    partBuilder.Append($"{partArguments}");
                    partBuilder.Append($"{CommonTools.MakeStringBlock(partRepresentation, partArgumentsLines[partArgumentsLines.Length - 1].Length + prefix.Length, encoding: part.Property?.Encoding ?? "")}");
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
            cardBuilder.AppendLine($"{CommonConstants._endSpecifier}:{objectType}");
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
        /// Deletes a string from the list of string values
        /// </summary>
        /// <param name="stringsEnum">String type</param>
        /// <param name="idx">Index of a string value</param>
        /// <returns>True if successful; false otherwise</returns>
        public virtual bool DeleteString(CalendarStringsEnum stringsEnum, int idx) =>
            DeleteString(stringsEnum, idx, strings);

        internal bool DeleteString(CalendarStringsEnum stringsEnum, int idx, Dictionary<CalendarStringsEnum, List<ValueInfo<string>>> strings)
        {
            // Get the string values
            var stringValues = GetString(stringsEnum, CalendarVersion, strings);

            // Check the index
            if (idx >= stringValues.Length)
                return false;

            // Remove the string value
            var stringValue = strings[stringsEnum][idx];
            bool result = strings[stringsEnum].Remove(stringValue);
            if (strings[stringsEnum].Count == 0)
                strings.Remove(stringsEnum);
            return result;
        }

        /// <summary>
        /// Deletes an integer from the list of integer values
        /// </summary>
        /// <param name="integersEnum">Integer type</param>
        /// <param name="idx">Index of a integer value</param>
        /// <returns>True if successful; false otherwise</returns>
        public virtual bool DeleteInteger(CalendarIntegersEnum integersEnum, int idx) =>
            DeleteInteger(integersEnum, idx, integers);

        internal bool DeleteInteger(CalendarIntegersEnum integersEnum, int idx, Dictionary<CalendarIntegersEnum, List<ValueInfo<double>>> integers)
        {
            // Get the integer values
            var integerValues = GetInteger(integersEnum, CalendarVersion, integers);

            // Check the index
            if (idx >= integerValues.Length)
                return false;

            // Remove the integer value
            var integerValue = integers[integersEnum][idx];
            bool result = integers[integersEnum].Remove(integerValue);
            if (integers[integersEnum].Count == 0)
                integers.Remove(integersEnum);
            return result;
        }

        /// <summary>
        /// Deletes a part from the list of parts
        /// </summary>
        /// <param name="partsArrayEnum">Part array type</param>
        /// <param name="idx">Index of a part</param>
        /// <returns>True if successful; false otherwise</returns>
        public virtual bool DeletePartsArray(CalendarPartsArrayEnum partsArrayEnum, int idx)
        {
            // Get the part type
            string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum(partsArrayEnum);
            var type = VCalendarParserTools.GetPartType(prefix, CalendarVersion, GetType());
            var partType = type.enumType ??
                throw new ArgumentException("Can't determine enumeration type to delete part.");

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
        public virtual bool DeletePartsArray(CalendarPartsArrayEnum partsArrayEnum, Type enumType, int idx)
        {
            // Get the string values
            var parts = GetPartsArray(enumType, partsArrayEnum, CalendarVersion, partsArray);

            // Check the index
            if (idx >= parts.Length)
                return false;

            // Remove the string value
            return DeletePartsArrayInternal(partsArrayEnum, idx, partsArray, extraParts);
        }

        /// <summary>
        /// Deletes an extra part from the list of parts
        /// </summary>
        /// <param name="idx">Index of an extra part</param>
        /// <returns>True if successful; false otherwise</returns>
        public virtual bool DeleteExtraPartsArray<TPart>(int idx)
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
        public virtual bool DeleteExtraPartsArray<TPart>(PartsArrayEnum partsArrayEnum, int idx)
            where TPart : BasePartInfo
        {
            // Get the parts
            var parts = GetExtraPartsArray(typeof(TPart), partsArrayEnum, CalendarVersion, extraParts);

            // Check the index
            if (idx >= parts.Length)
                return false;

            // Remove the part
            return DeletePartsArrayInternal((CalendarPartsArrayEnum)partsArrayEnum, idx, partsArray, extraParts);
        }

        /// <summary>
        /// Deletes an extra part from the list of parts
        /// </summary>
        /// <param name="partsArrayEnum">Part array type</param>
        /// <param name="idx">Index of an extra part</param>
        /// <returns>True if successful; false otherwise</returns>
        public virtual bool DeleteExtraPartsArray(PartsArrayEnum partsArrayEnum, int idx)
        {
            // Get the part type
            string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum((CalendarPartsArrayEnum)partsArrayEnum);
            var type = VCalendarParserTools.GetPartType(prefix, CalendarVersion, GetType());
            var partType = type.enumType ??
                throw new ArgumentException("Can't determine enumeration type to delete part.");

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
        public virtual bool DeleteExtraPartsArray(PartsArrayEnum partsArrayEnum, Type enumType, int idx)
        {
            // Get the string values
            var parts = GetExtraPartsArray(enumType, partsArrayEnum, CalendarVersion, extraParts);

            // Check the index
            if (idx >= parts.Length)
                return false;

            // Remove the string value
            return DeletePartsArrayInternal((CalendarPartsArrayEnum)partsArrayEnum, idx, partsArray, extraParts);
        }

        /// <summary>
        /// Deletes a part from the list of parts
        /// </summary>
        /// <param name="idx">Index of a part</param>
        /// <returns>True if successful; false otherwise</returns>
        public virtual bool DeletePartsArray<TPart>(int idx)
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
        public virtual bool DeletePartsArray<TPart>(CalendarPartsArrayEnum partsArrayEnum, int idx)
            where TPart : BaseCalendarPartInfo
        {
            // Get the parts
            var parts = GetPartsArray(typeof(TPart), partsArrayEnum, CalendarVersion, partsArray);

            // Check the index
            if (idx >= parts.Length)
                return false;

            // Remove the part
            return DeletePartsArrayInternal(partsArrayEnum, idx, partsArray, extraParts);
        }

        internal bool DeletePartsArrayInternal(CalendarPartsArrayEnum partsArrayEnum, int idx, Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> partsArray, Dictionary<PartsArrayEnum, List<BasePartInfo>> extraParts)
        {
            if (partsArrayEnum is CalendarPartsArrayEnum.IanaNames or CalendarPartsArrayEnum.NonstandardNames)
            {
                // Remove the extra part
                var extraPartEnum = (PartsArrayEnum)partsArrayEnum;
                var part = extraParts[extraPartEnum][idx];
                bool result = extraParts[extraPartEnum].Remove(part);
                if (extraParts[extraPartEnum].Count == 0)
                    extraParts.Remove(extraPartEnum);
                return result;
            }
            else
            {
                // Remove the part
                var part = partsArray[partsArrayEnum][idx];
                bool result = partsArray[partsArrayEnum].Remove(part);
                if (partsArray[partsArrayEnum].Count == 0)
                    partsArray.Remove(partsArrayEnum);
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
                throw new ArgumentException("Can't determine enumeration type to add part.");

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

        internal virtual void AddPartToArray(CalendarPartsArrayEnum key, BaseCalendarPartInfo value) =>
            AddPartToArray(key, value, version, partsArray);

        internal virtual void AddPartToArray(CalendarPartsArrayEnum key, BaseCalendarPartInfo value, Version version, Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> partsArray)
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
                partsArray.Add(key, [value]);
            else
            {
                // We need to check the cardinality.
                var cardinality = partType.cardinality;
                bool onlyOne =
                    cardinality == PartCardinality.ShouldBeOne ||
                    cardinality == PartCardinality.MayBeOne;
                if (onlyOne)
                    throw new InvalidOperationException($"Can't add part array {key}, because cardinality is {cardinality}.");
                partsArray[key].Add(value);
            }
        }

        internal virtual void AddExtraPartToArray(PartsArrayEnum key, BasePartInfo value) =>
            AddExtraPartToArray(key, value, version, extraParts);

        internal virtual void AddExtraPartToArray(PartsArrayEnum key, BasePartInfo value, Version version, Dictionary<PartsArrayEnum, List<BasePartInfo>> partsArray)
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
            if (!partsArray.ContainsKey(key))
                partsArray.Add(key, [value]);
            else
            {
                // We need to check the cardinality.
                var cardinality = partType.cardinality;
                bool onlyOne =
                    cardinality == PartCardinality.ShouldBeOne ||
                    cardinality == PartCardinality.MayBeOne;
                if (onlyOne)
                    throw new InvalidOperationException($"Can't add part array {key}, because cardinality is {cardinality}.");
                partsArray[key].Add(value);
            }
        }

        /// <summary>
        /// Adds a string to the array
        /// </summary>
        /// <param name="key">String type</param>
        /// <param name="rawValue">Raw value representing a group of values delimited by the semicolon</param>
        /// <param name="group">Property group (can be nested with a dot)</param>
        /// <param name="args">Argument list to be added</param>
        public virtual void AddString(CalendarStringsEnum key, string rawValue, string group = "", params ArgumentInfo[] args)
        {
            // Get the part type and build the line
            string prefix = VCalendarParserTools.GetPrefixFromStringsEnum(key);
            string line = CommonTools.BuildRawValue(prefix, rawValue, group, args);

            // Process the value
            VCalendarParser.Process(line, this, version);
        }

        internal virtual void AddString(CalendarStringsEnum key, ValueInfo<string> value) =>
            AddString(key, value, version, strings);

        internal virtual void AddString(CalendarStringsEnum key, ValueInfo<string> value, Version version, Dictionary<CalendarStringsEnum, List<ValueInfo<string>>> strings)
        {
            if (value is null || string.IsNullOrEmpty(value.Value))
                return;

            // Get the appropriate type
            string prefix = VCalendarParserTools.GetPrefixFromStringsEnum(key);
            var partType = VCalendarParserTools.GetPartType(prefix, version, GetType());

            // If we don't have this key yet, add it.
            if (!strings.ContainsKey(key))
                strings.Add(key, [value]);
            else
            {
                // We need to check the cardinality.
                var cardinality = partType.cardinality;
                bool onlyOne =
                    cardinality == PartCardinality.ShouldBeOne ||
                    cardinality == PartCardinality.MayBeOne;
                if (onlyOne)
                    throw new InvalidOperationException($"Can't add string {key}, because cardinality is {cardinality}.");
                strings[key].Add(value);
            }
        }

        /// <summary>
        /// Adds an integer to the array
        /// </summary>
        /// <param name="key">Integer type</param>
        /// <param name="rawValue">Raw value representing a group of values delimited by the semicolon</param>
        /// <param name="group">Property group (can be nested with a dot)</param>
        /// <param name="args">Argument list to be added</param>
        public virtual void AddInteger(CalendarIntegersEnum key, string rawValue, string group = "", params ArgumentInfo[] args)
        {
            // Get the part type and build the line
            string prefix = VCalendarParserTools.GetPrefixFromIntegersEnum(key);
            string line = CommonTools.BuildRawValue(prefix, rawValue, group, args);

            // Process the value
            VCalendarParser.Process(line, this, version);
        }

        internal virtual void AddInteger(CalendarIntegersEnum key, ValueInfo<double> value) =>
            AddInteger(key, value, version, integers);

        internal virtual void AddInteger(CalendarIntegersEnum key, ValueInfo<double> value, Version version, Dictionary<CalendarIntegersEnum, List<ValueInfo<double>>> integers)
        {
            if (value is null || value.Value < 0)
                return;

            // Get the appropriate type
            string prefix = VCalendarParserTools.GetPrefixFromIntegersEnum(key);
            var partType = VCalendarParserTools.GetPartType(prefix, version, GetType());

            // If we don't have this key yet, add it.
            if (!integers.ContainsKey(key))
                integers.Add(key, [value]);
            else
            {
                // We need to check the cardinality.
                var cardinality = partType.cardinality;
                bool onlyOne =
                    cardinality == PartCardinality.ShouldBeOne ||
                    cardinality == PartCardinality.MayBeOne;
                if (onlyOne)
                    throw new InvalidOperationException($"Can't add integer {key}, because cardinality is {cardinality}.");
                integers[key].Add(value);
            }
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
            if (!ValidateComponent(ref expectedFields, out string[] actualFields, this))
                throw new InvalidDataException($"The following keys [{string.Join(", ", expectedFields)}] are required in the root  Got [{string.Join(", ", actualFields)}].");

            // Now, track the individual components starting from events
            string[] expectedEventFields =
                CalendarVersion.Major == 2 ?
                [VCalendarConstants._uidSpecifier, VCalendarConstants._dateStampSpecifier] : [];
            string[] expectedTodoFields = expectedEventFields;
            expectedEventFields =
                CalendarVersion.Major == 2 && GetString(CalendarStringsEnum.Method).Length == 0 ?
                [VCalendarConstants._dateStartSpecifier, .. expectedEventFields] :
                expectedEventFields;
            foreach (var eventInfo in events)
            {
                if (!ValidateComponent(ref expectedEventFields, out string[] actualEventFields, eventInfo))
                    throw new InvalidDataException($"The following keys [{string.Join(", ", expectedEventFields)}] are required in the event representation. Got [{string.Join(", ", actualEventFields)}].");
                foreach (var alarmInfo in eventInfo.Alarms)
                    ValidateAlarm(alarmInfo);

                // Check the priority
                var priorities = eventInfo.GetInteger(CalendarIntegersEnum.Priority);
                foreach (var priority in priorities)
                {
                    if (priority.Value < 0 || priority.Value > 9)
                        throw new ArgumentOutOfRangeException(nameof(CalendarIntegersEnum.Priority), priority.Value, "Percent completion may not be less than zero or greater than 100");
                }

                // Check for conflicts
                var dtends = eventInfo.GetPartsArray<DateEndInfo>();
                var durations = eventInfo.GetString(CalendarStringsEnum.Duration);
                if (dtends.Length > 0 && durations.Length > 0)
                    throw new InvalidDataException("Date end and duration conflict found.");
            }
            foreach (var todoInfo in Todos)
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
                var priorities = todoInfo.GetInteger(CalendarIntegersEnum.Priority);
                foreach (var priority in priorities)
                {
                    if (priority.Value < 0 || priority.Value > 9)
                        throw new ArgumentOutOfRangeException(nameof(CalendarIntegersEnum.Priority), priority.Value, "Percent completion may not be less than zero or greater than 100");
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
            if (CalendarVersion.Major < 2)
                return;
            string[] expectedJournalFields = expectedEventFields;
            string[] expectedFreeBusyFields = expectedEventFields;
            string[] expectedTimeZoneFields = [VCalendarConstants._tzidSpecifier];
            string[] expectedStandardFields = [VCalendarConstants._dateStartSpecifier, VCalendarConstants._tzOffsetFromSpecifier, VCalendarConstants._tzOffsetToSpecifier];
            string[] expectedDaylightFields = expectedStandardFields;
            foreach (var journalInfo in Journals)
            {
                if (!ValidateComponent(ref expectedJournalFields, out string[] actualJournalFields, journalInfo))
                    throw new InvalidDataException($"The following keys [{string.Join(", ", expectedJournalFields)}] are required in the journal representation. Got [{string.Join(", ", actualJournalFields)}].");
            }
            foreach (var freebusyInfo in FreeBusyList)
            {
                if (!ValidateComponent(ref expectedFreeBusyFields, out string[] actualFreeBusyFields, freebusyInfo))
                    throw new InvalidDataException($"The following keys [{string.Join(", ", expectedFreeBusyFields)}] are required in the freebusy representation. Got [{string.Join(", ", actualFreeBusyFields)}].");
            }
            foreach (var timezoneInfo in TimeZones)
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

        internal void VerifyBasePartType(Type partType)
        {
            // Check the base type
            if (partType.BaseType != typeof(BasePartInfo) && partType != typeof(BasePartInfo))
                throw new InvalidOperationException($"Base type is not BasePartInfo [{partType.BaseType.Name}] and the part type is [{partType.Name}] that doesn't represent the part.");
        }

        internal void VerifyPartType(Type partType)
        {
            // Check the base type
            if (partType.BaseType != typeof(BaseCalendarPartInfo) && partType != typeof(BaseCalendarPartInfo))
                throw new InvalidOperationException($"Base type is not BaseCalendarPartInfo [{partType.BaseType.Name}] and the part type is [{partType.Name}] that doesn't represent calendar part.");
        }

        internal void VerifyPartsArrayType(CalendarPartsArrayEnum key, Type partType, Type componentType)
        {
            // Check the base type
            if (key == CalendarPartsArrayEnum.IanaNames || key == CalendarPartsArrayEnum.NonstandardNames)
                VerifyBasePartType(partType);
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
                if (key != partsArrayEnum)
                    throw new InvalidOperationException($"Parts array enumeration [{key}] is different from the expected one [{partsArrayEnum}] according to type {partType.Name}.");
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
                throw new ArgumentException($"Invalid vCalendar version {version} specified. The supported versions are 1.0 and 2.0.");
            this.version = version;
        }
    }
}
