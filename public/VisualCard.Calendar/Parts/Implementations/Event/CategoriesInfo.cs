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
using System.Text.RegularExpressions;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts;
using VisualCard.Common.Parts.Comparers;

namespace VisualCard.Calendar.Parts.Implementations.Event
{
    /// <summary>
    /// Event categories info
    /// </summary>
    [DebuggerDisplay("{Categories.Length} categories")]
    public class CategoriesInfo : BaseCalendarPartInfo, IEquatable<CategoriesInfo>
    {
        /// <summary>
        /// The calendar event's categories
        /// </summary>
        public string[]? Categories { get; set; }

        internal static BaseCalendarPartInfo FromStringStatic(string value, PropertyInfo property, int altId, string[] elementTypes, Version cardVersion) =>
            (BaseCalendarPartInfo)new CategoriesInfo().FromStringInternal(value, property, altId, elementTypes, cardVersion);

        internal override string ToStringInternal(Version cardVersion) =>
            $"{string.Join(cardVersion.Major == 1 ? ";" : ",", Categories)}";

        internal override BasePartInfo FromStringInternal(string value, PropertyInfo property, int altId, string[] elementTypes, Version cardVersion)
        {
            // Populate the fields
            var categories = Regex.Unescape(value).Split(cardVersion.Major == 1 ? ';' : ',');

            // Add the fetched information
            CategoriesInfo _time = new(property, elementTypes, categories);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((CategoriesInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="CategoriesInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(CategoriesInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="CategoriesInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="CategoriesInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(CategoriesInfo source, CategoriesInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;
            if (source.Categories is null || target.Categories is null)
                return false;

            // Check all the properties
            return
                CommonComparison.ContainsAll(source.Categories, target.Categories)
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -723142617;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]?>.Default.GetHashCode(Categories);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(CategoriesInfo left, CategoriesInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(CategoriesInfo left, CategoriesInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BasePartInfo source, BasePartInfo target) =>
            (CategoriesInfo)source == (CategoriesInfo)target;

        internal CategoriesInfo() { }

        internal CategoriesInfo(PropertyInfo? property, string[] elementTypes, string[] categories) :
            base(property, elementTypes)
        {
            Categories = categories;
        }
    }
}
