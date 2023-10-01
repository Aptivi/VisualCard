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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VisualCard.Parsers;

namespace VisualCard.Parts
{
    /// <summary>
    /// Contact address information
    /// </summary>
    [DebuggerDisplay("Address = {PostOfficeBox}, {ExtendedAddress}, {StreetAddress}, {Locality}, {Region}, {PostalCode}, {Country}")]
    public class AddressInfo : IEquatable<AddressInfo>
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
        /// The contact's address types
        /// </summary>
        public string[] AddressTypes { get; }
        /// <summary>
        /// The contact's post office box
        /// </summary>
        public string PostOfficeBox { get; }
        /// <summary>
        /// The contact's extended address
        /// </summary>
        public string ExtendedAddress { get; }
        /// <summary>
        /// The contact's street address
        /// </summary>
        public string StreetAddress { get; }
        /// <summary>
        /// The contact's locality
        /// </summary>
        public string Locality { get; }
        /// <summary>
        /// The contact's region
        /// </summary>
        public string Region { get; }
        /// <summary>
        /// The contact's postal code
        /// </summary>
        public string PostalCode { get; }
        /// <summary>
        /// The contact's country
        /// </summary>
        public string Country { get; }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            base.Equals(obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="AddressInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(AddressInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="AddressInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="AddressInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(AddressInfo source, AddressInfo target)
        {
            // We can't perform this operation on null.
            if (source is null)
                return false;

            // Check all the properties
            return
                source.AddressTypes.SequenceEqual(target.AddressTypes) &&
                source.AltArguments.SequenceEqual(target.AltArguments) &&
                source.AltId == target.AltId &&
                source.PostOfficeBox == target.PostOfficeBox &&
                source.ExtendedAddress == target.ExtendedAddress &&
                source.StreetAddress == target.StreetAddress &&
                source.Locality == target.Locality &&
                source.Region == target.Region &&
                source.PostalCode == target.PostalCode &&
                source.Country == target.Country
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -1858114484;
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AltArguments);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AddressTypes);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PostOfficeBox);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ExtendedAddress);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(StreetAddress);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Locality);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Region);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PostalCode);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Country);
            return hashCode;
        }

        internal string ToStringVcardTwo()
        {
            return
                $"{VcardConstants._addressSpecifier};" +
                $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", AddressTypes)}{VcardConstants._argumentDelimiter}" +
                $"{PostOfficeBox}{VcardConstants._fieldDelimiter}" +
                $"{ExtendedAddress}{VcardConstants._fieldDelimiter}" +
                $"{StreetAddress}{VcardConstants._fieldDelimiter}" +
                $"{Locality}{VcardConstants._fieldDelimiter}" +
                $"{Region}{VcardConstants._fieldDelimiter}" +
                $"{PostalCode}{VcardConstants._fieldDelimiter}" +
                $"{Country}";
        }

        internal string ToStringVcardThree()
        {
            return
                $"{VcardConstants._addressSpecifier};" +
                $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", AddressTypes)}{VcardConstants._argumentDelimiter}" +
                $"{PostOfficeBox}{VcardConstants._fieldDelimiter}" +
                $"{ExtendedAddress}{VcardConstants._fieldDelimiter}" +
                $"{StreetAddress}{VcardConstants._fieldDelimiter}" +
                $"{Locality}{VcardConstants._fieldDelimiter}" +
                $"{Region}{VcardConstants._fieldDelimiter}" +
                $"{PostalCode}{VcardConstants._fieldDelimiter}" +
                $"{Country}";
        }

        internal string ToStringVcardFour()
        {
            bool installAltId = AltId >= 0 && AltArguments.Length > 0;
            return
                $"{VcardConstants._addressSpecifier};" +
                $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", AddressTypes)}{VcardConstants._argumentDelimiter}" +
                $"{PostOfficeBox}{VcardConstants._fieldDelimiter}" +
                $"{ExtendedAddress}{VcardConstants._fieldDelimiter}" +
                $"{StreetAddress}{VcardConstants._fieldDelimiter}" +
                $"{Locality}{VcardConstants._fieldDelimiter}" +
                $"{Region}{VcardConstants._fieldDelimiter}" +
                $"{PostalCode}{VcardConstants._fieldDelimiter}" +
                $"{Country}";
        }

        internal static AddressInfo FromStringVcardTwo(string value)
        {
            // Get the value
            string adrValue = value.Substring(VcardConstants._addressSpecifier.Length + 1);
            string[] splitAdr = adrValue.Split(VcardConstants._argumentDelimiter);

            // Check the provided address
            string[] splitAddressValues = splitAdr[0].Split(VcardConstants._fieldDelimiter);
            if (splitAddressValues.Length < 7)
                throw new InvalidDataException("Address information must specify exactly seven values (P.O. Box, extended address, street address, locality, region, postal code, and country)");

            // Populate the fields
            string[] _addressTypes = new string[] { "HOME" };
            string _addressPOBox = Regex.Unescape(splitAddressValues[0]);
            string _addressExtended = Regex.Unescape(splitAddressValues[1]);
            string _addressStreet = Regex.Unescape(splitAddressValues[2]);
            string _addressLocality = Regex.Unescape(splitAddressValues[3]);
            string _addressRegion = Regex.Unescape(splitAddressValues[4]);
            string _addressPostalCode = Regex.Unescape(splitAddressValues[5]);
            string _addressCountry = Regex.Unescape(splitAddressValues[6]);
            AddressInfo _address = new(0, Array.Empty<string>(), _addressTypes, _addressPOBox, _addressExtended, _addressStreet, _addressLocality, _addressRegion, _addressPostalCode, _addressCountry);
            return _address;
        }

        internal static AddressInfo FromStringVcardTwoWithType(string value)
        {
            // Get the value
            string adrValue = value.Substring(VcardConstants._addressSpecifier.Length + 1);
            string[] splitAdr = adrValue.Split(VcardConstants._argumentDelimiter);
            if (splitAdr.Length < 2)
                throw new InvalidDataException("Address field must specify exactly two values (Type (optionally prepended with TYPE=), and address information)");

            // Check the provided address
            string[] splitAddressValues = splitAdr[1].Split(VcardConstants._fieldDelimiter);
            if (splitAddressValues.Length < 7)
                throw new InvalidDataException("Address information must specify exactly seven values (P.O. Box, extended address, street address, locality, region, postal code, and country)");

            // Populate the fields
            string[] _addressTypes = VcardParserTools.GetTypes(splitAdr, "HOME", false);
            string _addressPOBox = Regex.Unescape(splitAddressValues[0]);
            string _addressExtended = Regex.Unescape(splitAddressValues[1]);
            string _addressStreet = Regex.Unescape(splitAddressValues[2]);
            string _addressLocality = Regex.Unescape(splitAddressValues[3]);
            string _addressRegion = Regex.Unescape(splitAddressValues[4]);
            string _addressPostalCode = Regex.Unescape(splitAddressValues[5]);
            string _addressCountry = Regex.Unescape(splitAddressValues[6]);
            AddressInfo _address = new(0, Array.Empty<string>(), _addressTypes, _addressPOBox, _addressExtended, _addressStreet, _addressLocality, _addressRegion, _addressPostalCode, _addressCountry);
            return _address;
        }

        internal static AddressInfo FromStringVcardThree(string value)
        {
            // Get the value
            string adrValue = value.Substring(VcardConstants._addressSpecifier.Length + 1);
            string[] splitAdr = adrValue.Split(VcardConstants._argumentDelimiter);

            // Check the provided address
            string[] splitAddressValues = splitAdr[0].Split(VcardConstants._fieldDelimiter);
            if (splitAddressValues.Length < 7)
                throw new InvalidDataException("Address information must specify exactly seven values (P.O. Box, extended address, street address, locality, region, postal code, and country)");

            // Populate the fields
            string[] _addressTypes = new string[] { "HOME" };
            string _addressPOBox = Regex.Unescape(splitAddressValues[0]);
            string _addressExtended = Regex.Unescape(splitAddressValues[1]);
            string _addressStreet = Regex.Unescape(splitAddressValues[2]);
            string _addressLocality = Regex.Unescape(splitAddressValues[3]);
            string _addressRegion = Regex.Unescape(splitAddressValues[4]);
            string _addressPostalCode = Regex.Unescape(splitAddressValues[5]);
            string _addressCountry = Regex.Unescape(splitAddressValues[6]);
            AddressInfo _address = new(0, Array.Empty<string>(), _addressTypes, _addressPOBox, _addressExtended, _addressStreet, _addressLocality, _addressRegion, _addressPostalCode, _addressCountry);
            return _address;
        }

        internal static AddressInfo FromStringVcardThreeWithType(string value)
        {
            // Get the value
            string adrValue = value.Substring(VcardConstants._addressSpecifier.Length + 1);
            string[] splitAdr = adrValue.Split(VcardConstants._argumentDelimiter);
            if (splitAdr.Length < 2)
                throw new InvalidDataException("Address field must specify exactly two values (Type (must be prepended with TYPE=), and address information)");

            // Check the provided address
            string[] splitAddressValues = splitAdr[1].Split(VcardConstants._fieldDelimiter);
            if (splitAddressValues.Length < 7)
                throw new InvalidDataException("Address information must specify exactly seven values (P.O. Box, extended address, street address, locality, region, postal code, and country)");

            // Populate the fields
            string[] _addressTypes = VcardParserTools.GetTypes(splitAdr, "HOME", true);
            string _addressPOBox = Regex.Unescape(splitAddressValues[0]);
            string _addressExtended = Regex.Unescape(splitAddressValues[1]);
            string _addressStreet = Regex.Unescape(splitAddressValues[2]);
            string _addressLocality = Regex.Unescape(splitAddressValues[3]);
            string _addressRegion = Regex.Unescape(splitAddressValues[4]);
            string _addressPostalCode = Regex.Unescape(splitAddressValues[5]);
            string _addressCountry = Regex.Unescape(splitAddressValues[6]);
            AddressInfo _address = new(0, Array.Empty<string>(), _addressTypes, _addressPOBox, _addressExtended, _addressStreet, _addressLocality, _addressRegion, _addressPostalCode, _addressCountry);
            return _address;
        }

        internal static AddressInfo FromStringVcardFour(string value, int altId)
        {
            // Get the value
            string adrValue = value.Substring(VcardConstants._addressSpecifier.Length + 1);
            string[] splitAdr = adrValue.Split(VcardConstants._argumentDelimiter);

            // Check the provided address
            string[] splitAddressValues = splitAdr[0].Split(VcardConstants._fieldDelimiter);
            if (splitAddressValues.Length < 7)
                throw new InvalidDataException("Address information must specify exactly seven values (P.O. Box, extended address, street address, locality, region, postal code, and country)");

            // Populate the fields
            string[] _addressTypes = new string[] { "HOME" };
            string _addressPOBox = Regex.Unescape(splitAddressValues[0]);
            string _addressExtended = Regex.Unescape(splitAddressValues[1]);
            string _addressStreet = Regex.Unescape(splitAddressValues[2]);
            string _addressLocality = Regex.Unescape(splitAddressValues[3]);
            string _addressRegion = Regex.Unescape(splitAddressValues[4]);
            string _addressPostalCode = Regex.Unescape(splitAddressValues[5]);
            string _addressCountry = Regex.Unescape(splitAddressValues[6]);
            AddressInfo _address = new(altId, Array.Empty<string>(), _addressTypes, _addressPOBox, _addressExtended, _addressStreet, _addressLocality, _addressRegion, _addressPostalCode, _addressCountry);
            return _address;
        }

        internal static AddressInfo FromStringVcardFourWithType(string value, List<string> finalArgs, int altId)
        {
            // Get the value
            string adrValue = value.Substring(VcardConstants._addressSpecifier.Length + 1);
            string[] splitAdr = adrValue.Split(VcardConstants._argumentDelimiter);
            if (splitAdr.Length < 2)
                throw new InvalidDataException("Address field must specify exactly two values (Type (must be prepended with TYPE=), and address information)");

            // Check the provided address
            string[] splitAddressValues = splitAdr[1].Split(VcardConstants._fieldDelimiter);
            if (splitAddressValues.Length < 7)
                throw new InvalidDataException("Address information must specify exactly seven values (P.O. Box, extended address, street address, locality, region, postal code, and country)");

            // Populate the fields
            string[] _addressTypes = VcardParserTools.GetTypes(splitAdr, "HOME", true);
            string _addressPOBox = Regex.Unescape(splitAddressValues[0]);
            string _addressExtended = Regex.Unescape(splitAddressValues[1]);
            string _addressStreet = Regex.Unescape(splitAddressValues[2]);
            string _addressLocality = Regex.Unescape(splitAddressValues[3]);
            string _addressRegion = Regex.Unescape(splitAddressValues[4]);
            string _addressPostalCode = Regex.Unescape(splitAddressValues[5]);
            string _addressCountry = Regex.Unescape(splitAddressValues[6]);
            AddressInfo _address = new(altId, finalArgs.ToArray(), _addressTypes, _addressPOBox, _addressExtended, _addressStreet, _addressLocality, _addressRegion, _addressPostalCode, _addressCountry);
            return _address;
        }

        internal AddressInfo() { }

        internal AddressInfo(int altId, string[] altArguments, string[] addressTypes, string postOfficeBox, string extendedAddress, string streetAddress, string locality, string region, string postalCode, string country)
        {
            AltId = altId;
            AltArguments = altArguments;
            AddressTypes = addressTypes;
            PostOfficeBox = postOfficeBox;
            ExtendedAddress = extendedAddress;
            StreetAddress = streetAddress;
            Locality = locality;
            Region = region;
            PostalCode = postalCode;
            Country = country;
        }
    }
}
