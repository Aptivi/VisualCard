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
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using VisualCard.Exceptions;
using VisualCard.Parts;
using TimeZoneInfo = VisualCard.Parts.TimeZoneInfo;

namespace VisualCard.Parsers.Three
{
    /// <summary>
    /// Parser for VCard version 3.0. Consult the vcard-30-rfc2426.txt file in source for the specification.
    /// </summary>
    public class VcardThree : BaseVcardParser, IVcardParser
    {
        /// <inheritdoc/>
        public override string CardContent { get; }
        /// <inheritdoc/>
        public override string CardVersion { get; }

        /// <inheritdoc/>
        public override Card Parse()
        {
            // Check the version to ensure that we're really dealing with VCard 3.0 contact
            if (CardVersion != "3.0")
                throw new InvalidDataException($"Card version {CardVersion} doesn't match expected \"3.0\".");

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
            string _prodId = "";
            string _sortString = "";
            string _source = "";
            DateTime _rev = DateTime.MinValue;
            DateTime _bday = DateTime.MinValue;
            List<NameInfo> _names = new();
            List<TelephoneInfo> _telephones = new();
            List<EmailInfo> _emails = new();
            List<AddressInfo> _addresses = new();
            List<OrganizationInfo> _orgs = new();
            List<TitleInfo> _titles = new();
            List<PhotoInfo> _photos = new();
            List<LogoInfo> _logos = new();
            List<SoundInfo> _sounds = new();
            List<NicknameInfo> _nicks = new();
            List<RoleInfo> _roles = new();
            List<string> _categories = new();
            List<TimeZoneInfo> _timezones = new();
            List<GeoInfo> _geos = new();
            List<ImppInfo> _impps = new();
            List<XNameInfo> _xes = new();

            // Name and Full Name specifiers are required
            bool nameSpecifierSpotted = false;
            bool fullNameSpecifierSpotted = false;

            // Iterate through all the lines
            int lineNumber = 0;
            while (!CardContentReader.EndOfStream)
            {
                // Get line
                string _value = CardContentReader.ReadLine();
                lineNumber += 1;

                // Check for type
                bool isWithType = false;
                var valueSplit = VcardParserTools.SplitToKeyAndValueFromString(_value);
                if (valueSplit[0].Contains(";"))
                    isWithType = true;
                var delimiter = isWithType ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter;

                try
                {
                    // The name (N:Sanders;John;;;)
                    if (_value.StartsWith(VcardConstants._nameSpecifier + delimiter))
                    {
                        // Get the name
                        _names.Add(NameInfo.FromStringVcardThree(_value));

                        // Set flag to indicate that the required field is spotted
                        nameSpecifierSpotted = true;
                    }

                    // Full name (FN:John Sanders)
                    if (_value.StartsWith(VcardConstants._fullNameSpecifier + delimiter))
                    {
                        // Get the value
                        string fullNameValue = _value.Substring(VcardConstants._fullNameSpecifier.Length + 1);

                        // Populate field
                        _fullName = Regex.Unescape(fullNameValue);

                        // Set flag to indicate that the required field is spotted
                        fullNameSpecifierSpotted = true;
                    }

                    // Telephone (TEL;TYPE=CELL;HOME:495-522-3560 or TEL;TYPE=cell,home:495-522-3560 or TEL:495-522-3560)
                    // Type is supported
                    if (_value.StartsWith(VcardConstants._telephoneSpecifier + delimiter))
                    {
                        if (isWithType)
                            _telephones.Add(TelephoneInfo.FromStringVcardThreeWithType(_value));
                        else
                            _telephones.Add(TelephoneInfo.FromStringVcardThree(_value));
                    }

                    // Address (ADR;TYPE=HOME:;;Los Angeles, USA;;;; or ADR:;;Los Angeles, USA;;;;)
                    if (_value.StartsWith(VcardConstants._addressSpecifier + delimiter))
                    {
                        if (isWithType)
                            _addresses.Add(AddressInfo.FromStringVcardThreeWithType(_value));
                        else
                            _addresses.Add(AddressInfo.FromStringVcardThree(_value));
                    }

                    // Email (EMAIL;TYPE=HOME,INTERNET:john.s@acme.co)
                    if (_value.StartsWith(VcardConstants._emailSpecifier + delimiter))
                    {
                        if (isWithType)
                            _emails.Add(EmailInfo.FromStringVcardThreeWithType(_value));
                        else
                            _emails.Add(EmailInfo.FromStringVcardThree(_value));
                    }

                    // Organization (ORG:Acme Co. or ORG:ABC, Inc.;North American Division;Marketing)
                    if (_value.StartsWith(VcardConstants._orgSpecifier + delimiter))
                    {
                        if (isWithType)
                            _orgs.Add(OrganizationInfo.FromStringVcardThreeWithType(_value));
                        else
                            _orgs.Add(OrganizationInfo.FromStringVcardThree(_value));
                    }

                    // Title (TITLE:Product Manager)
                    if (_value.StartsWith(VcardConstants._titleSpecifier + delimiter))
                        _titles.Add(TitleInfo.FromStringVcardThree(_value));

                    // Website link (URL:https://sso.org/)
                    if (_value.StartsWith(VcardConstants._urlSpecifier + delimiter))
                    {
                        // Get the value
                        string urlValue = _value.Substring(VcardConstants._urlSpecifier.Length + 1);

                        // Try to parse the URL to ensure that it conforms to IETF RFC 1738: Uniform Resource Locators
                        if (!Uri.TryCreate(urlValue, UriKind.Absolute, out Uri uri))
                            throw new InvalidDataException($"URL {urlValue} is invalid");

                        // Populate field
                        _url = uri.ToString();
                    }

                    // Note (NOTE:Product Manager)
                    if (_value.StartsWith(VcardConstants._noteSpecifier + delimiter))
                    {
                        // Get the value
                        string noteValue = _value.Substring(VcardConstants._noteSpecifier.Length + 1);

                        // Populate field
                        _note = Regex.Unescape(noteValue);
                    }

                    // Photo (PHOTO;ENCODING=BASE64;JPEG:... or PHOTO;VALUE=URL:file:///jqpublic.gif or PHOTO;ENCODING=BASE64;TYPE=GIF:...)
                    if (_value.StartsWith(VcardConstants._photoSpecifier + delimiter))
                    {
                        if (isWithType)
                            _photos.Add(PhotoInfo.FromStringVcardThreeWithType(_value, CardContentReader));
                        else
                            throw new InvalidDataException("Photo field must not have empty type.");
                    }

                    // Logo (LOGO;ENCODING=BASE64;JPEG:... or LOGO;VALUE=URL:file:///jqpublic.gif or LOGO;ENCODING=BASE64;TYPE=GIF:...)
                    if (_value.StartsWith(VcardConstants._logoSpecifier + delimiter))
                    {
                        if (isWithType)
                            _logos.Add(LogoInfo.FromStringVcardThreeWithType(_value, CardContentReader));
                        else
                            throw new InvalidDataException("Logo field must not have empty type.");
                    }

                    // Sound (SOUND;VALUE=URL:file///multimed/audio/jqpublic.wav or SOUND;WAVE;BASE64:... or SOUND;TYPE=WAVE;ENCODING=BASE64:...)
                    if (_value.StartsWith(VcardConstants._soundSpecifier + delimiter))
                    {
                        if (isWithType)
                            _sounds.Add(SoundInfo.FromStringVcardThreeWithType(_value, CardContentReader));
                        else
                            throw new InvalidDataException("Sound field must not have empty type.");
                    }

                    // Revision (REV:1995-10-31T22:27:10Z or REV:19951031T222710)
                    if (_value.StartsWith(VcardConstants._revSpecifier + delimiter))
                    {
                        // Get the value
                        string revValue = _value.Substring(VcardConstants._revSpecifier.Length + 1);

                        // Populate field
                        _rev = DateTime.Parse(revValue);
                    }

                    // Nickname (NICKNAME;TYPE=work:Boss)
                    if (_value.StartsWith(VcardConstants._nicknameSpecifier + delimiter))
                    {
                        if (isWithType)
                            _nicks.Add(NicknameInfo.FromStringVcardThreeWithType(_value));
                        else
                            _nicks.Add(NicknameInfo.FromStringVcardThree(_value));
                    }

                    // Birthdate (BDAY:19950415 or BDAY:1953-10-15T23:10:00Z)
                    if (_value.StartsWith(VcardConstants._birthSpecifier + delimiter))
                    {
                        // Get the value
                        string bdayValue = "";
                        if (isWithType)
                            bdayValue = _value.Substring(_value.IndexOf(VcardConstants._argumentDelimiter) + 1);
                        else
                            bdayValue = _value.Substring(VcardConstants._birthSpecifier.Length + 1);

                        // Populate field
                        if (int.TryParse(bdayValue, out int bdayDigits) && bdayValue.Length == 8)
                        {
                            int birthNum = int.Parse(bdayValue);
                            var birthDigits = VcardParserTools.GetDigits(birthNum).ToList();
                            int birthYear = (birthDigits[0] * 1000) + (birthDigits[1] * 100) + (birthDigits[2] * 10) + birthDigits[3];
                            int birthMonth = (birthDigits[4] * 10) + birthDigits[5];
                            int birthDay = (birthDigits[6] * 10) + birthDigits[7];
                            _bday = new DateTime(birthYear, birthMonth, birthDay);
                        }
                        else
                            _bday = DateTime.Parse(bdayValue);
                    }

                    // Mailer (MAILER:ccMail 2.2 or MAILER:PigeonMail 2.1)
                    if (_value.StartsWith(VcardConstants._mailerSpecifier + delimiter))
                    {
                        // Get the value
                        string mailerValue = _value.Substring(VcardConstants._mailerSpecifier.Length + 1);

                        // Populate field
                        _mailer = Regex.Unescape(mailerValue);
                    }

                    // Role (ROLE:Programmer)
                    if (_value.StartsWith(VcardConstants._roleSpecifier + delimiter))
                        _roles.Add(RoleInfo.FromStringVcardThree(_value));

                    // Categories (CATEGORIES:INTERNET or CATEGORIES:INTERNET,IETF,INDUSTRY,INFORMATION TECHNOLOGY)
                    if (_value.StartsWith(VcardConstants._categoriesSpecifier + delimiter))
                    {
                        // Get the value
                        string categoriesValue = _value.Substring(VcardConstants._categoriesSpecifier.Length + 1);

                        // Populate field
                        _categories.AddRange(Regex.Unescape(categoriesValue).Split(','));
                    }

                    // Product ID (PRODID:-//ONLINE DIRECTORY//NONSGML Version 1//EN)
                    if (_value.StartsWith(VcardConstants._productIdSpecifier + delimiter))
                    {
                        // Get the value
                        string prodIdValue = _value.Substring(VcardConstants._productIdSpecifier.Length + 1);

                        // Populate field
                        _prodId = Regex.Unescape(prodIdValue);
                    }

                    // Sort string (SORT-STRING:Harten)
                    if (_value.StartsWith(VcardConstants._sortStringSpecifier + delimiter))
                    {
                        // Get the value
                        string sortStringValue = _value.Substring(VcardConstants._sortStringSpecifier.Length + 1);

                        // Populate field
                        _sortString = Regex.Unescape(sortStringValue);
                    }

                    // Time Zone (TZ;VALUE=text:-05:00; EST; Raleigh/North America)
                    if (_value.StartsWith(VcardConstants._timeZoneSpecifier + delimiter))
                    {
                        if (isWithType)
                            _timezones.Add(TimeZoneInfo.FromStringVcardThreeWithType(_value));
                        else
                            _timezones.Add(TimeZoneInfo.FromStringVcardThree(_value));
                    }

                    // Geo (GEO;VALUE=uri:https://...)
                    if (_value.StartsWith(VcardConstants._geoSpecifier + delimiter))
                    {
                        if (isWithType)
                            _geos.Add(GeoInfo.FromStringVcardThreeWithType(_value));
                        else
                            _geos.Add(GeoInfo.FromStringVcardThree(_value));
                    }

                    // IMPP information (IMPP;TYPE=home:sip:test)
                    if (_value.StartsWith(VcardConstants._imppSpecifier + delimiter))
                    {
                        if (isWithType)
                            _impps.Add(ImppInfo.FromStringVcardThreeWithType(_value));
                        else
                            _impps.Add(ImppInfo.FromStringVcardThree(_value));
                    }

                    // Source (SOURCE:http://johndoe.com/vcard.vcf)
                    if (_value.StartsWith(VcardConstants._sourceSpecifier + delimiter))
                    {
                        // Get the value
                        string sourceStringValue = _value.Substring(VcardConstants._sourceSpecifier.Length + 1);

                        // Try to parse the URL to ensure that it conforms to IETF RFC 1738: Uniform Resource Locators
                        if (!Uri.TryCreate(sourceStringValue, UriKind.Absolute, out Uri uri))
                            throw new InvalidDataException($"URL {sourceStringValue} is invalid");

                        // Populate field
                        _source = uri.ToString();
                    }

                    // X-nonstandard (X-AIM:john.s or X-DL;Design Work Group:List Item 1;List Item 2;List Item 3)
                    if (_value.StartsWith(VcardConstants._xSpecifier))
                        _xes.Add(XNameInfo.FromStringVcardThree(_value));
                }
                catch (Exception ex)
                {
                    throw new VCardParseException(ex.Message, _value, lineNumber, ex);
                }
            }

            // Requirement checks
            if (!nameSpecifierSpotted)
                throw new InvalidDataException("The name specifier, \"N:\", is required.");
            if (!fullNameSpecifierSpotted)
                throw new InvalidDataException("The full name specifier, \"FN:\", is required.");

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
                ContactNicknames = _nicks.ToArray(),
                ContactBirthdate = _bday,
                ContactMailer = _mailer,
                ContactRoles = _roles.ToArray(),
                ContactCategories = _categories.ToArray(),
                ContactLogos = _logos.ToArray(),
                ContactProdId = _prodId,
                ContactSortString = _sortString,
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
            // Check the version to ensure that we're really dealing with VCard 3.0 contact
            if (CardVersion != "3.0")
                throw new InvalidDataException($"Card version {CardVersion} doesn't match expected \"3.0\".");

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
                cardBuilder.AppendLine($"{VcardConstants._fullNameSpecifier}:{card.ContactFullName}");
            foreach (NameInfo name in card.ContactNames)
                cardBuilder.AppendLine(name.ToStringVcardThree());

            // Now, start filling in the rest...
            foreach (TelephoneInfo telephone in card.ContactTelephones)
                cardBuilder.AppendLine(telephone.ToStringVcardThree());
            foreach (AddressInfo address in card.ContactAddresses)
                cardBuilder.AppendLine(address.ToStringVcardThree());
            foreach (EmailInfo email in card.ContactMails)
                cardBuilder.AppendLine(email.ToStringVcardThree());
            foreach (OrganizationInfo organization in card.ContactOrganizations)
                cardBuilder.AppendLine(organization.ToStringVcardThree());
            foreach (TitleInfo title in card.ContactTitles)
                cardBuilder.AppendLine(title.ToStringVcardThree());
            if (!string.IsNullOrWhiteSpace(card.ContactURL))
                cardBuilder.AppendLine($"{VcardConstants._urlSpecifier}:{card.ContactURL}");
            if (!string.IsNullOrWhiteSpace(card.ContactNotes))
                cardBuilder.AppendLine($"{VcardConstants._noteSpecifier}:{card.ContactNotes}");
            foreach (PhotoInfo photo in card.ContactPhotos)
                cardBuilder.AppendLine(photo.ToStringVcardThree());
            foreach (LogoInfo logo in card.ContactLogos)
                cardBuilder.AppendLine(logo.ToStringVcardThree());
            foreach (SoundInfo sound in card.ContactSounds)
                cardBuilder.AppendLine(sound.ToStringVcardThree());
            if (card.CardRevision is not null && card.CardRevision != DateTime.MinValue)
                cardBuilder.AppendLine($"{VcardConstants._revSpecifier}:{card.CardRevision:yyyy-MM-dd HH:mm:ss}");
            foreach (NicknameInfo nickname in card.ContactNicknames)
                cardBuilder.AppendLine(nickname.ToStringVcardThree());
            if (card.ContactBirthdate is not null && card.ContactBirthdate != DateTime.MinValue)
                cardBuilder.AppendLine($"{VcardConstants._birthSpecifier}:{card.ContactBirthdate:yyyy-MM-dd}");
            if (!string.IsNullOrWhiteSpace(card.ContactMailer))
                cardBuilder.AppendLine($"{VcardConstants._mailerSpecifier}:{card.ContactMailer}");
            foreach (RoleInfo role in card.ContactRoles)
                cardBuilder.AppendLine(role.ToStringVcardThree());
            if (card.ContactCategories is not null && card.ContactCategories.Length > 0)
                cardBuilder.AppendLine($"{VcardConstants._categoriesSpecifier}:{string.Join(",", card.ContactCategories)}");
            if (!string.IsNullOrWhiteSpace(card.ContactProdId))
                cardBuilder.AppendLine($"{VcardConstants._productIdSpecifier}:{card.ContactProdId}");
            if (!string.IsNullOrWhiteSpace(card.ContactSortString))
                cardBuilder.AppendLine($"{VcardConstants._sortStringSpecifier}:{card.ContactSortString}");
            foreach (TimeZoneInfo timeZone in card.ContactTimeZone)
                cardBuilder.AppendLine(timeZone.ToStringVcardThree());
            foreach (GeoInfo geo in card.ContactGeo)
                cardBuilder.AppendLine(geo.ToStringVcardThree());
            foreach (ImppInfo impp in card.ContactImpps)
                cardBuilder.AppendLine(impp.ToStringVcardThree());
            foreach (XNameInfo xname in card.ContactXNames)
                cardBuilder.AppendLine(xname.ToStringVcardThree());

            // Finally, end the card and return it
            cardBuilder.AppendLine("END:VCARD");
            return cardBuilder.ToString();
        }

        internal override void SaveTo(string path, Card card)
        {
            // Check the version to ensure that we're really dealing with VCard 3.0 contact
            if (CardVersion != "3.0")
                throw new InvalidDataException($"Card version {CardVersion} doesn't match expected \"3.0\".");

            // Check the content to ensure that we really have data
            if (string.IsNullOrEmpty(CardContent))
                throw new InvalidDataException($"Card content is empty.");

            // Save all the changes to the file
            var cardString = SaveToString(card);
            File.WriteAllText(path, cardString);
        }

        internal VcardThree(string cardContent, string cardVersion)
        {
            CardContent = cardContent;
            CardVersion = cardVersion;
        }
    }
}
