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

namespace VisualCard.Calendar.Parts.Implementations
{
    /// <summary>
    /// Calendar comment info
    /// </summary>
    [DebuggerDisplay("Comment = {Comment}")]
    public class CommentInfo : BaseCalendarPartInfo, IEquatable<CommentInfo>
    {
        /// <summary>
        /// The calendar's comment
        /// </summary>
        public string? Comment { get; }

        internal static BaseCalendarPartInfo FromStringVcalendarStatic(string value, string[] finalArgs, string[] elementTypes, string valueType, Version cardVersion) =>
            new CommentInfo().FromStringVcalendarInternal(value, finalArgs, elementTypes, valueType, cardVersion);

        internal override string ToStringVcalendarInternal(Version cardVersion) =>
            Comment ?? "";

        internal override BaseCalendarPartInfo FromStringVcalendarInternal(string value, string[] finalArgs, string[] elementTypes, string valueType, Version cardVersion)
        {
            // Populate the fields
            var comment = Regex.Unescape(value);

            // Add the fetched information
            CommentInfo _time = new([], elementTypes, valueType, comment);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((CommentInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="CommentInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(CommentInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="CommentInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="CommentInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(CommentInfo source, CommentInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.Comment == target.Comment
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -1115589996;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(Comment);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(CommentInfo left, CommentInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(CommentInfo left, CommentInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCalendarPartInfo source, BaseCalendarPartInfo target) =>
            (CommentInfo)source == (CommentInfo)target;

        internal CommentInfo() { }

        internal CommentInfo(string[] arguments, string[] elementTypes, string valueType, string comment) :
            base(arguments, elementTypes, valueType)
        {
            Comment = comment;
        }
    }
}
