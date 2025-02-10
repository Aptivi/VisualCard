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
using VisualCard.Common.Parsers;
using VisualCard.Common.Parsers.Arguments;

namespace VisualCard.Common.Parts.Implementations
{
    /// <summary>
    /// Non-standard field entry information
    /// </summary>
    [DebuggerDisplay("Non-standard: {XKeyName} = {string.Join(\", \", XValues)}")]
    public class XNameInfo : BasePartInfo, IEquatable<XNameInfo>
    {
        /// <summary>
        /// X- key name
        /// </summary>
        public string? XKeyName { get; }
        /// <summary>
        /// X- values
        /// </summary>
        public string[]? XValues { get; set; }

        internal static BasePartInfo FromStringStatic(string value, PropertyInfo property, int altId, string[] elementTypes, string group, string valueType, Version cardVersion) =>
            new XNameInfo().FromStringInternal(value, property, altId, elementTypes, group, valueType, cardVersion);

        internal override string ToStringInternal(Version cardVersion) =>
            string.Join(CommonConstants._fieldDelimiter.ToString(), XValues);

        internal override BasePartInfo FromStringInternal(string value, PropertyInfo property, int altId, string[] elementTypes, string group, string valueType, Version cardVersion)
        {
            string xValue = value.Substring(CommonConstants._xSpecifier.Length);
            string[] splitX = xValue.Split(CommonConstants._argumentDelimiter);

            // Populate the name
            string _xName = splitX[0].Contains(CommonConstants._fieldDelimiter.ToString()) ?
                            splitX[0].Substring(0, splitX[0].IndexOf(CommonConstants._fieldDelimiter)) :
                            splitX[0];

            // Populate the fields
            string[] _xValues = splitX[1].Split(CommonConstants._fieldDelimiter);
            XNameInfo _x = new(property, altId, elementTypes, group, valueType, _xName, _xValues);
            return _x;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((XNameInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="XNameInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(XNameInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="XNameInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="XNameInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(XNameInfo source, XNameInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.XValues.SequenceEqual(target.XValues) &&
                source.XKeyName == target.XKeyName
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 390070728;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(XKeyName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]?>.Default.GetHashCode(XValues);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(XNameInfo left, XNameInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(XNameInfo left, XNameInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BasePartInfo source, BasePartInfo target) =>
            (XNameInfo)source == (XNameInfo)target;

        internal XNameInfo() { }

        internal XNameInfo(PropertyInfo? property, int altId, string[] elementTypes, string group, string valueType, string xKeyName, string[] xValues) :
            base(property, altId, elementTypes, group, valueType)
        {
            XKeyName = xKeyName;
            XValues = xValues;
        }
    }
}
