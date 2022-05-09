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

namespace VisualCard
{
    public class Card
    {
        /// <summary>
        /// The VCard version
        /// </summary>
        public string? CardVersion { get; }
        /// <summary>
        /// The contact's first name
        /// </summary>
        public string? ContactFirstName { get; set; }
        /// <summary>
        /// The contact's middle name
        /// </summary>
        public string? ContactMiddleName { get; set; }
        /// <summary>
        /// The contact's last name
        /// </summary>
        public string? ContactLastName { get; set; }
        /// <summary>
        /// The contact's name suffix
        /// </summary>
        public string? ContactNameSuffix { get; set; }
        /// <summary>
        /// The contact's full name
        /// </summary>
        public string? ContactFullName { get; }
        /// <summary>
        /// The contact's phone type
        /// </summary>
        public string? ContactPhoneType { get; set; }
        /// <summary>
        /// The contact's phone number
        /// </summary>
        public string? ContactPhoneNumber { get; set; }
        /// <summary>
        /// The contact's address
        /// </summary>
        public string? ContactAddress { get; set; }
        /// <summary>
        /// The contact's address type
        /// </summary>
        public string? ContactAddressType { get; set; }
        /// <summary>
        /// The contact's organization
        /// </summary>
        public string? ContactOrganization { get; set; }
        /// <summary>
        /// The contact's title
        /// </summary>
        public string? ContactTitle { get; set; }
        /// <summary>
        /// The contact's URL
        /// </summary>
        public string? ContactURL { get; set; }
        /// <summary>
        /// The contact's photo encoding
        /// </summary>
        public string? ContactPhotoEncoding { get; set; }
        /// <summary>
        /// The contact's photo
        /// </summary>
        public string? ContactPhoto { get; set; }
    }
}