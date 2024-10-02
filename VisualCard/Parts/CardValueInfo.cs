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
using VisualCard.Parsers.Arguments;

namespace VisualCard.Parts
{
    /// <summary>
    /// Card value information
    /// </summary>
    [DebuggerDisplay("Card value | ALTID: {AltId}, TYPE: {ElementTypes}, VALUE: {ValueType}")]
    public class CardValueInfo<TValue> : IEquatable<CardValueInfo<TValue>>
    {
        /// <summary>
        /// Final arguments
        /// </summary>
        public virtual ArgumentInfo[] Arguments { get; internal set; } = [];

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
        public virtual string Group { get; internal set; } = "";

        /// <summary>
        /// Value
        /// </summary>
        public virtual TValue Value { get; internal set; }

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
        /// <param name="other">The target <see cref="CardValueInfo{TValue}"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(CardValueInfo<TValue> other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="CardValueInfo{TValue}"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="CardValueInfo{TValue}"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(CardValueInfo<TValue> source, CardValueInfo<TValue> target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.Arguments.SequenceEqual(target.Arguments) &&
                source.ElementTypes.SequenceEqual(target.ElementTypes) &&
                source.AltId == target.AltId &&
                source.ValueType == target.ValueType &&
                source.Group == target.Group &&
                EqualsInternal(source, target)
            ;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((CardValueInfo<TValue>)obj);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 975087586;
            hashCode = hashCode * -1521134295 + EqualityComparer<ArgumentInfo[]>.Default.GetHashCode(Arguments);
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(ElementTypes);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ValueType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Group);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(CardValueInfo<TValue> left, CardValueInfo<TValue> right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(CardValueInfo<TValue> left, CardValueInfo<TValue> right) =>
            !(left == right);

        internal virtual bool EqualsInternal(CardValueInfo<TValue> source, CardValueInfo<TValue> target) =>
            true;

        internal CardValueInfo(ArgumentInfo[] arguments, int altId, string[] elementTypes, string valueType, string group, TValue? value)
        {
            Arguments = arguments;
            AltId = altId;
            ElementTypes = elementTypes;
            ValueType = valueType;
            Group = group;
            Value = value ??
                throw new ArgumentNullException(nameof(value));
        }
    }
}
