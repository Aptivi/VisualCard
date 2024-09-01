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
using VisualCard.Calendar.Parsers;

namespace VisualCard.Calendar.Parts.Implementations
{
    /// <summary>
    /// Contact non-standard field entry information
    /// </summary>
    [DebuggerDisplay("Non-standard = {XKeyName}")]
    public class XNameInfo : BaseCalendarPartInfo, IEquatable<XNameInfo>
    {
        /// <summary>
        /// X- key name
        /// </summary>
        public string XKeyName { get; }
        /// <summary>
        /// X- values
        /// </summary>
        public string[] XValues { get; }

        internal static BaseCalendarPartInfo FromStringVcalendarStatic(string value, string[] finalArgs, string[] elementTypes, string valueType, Version cardVersion) =>
            new XNameInfo().FromStringVcalendarInternal(value, finalArgs, elementTypes, valueType, cardVersion);

        internal override string ToStringVcalendarInternal(Version cardVersion) =>
            string.Join(VCalendarConstants._fieldDelimiter.ToString(), XValues);

        internal override BaseCalendarPartInfo FromStringVcalendarInternal(string value, string[] finalArgs, string[] elementTypes, string valueType, Version cardVersion)
        {
            string xValue = value.Substring(VCalendarConstants._xSpecifier.Length);
            string[] splitX = xValue.Split(VCalendarConstants._argumentDelimiter);

            // Populate the name
            string _xName = splitX[0].Contains(VCalendarConstants._fieldDelimiter.ToString()) ?
                            splitX[0].Substring(0, splitX[0].IndexOf(VCalendarConstants._fieldDelimiter)) :
                            splitX[0];

            // Populate the fields
            string[] _xValues = splitX[1].Split(VCalendarConstants._fieldDelimiter);
            XNameInfo _x = new(finalArgs, elementTypes, valueType, _xName, _xValues);
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
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(XKeyName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(XValues);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(XNameInfo left, XNameInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(XNameInfo left, XNameInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCalendarPartInfo source, BaseCalendarPartInfo target) =>
            (XNameInfo)source == (XNameInfo)target;

        internal XNameInfo() { }

        internal XNameInfo(string[] arguments, string[] elementTypes, string valueType, string xKeyName, string[] xValues) :
            base(arguments, elementTypes, valueType)
        {
            XKeyName = xKeyName;
            XValues = xValues;
        }
    }
}
