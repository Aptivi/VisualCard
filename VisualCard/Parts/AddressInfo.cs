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
using System.Linq;
using System.Net;
using System.Xml.Linq;
using VisualCard.Parsers;

namespace VisualCard.Parts
{
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
                $"{VcardConstants._addressSpecifierWithType}" +
                $"TYPE={string.Join(",", AddressTypes)}{VcardConstants._argumentDelimiter}" +
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
                $"{VcardConstants._addressSpecifierWithType}" +
                $"TYPE={string.Join(",", AddressTypes)}{VcardConstants._argumentDelimiter}" +
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
                $"{VcardConstants._addressSpecifierWithType}" +
                $"{(installAltId ? "ALTID=" + AltId + VcardConstants._fieldDelimiter : "")}" +
                $"TYPE={string.Join(",", AddressTypes)}{VcardConstants._argumentDelimiter}" +
                $"{PostOfficeBox}{VcardConstants._fieldDelimiter}" +
                $"{ExtendedAddress}{VcardConstants._fieldDelimiter}" +
                $"{StreetAddress}{VcardConstants._fieldDelimiter}" +
                $"{Locality}{VcardConstants._fieldDelimiter}" +
                $"{Region}{VcardConstants._fieldDelimiter}" +
                $"{PostalCode}{VcardConstants._fieldDelimiter}" +
                $"{Country}";
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
