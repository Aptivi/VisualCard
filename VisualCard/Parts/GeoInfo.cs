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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VisualCard.Parsers;

namespace VisualCard.Parts
{
    /// <summary>
    /// Contact geographical information
    /// </summary>
    [DebuggerDisplay("Geography = {Geo}")]
    public class GeoInfo : IEquatable<GeoInfo>
    {
        /// <summary>
        /// Alternative ID. Zero if unspecified.
        /// </summary>
        public int AltId { get; }
        /// <summary>
        /// Arguments that follow the AltId
        /// </summary>
        public string[] AltArguments { get; }
        /// <summary>
        /// The contact's geographical information types
        /// </summary>
        public string[] GeoTypes { get; }
        /// <summary>
        /// The contact's geographical information
        /// </summary>
        public string Geo { get; }

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
                source.AltArguments.SequenceEqual(target.AltArguments) &&
                source.GeoTypes.SequenceEqual(target.GeoTypes) &&
                source.AltId == target.AltId &&
                source.Geo == target.Geo
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -772623698;
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AltArguments);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(GeoTypes);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Geo);
            return hashCode;
        }

        internal string ToStringVcardTwo()
        {
            return
                $"{VcardConstants._geoSpecifier}:" +
                $"{Geo}";
        }

        internal string ToStringVcardThree()
        {
            return
                $"{VcardConstants._geoSpecifier}:" +
                $"{Geo}";
        }

        internal string ToStringVcardFour()
        {
            bool installAltId = AltId >= 0 && AltArguments.Length > 0;
            return
                $"{VcardConstants._geoSpecifier}{(installAltId ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter)}" +
                $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                $"{(installAltId ? string.Join(VcardConstants._fieldDelimiter.ToString(), AltArguments) + VcardConstants._argumentDelimiter : "")}" +
                $"{Geo}";
        }

        internal string ToStringVcardFive() =>
            ToStringVcardFour();

        internal static GeoInfo FromStringVcardTwo(string value)
        {
            string geoValue = value.Substring(VcardConstants._geoSpecifier.Length + 1);
            string _geoStr = Regex.Unescape(geoValue);
            GeoInfo _geo = new(0, [], [], _geoStr);
            return _geo;
        }

        internal static GeoInfo FromStringVcardTwoWithType(string value)
        {
            string geoValue = value.Substring(VcardConstants._geoSpecifier.Length + 1);
            string[] splitGeo = geoValue.Split(VcardConstants._argumentDelimiter);
            if (splitGeo.Length < 2)
                throw new InvalidDataException("Geo field must specify exactly two values (VALUE=\"uri\", and geo info)");

            // Get the types and the number
            string[] _geoTypes = VcardParserTools.GetValues(splitGeo, "", VcardConstants._valueArgumentSpecifier);
            string _geoNumber = Regex.Unescape(splitGeo[1]);

            // Add the fetched information
            GeoInfo _geo = new(0, [], _geoTypes, _geoNumber);
            return _geo;
        }

        internal static GeoInfo FromStringVcardThree(string value)
        {
            // Get the value
            string geoValue = value.Substring(VcardConstants._geoSpecifier.Length + 1);

            // Populate the fields
            string[] _geoTypes = ["uri"];
            string _geoNumber = Regex.Unescape(geoValue);
            GeoInfo _geo = new(0, [], _geoTypes, _geoNumber);
            return _geo;
        }

        internal static GeoInfo FromStringVcardThreeWithType(string value)
        {
            // Get the value
            string geoValue = value.Substring(VcardConstants._geoSpecifier.Length + 1);
            string[] splitGeo = geoValue.Split(VcardConstants._argumentDelimiter);
            if (splitGeo.Length < 2)
                throw new InvalidDataException("Geo field must specify exactly two values (VALUE=\"uri\", and geo info)");

            // Get the types and the number
            string[] _geoTypes = VcardParserTools.GetValues(splitGeo, "", VcardConstants._valueArgumentSpecifier);
            string _geoNumber = Regex.Unescape(splitGeo[1]);

            // Add the fetched information
            GeoInfo _geo = new(0, [], _geoTypes, _geoNumber);
            return _geo;
        }

        internal static GeoInfo FromStringVcardFour(string value, int altId)
        {
            // Get the value
            string geoValue = value.Substring(VcardConstants._geoSpecifier.Length + 1);

            // Populate the fields
            string[] _geoTypes = ["uri"];
            string _geoNumber = Regex.Unescape(geoValue);
            GeoInfo _geo = new(altId, [], _geoTypes, _geoNumber);
            return _geo;
        }

        internal static GeoInfo FromStringVcardFourWithType(string value, List<string> finalArgs, int altId)
        {
            // Get the value
            string geoValue = value.Substring(VcardConstants._geoSpecifier.Length + 1);
            string[] splitGeo = geoValue.Split(VcardConstants._argumentDelimiter);
            if (splitGeo.Length < 2)
                throw new InvalidDataException("Geo field must specify exactly two values (VALUE=\"uri\", and geo info)");

            // Get the types and the number
            string[] _geoTypes = VcardParserTools.GetValues(splitGeo, "", VcardConstants._valueArgumentSpecifier);
            string _geoNumber = Regex.Unescape(splitGeo[1]);

            // Add the fetched information
            GeoInfo _geo = new(altId, [.. finalArgs], _geoTypes, _geoNumber);
            return _geo;
        }

        internal static GeoInfo FromStringVcardFive(string value, int altId) =>
            FromStringVcardFour(value, altId);

        internal static GeoInfo FromStringVcardFiveWithType(string value, List<string> finalArgs, int altId) =>
            FromStringVcardFourWithType(value, finalArgs, altId);

        internal GeoInfo() { }

        internal GeoInfo(int altId, string[] altArguments, string[] geoTypes, string geo)
        {
            AltId = altId;
            AltArguments = altArguments;
            GeoTypes = geoTypes;
            Geo = geo;
        }
    }
}
