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
using VisualCard.Parsers;

namespace VisualCard.Parts
{
    public class Card : IEquatable<Card>
    {
        private readonly BaseVcardParser _parser;

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
        public NameInfo[] ContactNames { get; set; }
        /// <summary>
        /// The contact's full name
        /// </summary>
        public string ContactFullName { get; set; }
        /// <summary>
        /// The contact's telephones
        /// </summary>
        public TelephoneInfo[] ContactTelephones { get; set; }
        /// <summary>
        /// The contact's addresses
        /// </summary>
        public AddressInfo[] ContactAddresses { get; set; }
        /// <summary>
        /// The contact's e-mails
        /// </summary>
        public EmailInfo[] ContactMails { get; set; }
        /// <summary>
        /// The contact's organizations
        /// </summary>
        public OrganizationInfo[] ContactOrganizations { get; set; }
        /// <summary>
        /// The contact's titles
        /// </summary>
        public TitleInfo[] ContactTitles { get; set; }
        /// <summary>
        /// The contact's URL
        /// </summary>
        public string ContactURL { get; set; }
        /// <summary>
        /// The contact's photos
        /// </summary>
        public PhotoInfo[] ContactPhotos { get; set; }
        /// <summary>
        /// The contact's notes
        /// </summary>
        public string ContactNotes { get; set; }
        /// <summary>
        /// The contact's extended options (usually starts with X-SOMETHING:Value1;Value2...)
        /// </summary>
        public XNameInfo[] ContactXNames { get; set; }
        /// <summary>
        /// The card revision
        /// </summary>
        public DateTime? CardRevision { get; set; }
        /// <summary>
        /// The contact's nicknames
        /// </summary>
        public NicknameInfo[] ContactNicknames { get; set; }
        /// <summary>
        /// The contact's birthdate
        /// </summary>
        public DateTime? ContactBirthdate { get; set; }
        /// <summary>
        /// The contact's mailing software
        /// </summary>
        public string ContactMailer { get; set; }
        /// <summary>
        /// The contact's roles
        /// </summary>
        public RoleInfo[] ContactRoles { get; set; }
        /// <summary>
        /// The contact's categories
        /// </summary>
        public string[] ContactCategories { get; set; }
        /// <summary>
        /// The contact's logos
        /// </summary>
        public LogoInfo[] ContactLogos { get; set; }
        /// <summary>
        /// The contact's product ID
        /// </summary>
        public string ContactProdId { get; set; }
        /// <summary>
        /// The contact's sort string
        /// </summary>
        public string ContactSortString { get; set; }
        /// <summary>
        /// The contact's time zones
        /// </summary>
        public TimeZoneInfo[] ContactTimeZone { get; set; }
        /// <summary>
        /// The contact's geographical coordinates in (lat;long)
        /// </summary>
        public GeoInfo[] ContactGeo { get; set; }
        /// <summary>
        /// The contact's sounds
        /// </summary>
        public SoundInfo[] ContactSounds { get; set; }
        /// <summary>
        /// The contact's IMPP information
        /// </summary>
        public ImppInfo[] ContactImpps { get; set; }
        /// <summary>
        /// The contact's card source
        /// </summary>
        public string ContactSource { get; set; }
        /// <summary>
        /// The contact's XML code
        /// </summary>
        public string ContactXml { get; set; }
        /// <summary>
        /// The contact's free/busy indicator URL
        /// </summary>
        public string ContactFreeBusyUrl { get; set; }
        /// <summary>
        /// The contact's calendar URL
        /// </summary>
        public string ContactCalendarUrl { get; set; }
        /// <summary>
        /// The contact's calendar scheduling request URL
        /// </summary>
        public string ContactCalendarSchedulingRequestUrl { get; set; }

        internal BaseVcardParser Parser => _parser;

        /// <summary>
        /// Saves the contact file to the path
        /// </summary>
        /// <param name="path">Path to the VCard file that is going to be created</param>
        public void SaveTo(string path) =>
            Parser.SaveTo(path, this);

        /// <summary>
        /// Saves the contact to the returned string
        /// </summary>
        public string SaveToString() =>
            Parser.SaveToString(this);

        public override bool Equals(object obj) =>
            base.Equals(obj);

