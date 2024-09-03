﻿//
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
using VisualCard.Calendar.Parsers;

namespace VisualCard.Calendar.Parts.Implementations.Event
{
    /// <summary>
    /// Card date stamp info
    /// </summary>
    [DebuggerDisplay("Date Stamp = {dateStamp}")]
    public class DateStampInfo : BaseCalendarPartInfo, IEquatable<DateStampInfo>
    {
        /// <summary>
        /// The card's revision
        /// </summary>
        public DateTime? DateStamp { get; }

        internal static BaseCalendarPartInfo FromStringVcalendarStatic(string value, string[] finalArgs, string[] elementTypes, string valueType, Version cardVersion) =>
            new DateStampInfo().FromStringVcalendarInternal(value, finalArgs, elementTypes, valueType, cardVersion);

        internal override string ToStringVcalendarInternal(Version cardVersion) =>
            $"{DateStamp:yyyy-MM-dd HH:mm:ss}";

        internal override BaseCalendarPartInfo FromStringVcalendarInternal(string value, string[] finalArgs, string[] elementTypes, string valueType, Version cardVersion)
        {
            // Populate the fields
            DateTime stamp = DateTime.Parse(value);

            // Add the fetched information
            DateStampInfo _time = new(finalArgs, elementTypes, valueType, stamp);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((DateStampInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="DateStampInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(DateStampInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="DateStampInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="DateStampInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(DateStampInfo source, DateStampInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.DateStamp == target.DateStamp
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 47832270;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + DateStamp.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(DateStampInfo left, DateStampInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(DateStampInfo left, DateStampInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCalendarPartInfo source, BaseCalendarPartInfo target) =>
            (DateStampInfo)source == (DateStampInfo)target;

        internal DateStampInfo() { }

        internal DateStampInfo(string[] arguments, string[] elementTypes, string valueType, DateTime? rev) :
            base(arguments, elementTypes, valueType)
        {
            DateStamp = rev;
        }
    }
}