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

namespace VisualCard.Parts
{
    public class Card
    {
        /// <summary>
        /// The VCard version
        /// </summary>
        public string CardVersion { get; }
        /// <summary>
        /// The VCard kind (individual is the default)
        /// </summary>
        public string CardKind { get; }
        /// <summary>
        /// The contact's names
        /// </summary>
        public NameInfo[] ContactNames { get; }
        /// <summary>
        /// The contact's full name
        /// </summary>
        public string ContactFullName { get; }
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
        /// The contact's titles
        /// </summary>
        public TitleInfo[] ContactTitles { get; }
        /// <summary>
        /// The contact's URL
        /// </summary>
        public string ContactURL { get; }
        /// <summary>
        /// The contact's photos
        /// </summary>
        public PhotoInfo[] ContactPhotos { get; }
        /// <summary>
        /// The contact's notes
        /// </summary>
        public string ContactNotes { get; }
        /// <summary>
        /// The contact's extended options (usually starts with X-SOMETHING:Value1;Value2...)
        /// </summary>
        public XNameInfo[] ContactXNames { get; }
        /// <summary>
        /// The card revision
        /// </summary>
        public DateTime? CardRevision { get; }
        /// <summary>
        /// The contact's nicknames
        /// </summary>
        public NicknameInfo[] ContactNicknames { get; }
        /// <summary>
        /// The contact's birthdate
        /// </summary>
        public DateTime? ContactBirthdate { get; }
        /// <summary>
        /// The contact's mailing software
        /// </summary>
        public string ContactMailer { get; }
        /// <summary>
        /// The contact's roles
        /// </summary>
        public RoleInfo[] ContactRoles { get; }
        /// <summary>
        /// The contact's categories
        /// </summary>
        public string[] ContactCategories { get; }

        internal Card(string cardVersion, NameInfo[] contactNames, string contactFullName, TelephoneInfo[] contactTelephones, AddressInfo[] contactAddresses, OrganizationInfo[] contactOrganizations, TitleInfo[] contactTitles, string contactURL, string contactNotes, EmailInfo[] contactMails, XNameInfo[] contactXNames, string cardKind, PhotoInfo[] contactPhotos, DateTime cardRevision, NicknameInfo[] contactNicknames, DateTime? contactBirthdate, string contactMailer, RoleInfo[] contactRoles, string[] contactCategories)
        {
            CardVersion = cardVersion;
            ContactNames = contactNames;
            ContactFullName = contactFullName;
            ContactTelephones = contactTelephones;
            ContactAddresses = contactAddresses;
            ContactOrganizations = contactOrganizations;
            ContactTitles = contactTitles;
            ContactURL = contactURL;
            ContactNotes = contactNotes;
            ContactMails = contactMails;
            ContactXNames = contactXNames;
            CardKind = cardKind;
            ContactPhotos = contactPhotos;
            CardRevision = cardRevision;
            ContactNicknames = contactNicknames;
            ContactBirthdate = contactBirthdate;
            ContactMailer = contactMailer;
            ContactRoles = contactRoles;
            ContactCategories = contactCategories;
        }
    }
}