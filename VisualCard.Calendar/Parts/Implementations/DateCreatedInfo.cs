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
using VisualCard.Parsers;
using VisualCard.Parsers.Arguments;

namespace VisualCard.Calendar.Parts.Implementations
{
    /// <summary>
    /// Calendar date created info
    /// </summary>
    [DebuggerDisplay("Date Created = {DateCreated}")]
    public class DateCreatedInfo : BaseCalendarPartInfo, IEquatable<DateCreatedInfo>
    {
        /// <summary>
        /// The calendar's creation date
        /// </summary>
        public DateTimeOffset DateCreated { get; }

        internal static BaseCalendarPartInfo FromStringVcalendarStatic(string value, PropertyInfo property, string[] elementTypes, string group, string valueType, Version cardVersion) =>
            new DateCreatedInfo().FromStringVcalendarInternal(value, property, elementTypes, group, valueType, cardVersion);

        internal override string ToStringVcalendarInternal(Version cardVersion) =>
            $"{VcardCommonTools.SavePosixDate(DateCreated)}";

        internal override BaseCalendarPartInfo FromStringVcalendarInternal(string value, PropertyInfo property, string[] elementTypes, string group, string valueType, Version cardVersion)
        {
            // Populate the fields
            DateTimeOffset created = VcardCommonTools.ParsePosixDateTime(value);

            // Add the fetched information
            DateCreatedInfo _time = new(property, elementTypes, group, valueType, created);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((DateCreatedInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="DateCreatedInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(DateCreatedInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="DateCreatedInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="DateCreatedInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(DateCreatedInfo source, DateCreatedInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.DateCreated == target.DateCreated
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 47832270;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + DateCreated.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(DateCreatedInfo left, DateCreatedInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(DateCreatedInfo left, DateCreatedInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCalendarPartInfo source, BaseCalendarPartInfo target) =>
            (DateCreatedInfo)source == (DateCreatedInfo)target;

        internal DateCreatedInfo() { }

        internal DateCreatedInfo(PropertyInfo? property, string[] elementTypes, string group, string valueType, DateTimeOffset rev) :
            base(property, elementTypes, group, valueType)
        {
            DateCreated = rev;
        }
    }
}
