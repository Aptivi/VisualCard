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
using VisualCard.Parsers;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts;
using VisualCard.Common.Parsers;

namespace VisualCard.Calendar.Parts.Implementations
{
    /// <summary>
    /// Calendar resources info
    /// </summary>
    [DebuggerDisplay("{Resources.Length} resources")]
    public class ResourcesInfo : BaseCalendarPartInfo, IEquatable<ResourcesInfo>
    {
        /// <summary>
        /// The calendar's resources
        /// </summary>
        public string[]? Resources { get; set; }

        internal static BaseCalendarPartInfo FromStringStatic(string value, PropertyInfo property, int altId, string[] elementTypes, string group, string valueType, Version cardVersion) =>
            (BaseCalendarPartInfo)new ResourcesInfo().FromStringInternal(value, property, altId, elementTypes, group, valueType, cardVersion);

        internal override string ToStringInternal(Version cardVersion) =>
            $"{string.Join(CommonConstants._valueDelimiter.ToString(), Resources)}";

        internal override BasePartInfo FromStringInternal(string value, PropertyInfo property, int altId, string[] elementTypes, string group, string valueType, Version cardVersion)
        {
            // Populate the fields
            var resources = Regex.Unescape(value).Split(cardVersion.Major == 1 ? ';' : ',');

            // Add the fetched information
            ResourcesInfo _time = new(property, elementTypes, group, valueType, resources);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((ResourcesInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="ResourcesInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(ResourcesInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="ResourcesInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="ResourcesInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(ResourcesInfo source, ResourcesInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.Resources == target.Resources
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -723142617;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]?>.Default.GetHashCode(Resources);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(ResourcesInfo left, ResourcesInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(ResourcesInfo left, ResourcesInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BasePartInfo source, BasePartInfo target) =>
            (ResourcesInfo)source == (ResourcesInfo)target;

        internal ResourcesInfo() { }

        internal ResourcesInfo(PropertyInfo? property, string[] elementTypes, string group, string valueType, string[] resources) :
            base(property, elementTypes, group, valueType)
        {
            Resources = resources;
        }
    }
}
