﻿//
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
using System.Diagnostics;
using VisualCard.Parsers;
using VisualCard.Parsers.Arguments;

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
        public DateTimeOffset Anniversary { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, PropertyInfo property, int altId, string[] elementTypes, string valueType, Version cardVersion) =>
            new AnniversaryInfo().FromStringVcardInternal(value, property, altId, elementTypes, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion) =>
            $"{VcardCommonTools.SavePosixDate(Anniversary, true)}";

        internal override BaseCardPartInfo FromStringVcardInternal(string value, PropertyInfo property, int altId, string[] elementTypes, string valueType, Version cardVersion)
        {
            // Populate the fields
            DateTimeOffset anniversary = VcardCommonTools.ParsePosixDateTime(value);

            // Add the fetched information
            AnniversaryInfo _time = new(-1, property, [], valueType, anniversary);
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

        internal override bool EqualsInternal(BaseCardPartInfo source, BaseCardPartInfo target) =>
            ((AnniversaryInfo)source) == ((AnniversaryInfo)target);

        internal AnniversaryInfo() { }

        internal AnniversaryInfo(int altId, PropertyInfo? property, string[] elementTypes, string valueType, DateTimeOffset anniversary) :
            base(property, altId, elementTypes, valueType)
        {
            Anniversary = anniversary;
        }
    }
}
