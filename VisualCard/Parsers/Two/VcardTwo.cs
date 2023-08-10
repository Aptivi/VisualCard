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
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using VisualCard.Exceptions;
using VisualCard.Parts;
using TimeZoneInfo = VisualCard.Parts.TimeZoneInfo;

namespace VisualCard.Parsers.Two
{
    /// <summary>
    /// Parser for VCard version 2.1. Consult the vcard-21.txt file in source for the specification.
    /// </summary>
    public class VcardTwo : BaseVcardParser, IVcardParser
    {
        public override string CardContent { get; }
        public override string CardVersion { get; }

        public override Card Parse()
        {
            // Check the version to ensure that we're really dealing with VCard 2.1 contact
            if (CardVersion != "2.1")
                throw new InvalidDataException($"Card version {CardVersion} doesn't match expected \"2.1\".");

            // Check the content to ensure that we really have data
            if (string.IsNullOrEmpty(CardContent))
                throw new InvalidDataException($"Card content is empty.");

            // Now, make a stream out of card content
            byte[] CardContentData = Encoding.Default.GetBytes(CardContent);
            MemoryStream CardContentStream = new(CardContentData, false);
            StreamReader CardContentReader = new(CardContentStream);

            // Some variables to assign to the Card() ctor
            string _fullName = "";
            string _url = "";
            string _note = "";
            string _mailer = "";
            string _source = "";
            DateTime _rev = DateTime.MinValue;
            DateTime _bday = DateTime.MinValue;
            List<TelephoneInfo> _telephones = new();
            List<NameInfo> _names = new();
            List<EmailInfo> _emails = new();
            List<AddressInfo> _addresses = new();
            List<OrganizationInfo> _orgs = new();
            List<TitleInfo> _titles = new();
            List<PhotoInfo> _photos = new();
            List<LogoInfo> _logos = new();
            List<SoundInfo> _sounds = new();
            List<RoleInfo> _roles = new();
            List<TimeZoneInfo> _timezones = new();
            List<GeoInfo> _geos = new();
            List<ImppInfo> _impps = new();
            List<XNameInfo> _xes = new();

            // Name specifier is required
            bool nameSpecifierSpotted = false;

            // Iterate through all the lines
            int lineNumber = 0;
            while (!CardContentReader.EndOfStream)
            {
                // Get line
                string _value = CardContentReader.ReadLine();
                lineNumber += 1;

                try
                {
                    // The name (N:Sanders;John;;;)
                    if (_value.StartsWith(VcardConstants._nameSpecifier))
                    {
                        // Get the name
                        _names.Add(NameInfo.FromStringVcardTwo(_value));

                        // Set flag to indicate that the required field is spotted
                        nameSpecifierSpotted = true;
                    }

                    // Full name (FN:John Sanders)
                    if (_value.StartsWith(VcardConstants._fullNameSpecifier))
                    {
                        // Get the value
                        string fullNameValue = _value.Substring(VcardConstants._fullNameSpecifier.Length);

                        // Populate field
                        _fullName = Regex.Unescape(fullNameValue);
                    }

                    // Telephone (TEL;CELL;HOME:495-522-3560 or TEL;TYPE=cell,home:495-522-3560)
                    if (_value.StartsWith(VcardConstants._telephoneSpecifierWithType))
                        _telephones.Add(TelephoneInfo.FromStringVcardTwoWithType(_value));

                    // Telephone (TEL:495-522-3560)
                    if (_value.StartsWith(VcardConstants._telephoneSpecifier))
                        _telephones.Add(TelephoneInfo.FromStringVcardTwo(_value));

                    // Address (ADR;HOME:;;Los Angeles, USA;;;;)
                    if (_value.StartsWith(VcardConstants._addressSpecifierWithType))
                        _addresses.Add(AddressInfo.FromStringVcardTwoWithType(_value));

                    // Email (EMAIL;HOME;INTERNET:john.s@acme.co or EMAIL;TYPE=HOME,INTERNET:john.s@acme.co)
                    if (_value.StartsWith(VcardConstants._emailSpecifier))
                        _emails.Add(EmailInfo.FromStringVcardTwoWithType(_value));

                    // Organization (ORG:Acme Co. or ORG:ABC, Inc.;North American Division;Marketing)
                    if (_value.StartsWith(VcardConstants._orgSpecifier))
                        _orgs.Add(OrganizationInfo.FromStringVcardTwo(_value));

                    // Organization (ORG;TYPE=WORK:Acme Co. or ORG:ABC, Inc.;North American Division;Marketing)
                    if (_value.StartsWith(VcardConstants._orgSpecifierWithType))
                        _orgs.Add(OrganizationInfo.FromStringVcardTwoWithType(_value));

                    // Title (TITLE:Product Manager)
                    if (_value.StartsWith(VcardConstants._titleSpecifier))
                        _titles.Add(TitleInfo.FromStringVcardTwo(_value));

                    // Website link (URL:https://sso.org/)
                    if (_value.StartsWith(VcardConstants._urlSpecifier))
                    {
                        // Get the value
                        string urlValue = _value.Substring(VcardConstants._urlSpecifier.Length);

                        // Try to parse the URL to ensure that it conforms to IETF RFC 1738: Uniform Resource Locators
                        if (!Uri.TryCreate(urlValue, UriKind.Absolute, out Uri uri))
                            throw new InvalidDataException($"URL {urlValue} is invalid");

                        // Populate field
                        _url = uri.ToString();
                    }

                    // Note (NOTE:Product Manager)
                    if (_value.StartsWith(VcardConstants._noteSpecifier))
                    {
                        // Get the value
                        string noteValue = _value.Substring(VcardConstants._noteSpecifier.Length);

                        // Populate field
                        _note = Regex.Unescape(noteValue);
                    }

                    // Photo (PHOTO;ENCODING=BASE64;JPEG:... or PHOTO;VALUE=URL:file:///jqpublic.gif or PHOTO;ENCODING=BASE64;TYPE=GIF:...)
                    if (_value.StartsWith(VcardConstants._photoSpecifierWithType))
                        _photos.Add(PhotoInfo.FromStringVcardTwoWithType(_value, CardContentReader));

                    // Logo (LOGO;ENCODING=BASE64;JPEG:... or LOGO;VALUE=URL:file:///jqpublic.gif or LOGO;ENCODING=BASE64;TYPE=GIF:...)
                    if (_value.StartsWith(VcardConstants._logoSpecifierWithType))
                        _logos.Add(LogoInfo.FromStringVcardTwoWithType(_value, CardContentReader));

                    // Sound (SOUND;VALUE=URL:file///multimed/audio/jqpublic.wav or SOUND;WAVE;BASE64:... or SOUND;TYPE=WAVE;ENCODING=BASE64:...)
                    if (_value.StartsWith(VcardConstants._soundSpecifierWithType))
                        _sounds.Add(SoundInfo.FromStringVcardTwoWithType(_value, CardContentReader));

                    // Revision (REV:1995-10-31T22:27:10Z or REV:19951031T222710)
                    if (_value.StartsWith(VcardConstants._revSpecifier))
                    {
                        string revValue = _value.Substring(VcardConstants._revSpecifier.Length);
                        _rev = DateTime.Parse(revValue);
                    }

                    // Birthdate (BDAY:19950415 or BDAY:1953-10-15T23:10:00Z)
                    if (_value.StartsWith(VcardConstants._birthSpecifier))
                    {
                        string bdayValue = _value.Substring(VcardConstants._birthSpecifier.Length);
                        _bday = DateTime.Parse(bdayValue);
                    }

                    // Mailer (MAILER:ccMail 2.2 or MAILER:PigeonMail 2.1)
                    if (_value.StartsWith(VcardConstants._mailerSpecifier))
                    {
                        string mailerValue = _value.Substring(VcardConstants._mailerSpecifier.Length);
                        _mailer = Regex.Unescape(mailerValue);
                    }

                    // Role (ROLE:Programmer)
                    if (_value.StartsWith(VcardConstants._roleSpecifier))
                        _roles.Add(RoleInfo.FromStringVcardTwo(_value));

                    // Time Zone (TZ;VALUE=text:-05:00; EST; Raleigh/North America)
                    if (_value.StartsWith(VcardConstants._timeZoneSpecifierWithType))
                        _timezones.Add(TimeZoneInfo.FromStringVcardTwoWithType(_value));

                    // Time Zone (TZ:-05:00)
                    if (_value.StartsWith(VcardConstants._timeZoneSpecifier))
                        _timezones.Add(TimeZoneInfo.FromStringVcardTwo(_value));

                    // Geo (GEO;VALUE=uri:https://...)
                    if (_value.StartsWith(VcardConstants._geoSpecifierWithType))
                        _geos.Add(GeoInfo.FromStringVcardTwoWithType(_value));

                    // Geo (GEO:geo:37.386013,-122.082932)
                    if (_value.StartsWith(VcardConstants._geoSpecifier))
                        _geos.Add(GeoInfo.FromStringVcardTwo(_value));

                    // IMPP information (IMPP;TYPE=home:sip:test)
                    if (_value.StartsWith(VcardConstants._imppSpecifierWithType))
                        _impps.Add(ImppInfo.FromStringVcardTwoWithType(_value));

                    // IMPP information (IMPP:sip:test)
                    if (_value.StartsWith(VcardConstants._imppSpecifier))
                        _impps.Add(ImppInfo.FromStringVcardTwo(_value));

                    // Source (SOURCE:http://johndoe.com/vcard.vcf)
                    if (_value.StartsWith(VcardConstants._sourceSpecifier))
                    {
                        // Get the value
                        string sourceStringValue = _value.Substring(VcardConstants._sourceSpecifier.Length);

                        // Try to parse the URL to ensure that it conforms to IETF RFC 1738: Uniform Resource Locators
                        if (!Uri.TryCreate(sourceStringValue, UriKind.Absolute, out Uri uri))
                            throw new InvalidDataException($"URL {sourceStringValue} is invalid");

                        // Populate field
                        _source = uri.ToString();
                    }

                    // X-nonstandard (X-AIM:john.s or X-DL;Design Work Group:List Item 1;List Item 2;List Item 3)
                    if (_value.StartsWith(VcardConstants._xSpecifier))
                        _xes.Add(XNameInfo.FromStringVcardTwo(_value));
                }
                catch (Exception ex)
                {
                    throw new VCardParseException(ex.Message, _value, lineNumber, ex);
                }
            }

            // Requirement checks
            if (!nameSpecifierSpotted)
                throw new InvalidDataException("The name specifier, \"N:\", is required.");

            // Make a new instance of the card
            return new Card(this, CardVersion, "individual")
            {
                CardRevision = _rev,
                ContactNames = _names.ToArray(),
                ContactFullName = _fullName,
                ContactTelephones = _telephones.ToArray(),
                ContactAddresses = _addresses.ToArray(),
                ContactOrganizations = _orgs.ToArray(),
                ContactTitles = _titles.ToArray(),
                ContactURL = _url,
                ContactNotes = _note,
                ContactMails = _emails.ToArray(),
                ContactXNames = _xes.ToArray(),
                ContactPhotos = _photos.ToArray(),
                ContactNicknames = Array.Empty<NicknameInfo>(),
                ContactBirthdate = _bday,
                ContactMailer = _mailer,
                ContactRoles = _roles.ToArray(),
                ContactCategories = Array.Empty<string>(),
                ContactLogos = _logos.ToArray(),
                ContactProdId = "",
                ContactSortString = "",
                ContactTimeZone = _timezones.ToArray(),
                ContactGeo = _geos.ToArray(),
                ContactSounds = _sounds.ToArray(),
                ContactImpps = _impps.ToArray(),
                ContactSource = _source,
                ContactXml = "",
                ContactFreeBusyUrl = "",
                ContactCalendarUrl = "",
                ContactCalendarSchedulingRequestUrl = ""
            };
        }

