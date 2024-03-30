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
using System.Linq;
using System.Text.RegularExpressions;
using VisualCard.Parsers;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact category info
    /// </summary>
    [DebuggerDisplay("Category = {Category}")]
    public class CategoryInfo : BaseCardPartInfo, IEquatable<CategoryInfo>
    {
        /// <summary>
        /// The contact's categories
        /// </summary>
        public string[] Category { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, int altId, Version cardVersion) =>
            new CategoryInfo().FromStringVcardInternal(value, altId, cardVersion);

        internal static BaseCardPartInfo FromStringVcardWithTypeStatic(string value, string[] finalArgs, int altId, Version cardVersion) =>
            new CategoryInfo().FromStringVcardWithTypeInternal(value, finalArgs, altId, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion)
        {
            return
                $"{VcardConstants._categoriesSpecifier}:" +
                $"{Category}";
        }

        internal override BaseCardPartInfo FromStringVcardInternal(string value, int altId, Version cardVersion)
        {
            // Get the value
            string categoryValue = value.Substring(VcardConstants._categoriesSpecifier.Length + 1);

            // Populate the fields
            return InstallInfo(categoryValue);
        }

        internal override BaseCardPartInfo FromStringVcardWithTypeInternal(string value, string[] finalArgs, int altId, Version cardVersion) =>
            FromStringVcardInternal(value, altId, cardVersion);

        private CategoryInfo InstallInfo(string value)
        {
            // Populate field
            var categories = Regex.Unescape(value).Split(',');

            // Add the fetched information
            CategoryInfo _time = new(0, [], categories);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            base.Equals(obj);

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
            if (source is null)
                return false;

            // Check all the properties
            return
                source.AltArguments.SequenceEqual(target.AltArguments) &&
                source.AltId == target.AltId &&
                source.Category == target.Category
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 1152977432;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AltArguments);
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(Category);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(CategoryInfo left, CategoryInfo right) =>
            EqualityComparer<CategoryInfo>.Default.Equals(left, right);

        /// <inheritdoc/>
        public static bool operator !=(CategoryInfo left, CategoryInfo right) =>
            !(left == right);

        internal CategoryInfo() { }

        internal CategoryInfo(int altId, string[] altArguments, string[] category)
        {
            AltId = altId;
            AltArguments = altArguments;
            Category = category;
        }
    }
}
