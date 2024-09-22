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
using System.Diagnostics;
using VisualCard.Parsers;

namespace VisualCard.Calendar.Parts.Implementations.TimeZone
{
    /// <summary>
    /// Time zone offset from info
    /// </summary>
    [DebuggerDisplay("Time zone offset from = {offset}")]
    public class TimeZoneOffsetFromInfo : BaseCalendarPartInfo, IEquatable<TimeZoneOffsetFromInfo>
    {
        /// <summary>
        /// The card's revision
        /// </summary>
        public TimeSpan Offset { get; }

        internal static BaseCalendarPartInfo FromStringVcalendarStatic(string value, string[] finalArgs, string[] elementTypes, string valueType, Version cardVersion) =>
            new TimeZoneOffsetFromInfo().FromStringVcalendarInternal(value, finalArgs, elementTypes, valueType, cardVersion);

        internal override string ToStringVcalendarInternal(Version cardVersion) =>
            $"{VcardParserTools.SaveUtcOffset(Offset)}";

        internal override BaseCalendarPartInfo FromStringVcalendarInternal(string value, string[] finalArgs, string[] elementTypes, string valueType, Version cardVersion)
        {
            // Populate the fields
            TimeSpan start = VcardParserTools.ParseUtcOffset(value);

            // Add the fetched information
            TimeZoneOffsetFromInfo _time = new(finalArgs, elementTypes, valueType, start);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((TimeZoneOffsetFromInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="TimeZoneOffsetFromInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(TimeZoneOffsetFromInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="TimeZoneOffsetFromInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="TimeZoneOffsetFromInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(TimeZoneOffsetFromInfo source, TimeZoneOffsetFromInfo target)
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
        public static bool operator ==(TimeZoneOffsetFromInfo left, TimeZoneOffsetFromInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(TimeZoneOffsetFromInfo left, TimeZoneOffsetFromInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCalendarPartInfo source, BaseCalendarPartInfo target) =>
            (TimeZoneOffsetFromInfo)source == (TimeZoneOffsetFromInfo)target;

        internal TimeZoneOffsetFromInfo() { }

        internal TimeZoneOffsetFromInfo(string[] arguments, string[] elementTypes, string valueType, TimeSpan rev) :
            base(arguments, elementTypes, valueType)
        {
            Offset = rev;
        }
    }
}
