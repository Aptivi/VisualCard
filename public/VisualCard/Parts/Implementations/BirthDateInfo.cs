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
using System.Diagnostics;
using VisualCard.Common.Parsers;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact birth date info
    /// </summary>
    [DebuggerDisplay("Birth date = {BirthDate}")]
    public class BirthDateInfo : BaseCardPartInfo, IEquatable<BirthDateInfo>
    {
        /// <summary>
        /// The contact's birth date
        /// </summary>
        public DateTimeOffset BirthDate { get; set; }

        internal static BaseCardPartInfo FromStringStatic(string value, PropertyInfo property, int altId, string[] elementTypes, string group, string valueType, Version cardVersion) =>
            (BaseCardPartInfo)new BirthDateInfo().FromStringInternal(value, property, altId, elementTypes, group, valueType, cardVersion);

        internal override string ToStringInternal(Version cardVersion) =>
            $"{CommonTools.SavePosixDate(BirthDate, true)}";

        internal override BasePartInfo FromStringInternal(string value, PropertyInfo property, int altId, string[] elementTypes, string group, string valueType, Version cardVersion)
        {
            // Populate field
            DateTimeOffset bday = CommonTools.ParsePosixDateTime(value);

            // Add the fetched information
            BirthDateInfo _time = new(altId, property, elementTypes, group, valueType, bday);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((BirthDateInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="BirthDateInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(BirthDateInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="BirthDateInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="BirthDateInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(BirthDateInfo source, BirthDateInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.BirthDate == target.BirthDate
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 653635456;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + BirthDate.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(BirthDateInfo left, BirthDateInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(BirthDateInfo left, BirthDateInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BasePartInfo source, BasePartInfo target) =>
            ((BirthDateInfo)source) == ((BirthDateInfo)target);

        internal BirthDateInfo() { }

        internal BirthDateInfo(int altId, PropertyInfo? property, string[] elementTypes, string group, string valueType, DateTimeOffset birth) :
            base(property, altId, elementTypes, group, valueType)
        {
            BirthDate = birth;
        }
    }
}
