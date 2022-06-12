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
    public class Card
    {
        /// <summary>
        /// The VCard version
        /// </summary>
        public string? CardVersion { get; }
        /// <summary>
        /// The contact's first name
        /// </summary>
        public string? ContactFirstName { get; }
        /// <summary>
        /// The contact's last name
        /// </summary>
        public string? ContactLastName { get; }
        /// <summary>
        /// The contact's full name
        /// </summary>
        public string? ContactFullName { get; }
        /// <summary>
        /// The contact's telephones
        /// </summary>
        public TelephoneInfo[] ContactTelephones { get; }
        /// <summary>
        /// The contact's addresses
        /// </summary>
        public AddressInfo[] ContactAddresses { get; }
        /// <summary>
        /// The contact's e-mails
        /// </summary>
        public EmailInfo[] ContactMails { get; }
        /// <summary>
        /// The contact's organizations
        /// </summary>
        public OrganizationInfo[] ContactOrganizations { get; }
        /// <summary>
        /// The contact's title
        /// </summary>
        public string? ContactTitle { get; }
        /// <summary>
        /// The contact's URL
        /// </summary>
        public string? ContactURL { get; }
        /// <summary>
        /// The contact's notes
        /// </summary>
        public string? ContactNotes { get; }
        /// <summary>
        /// The contact's extended options (usually starts with X-SOMETHING:Value1;Value2...)
        /// </summary>
        public XNameInfo[] ContactXNames { get; }

        internal Card(string? cardVersion, string? contactFirstName, string? contactLastName, string? contactFullName, TelephoneInfo[] contactTelephones, AddressInfo[] contactAddresses, OrganizationInfo[] contactOrganizations, string? contactTitle, string? contactURL, string? contactNotes, EmailInfo[] contactMails, XNameInfo[] contactXNames)
        {
            CardVersion = cardVersion;
            ContactFirstName = contactFirstName;
            ContactLastName = contactLastName;
            ContactFullName = contactFullName;
            ContactTelephones = contactTelephones;
            ContactAddresses = contactAddresses;
            ContactOrganizations = contactOrganizations;
            ContactTitle = contactTitle;
            ContactURL = contactURL;
            ContactNotes = contactNotes;
            ContactMails = contactMails;
            ContactXNames = contactXNames;
        }
    }
}