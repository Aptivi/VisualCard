/*
 * MIT License
 *
 * Copyright (c) 2021-2022 EoflaOE and its companies
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

namespace VisualCard.Parts
{
    public class AddressInfo
    {
        /// <summary>
        /// The contact's address types
        /// </summary>
        public string[]? AddressTypes { get; }
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

        internal AddressInfo() { }

        internal AddressInfo(string[] addressTypes, string? postOfficeBox, string? extendedAddress, string? streetAddress, string? locality, string? region, string? postalCode, string? country)
        {
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
