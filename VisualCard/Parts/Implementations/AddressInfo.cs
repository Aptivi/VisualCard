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
using System.Text.RegularExpressions;
using VisualCard.Parsers;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact address information
    /// </summary>
    [DebuggerDisplay("Address = {PostOfficeBox}, {ExtendedAddress}, {StreetAddress}, {Locality}, {Region}, {PostalCode}, {Country}")]
    public class AddressInfo : BaseCardPartInfo, IEquatable<AddressInfo>
    {
        /// <summary>
        /// The contact's post office box
        /// </summary>
        public string? PostOfficeBox { get; }
        /// <summary>
        /// The contact's extended address
        /// </summary>
        public string? ExtendedAddress { get; }
        /// <summary>
        /// The contact's street address
        /// </summary>
        public string? StreetAddress { get; }
        /// <summary>
        /// The contact's locality
        /// </summary>
        public string? Locality { get; }
        /// <summary>
        /// The contact's region
        /// </summary>
        public string? Region { get; }
        /// <summary>
        /// The contact's postal code
        /// </summary>
        public string? PostalCode { get; }
        /// <summary>
        /// The contact's country
        /// </summary>
        public string? Country { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion) =>
            new AddressInfo().FromStringVcardInternal(value, finalArgs, altId, elementTypes, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion) =>
            $"{PostOfficeBox}{VcardConstants._fieldDelimiter}" +
            $"{ExtendedAddress}{VcardConstants._fieldDelimiter}" +
            $"{StreetAddress}{VcardConstants._fieldDelimiter}" +
            $"{Locality}{VcardConstants._fieldDelimiter}" +
            $"{Region}{VcardConstants._fieldDelimiter}" +
            $"{PostalCode}{VcardConstants._fieldDelimiter}" +
            $"{Country}";

        internal override BaseCardPartInfo FromStringVcardInternal(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion)
        {
            // Get the value
            string[] splitAdr = value.Split(VcardConstants._fieldDelimiter);

            // Check the provided address
            if (splitAdr.Length < 7)
                throw new InvalidDataException("Address information must specify exactly seven values (P.O. Box, extended address, street address, locality, region, postal code, and country)");

            // Populate the fields
            string[] _addressTypes = elementTypes.Length >= 1 ? elementTypes : ["HOME"];
            string _addressPOBox = Regex.Unescape(splitAdr[0]);
            string _addressExtended = Regex.Unescape(splitAdr[1]);
            string _addressStreet = Regex.Unescape(splitAdr[2]);
            string _addressLocality = Regex.Unescape(splitAdr[3]);
            string _addressRegion = Regex.Unescape(splitAdr[4]);
            string _addressPostalCode = Regex.Unescape(splitAdr[5]);
            string _addressCountry = Regex.Unescape(splitAdr[6]);
            AddressInfo _address = new(altId, finalArgs, _addressTypes, valueType, _addressPOBox, _addressExtended, _addressStreet, _addressLocality, _addressRegion, _addressPostalCode, _addressCountry);
            return _address;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((AddressInfo)obj);

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
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
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
            int hashCode = -427937047;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(PostOfficeBox);
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(ExtendedAddress);
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(StreetAddress);
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(Locality);
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(Region);
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(PostalCode);
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(Country);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(AddressInfo left, AddressInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(AddressInfo left, AddressInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCardPartInfo source, BaseCardPartInfo target) =>
            ((AddressInfo)source) == ((AddressInfo)target);

        internal AddressInfo() :
            base()
        { }

        internal AddressInfo(int altId, string[] arguments, string[] elementTypes, string valueType, string postOfficeBox, string extendedAddress, string streetAddress, string locality, string region, string postalCode, string country) :
            base(arguments, altId, elementTypes, valueType)
        {
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
