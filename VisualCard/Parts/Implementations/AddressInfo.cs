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

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact address information
    /// </summary>
    [DebuggerDisplay("Address = {PostOfficeBox}, {ExtendedAddress}, {StreetAddress}, {Locality}, {Region}, {PostalCode}, {Country}")]
    public class AddressInfo : BaseCardPartInfo, IEquatable<AddressInfo>
    {
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

        internal static BaseCardPartInfo FromStringVcardStatic(string value, int altId, Version cardVersion) =>
            new AddressInfo().FromStringVcardInternal(value, altId, cardVersion);

        internal static BaseCardPartInfo FromStringVcardWithTypeStatic(string value, string[] finalArgs, int altId, Version cardVersion) =>
            new AddressInfo().FromStringVcardWithTypeInternal(value, finalArgs, altId, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;
            if (altIdSupported)
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
            else
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
        }

        internal override BaseCardPartInfo FromStringVcardInternal(string value, int altId, Version cardVersion)
        {
            // Get the value
            string adrValue = value.Substring(VcardConstants._addressSpecifier.Length + 1);
            string[] splitAdr = adrValue.Split(VcardConstants._argumentDelimiter);

            // Check the provided address
            string[] splitAddressValues = splitAdr[0].Split(VcardConstants._fieldDelimiter);
            if (splitAddressValues.Length < 7)
                throw new InvalidDataException("Address information must specify exactly seven values (P.O. Box, extended address, street address, locality, region, postal code, and country)");

            // Populate the fields
            return InstallInfo([], splitAddressValues, altId, cardVersion);
        }

        internal override BaseCardPartInfo FromStringVcardWithTypeInternal(string value, string[] finalArgs, int altId, Version cardVersion)
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
            return InstallInfo(splitAdr, splitAddressValues, finalArgs, altId, cardVersion);
        }

        private AddressInfo InstallInfo(string[] splitAdr, string[] splitAddressValues, int altId, Version cardVersion) =>
            InstallInfo(splitAdr, splitAddressValues, [], altId, cardVersion);

        private AddressInfo InstallInfo(string[] splitAdr, string[] splitAddressValues, string[] finalArgs, int altId, Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;
            bool specifierRequired = cardVersion.Major >= 3;

            // Populate the fields
            string[] _addressTypes = splitAdr.Length == 0 ? ["HOME"] : VcardParserTools.GetTypes(splitAdr, "HOME", specifierRequired);
            string _addressPOBox = Regex.Unescape(splitAddressValues[0]);
            string _addressExtended = Regex.Unescape(splitAddressValues[1]);
            string _addressStreet = Regex.Unescape(splitAddressValues[2]);
            string _addressLocality = Regex.Unescape(splitAddressValues[3]);
            string _addressRegion = Regex.Unescape(splitAddressValues[4]);
            string _addressPostalCode = Regex.Unescape(splitAddressValues[5]);
            string _addressCountry = Regex.Unescape(splitAddressValues[6]);
            AddressInfo _address = new(altIdSupported ? altId : 0, altIdSupported ? finalArgs : [], _addressTypes, _addressPOBox, _addressExtended, _addressStreet, _addressLocality, _addressRegion, _addressPostalCode, _addressCountry);
            return _address;
        }

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

        /// <inheritdoc/>
        public static bool operator ==(AddressInfo left, AddressInfo right) =>
            EqualityComparer<AddressInfo>.Default.Equals(left, right);

        /// <inheritdoc/>
        public static bool operator !=(AddressInfo left, AddressInfo right) =>
            !(left == right);

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
