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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using VisualCard.Calendar.Parsers;
using VisualCard.Calendar.Parts.Comparers;
using VisualCard.Calendar.Parts.Enums;
using VisualCard.Parts;

namespace VisualCard.Calendar.Parts
{
    /// <summary>
    /// A vCalendar card instance
    /// </summary>
    [DebuggerDisplay("vCalendar daylight time version {CalendarVersion.ToString()}, parts: (A [{partsArray.Count}] | S [{strings.Count}] | I [{integers.Count}])")]
    public class CalendarDaylight : Calendar, IEquatable<CalendarDaylight>
    {
        private readonly Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> partsArray = [];
        private readonly Dictionary<CalendarStringsEnum, List<ValueInfo<string>>> strings = [];
        private readonly Dictionary<CalendarIntegersEnum, List<ValueInfo<double>>> integers = [];

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
            var partType = VCalendarParserTools.GetPartType(prefix, CalendarVersion, typeof(CalendarDaylight));
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
        /// Saves this parsed card to the string
        /// </summary>
        public override string SaveToString() =>
            SaveToString(CalendarVersion, partsArray, strings, integers, VCalendarConstants._objectVDaylightSpecifier);

        /// <summary>
        /// Saves the contact to the returned string
        /// </summary>
        public override string ToString() =>
            SaveToString();

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((CalendarDaylight)obj);

        /// <summary>
        /// Checks to see if both the cards are equal
        /// </summary>
        /// <param name="other">The target <see cref="Calendar"/> instance to check to see if they equal</param>
        /// <returns>True if all the card elements are equal. Otherwise, false.</returns>
        public bool Equals(CalendarDaylight other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the cards are equal
        /// </summary>
        /// <param name="source">The source <see cref="Calendar"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="Calendar"/> instance to check to see if they equal</param>
        /// <returns>True if all the card elements are equal. Otherwise, false.</returns>
        public bool Equals(CalendarDaylight source, CalendarDaylight target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                PartComparison.PartsArrayEnumEqual(source.partsArray, target.partsArray) &&
                PartComparison.StringsEqual(source.strings, target.strings) &&
                PartComparison.IntegersEqual(source.integers, target.integers)
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 1333672311;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>>>.Default.GetHashCode(partsArray);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CalendarStringsEnum, List<ValueInfo<string>>>>.Default.GetHashCode(strings);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CalendarIntegersEnum, List<ValueInfo<double>>>>.Default.GetHashCode(integers);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(CalendarDaylight a, CalendarDaylight b)
            => a.Equals(b);

        /// <inheritdoc/>
        public static bool operator !=(CalendarDaylight a, CalendarDaylight b)
            => !a.Equals(b);

        internal override void AddPartToArray(CalendarPartsArrayEnum key, BaseCalendarPartInfo value) =>
            AddPartToArray(key, value, CalendarVersion, partsArray);

        internal override void AddString(CalendarStringsEnum key, ValueInfo<string> value) =>
            AddString(key, value, CalendarVersion, strings);

        internal override void AddInteger(CalendarIntegersEnum key, ValueInfo<double> value) =>
            AddInteger(key, value, CalendarVersion, integers);

        internal CalendarDaylight(Version version) :
            base(version)
        { }
    }
}
