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
using VisualCard.Parsers;
using VisualCard.Parsers.Arguments;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact category info
    /// </summary>
    [DebuggerDisplay("{Category.Length} categories")]
    public class CategoryInfo : BaseCardPartInfo, IEquatable<CategoryInfo>
    {
        /// <summary>
        /// The contact's categories
        /// </summary>
        public string[]? Category { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, ArgumentInfo[] finalArgs, int altId, string[] elementTypes, string group, string valueType, Version cardVersion) =>
            new CategoryInfo().FromStringVcardInternal(value, finalArgs, altId, elementTypes, group, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion) =>
            $"{string.Join(VcardConstants._valueDelimiter.ToString(), Category)}";

        internal override BaseCardPartInfo FromStringVcardInternal(string value, ArgumentInfo[] finalArgs, int altId, string[] elementTypes, string group, string valueType, Version cardVersion)
        {
            // Populate the fields
            var categories = Regex.Unescape(value).Split(',');

            // Add the fetched information
            CategoryInfo _time = new(-1, [], elementTypes, valueType, group, categories);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((CategoryInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="CategoryInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(CategoryInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="CategoryInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="CategoryInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(CategoryInfo source, CategoryInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.Category == target.Category
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -723142617;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]?>.Default.GetHashCode(Category);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(CategoryInfo left, CategoryInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(CategoryInfo left, CategoryInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCardPartInfo source, BaseCardPartInfo target) =>
            ((CategoryInfo)source) == ((CategoryInfo)target);

        internal CategoryInfo() { }

        internal CategoryInfo(int altId, ArgumentInfo[] arguments, string[] elementTypes, string valueType, string group, string[] category) :
            base(arguments, altId, elementTypes, valueType, group)
        {
            Category = category;
        }
    }
}
