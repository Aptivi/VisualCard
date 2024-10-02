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
using System.Text.RegularExpressions;
using VisualCard.Parsers;
using VisualCard.Parsers.Arguments;

namespace VisualCard.Calendar.Parts.Implementations
{
    /// <summary>
    /// Duration info
    /// </summary>
    [DebuggerDisplay("Duration = {Duration}")]
    public class DurationInfo : BaseCalendarPartInfo, IEquatable<DurationInfo>
    {
        /// <summary>
        /// Duration in a string
        /// </summary>
        public string? Duration { get; }
        
        /// <summary>
        /// Duration in a time span
        /// </summary>
        public TimeSpan DurationSpan =>
            VcardCommonTools.GetDurationSpan(Duration ?? "").span;
        
        /// <summary>
        /// Duration in a date/time representation
        /// </summary>
        public DateTimeOffset DurationResult =>
            VcardCommonTools.GetDurationSpan(Duration ?? "").result;

        internal static BaseCalendarPartInfo FromStringVcalendarStatic(string value, ArgumentInfo[] finalArgs, string[] elementTypes, string valueType, Version cardVersion) =>
            new DurationInfo().FromStringVcalendarInternal(value, finalArgs, elementTypes, valueType, cardVersion);

        internal override string ToStringVcalendarInternal(Version cardVersion) =>
            Duration ?? "";

        internal override BaseCalendarPartInfo FromStringVcalendarInternal(string value, ArgumentInfo[] finalArgs, string[] elementTypes, string valueType, Version cardVersion)
        {
            // Populate the fields
            string duration = Regex.Unescape(value);

            // Add the fetched information
            DurationInfo _time = new(finalArgs, elementTypes, valueType, duration);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((DurationInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="DurationInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(DurationInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="DurationInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="DurationInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(DurationInfo source, DurationInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.Duration == target.Duration
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -101114941;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(Duration);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(DurationInfo left, DurationInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(DurationInfo left, DurationInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCalendarPartInfo source, BaseCalendarPartInfo target) =>
            (DurationInfo)source == (DurationInfo)target;

        internal DurationInfo() { }

        internal DurationInfo(ArgumentInfo[] arguments, string[] elementTypes, string valueType, string duration) :
            base(arguments, elementTypes, valueType)
        {
            Duration = duration;
        }
    }
}