        /// <summary>
        /// Checks to see if both the cards are equal
        /// </summary>
        /// <param name="other">The target <see cref="Card"/> instance to check to see if they equal</param>
        /// <returns>True if all the card elements are equal. Otherwise, false.</returns>
        public bool Equals(Card other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the cards are equal
        /// </summary>
        /// <param name="source">The source <see cref="Card"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="Card"/> instance to check to see if they equal</param>
        /// <returns>True if all the card elements are equal. Otherwise, false.</returns>
        public bool Equals(Card source, Card target)
        {
            // We can't perform this operation on null.
            if (source is null)
                return false;

            // Check all the properties
            return
                source.ContactNames.SequenceEqual(target.ContactNames) &&
                source.ContactTelephones.SequenceEqual(target.ContactTelephones) &&
                source.ContactAddresses.SequenceEqual(target.ContactAddresses) &&
                source.ContactMails.SequenceEqual(target.ContactMails) &&
                source.ContactOrganizations.SequenceEqual(target.ContactOrganizations) &&
                source.ContactTitles.SequenceEqual(target.ContactTitles) &&
                source.ContactPhotos.SequenceEqual(target.ContactPhotos) &&
                source.ContactXNames.SequenceEqual(target.ContactXNames) &&
                source.ContactNicknames.SequenceEqual(target.ContactNicknames) &&
                source.ContactRoles.SequenceEqual(target.ContactRoles) &&
                source.ContactCategories.SequenceEqual(target.ContactCategories) &&
                source.ContactLogos.SequenceEqual(target.ContactLogos) &&
                source.ContactTimeZone.SequenceEqual(target.ContactTimeZone) &&
                source.ContactGeo.SequenceEqual(target.ContactGeo) &&
                source.ContactSounds.SequenceEqual(target.ContactSounds) &&
                source.ContactImpps.SequenceEqual(target.ContactImpps) &&
                source.ContactFullName == target.ContactFullName &&
                source.ContactURL == target.ContactURL &&
                source.ContactNotes == target.ContactNotes &&
                source.CardRevision == target.CardRevision &&
                source.ContactBirthdate == target.ContactBirthdate &&
                source.ContactMailer == target.ContactMailer &&
                source.ContactProdId == target.ContactProdId &&
                source.ContactSortString == target.ContactSortString
            ;
        }

        public override int GetHashCode()
        {
            int hashCode = 1867659713;
            hashCode = hashCode * -1521134295 + EqualityComparer<NameInfo[]>.Default.GetHashCode(ContactNames);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContactFullName);
            hashCode = hashCode * -1521134295 + EqualityComparer<TelephoneInfo[]>.Default.GetHashCode(ContactTelephones);
            hashCode = hashCode * -1521134295 + EqualityComparer<AddressInfo[]>.Default.GetHashCode(ContactAddresses);
            hashCode = hashCode * -1521134295 + EqualityComparer<EmailInfo[]>.Default.GetHashCode(ContactMails);
            hashCode = hashCode * -1521134295 + EqualityComparer<OrganizationInfo[]>.Default.GetHashCode(ContactOrganizations);
            hashCode = hashCode * -1521134295 + EqualityComparer<TitleInfo[]>.Default.GetHashCode(ContactTitles);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContactURL);
            hashCode = hashCode * -1521134295 + EqualityComparer<PhotoInfo[]>.Default.GetHashCode(ContactPhotos);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContactNotes);
            hashCode = hashCode * -1521134295 + EqualityComparer<XNameInfo[]>.Default.GetHashCode(ContactXNames);
            hashCode = hashCode * -1521134295 + CardRevision.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<NicknameInfo[]>.Default.GetHashCode(ContactNicknames);
            hashCode = hashCode * -1521134295 + ContactBirthdate.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContactMailer);
            hashCode = hashCode * -1521134295 + EqualityComparer<RoleInfo[]>.Default.GetHashCode(ContactRoles);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(ContactCategories);
            hashCode = hashCode * -1521134295 + EqualityComparer<LogoInfo[]>.Default.GetHashCode(ContactLogos);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContactProdId);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContactSortString);
            hashCode = hashCode * -1521134295 + EqualityComparer<TimeZoneInfo[]>.Default.GetHashCode(ContactTimeZone);
            hashCode = hashCode * -1521134295 + EqualityComparer<GeoInfo[]>.Default.GetHashCode(ContactGeo);
            hashCode = hashCode * -1521134295 + EqualityComparer<SoundInfo[]>.Default.GetHashCode(ContactSounds);
            hashCode = hashCode * -1521134295 + EqualityComparer<ImppInfo[]>.Default.GetHashCode(ContactImpps);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContactSource);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContactXml);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContactFreeBusyUrl);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContactCalendarUrl);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContactCalendarSchedulingRequestUrl);
            return hashCode;
        }

        public static bool operator ==(Card a, Card b)
            => a.Equals(b);

        public static bool operator !=(Card a, Card b)
            => !a.Equals(b);

        internal Card(BaseVcardParser parser, string cardVersion, NameInfo[] contactNames, string contactFullName, TelephoneInfo[] contactTelephones, AddressInfo[] contactAddresses, OrganizationInfo[] contactOrganizations, TitleInfo[] contactTitles, string contactURL, string contactNotes, EmailInfo[] contactMails, XNameInfo[] contactXNames, string cardKind, PhotoInfo[] contactPhotos, DateTime cardRevision, NicknameInfo[] contactNicknames, DateTime? contactBirthdate, string contactMailer, RoleInfo[] contactRoles, string[] contactCategories, LogoInfo[] contactLogos, string contactProdId, string contactSortString, TimeZoneInfo[] contactTimeZone, GeoInfo[] contactGeo, SoundInfo[] contactSounds, ImppInfo[] contactImpps, string contactSource, string contactXml, string contactFreeBusyUrl, string contactCalendarUrl, string contactCalendarSchedulingRequestUrl)
        {
            _parser = parser;
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
            ContactLogos = contactLogos;
            ContactProdId = contactProdId;
            ContactSortString = contactSortString;
            ContactTimeZone = contactTimeZone;
            ContactGeo = contactGeo;
            ContactSounds = contactSounds;
            ContactImpps = contactImpps;
            ContactSource = contactSource;
            ContactXml = contactXml;
            ContactFreeBusyUrl = contactFreeBusyUrl;
            ContactCalendarUrl = contactCalendarUrl;
            ContactCalendarSchedulingRequestUrl = contactCalendarSchedulingRequestUrl;
        }
    }
}
