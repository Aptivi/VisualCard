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
    /// Contact wedding anniversary info
    /// </summary>
    [DebuggerDisplay("Wedding anniversary = {Anniversary}")]
    public class AnniversaryInfo : BaseCardPartInfo, IEquatable<AnniversaryInfo>
    {
        /// <summary>
        /// The contact's wedding anniversary date (that is, the day that this contact is married)
        /// </summary>
        public DateTimeOffset Anniversary { get; set; }

        internal static BaseCardPartInfo FromStringStatic(string value, PropertyInfo property, int altId, string[] elementTypes, Version cardVersion) =>
            (BaseCardPartInfo)new AnniversaryInfo().FromStringInternal(value, property, altId, elementTypes, cardVersion);

        internal override string ToStringInternal(Version cardVersion) =>
            $"{CommonTools.SavePosixDate(Anniversary, true)}";

        internal override BasePartInfo FromStringInternal(string value, PropertyInfo property, int altId, string[] elementTypes, Version cardVersion)
        {
            // Populate the fields
            DateTimeOffset anniversary = CommonTools.ParsePosixDateTime(value);

            // Add the fetched information
            AnniversaryInfo _time = new(-1, property, [], anniversary);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((AnniversaryInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="AnniversaryInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(AnniversaryInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="AnniversaryInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="AnniversaryInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(AnniversaryInfo source, AnniversaryInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.Anniversary == target.Anniversary
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 382927327;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Anniversary.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(AnniversaryInfo left, AnniversaryInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(AnniversaryInfo left, AnniversaryInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BasePartInfo source, BasePartInfo target) =>
            ((AnniversaryInfo)source) == ((AnniversaryInfo)target);

        internal AnniversaryInfo() { }

        internal AnniversaryInfo(int altId, PropertyInfo? property, string[] elementTypes, DateTimeOffset anniversary) :
            base(property, altId, elementTypes)
        {
            Anniversary = anniversary;
        }
    }
}
