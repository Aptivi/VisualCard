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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VisualCard.Parsers;

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
        public string Geo { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion) =>
            new GeoInfo().FromStringVcardInternal(value, finalArgs, altId, elementTypes, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;
            if (altIdSupported)
            {
                bool installAltId = AltId >= 0 && Arguments.Length > 0;
                return
                    $"{VcardConstants._geoSpecifier}{(installAltId ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter)}" +
                    $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                    $"{(installAltId ? string.Join(VcardConstants._fieldDelimiter.ToString(), Arguments) + VcardConstants._argumentDelimiter : "")}" +
                    $"{Geo}";
            }
            else
            {
                return
                    $"{VcardConstants._geoSpecifier}:" +
                    $"{Geo}";
            }
        }

        internal override BaseCardPartInfo FromStringVcardInternal(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion)
        {
            // Get the value
            string geoValue = value.Substring(VcardConstants._geoSpecifier.Length + 1);
            string _geoStr = Regex.Unescape(geoValue);

            bool altIdSupported = cardVersion.Major >= 4;

            // Populate the fields
            GeoInfo _geo = new(altIdSupported ? altId : 0, finalArgs, elementTypes, valueType, _geoStr);
            return _geo;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            base.Equals(obj);

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
            if (source is null)
                return false;

            // Check all the properties
            return
                base.Equals(source, target) &&
                source.Geo == target.Geo
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -456581192;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Geo);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(GeoInfo left, GeoInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(GeoInfo left, GeoInfo right) =>
            !(left == right);

        internal GeoInfo() { }

        internal GeoInfo(int altId, string[] arguments, string[] elementTypes, string valueType, string geo) :
            base(arguments, altId, elementTypes, valueType)
        {
            Geo = geo;
        }
    }
}
