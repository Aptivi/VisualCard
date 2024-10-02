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
using VisualCard.Parsers.Arguments;

namespace VisualCard.Calendar.Parts.Implementations.Event
{
    /// <summary>
    /// Event date end info
    /// </summary>
    [DebuggerDisplay("Date End = {DateEnd}")]
    public class DateEndInfo : BaseCalendarPartInfo, IEquatable<DateEndInfo>
    {
        /// <summary>
        /// The event's end date
        /// </summary>
        public DateTimeOffset DateEnd { get; }

        internal static BaseCalendarPartInfo FromStringVcalendarStatic(string value, ArgumentInfo[] finalArgs, string[] elementTypes, string valueType, Version cardVersion) =>
            new DateEndInfo().FromStringVcalendarInternal(value, finalArgs, elementTypes, valueType, cardVersion);

        internal override string ToStringVcalendarInternal(Version cardVersion)
        {
            string type = ValueType ?? "";
            string value =
                type.Equals("date", StringComparison.OrdinalIgnoreCase) ?
                VcardCommonTools.SavePosixDate(DateEnd, true) :
                VcardCommonTools.SavePosixDate(DateEnd);
            return value;
        }

        internal override BaseCalendarPartInfo FromStringVcalendarInternal(string value, ArgumentInfo[] finalArgs, string[] elementTypes, string valueType, Version cardVersion)
        {
            // Populate the fields
            string type = valueType ?? "";
            DateTimeOffset end =
                type.Equals("date", StringComparison.OrdinalIgnoreCase) ?
                VcardCommonTools.ParsePosixDate(value) :
                VcardCommonTools.ParsePosixDateTime(value);

            // Add the fetched information
            DateEndInfo _time = new(finalArgs, elementTypes, valueType ?? "", end);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((DateEndInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="DateEndInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(DateEndInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="DateEndInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="DateEndInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(DateEndInfo source, DateEndInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.DateEnd == target.DateEnd
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 47832270;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + DateEnd.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(DateEndInfo left, DateEndInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(DateEndInfo left, DateEndInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCalendarPartInfo source, BaseCalendarPartInfo target) =>
            (DateEndInfo)source == (DateEndInfo)target;

        internal DateEndInfo() { }

        internal DateEndInfo(ArgumentInfo[] arguments, string[] elementTypes, string valueType, DateTimeOffset rev) :
            base(arguments, elementTypes, valueType)
        {
            DateEnd = rev;
        }
    }
}
