﻿//
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

namespace VisualCard.Calendar.Parts.Implementations
{
    /// <summary>
    /// Last modified info
    /// </summary>
    [DebuggerDisplay("Last Modified = {LastModified}")]
    public class LastModifiedInfo : BaseCalendarPartInfo, IEquatable<LastModifiedInfo>
    {
        /// <summary>
        /// The calendar's revision
        /// </summary>
        public DateTimeOffset LastModified { get; set; }

        internal static BaseCalendarPartInfo FromStringStatic(string value, PropertyInfo property, int altId, string[] elementTypes, Version cardVersion) =>
            (BaseCalendarPartInfo)new LastModifiedInfo().FromStringInternal(value, property, altId, elementTypes, cardVersion);

        internal override string ToStringInternal(Version cardVersion) =>
            $"{CommonTools.SavePosixDate(LastModified)}";

        internal override BasePartInfo FromStringInternal(string value, PropertyInfo property, int altId, string[] elementTypes, Version cardVersion)
        {
            // Populate the fields
            DateTimeOffset created = CommonTools.ParsePosixDateTime(value);

            // Add the fetched information
            LastModifiedInfo _time = new(property, elementTypes, created);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((LastModifiedInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="LastModifiedInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(LastModifiedInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="LastModifiedInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="LastModifiedInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(LastModifiedInfo source, LastModifiedInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.LastModified == target.LastModified
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 1594941628;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + LastModified.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(LastModifiedInfo left, LastModifiedInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(LastModifiedInfo left, LastModifiedInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BasePartInfo source, BasePartInfo target) =>
            (LastModifiedInfo)source == (LastModifiedInfo)target;

        internal LastModifiedInfo() { }

        internal LastModifiedInfo(PropertyInfo? property, string[] elementTypes, DateTimeOffset rev) :
            base(property, elementTypes)
        {
            LastModified = rev;
        }
    }
}
