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
using System.Linq;
using VisualCard.Calendar.Parsers;
using VisualCard.Calendar.Parts.Comparers;
using VisualCard.Calendar.Parts.Enums;
using VisualCard.Common.Parts;
using VisualCard.Common.Parts.Enums;

namespace VisualCard.Calendar.Parts
{
    /// <summary>
    /// A vCalendar card instance
    /// </summary>
    [DebuggerDisplay("vCalendar other component version {CalendarVersion.ToString()}, parts: (A [{partsArray.Count}] | S [{strings.Count}] | I [{integers.Count} | E [{extraParts.Count}])")]
    public class CalendarOtherComponent : Calendar, IEquatable<CalendarOtherComponent>
    {
        internal readonly string componentName = "";
        private readonly Dictionary<PartsArrayEnum, List<BasePartInfo>> extraParts = [];
        private readonly Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> partsArray = [];
        private readonly Dictionary<CalendarStringsEnum, List<ValueInfo<string>>> strings = [];
        private readonly Dictionary<CalendarIntegersEnum, List<ValueInfo<double>>> integers = [];

        /// <summary>
        /// Extra part array list in a dictionary (for enumeration operations)
        /// </summary>
        public override ReadOnlyDictionary<PartsArrayEnum, ReadOnlyCollection<BasePartInfo>> ExtraParts =>
            new(extraParts.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.AsReadOnly()));

