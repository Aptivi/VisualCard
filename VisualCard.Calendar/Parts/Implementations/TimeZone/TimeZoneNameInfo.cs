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

namespace VisualCard.Calendar.Parts.Implementations.TimeZone
{
    /// <summary>
    /// Calendar time zone name info
    /// </summary>
    [DebuggerDisplay("Time Zone Name = {TimeZoneName}")]
    public class TimeZoneNameInfo : BaseCalendarPartInfo, IEquatable<TimeZoneNameInfo>
    {
        /// <summary>
        /// The time zone name
        /// </summary>
        public string? TimeZoneName { get; }

        internal static BaseCalendarPartInfo FromStringVcalendarStatic(string value, string[] finalArgs, string[] elementTypes, string valueType, Version cardVersion) =>
            new TimeZoneNameInfo().FromStringVcalendarInternal(value, finalArgs, elementTypes, valueType, cardVersion);

        internal override string ToStringVcalendarInternal(Version cardVersion) =>
            TimeZoneName ?? "";

        internal override BaseCalendarPartInfo FromStringVcalendarInternal(string value, string[] finalArgs, string[] elementTypes, string valueType, Version cardVersion)
        {
            // Populate the fields
            var timezonename = Regex.Unescape(value);

            // Add the fetched information
            TimeZoneNameInfo _time = new([], elementTypes, valueType, timezonename);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((TimeZoneNameInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="TimeZoneNameInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(TimeZoneNameInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="TimeZoneNameInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="TimeZoneNameInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(TimeZoneNameInfo source, TimeZoneNameInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.TimeZoneName == target.TimeZoneName
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -1115589996;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(TimeZoneName);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(TimeZoneNameInfo left, TimeZoneNameInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(TimeZoneNameInfo left, TimeZoneNameInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCalendarPartInfo source, BaseCalendarPartInfo target) =>
            (TimeZoneNameInfo)source == (TimeZoneNameInfo)target;

        internal TimeZoneNameInfo() { }

        internal TimeZoneNameInfo(string[] arguments, string[] elementTypes, string valueType, string timezonename) :
            base(arguments, elementTypes, valueType)
        {
            TimeZoneName = timezonename;
        }
    }
}
