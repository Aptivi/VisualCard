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
using VisualCard.Parsers;
using VisualCard.Parsers.Arguments;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Extraneous information that a card may hold
    /// </summary>
    [DebuggerDisplay("Extra: {KeyName} = {string.Join(\", \", Values)}")]
    public class ExtraInfo : BaseCardPartInfo, IEquatable<ExtraInfo>
    {
        /// <summary>
        /// Key name
        /// </summary>
        public string? KeyName { get; }
        /// <summary>
        /// Values
        /// </summary>
        public string[]? Values { get; set; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, PropertyInfo property, int altId, string[] elementTypes, string valueType, Version cardVersion) =>
            new ExtraInfo().FromStringVcardInternal(value, property, altId, elementTypes, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion) =>
            string.Join(VcardConstants._fieldDelimiter.ToString(), Values);

        internal override BaseCardPartInfo FromStringVcardInternal(string value, PropertyInfo property, int altId, string[] elementTypes, string valueType, Version cardVersion)
        {
            string[] split = value.Split(VcardConstants._argumentDelimiter);

            // Populate the name
            string _extra = split[0].Contains(VcardConstants._fieldDelimiter.ToString()) ?
                            split[0].Substring(0, split[0].IndexOf(VcardConstants._fieldDelimiter)) :
                            split[0];

            // Populate the fields
            string[] _values = split[1].Split(VcardConstants._fieldDelimiter);
            ExtraInfo _extraInfo = new(altId, property, elementTypes, valueType, _extra, _values);
            return _extraInfo;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((ExtraInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="ExtraInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(ExtraInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="ExtraInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="ExtraInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(ExtraInfo source, ExtraInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.Values.SequenceEqual(target.Values) &&
                source.KeyName == target.KeyName
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 1248266464;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(KeyName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]?>.Default.GetHashCode(Values);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(ExtraInfo left, ExtraInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(ExtraInfo left, ExtraInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCardPartInfo source, BaseCardPartInfo target) =>
            (ExtraInfo)source == (ExtraInfo)target;

        internal ExtraInfo() { }

        internal ExtraInfo(int altId, PropertyInfo? property, string[] elementTypes, string valueType, string keyName, string[] values) :
            base(property, altId, elementTypes, valueType)
        {
            KeyName = keyName;
            Values = values;
        }
    }
}
