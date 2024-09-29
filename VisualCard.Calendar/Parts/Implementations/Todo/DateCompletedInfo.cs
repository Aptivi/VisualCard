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

namespace VisualCard.Calendar.Parts.Implementations.Todo
{
    /// <summary>
    /// Card date completed info
    /// </summary>
    [DebuggerDisplay("Date Completed = {DateCompleted}")]
    public class DateCompletedInfo : BaseCalendarPartInfo, IEquatable<DateCompletedInfo>
    {
        /// <summary>
        /// The to-do completion date
        /// </summary>
        public DateTimeOffset DateCompleted { get; }

        internal static BaseCalendarPartInfo FromStringVcalendarStatic(string value, string[] finalArgs, string[] elementTypes, string valueType, Version cardVersion) =>
            new DateCompletedInfo().FromStringVcalendarInternal(value, finalArgs, elementTypes, valueType, cardVersion);

        internal override string ToStringVcalendarInternal(Version cardVersion) =>
            $"{VcardParserTools.SavePosixDate(DateCompleted)}";

        internal override BaseCalendarPartInfo FromStringVcalendarInternal(string value, string[] finalArgs, string[] elementTypes, string valueType, Version cardVersion)
        {
            // Populate the fields
            DateTimeOffset completed = VcardParserTools.ParsePosixDate(value);

            // Add the fetched information
            DateCompletedInfo _time = new(finalArgs, elementTypes, valueType, completed);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((DateCompletedInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="DateCompletedInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(DateCompletedInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="DateCompletedInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="DateCompletedInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(DateCompletedInfo source, DateCompletedInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.DateCompleted == target.DateCompleted
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 47832270;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + DateCompleted.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(DateCompletedInfo left, DateCompletedInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(DateCompletedInfo left, DateCompletedInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCalendarPartInfo source, BaseCalendarPartInfo target) =>
            (DateCompletedInfo)source == (DateCompletedInfo)target;

        internal DateCompletedInfo() { }

        internal DateCompletedInfo(string[] arguments, string[] elementTypes, string valueType, DateTimeOffset rev) :
            base(arguments, elementTypes, valueType)
        {
            DateCompleted = rev;
        }
    }
}
