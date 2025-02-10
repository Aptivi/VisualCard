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
using System.Linq;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts;

namespace VisualCard.Parts
{
    /// <summary>
    /// Base card part class
    /// </summary>
    [DebuggerDisplay("Base card part | ALTID: {AltId}, TYPE: {ElementTypes}, VALUE: {ValueType}")]
    public abstract class BaseCardPartInfo : BasePartInfo, IEquatable<BaseCardPartInfo>
    {
        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((BaseCardPartInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="BaseCardPartInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(BaseCardPartInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="BaseCardPartInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="BaseCardPartInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(BaseCardPartInfo source, BaseCardPartInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.Property == target.Property &&
                source.ElementTypes.SequenceEqual(target.ElementTypes) &&
                source.AltId == target.AltId &&
                source.ValueType == target.ValueType &&
                source.Group == target.Group &&
                EqualsInternal(source, target)
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 1046930009;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<PropertyInfo?>.Default.GetHashCode(Property);
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(ElementTypes);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ValueType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Group);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(BaseCardPartInfo left, BaseCardPartInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(BaseCardPartInfo left, BaseCardPartInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BasePartInfo source, BasePartInfo target) =>
            (BaseCardPartInfo)source == (BaseCardPartInfo)target;

        internal BaseCardPartInfo()
        { }

        internal BaseCardPartInfo(PropertyInfo? property, int altId, string[] elementTypes, string group, string valueType) :
            base(property, altId, elementTypes, group, valueType)
        { }
    }
}
