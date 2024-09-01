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
using VisualCard.Parts.Enums;

namespace VisualCard.Calendar.Parts
{
    /// <summary>
    /// A vCalendar card instance
    /// </summary>
    [DebuggerDisplay("vCalendar version {CalendarVersion.ToString()}, parts: (A [{partsArray.Count}] | S [{strings.Count}])")]
    public class Calendar : IEquatable<Calendar>
    {
        private readonly Version version;
        private readonly Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> partsArray = [];
        private readonly Dictionary<CalendarStringsEnum, string> strings = [];

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
        public TPart[] GetPartsArray<TPart>(CalendarPartsArrayEnum key) where TPart : BaseCalendarPartInfo
        {
            // Check for version support
            if (!VCalendarParserTools.EnumArrayTypeSupported(key, CalendarVersion))
                return [];

            // Get the parts enumeration according to the type
            var type = typeof(TPart);
            if (type != typeof(BaseCalendarPartInfo))
            {
                // We don't need the base, but a derivative of it. Check it.
                var partsArrayEnum = VCalendarParserTools.GetPartsArrayEnumFromType(typeof(TPart), CalendarVersion).Item1;
                if (key != partsArrayEnum)
                    throw new InvalidOperationException($"Parts array enumeration [{key}] is different from the expected one [{partsArrayEnum}] according to type {typeof(TPart).Name}.");
            }

            // Get the fallback value
            TPart[] fallback = [];

            // Check to see if the partarray has a value or not
            bool hasValue = partsArray.ContainsKey(key);
            if (!hasValue)
                return fallback;

            // Cast the values
            var value = partsArray[key];
            TPart[] parts = value.Cast<TPart>().ToArray();

            // Now, return the value
            return parts;
        }

        /// <summary>
        /// Gets a string from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>A value, or "individual" if the kind doesn't exist, or an empty string ("") if any other type either doesn't exist or the type is not supported by the card version</returns>
        public string GetString(CalendarStringsEnum key)
        {
            // Check for version support
            if (!VCalendarParserTools.StringSupported(key, CalendarVersion))
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
        /// Saves this parsed card to the string
        /// </summary>
        public string SaveToString()
        {
            // Initialize the card builder
            var cardBuilder = new StringBuilder();
            var version = CalendarVersion;

            // First, write the header
            cardBuilder.AppendLine(VCalendarConstants._beginText);
            cardBuilder.AppendLine($"{VCalendarConstants._versionSpecifier}:{version}");

            // Then, enumerate all the strings
            CalendarStringsEnum[] stringEnums = (CalendarStringsEnum[])Enum.GetValues(typeof(CalendarStringsEnum));
            foreach (CalendarStringsEnum stringEnum in stringEnums)
            {
                // Get the string value
                string stringValue = GetString(stringEnum);
                if (string.IsNullOrEmpty(stringValue))
                    continue;

                // Now, locate the prefix and assemble the line
                string prefix = VCalendarParserTools.GetPrefixFromStringsEnum(stringEnum);
                cardBuilder.AppendLine($"{prefix}{VCalendarConstants._argumentDelimiter}{stringValue}");
            }

            // Then, enumerate all the arrays
            CalendarPartsArrayEnum[] partsArrayEnums = (CalendarPartsArrayEnum[])Enum.GetValues(typeof(CalendarPartsArrayEnum));
            foreach (CalendarPartsArrayEnum partsArrayEnum in partsArrayEnums)
            {
                // Get the array value
                var array = GetPartsArray<BaseCalendarPartInfo>(partsArrayEnum);
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
                    partBuilder.Append($"{VCalendarParserTools.MakeStringBlock(partRepresentation, partArgumentsLines[partArgumentsLines.Length - 1].Length + prefix.Length)}");
                    cardBuilder.AppendLine($"{partBuilder}");
                }
            }

            // End the card and return it
            cardBuilder.AppendLine(VCalendarConstants._endText);
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
                PartComparison.StringsEqual(source.strings, target.strings)
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 1047895655;
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>>>.Default.GetHashCode(partsArray);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CalendarStringsEnum, string>>.Default.GetHashCode(strings);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(Calendar a, Calendar b)
            => a.Equals(b);

        /// <inheritdoc/>
        public static bool operator !=(Calendar a, Calendar b)
            => !a.Equals(b);

        internal void AddPartToArray(CalendarPartsArrayEnum key, BaseCalendarPartInfo value)
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
                var cardinality = VCalendarParserTools.GetPartsArrayEnumFromType(enumType, CalendarVersion).Item2;
                bool onlyOne =
                    cardinality == PartCardinality.ShouldBeOne ||
                    cardinality == PartCardinality.MayBeOne;
                if (onlyOne)
                    throw new InvalidOperationException($"Can't add part array {key}, because cardinality is {cardinality}.");
                partsArray[key].Add(value);
            }
        }

        internal void SetString(CalendarStringsEnum key, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            // If we don't have this key yet, add it.
            if (!strings.ContainsKey(key))
                strings.Add(key, value);
            else
                throw new InvalidOperationException($"Can't overwrite string {key}.");
        }

        internal Calendar(Version version) =>
            this.version = version;
    }
}
