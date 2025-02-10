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
using System.Linq;
using System.Text.RegularExpressions;
using VisualCard.Common.Parsers;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts;

namespace VisualCard.Calendar.Parts.Implementations
{
    /// <summary>
    /// Calendar excluded date info
    /// </summary>
    [DebuggerDisplay("{ExDates.Length} excluded dates")]
    public class ExDateInfo : BaseCalendarPartInfo, IEquatable<ExDateInfo>
    {
        /// <summary>
        /// The excluded date list
        /// </summary>
        public DateTimeOffset[]? ExDates { get; set; }

        internal static BaseCalendarPartInfo FromStringStatic(string value, PropertyInfo property, int altId, string[] elementTypes, string group, string valueType, Version cardVersion) =>
            (BaseCalendarPartInfo)new ExDateInfo().FromStringInternal(value, property, altId, elementTypes, group, valueType, cardVersion);

        internal override string ToStringInternal(Version cardVersion)
        {
            string type = ValueType ?? "";
            bool justDate = type.Equals("date", StringComparison.OrdinalIgnoreCase);
            return $"{string.Join(cardVersion.Major == 1 ? ";" : ",", ExDates.Select((dt) => CommonTools.SavePosixDate(dt, justDate)))}";
        }

        internal override BasePartInfo FromStringInternal(string value, PropertyInfo property, int altId, string[] elementTypes, string group, string valueType, Version cardVersion)
        {
            // Populate the fields
            var exDates = Regex.Unescape(value).Split(cardVersion.Major == 1 ? ';' : ',');
            List<DateTimeOffset> dates = [];
            foreach (var exDate in exDates)
            {
                DateTimeOffset date =
                    valueType.Equals("date", StringComparison.OrdinalIgnoreCase) ?
                    CommonTools.ParsePosixDate(exDate) :
                    CommonTools.ParsePosixDateTime(exDate);
                dates.Add(date);
            }

            // Add the fetched information
            ExDateInfo _time = new(property, elementTypes, group, valueType, [.. dates]);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((ExDateInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="ExDateInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(ExDateInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="ExDateInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="ExDateInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(ExDateInfo source, ExDateInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.ExDates == target.ExDates
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 1289504723;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<DateTimeOffset[]?>.Default.GetHashCode(ExDates);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(ExDateInfo left, ExDateInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(ExDateInfo left, ExDateInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BasePartInfo source, BasePartInfo target) =>
            (ExDateInfo)source == (ExDateInfo)target;

        internal ExDateInfo() { }

        internal ExDateInfo(PropertyInfo? property, string[] elementTypes, string group, string valueType, DateTimeOffset[] exDates) :
            base(property, elementTypes, group, valueType)
        {
            ExDates = exDates;
        }
    }
}