        internal override string SaveToString(Card card)
        {
            // Check the version to ensure that we're really dealing with VCard 2.1 contact
            if (CardVersion != "2.1")
                throw new InvalidDataException($"Card version {CardVersion} doesn't match expected \"2.1\".");

            // Check the content to ensure that we really have data
            if (string.IsNullOrEmpty(CardContent))
                throw new InvalidDataException($"Card content is empty.");

            // Initialize the card builder
            var cardBuilder = new StringBuilder();

            // First, write the header
            cardBuilder.AppendLine("BEGIN:VCARD");
            cardBuilder.AppendLine($"VERSION:{CardVersion}");

            // Then, write the full name and the name
            if (!string.IsNullOrWhiteSpace(card.ContactFullName))
                cardBuilder.AppendLine($"{VcardConstants._fullNameSpecifier}{card.ContactFullName}");
            foreach (NameInfo name in card.ContactNames)
                cardBuilder.AppendLine(name.ToStringVcardTwo());

            // Now, start filling in the rest...
            foreach (TelephoneInfo telephone in card.ContactTelephones)
                cardBuilder.AppendLine(telephone.ToStringVcardTwo());
            foreach (AddressInfo address in card.ContactAddresses)
                cardBuilder.AppendLine(address.ToStringVcardTwo());
            foreach (EmailInfo email in card.ContactMails)
                cardBuilder.AppendLine(email.ToStringVcardTwo());
            foreach (OrganizationInfo organization in card.ContactOrganizations)
                cardBuilder.AppendLine(organization.ToStringVcardTwo());
            foreach (TitleInfo title in card.ContactTitles)
                cardBuilder.AppendLine(title.ToStringVcardTwo());
            if (!string.IsNullOrWhiteSpace(card.ContactURL))
                cardBuilder.AppendLine($"{VcardConstants._urlSpecifier}{card.ContactURL}");
            if (!string.IsNullOrWhiteSpace(card.ContactNotes))
                cardBuilder.AppendLine($"{VcardConstants._noteSpecifier}{card.ContactNotes}");
            foreach (PhotoInfo photo in card.ContactPhotos)
                cardBuilder.AppendLine(photo.ToStringVcardTwo());
            foreach (LogoInfo logo in card.ContactLogos)
                cardBuilder.AppendLine(logo.ToStringVcardTwo());
            foreach (SoundInfo sound in card.ContactSounds)
                cardBuilder.AppendLine(sound.ToStringVcardTwo());
            if (card.CardRevision is not null && card.CardRevision != DateTime.MinValue)
                cardBuilder.AppendLine($"{VcardConstants._revSpecifier}{card.CardRevision:dd-MM-yyyy_HH-mm-ss}");
            if (card.ContactBirthdate is not null && card.ContactBirthdate != DateTime.MinValue)
                cardBuilder.AppendLine($"{VcardConstants._birthSpecifier}{card.ContactBirthdate:dd-MM-yyyy}");
            if (!string.IsNullOrWhiteSpace(card.ContactMailer))
                cardBuilder.AppendLine($"{VcardConstants._mailerSpecifier}{card.ContactMailer}");
            foreach (RoleInfo role in card.ContactRoles)
                cardBuilder.AppendLine(role.ToStringVcardTwo());
            foreach (TimeZoneInfo timeZone in card.ContactTimeZone)
                cardBuilder.AppendLine(timeZone.ToStringVcardTwo());
            foreach (GeoInfo geo in card.ContactGeo)
                cardBuilder.AppendLine(geo.ToStringVcardTwo());
            foreach (ImppInfo impp in card.ContactImpps)
                cardBuilder.AppendLine(impp.ToStringVcardTwo());
            foreach (XNameInfo xname in card.ContactXNames)
                cardBuilder.AppendLine(xname.ToStringVcardTwo());

            // Finally, end the card and return it
            cardBuilder.AppendLine("END:VCARD");
            return cardBuilder.ToString();
        }

        internal override void SaveTo(string path, Card card)
        {
            // Check the version to ensure that we're really dealing with VCard 2.1 contact
            if (CardVersion != "2.1")
                throw new InvalidDataException($"Card version {CardVersion} doesn't match expected \"2.1\".");

            // Check the content to ensure that we really have data
            if (string.IsNullOrEmpty(CardContent))
                throw new InvalidDataException($"Card content is empty.");

            // Save all the changes to the file
            var cardString = SaveToString(card);
            File.WriteAllText(path, cardString);
        }

        internal VcardTwo(string cardContent, string cardVersion)
        {
            CardContent = cardContent;
            CardVersion = cardVersion;
        }
    }
}
