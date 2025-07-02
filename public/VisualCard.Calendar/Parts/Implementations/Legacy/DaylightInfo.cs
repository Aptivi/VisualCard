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
using Textify.General;
using VisualCard.Calendar.Languages;
using VisualCard.Common.Parsers;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts;

namespace VisualCard.Calendar.Parts.Implementations.Legacy
{
    /// <summary>
    /// Daylight information (entire calendar)
    /// </summary>
    [DebuggerDisplay("Daylight Alarm = {Flag} @ {UtcOffset} (Daylight: {DaylightStart} -> {DaylightEnd} {DaylightDesignation}), {StandardDesignation}")]
    public class DaylightInfo : BaseCalendarPartInfo, IEquatable<DaylightInfo>
    {
        /// <summary>
        /// Whether it's enabled or not
        /// </summary>
        public bool Flag { get; set; }

        /// <summary>
        /// UTC offset of the daylight saving time
        /// </summary>
        public TimeSpan UtcOffset { get; set; }

        /// <summary>
        /// Start date and time of the daylight saving time
        /// </summary>
        public DateTimeOffset DaylightStart { get; set; }

        /// <summary>
        /// End date and time of the daylight saving time
        /// </summary>
        public DateTimeOffset DaylightEnd { get; set; }

        /// <summary>
        /// Standard time designation
        /// </summary>
        public string? StandardDesignation { get; set; }

        /// <summary>
        /// Daylight saving time designation
        /// </summary>
        public string? DaylightDesignation { get; set; }

        internal static BaseCalendarPartInfo FromStringStatic(string value, PropertyInfo property, int altId, string[] elementTypes, Version calendarVersion) =>
            (BaseCalendarPartInfo)new DaylightInfo().FromStringInternal(value, property, altId, elementTypes, calendarVersion);

        internal override string ToStringInternal(Version calendarVersion)
        {
            if (!Flag)
                return "FALSE";
            string posixUtc = CommonTools.SaveUtcOffset(UtcOffset);
            string posixStart = CommonTools.SavePosixDate(DaylightStart);
            string posixEnd = CommonTools.SavePosixDate(DaylightEnd);
            return $"TRUE;{posixUtc};{posixStart};{posixEnd};{StandardDesignation};{DaylightDesignation}";
        }

        internal override BasePartInfo FromStringInternal(string value, PropertyInfo property, int altId, string[] elementTypes, Version calendarVersion)
        {
            // Get the values
            string[] split = value.Split(';');
            if (split.Length == 1 && split[0] == "FALSE")
                return new DaylightInfo(property, elementTypes, false, new(), new(), new(), "", "");
            if (split.Length != 6)
                throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_LEGACY_DAYLIGHTINFO_ARGMISMATCH").FormatString(split.Length));
            string unprocessedUtc = split[1];
            string unprocessedStart = split[2];
            string unprocessedEnd = split[3];
            string standard = split[4];
            string daylight = split[5];

            // Process the UTC offset and start/end dates
            TimeSpan utcOffset = CommonTools.ParseUtcOffset(unprocessedUtc);
            DateTimeOffset startDate = CommonTools.ParsePosixDateTime(unprocessedStart);
            DateTimeOffset endDate = CommonTools.ParsePosixDateTime(unprocessedEnd);

            // Populate the fields
            DaylightInfo _geo = new(property, elementTypes, true, utcOffset, startDate, endDate, standard, daylight);
            return _geo;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((DaylightInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="DaylightInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(DaylightInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="DaylightInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="DaylightInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(DaylightInfo source, DaylightInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.Flag == target.Flag &&
                source.UtcOffset == target.UtcOffset &&
                source.DaylightStart == target.DaylightStart &&
                source.DaylightEnd == target.DaylightEnd &&
                source.StandardDesignation == target.StandardDesignation &&
                source.DaylightDesignation == target.DaylightDesignation
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 269991333;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Flag.GetHashCode();
            hashCode = hashCode * -1521134295 + UtcOffset.GetHashCode();
            hashCode = hashCode * -1521134295 + DaylightStart.GetHashCode();
            hashCode = hashCode * -1521134295 + DaylightEnd.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(StandardDesignation);
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(DaylightDesignation);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(DaylightInfo left, DaylightInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(DaylightInfo left, DaylightInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BasePartInfo source, BasePartInfo target) =>
            (DaylightInfo)source == (DaylightInfo)target;

        internal DaylightInfo() { }

        internal DaylightInfo(PropertyInfo? property, string[] elementTypes, bool flag, TimeSpan utcOffset, DateTimeOffset daylightStart, DateTimeOffset daylightEnd, string standardDesignation, string daylightDesignation) :
            base(property, elementTypes)
        {
            Flag = flag;
            UtcOffset = utcOffset;
            DaylightStart = daylightStart;
            DaylightEnd = daylightEnd;
            StandardDesignation = standardDesignation;
            DaylightDesignation = daylightDesignation;
        }
    }
}
