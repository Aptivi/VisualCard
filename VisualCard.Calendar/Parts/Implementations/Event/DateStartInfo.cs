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

namespace VisualCard.Calendar.Parts.Implementations.Event
{
    /// <summary>
    /// Card date start info
    /// </summary>
    [DebuggerDisplay("Date Start = {dateStart}")]
    public class DateStartInfo : BaseCalendarPartInfo, IEquatable<DateStartInfo>
    {
        /// <summary>
        /// The card's revision
        /// </summary>
        public DateTime? DateStart { get; }

        internal static BaseCalendarPartInfo FromStringVcalendarStatic(string value, string[] finalArgs, string[] elementTypes, string valueType, Version cardVersion) =>
            new DateStartInfo().FromStringVcalendarInternal(value, finalArgs, elementTypes, valueType, cardVersion);

        internal override string ToStringVcalendarInternal(Version cardVersion) =>
            $"{DateStart:yyyy-MM-dd HH:mm:ss}";

        internal override BaseCalendarPartInfo FromStringVcalendarInternal(string value, string[] finalArgs, string[] elementTypes, string valueType, Version cardVersion)
        {
            // Populate the fields
            DateTime start = DateTime.Parse(value);

            // Add the fetched information
            DateStartInfo _time = new(finalArgs, elementTypes, valueType, start);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((DateStartInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="DateStartInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(DateStartInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="DateStartInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="DateStartInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(DateStartInfo source, DateStartInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.DateStart == target.DateStart
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 47832270;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + DateStart.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(DateStartInfo left, DateStartInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(DateStartInfo left, DateStartInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCalendarPartInfo source, BaseCalendarPartInfo target) =>
            (DateStartInfo)source == (DateStartInfo)target;

        internal DateStartInfo() { }

        internal DateStartInfo(string[] arguments, string[] elementTypes, string valueType, DateTime? rev) :
            base(arguments, elementTypes, valueType)
        {
            DateStart = rev;
        }
    }
}
