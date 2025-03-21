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

namespace VisualCard.Calendar.Parts.Implementations.TimeZone
{
    /// <summary>
    /// Time zone offset to info
    /// </summary>
    [DebuggerDisplay("Time zone offset to = {Offset}")]
    public class TimeZoneOffsetToInfo : BaseCalendarPartInfo, IEquatable<TimeZoneOffsetToInfo>
    {
        /// <summary>
        /// Time zone offset
        /// </summary>
        public TimeSpan Offset { get; set; }

        internal static BaseCalendarPartInfo FromStringStatic(string value, PropertyInfo property, int altId, string[] elementTypes, Version cardVersion) =>
            (BaseCalendarPartInfo)new TimeZoneOffsetToInfo().FromStringInternal(value, property, altId, elementTypes, cardVersion);

        internal override string ToStringInternal(Version cardVersion) =>
            $"{CommonTools.SaveUtcOffset(Offset)}";

        internal override BasePartInfo FromStringInternal(string value, PropertyInfo property, int altId, string[] elementTypes, Version cardVersion)
        {
            // Populate the fields
            TimeSpan start = CommonTools.ParseUtcOffset(value);

            // Add the fetched information
            TimeZoneOffsetToInfo _time = new(property, elementTypes, start);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((TimeZoneOffsetToInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="TimeZoneOffsetToInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(TimeZoneOffsetToInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="TimeZoneOffsetToInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="TimeZoneOffsetToInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(TimeZoneOffsetToInfo source, TimeZoneOffsetToInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.Offset == target.Offset
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -1362650496;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Offset.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(TimeZoneOffsetToInfo left, TimeZoneOffsetToInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(TimeZoneOffsetToInfo left, TimeZoneOffsetToInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BasePartInfo source, BasePartInfo target) =>
            (TimeZoneOffsetToInfo)source == (TimeZoneOffsetToInfo)target;

        internal TimeZoneOffsetToInfo() { }

        internal TimeZoneOffsetToInfo(PropertyInfo? property, string[] elementTypes, TimeSpan rev) :
            base(property, elementTypes)
        {
            Offset = rev;
        }
    }
}
