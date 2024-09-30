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

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact geographical information
    /// </summary>
    [DebuggerDisplay("Geography = {Geo}")]
    public class GeoInfo : BaseCardPartInfo, IEquatable<GeoInfo>
    {
        /// <summary>
        /// The contact's geographical information
        /// </summary>
        public string? Geo { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, string[] finalArgs, int altId, string[] elementTypes, string group, string valueType, Version cardVersion) =>
            new GeoInfo().FromStringVcardInternal(value, finalArgs, altId, elementTypes, group, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion) =>
            Geo ?? "";

        internal override BaseCardPartInfo FromStringVcardInternal(string value, string[] finalArgs, int altId, string[] elementTypes, string group, string valueType, Version cardVersion)
        {
            // Get the value
            string _geoStr = Regex.Unescape(value);

            // Populate the fields
            GeoInfo _geo = new(altId, finalArgs, elementTypes, valueType, group, _geoStr);
            return _geo;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((GeoInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="GeoInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(GeoInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="GeoInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="GeoInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(GeoInfo source, GeoInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.Geo == target.Geo
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -456581192;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(Geo);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(GeoInfo left, GeoInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(GeoInfo left, GeoInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCardPartInfo source, BaseCardPartInfo target) =>
            ((GeoInfo)source) == ((GeoInfo)target);

        internal GeoInfo() { }

        internal GeoInfo(int altId, string[] arguments, string[] elementTypes, string valueType, string group, string geo) :
            base(arguments, altId, elementTypes, valueType, group)
        {
            Geo = geo;
        }
    }
}
