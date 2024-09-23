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
using VisualCard.Parsers;

namespace VisualCard.Calendar.Parts.Implementations.Legacy
{
    /// <summary>
    /// Daylight information (entire calendar)
    /// </summary>
    [DebuggerDisplay("Daylight = {Flag}")]
    public class DaylightInfo : BaseCalendarPartInfo, IEquatable<DaylightInfo>
    {
        /// <summary>
        /// Whether it's enabled or not
        /// </summary>
        public bool Flag { get; }

        /// <summary>
        /// UTC offset of the daylight saving time
        /// </summary>
        public TimeSpan UtcOffset { get; }

        /// <summary>
        /// Start date and time of the daylight saving time
        /// </summary>
        public DateTime DaylightStart { get; }

        /// <summary>
        /// End date and time of the daylight saving time
        /// </summary>
        public DateTime DaylightEnd { get; }

        /// <summary>
        /// Standard time designation
        /// </summary>
        public string? StandardDesignation { get; }

        /// <summary>
        /// Daylight saving time designation
        /// </summary>
        public string? DaylightDesignation { get; }

        internal static BaseCalendarPartInfo FromStringVcalendarStatic(string value, string[] finalArgs, string[] elementTypes, string valueType, Version calendarVersion) =>
            new DaylightInfo().FromStringVcalendarInternal(value, finalArgs, elementTypes, valueType, calendarVersion);

        internal override string ToStringVcalendarInternal(Version calendarVersion)
        {
            if (!Flag)
                return "FALSE";
            string posixUtc = VcardParserTools.SaveUtcOffset(UtcOffset);
            string posixStart = VcardParserTools.SavePosixDate(DaylightStart);
            string posixEnd = VcardParserTools.SavePosixDate(DaylightEnd);
            return $"TRUE;{posixUtc};{posixStart};{posixEnd};{StandardDesignation};{DaylightDesignation}";
        }

        internal override BaseCalendarPartInfo FromStringVcalendarInternal(string value, string[] finalArgs, string[] elementTypes, string valueType, Version calendarVersion)
        {
            // Get the values
            string[] split = value.Split(';');
            if (split.Length == 1 && split[0] == "FALSE")
                return new DaylightInfo(finalArgs, elementTypes, valueType, false, new(), new(), new(), "", "");
            if (split.Length != 6)
                throw new ArgumentException($"When splitting daylight information, the split value is {split.Length} instead of 6.");
            string unprocessedUtc = split[1];
            string unprocessedStart = split[2];
            string unprocessedEnd = split[3];
            string standard = split[4];
            string daylight = split[5];

            // Process the UTC offset and start/end dates
            TimeSpan utcOffset = VcardParserTools.ParseUtcOffset(unprocessedUtc);
            DateTime startDate = VcardParserTools.ParsePosixDate(unprocessedStart);
            DateTime endDate = VcardParserTools.ParsePosixDate(unprocessedEnd);

            // Populate the fields
            DaylightInfo _geo = new(finalArgs, elementTypes, valueType, true, utcOffset, startDate, endDate, standard, daylight);
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

        internal override bool EqualsInternal(BaseCalendarPartInfo source, BaseCalendarPartInfo target) =>
            (DaylightInfo)source == (DaylightInfo)target;

        internal DaylightInfo() { }

        internal DaylightInfo(string[] arguments, string[] elementTypes, string valueType, bool flag, TimeSpan utcOffset, DateTime daylightStart, DateTime daylightEnd, string standardDesignation, string daylightDesignation) :
            base(arguments, elementTypes, valueType)
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
