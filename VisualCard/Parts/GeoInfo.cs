/*
 * MIT License
 *
 * Copyright (c) 2021-2022 Aptivi
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VisualCard.Parsers;

namespace VisualCard.Parts
{
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
                $"{VcardConstants._geoSpecifier}" +
                $"{Geo}";
        }

        internal string ToStringVcardThree()
        {
            return
                $"{VcardConstants._geoSpecifier}" +
                $"{Geo}";
        }

        internal string ToStringVcardFour()
        {
            bool installAltId = AltId >= 0 && AltArguments.Length > 0;
            return
                $"{(installAltId ? VcardConstants._geoSpecifierWithType : VcardConstants._geoSpecifier)}" +
                $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                $"{(installAltId ? string.Join(VcardConstants._fieldDelimiter.ToString(), AltArguments) + VcardConstants._argumentDelimiter : "")}" +
                $"{Geo}";
        }

        internal static GeoInfo FromStringVcardTwo(string value)
        {
            string geoValue = value.Substring(VcardConstants._geoSpecifier.Length);
            string _geoStr = Regex.Unescape(geoValue);
            GeoInfo _geo = new(0, Array.Empty<string>(), Array.Empty<string>(), _geoStr);
            return _geo;
        }

        internal static GeoInfo FromStringVcardTwoWithType(string value)
        {
            string geoValue = value.Substring(VcardConstants._geoSpecifierWithType.Length);
            string[] splitGeo = geoValue.Split(VcardConstants._argumentDelimiter);
            if (splitGeo.Length < 2)
                throw new InvalidDataException("Geo field must specify exactly two values (VALUE=\"uri\", and geo info)");

            // Get the types and the number
            string[] _geoTypes = VcardParserTools.GetValues(splitGeo, "", VcardConstants._valueArgumentSpecifier);
            string _geoNumber = Regex.Unescape(splitGeo[1]);

            // Add the fetched information
            GeoInfo _geo = new(0, Array.Empty<string>(), _geoTypes, _geoNumber);
            return _geo;
        }

        internal static GeoInfo FromStringVcardThree(string value)
        {
            // Get the value
            string geoValue = value.Substring(VcardConstants._geoSpecifier.Length);

            // Populate the fields
            string[] _geoTypes = new string[] { "uri" };
            string _geoNumber = Regex.Unescape(geoValue);
            GeoInfo _geo = new(0, Array.Empty<string>(), _geoTypes, _geoNumber);
            return _geo;
        }

        internal static GeoInfo FromStringVcardThreeWithType(string value)
        {
            // Get the value
            string geoValue = value.Substring(VcardConstants._geoSpecifierWithType.Length);
            string[] splitGeo = geoValue.Split(VcardConstants._argumentDelimiter);
            if (splitGeo.Length < 2)
                throw new InvalidDataException("Geo field must specify exactly two values (VALUE=\"uri\", and geo info)");

            // Get the types and the number
            string[] _geoTypes = VcardParserTools.GetValues(splitGeo, "", VcardConstants._valueArgumentSpecifier);
            string _geoNumber = Regex.Unescape(splitGeo[1]);

            // Add the fetched information
            GeoInfo _geo = new(0, Array.Empty<string>(), _geoTypes, _geoNumber);
            return _geo;
        }

        internal static GeoInfo FromStringVcardFour(string value, int altId)
        {
            // Get the value
            string geoValue = value.Substring(VcardConstants._geoSpecifier.Length);

            // Populate the fields
            string[] _geoTypes = new string[] { "uri" };
            string _geoNumber = Regex.Unescape(geoValue);
            GeoInfo _geo = new(altId, Array.Empty<string>(), _geoTypes, _geoNumber);
            return _geo;
        }

        internal static GeoInfo FromStringVcardFourWithType(string value, List<string> finalArgs, int altId)
        {
            // Get the value
            string geoValue = value.Substring(VcardConstants._geoSpecifierWithType.Length);
            string[] splitGeo = geoValue.Split(VcardConstants._argumentDelimiter);
            if (splitGeo.Length < 2)
                throw new InvalidDataException("Geo field must specify exactly two values (VALUE=\"uri\", and geo info)");

            // Get the types and the number
            string[] _geoTypes = VcardParserTools.GetValues(splitGeo, "", VcardConstants._valueArgumentSpecifier);
            string _geoNumber = Regex.Unescape(splitGeo[1]);

            // Add the fetched information
            GeoInfo _geo = new(altId, finalArgs.ToArray(), _geoTypes, _geoNumber);
            return _geo;
        }

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
