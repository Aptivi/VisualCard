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
using System.Text;
using VisualCard.Common.Parsers;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts;

namespace VisualCard.Calendar.Parts.Implementations.FreeBusy
{
    /// <summary>
    /// Free/busy info
    /// </summary>
    [DebuggerDisplay("{FreeBusy.StartDate} -> {FreeBusy.EndDate}")]
    public class CalendarFreeBusyInfo : BaseCalendarPartInfo, IEquatable<CalendarFreeBusyInfo>
    {
        /// <summary>
        /// Free/busy time periods
        /// </summary>
        public TimePeriod? FreeBusy { get; set; }

        internal static BaseCalendarPartInfo FromStringStatic(string value, PropertyInfo property, int altId, string[] elementTypes, Version cardVersion) =>
            (BaseCalendarPartInfo)new CalendarFreeBusyInfo().FromStringInternal(value, property, altId, elementTypes, cardVersion);

        internal override string ToStringInternal(Version cardVersion)
        {
            if (FreeBusy is null)
                return "";

            var builder = new StringBuilder();
            builder.Append(CommonTools.SavePosixDate(FreeBusy.StartDate) + "/" + CommonTools.SavePosixDate(FreeBusy.EndDate));
            return builder.ToString();
        }

        internal override BasePartInfo FromStringInternal(string value, PropertyInfo property, int altId, string[] elementTypes, Version cardVersion)
        {
            // Populate the fields
            var freeBusy = CommonTools.GetTimePeriod(value);

            // Add the fetched information
            CalendarFreeBusyInfo _time = new(property, elementTypes, freeBusy);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((CalendarFreeBusyInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="CalendarFreeBusyInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(CalendarFreeBusyInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="CalendarFreeBusyInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="CalendarFreeBusyInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(CalendarFreeBusyInfo source, CalendarFreeBusyInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.FreeBusy == target.FreeBusy
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 478819452;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<TimePeriod?>.Default.GetHashCode(FreeBusy);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(CalendarFreeBusyInfo left, CalendarFreeBusyInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(CalendarFreeBusyInfo left, CalendarFreeBusyInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BasePartInfo source, BasePartInfo target) =>
            (CalendarFreeBusyInfo)source == (CalendarFreeBusyInfo)target;

        internal CalendarFreeBusyInfo() { }

        internal CalendarFreeBusyInfo(PropertyInfo? property, string[] elementTypes, TimePeriod freeBusy) :
            base(property, elementTypes)
        {
            FreeBusy = freeBusy;
        }
    }
}
