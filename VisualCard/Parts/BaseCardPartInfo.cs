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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VisualCard.Parsers.Arguments;

namespace VisualCard.Parts
{
    /// <summary>
    /// Base card part class
    /// </summary>
    [DebuggerDisplay("Base card part | ALTID: {AltId}, TYPE: {ElementTypes}, VALUE: {ValueType}")]
    public abstract class BaseCardPartInfo : IEquatable<BaseCardPartInfo>
    {
        /// <summary>
        /// Property information containing details about this property that this class instance was created from
        /// </summary>
        public virtual PropertyInfo? Property { get; internal set; }

        /// <summary>
        /// Alternative ID. Zero if unspecified.
        /// </summary>
        public virtual int AltId { get; internal set; }

        /// <summary>
        /// Card element type (home, work, ...)
        /// </summary>
        public virtual string[] ElementTypes { get; internal set; } = [];

        /// <summary>
        /// Value type (usually set by VALUE=)
        /// </summary>
        public virtual string ValueType { get; internal set; } = "";

        /// <summary>
        /// Property group
        /// </summary>
        public string Group =>
            Property?.Group ?? "";

        /// <summary>
        /// Nested property groups
        /// </summary>
        public string[] NestedGroups =>
            Group.Split('.');

        /// <summary>
        /// Is this part preferred?
        /// </summary>
        public bool IsPreferred =>
            HasType("PREF");

        /// <summary>
        /// Checks to see if this part has a specific type
        /// </summary>
        /// <param name="type">Type to check (home, work, ...)</param>
        /// <returns>True if found; otherwise, false.</returns>
        public bool HasType(string type)
        {
            bool found = false;
            foreach (string elementType in ElementTypes)
            {
                if (type.Equals(elementType, StringComparison.OrdinalIgnoreCase))
                    found = true;
            }
            return found;
        }

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
        public override bool Equals(object obj) =>
            Equals((BaseCardPartInfo)obj);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 516898731;
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

        internal virtual bool EqualsInternal(BaseCardPartInfo source, BaseCardPartInfo target) =>
            true;

        internal abstract BaseCardPartInfo FromStringVcardInternal(string value, PropertyInfo property, int altId, string[] elementTypes, string valueType, Version cardVersion);

        internal abstract string ToStringVcardInternal(Version cardVersion);

        internal BaseCardPartInfo()
        { }

        internal BaseCardPartInfo(PropertyInfo? property, int altId, string[] elementTypes, string valueType)
        {
            Property = property;
            AltId = altId;
            ElementTypes = elementTypes;
            ValueType = valueType;
        }
    }
}