        /// <summary>
        /// Part array list in a dictionary (for enumeration operations)
        /// </summary>
        public override ReadOnlyDictionary<CalendarPartsArrayEnum, ReadOnlyCollection<BaseCalendarPartInfo>> PartsArray =>
            new(partsArray.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.AsReadOnly()));

        /// <summary>
        /// String list in a dictionary (for enumeration operations)
        /// </summary>
        public override ReadOnlyDictionary<CalendarStringsEnum, ReadOnlyCollection<ValueInfo<string>>> Strings =>
            new(strings.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.AsReadOnly()));

        /// <summary>
        /// Integer list in a dictionary (for enumeration operations)
        /// </summary>
        public override ReadOnlyDictionary<CalendarIntegersEnum, ReadOnlyCollection<ValueInfo<double>>> Integers =>
            new(integers.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.AsReadOnly()));

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <returns>An array of values or an empty part array []</returns>
        public override TPart[] GetExtraPartsArray<TPart>()
        {
            // Get the parts enumeration according to the type
            var key = VCalendarParserTools.GetPartsArrayEnumFromType(typeof(TPart), CalendarVersion, GetType());

            // Now, return the value
            return GetExtraPartsArray<TPart>((PartsArrayEnum)key, CalendarVersion, extraParts);
        }

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>An array of values or an empty part array []</returns>
        public override TPart[] GetExtraPartsArray<TPart>(PartsArrayEnum key) =>
            GetExtraPartsArray<TPart>(key, CalendarVersion, extraParts);

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>An array of values or an empty part array []</returns>
        public override BasePartInfo[] GetExtraPartsArray(PartsArrayEnum key)
        {
            string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum((CalendarPartsArrayEnum)key);
            var partType = VCalendarParserTools.GetPartType(prefix, CalendarVersion, typeof(CalendarAlarm));
            if (partType.enumType is null)
                throw new ArgumentException($"Enumeration type is not found for {key}");
            return GetExtraPartsArray(partType.enumType, key, CalendarVersion, extraParts);
        }

        /// <summary>
        /// Component name
        /// </summary>
        public string ComponentName =>
            componentName;

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <returns>An array of values or an empty part array []</returns>
        public override TPart[] GetPartsArray<TPart>()
        {
            // Get the parts enumeration according to the type
            var key = VCalendarParserTools.GetPartsArrayEnumFromType(typeof(TPart), CalendarVersion, GetType());

            // Now, return the value
            return GetPartsArray<TPart>(key, CalendarVersion, partsArray);
        }

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>An array of values or an empty part array []</returns>
        public override TPart[] GetPartsArray<TPart>(CalendarPartsArrayEnum key) =>
            GetPartsArray<TPart>(key, CalendarVersion, partsArray);

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>An array of values or an empty part array []</returns>
        public override BaseCalendarPartInfo[] GetPartsArray(CalendarPartsArrayEnum key)
        {
            string prefix = VCalendarParserTools.GetPrefixFromPartsArrayEnum(key);
            var partType = VCalendarParserTools.GetPartType(prefix, CalendarVersion, typeof(CalendarOtherComponent));
            if (partType.enumType is null)
                throw new ArgumentException($"Enumeration type is not found for {key}");
            return GetPartsArray(partType.enumType, key, CalendarVersion, partsArray);
        }

        /// <summary>
        /// Gets a string from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>A value or null if any other type either doesn't exist or the type is not supported by the card version</returns>
        public override ValueInfo<string>[] GetString(CalendarStringsEnum key) =>
            GetString(key, CalendarVersion, strings);

        /// <summary>
        /// Gets a integer from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>A value or null if any other type either doesn't exist or the type is not supported by the card version</returns>
        public override ValueInfo<double>[] GetInteger(CalendarIntegersEnum key) =>
            GetInteger(key, CalendarVersion, integers);

        /// <summary>
        /// Saves this parsed calendar component to the string
        /// </summary>
        /// <param name="validate">Whether to validate all the fields or not</param>
        public override string SaveToString(bool validate = false) =>
            SaveToString(CalendarVersion, extraParts, partsArray, strings, integers, componentName, validate);

        /// <summary>
        /// Saves the calendar component to the returned string
        /// </summary>
        public override string ToString() =>
            SaveToString();

        /// <summary>
        /// Saves the calendar component to the returned string
        /// </summary>
        /// <param name="validate">Whether to validate all the fields or not</param>
        public override string ToString(bool validate) =>
            SaveToString(validate);

        /// <summary>
        /// Deletes a string from the list of string values
        /// </summary>
        /// <param name="stringsEnum">String type</param>
        /// <param name="idx">Index of a string value</param>
        /// <returns>True if successful; false otherwise</returns>
        public override bool DeleteString(CalendarStringsEnum stringsEnum, int idx) =>
            DeleteString(stringsEnum, idx, strings);

        /// <summary>
        /// Deletes an integer from the list of integer values
        /// </summary>
        /// <param name="integersEnum">Integer type</param>
        /// <param name="idx">Index of a integer value</param>
        /// <returns>True if successful; false otherwise</returns>
        public override bool DeleteInteger(CalendarIntegersEnum integersEnum, int idx) =>
            DeleteInteger(integersEnum, idx, integers);

        /// <summary>
        /// Deletes a part from the list of parts
        /// </summary>
        /// <param name="partsArrayEnum">Part array type</param>
        /// <param name="idx">Index of a part</param>
        /// <returns>True if successful; false otherwise</returns>
        public override bool DeletePartsArray(CalendarPartsArrayEnum partsArrayEnum, int idx)
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
        public override bool DeletePartsArray(CalendarPartsArrayEnum partsArrayEnum, Type enumType, int idx)
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
        /// Deletes a part from the list of parts
        /// </summary>
        /// <param name="idx">Index of a part</param>
        /// <returns>True if successful; false otherwise</returns>
        public override bool DeletePartsArray<TPart>(int idx)
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
        public override bool DeletePartsArray<TPart>(CalendarPartsArrayEnum partsArrayEnum, int idx)
        {
            // Get the parts
            var parts = GetPartsArray(typeof(TPart), partsArrayEnum, CalendarVersion, partsArray);

            // Check the index
            if (idx >= parts.Length)
                return false;

            // Remove the part
            return DeletePartsArrayInternal(partsArrayEnum, idx, partsArray, extraParts);
        }

        /// <summary>
        /// Deletes an extra part from the list of parts
        /// </summary>
        /// <param name="idx">Index of an extra part</param>
        /// <returns>True if successful; false otherwise</returns>
        public override bool DeleteExtraPartsArray<TPart>(int idx)
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
        public override bool DeleteExtraPartsArray<TPart>(PartsArrayEnum partsArrayEnum, int idx)
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
        public override bool DeleteExtraPartsArray(PartsArrayEnum partsArrayEnum, int idx)
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
        public override bool DeleteExtraPartsArray(PartsArrayEnum partsArrayEnum, Type enumType, int idx)
        {
            // Get the string values
            var parts = GetExtraPartsArray(enumType, partsArrayEnum, CalendarVersion, extraParts);

            // Check the index
            if (idx >= parts.Length)
                return false;

            // Remove the string value
            return DeletePartsArrayInternal((CalendarPartsArrayEnum)partsArrayEnum, idx, partsArray, extraParts);
        }

        internal override void AddExtraPartToArray(PartsArrayEnum key, BasePartInfo value) =>
            AddExtraPartToArray(key, value, CalendarVersion, extraParts);

        internal override void AddPartToArray(CalendarPartsArrayEnum key, BaseCalendarPartInfo value) =>
            AddPartToArray(key, value, CalendarVersion, partsArray);

        internal override void AddString(CalendarStringsEnum key, ValueInfo<string> value) =>
            AddString(key, value, CalendarVersion, strings);

        internal override void AddInteger(CalendarIntegersEnum key, ValueInfo<double> value) =>
            AddInteger(key, value, CalendarVersion, integers);

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((CalendarOtherComponent)obj);

        /// <summary>
        /// Checks to see if both the cards are equal
        /// </summary>
        /// <param name="other">The target <see cref="Calendar"/> instance to check to see if they equal</param>
        /// <returns>True if all the card elements are equal. Otherwise, false.</returns>
        public bool Equals(CalendarOtherComponent other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the cards are equal
        /// </summary>
        /// <param name="source">The source <see cref="Calendar"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="Calendar"/> instance to check to see if they equal</param>
        /// <returns>True if all the card elements are equal. Otherwise, false.</returns>
        public bool Equals(CalendarOtherComponent source, CalendarOtherComponent target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                CalendarPartComparison.PartsArrayEnumEqual(source.partsArray, target.partsArray) &&
                CalendarPartComparison.StringsEqual(source.strings, target.strings) &&
                CalendarPartComparison.IntegersEqual(source.integers, target.integers)
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 2054266066;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(componentName);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>>>.Default.GetHashCode(partsArray);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CalendarStringsEnum, List<ValueInfo<string>>>>.Default.GetHashCode(strings);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CalendarIntegersEnum, List<ValueInfo<double>>>>.Default.GetHashCode(integers);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(CalendarOtherComponent a, CalendarOtherComponent b)
            => a.Equals(b);

        /// <inheritdoc/>
        public static bool operator !=(CalendarOtherComponent a, CalendarOtherComponent b)
            => !a.Equals(b);

        /// <summary>
        /// Makes an empty calendar for other components
        /// </summary>
        /// <param name="version">vCalendar version to use</param>
        /// <param name="componentName">Component name</param>
        /// <exception cref="ArgumentException"></exception>
        public CalendarOtherComponent(Version version, string componentName) :
            base(version)
        {
            if (version.Major != 2 && version.Minor != 0)
                throw new ArgumentException($"Invalid vCalendar version {version} specified. The supported version is 2.0.");
            this.componentName = componentName.ToUpper();
        }
    }
}
