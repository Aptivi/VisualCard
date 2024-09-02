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
using VisualCard.Calendar.Parsers;
using VisualCard.Calendar.Parts;

namespace VisualCard.Calendar.Parts.Implementations.Event
{
    /// <summary>
    /// Contact categories info
    /// </summary>
    [DebuggerDisplay("Categories = {Categories}")]
    public class CategoriesInfo : BaseCalendarPartInfo, IEquatable<CategoriesInfo>
    {
        /// <summary>
        /// The contact's categories
        /// </summary>
        public string[] Categories { get; }

        internal static BaseCalendarPartInfo FromStringVcalendarStatic(string value, string[] finalArgs, string[] elementTypes, string valueType, Version cardVersion) =>
            new CategoriesInfo().FromStringVcalendarInternal(value, finalArgs, elementTypes, valueType, cardVersion);

        internal override string ToStringVcalendarInternal(Version cardVersion) =>
            $"{string.Join(VCalendarConstants._valueDelimiter.ToString(), Categories)}";

        internal override BaseCalendarPartInfo FromStringVcalendarInternal(string value, string[] finalArgs, string[] elementTypes, string valueType, Version cardVersion)
        {
            // Populate the fields
            var categories = Regex.Unescape(value).Split(cardVersion.Major == 1 ? ';' : ',');

            // Add the fetched information
            CategoriesInfo _time = new([], elementTypes, valueType, categories);
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

            // Check all the properties
            return
                source.Categories == target.Categories
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -723142617;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(Categories);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(CategoriesInfo left, CategoriesInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(CategoriesInfo left, CategoriesInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCalendarPartInfo source, BaseCalendarPartInfo target) =>
            (CategoriesInfo)source == (CategoriesInfo)target;

        internal CategoriesInfo() { }

        internal CategoriesInfo(string[] arguments, string[] elementTypes, string valueType, string[] categories) :
            base(arguments, elementTypes, valueType)
        {
            Categories = categories;
        }
    }
}
