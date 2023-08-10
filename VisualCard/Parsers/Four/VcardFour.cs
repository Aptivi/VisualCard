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

namespace VisualCard.Parsers.Four
{
    /// <summary>
    /// Parser for VCard version 4.0. Consult the vcard-40-rfc6350.txt file in source for the specification.
    /// </summary>
    public class VcardFour : BaseVcardParser, IVcardParser
    {
        public override string CardContent { get; }
        public override string CardVersion { get; }

        public override Card Parse()
        {
            // Check the version to ensure that we're really dealing with VCard 4.0 contact
            if (CardVersion != "4.0")
                throw new InvalidDataException($"Card version {CardVersion} doesn't match expected \"4.0\".");

            // Check the content to ensure that we really have data
            if (string.IsNullOrEmpty(CardContent))
                throw new InvalidDataException($"Card content is empty.");

            // Now, make a stream out of card content
            byte[] CardContentData = Encoding.Default.GetBytes(CardContent);
            MemoryStream CardContentStream = new(CardContentData, false);
            StreamReader CardContentReader = new(CardContentStream);

            // Some variables to assign to the Card() ctor
            string _kind                    = "individual";
            string _fullName                = "";
            string _url                     = "";
            string _note                    = "";
            string _prodId                  = "";
            string _sortString              = "";
            DateTime _rev                   = DateTime.MinValue;
            DateTime _bday                  = DateTime.MinValue;
            List<NameInfo> _names           = new();
            List<TelephoneInfo> _telephones = new();
            List<EmailInfo> _emails         = new();
            List<AddressInfo> _addresses    = new();
            List<OrganizationInfo> _orgs    = new();
            List<TitleInfo> _titles         = new();
            List<LogoInfo> _logos           = new();
            List<PhotoInfo> _photos         = new();
            List<SoundInfo> _sounds         = new();
            List<NicknameInfo> _nicks       = new();
            List<RoleInfo> _roles           = new();
            List<string> _categories        = new();
            List<TimeZoneInfo> _timezones   = new();
            List<GeoInfo> _geos             = new();
            List<ImppInfo> _impps           = new();
            List<XNameInfo> _xes            = new();

            // Full Name specifier is required
            bool fullNameSpecifierSpotted = false;

            // Flags
            bool idReservedForName = false;

            // Iterate through all the lines
            int lineNumber = 0;
            while (!CardContentReader.EndOfStream)
            {
                // Get line
                string _value = CardContentReader.ReadLine();
                lineNumber += 1;

                try
                {
                    // Variables
                    string[] splitValueParts = _value.Split(VcardConstants._argumentDelimiter);
                    string[] splitArgs = splitValueParts[0].Split(VcardConstants._fieldDelimiter);
                    splitArgs = splitArgs.Except(new string[] { splitArgs[0] }).ToArray();
                    string[] splitValues = splitValueParts[1].Split(VcardConstants._fieldDelimiter);
                    List<string> finalArgs = new();
                    int altId = 0;

                    if (splitArgs.Length > 0)
                    {
                        // If we have more than one argument, check for ALTID
                        if (splitArgs[0].StartsWith(VcardConstants._altIdArgumentSpecifier))
                        {
                            if (!int.TryParse(splitArgs[0].Substring(VcardConstants._altIdArgumentSpecifier.Length), out altId))
                                throw new InvalidDataException("ALTID must be numeric");

                            // Here, we require arguments for ALTID
                            if (splitArgs.Length <= 1)
                                throw new InvalidDataException("ALTID must have one or more arguments to specify why is this instance an alternative");
                        }

                        // Finalize the arguments
                        finalArgs.AddRange(splitArgs.Except(
                            splitArgs.Where((arg) =>
                                arg.StartsWith(VcardConstants._altIdArgumentSpecifier) ||
                                arg.StartsWith(VcardConstants._valueArgumentSpecifier) ||
                                arg.StartsWith(VcardConstants._typeArgumentSpecifier)
                            )
                        ));
                    }

                    // Card type (KIND:individual, KIND:group, KIND:org, KIND:location, ...)
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(VcardConstants._kindSpecifier))
                    {
                        // Get the value
                        string kindValue = _value.Substring(VcardConstants._kindSpecifier.Length);

                        // Populate field
                        if (!string.IsNullOrEmpty(kindValue))
                            _kind = Regex.Unescape(kindValue);
                    }

                    // The name (N:Sanders;John;;;)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._nameSpecifier))
                    {
                        // Get the name
                        _names.Add(NameInfo.FromStringVcardFour(splitValues, idReservedForName));

                        // Since we've reserved id 0, set the flag
                        idReservedForName = true;
                    }

                    // The name (N;ALTID=1;LANGUAGE=en:Sanders;John;;;)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._nameSpecifierWithType))
                    {
                        // Get the name
                        _names.Add(NameInfo.FromStringVcardFourWithType(_value, splitArgs, finalArgs, altId, _names, idReservedForName));

                        // Since we've reserved a specific id, set the flag
                        idReservedForName = true;
                    }

                    // Full name (FN:John Sanders)
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(VcardConstants._fullNameSpecifier))
                    {
                        // Get the value
                        string fullNameValue = _value.Substring(VcardConstants._fullNameSpecifier.Length);

                        // Populate field
                        _fullName = Regex.Unescape(fullNameValue);

                        // Set flag to indicate that the required field is spotted
                        fullNameSpecifierSpotted = true;
                    }

                    // Telephone (TEL;TYPE=cell,home:495-522-3560)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._telephoneSpecifierWithType))
                        _telephones.Add(TelephoneInfo.FromStringVcardFourWithType(_value, finalArgs, altId));

                    // Telephone (TEL:495-522-3560)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._telephoneSpecifier))
                        _telephones.Add(TelephoneInfo.FromStringVcardFour(_value, altId));

                    // Address (ADR;TYPE=HOME:;;Los Angeles, USA;;;;)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._addressSpecifierWithType))
                        _addresses.Add(AddressInfo.FromStringVcardFourWithType(_value, finalArgs, altId));

                    // Email (EMAIL;TYPE=HOME,INTERNET:john.s@acme.co)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._emailSpecifier))
                        _emails.Add(EmailInfo.FromStringVcardFourWithType(_value, finalArgs, altId));

                    // Organization (ORG:Acme Co. or ORG:ABC, Inc.;North American Division;Marketing)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._orgSpecifier))
                        _orgs.Add(OrganizationInfo.FromStringVcardFour(_value, altId));

                    // Organization (ORG;TYPE=WORK:Acme Co. or ORG:ABC, Inc.;North American Division;Marketing)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._orgSpecifierWithType))
                        _orgs.Add(OrganizationInfo.FromStringVcardFourWithType(_value, finalArgs, altId));

                    // Title (TITLE:Product Manager)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._titleSpecifier))
                        _titles.Add(TitleInfo.FromStringVcardFour(_value, altId));

                    // Title (TITLE;ALTID=1;LANGUAGE=fr:Patron or TITLE;LANGUAGE=fr:Patron)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._titleSpecifierWithArguments))
                        _titles.Add(TitleInfo.FromStringVcardFourWithType(_value, finalArgs, altId));

                    // Website link (URL:https://sso.org/)
                    // Here, we don't support ALTID.
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
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(VcardConstants._noteSpecifier))
                    {
                        // Get the value
                        string noteValue = _value.Substring(VcardConstants._noteSpecifier.Length);

                        // Populate field
                        _note = Regex.Unescape(noteValue);
                    }

                    // Photo (PHOTO;ENCODING=BASE64;JPEG:... or PHOTO;VALUE=URL:file:///jqpublic.gif or PHOTO;ENCODING=BASE64;TYPE=GIF:...)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._photoSpecifierWithType))
                        _photos.Add(PhotoInfo.FromStringVcardFourWithType(_value, finalArgs, altId, CardContentReader));

                    // Logo (LOGO;ENCODING=BASE64;JPEG:... or LOGO;VALUE=URL:file:///jqpublic.gif or LOGO;ENCODING=BASE64;TYPE=GIF:...)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._logoSpecifierWithType))
                        _logos.Add(LogoInfo.FromStringVcardFourWithType(_value, finalArgs, altId, CardContentReader));

                    // Sound (SOUND;VALUE=URL:file///multimed/audio/jqpublic.wav or SOUND;WAVE;BASE64:... or SOUND;TYPE=WAVE;ENCODING=BASE64:...)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._soundSpecifierWithType))
                        _sounds.Add(SoundInfo.FromStringVcardFourWithType(_value, finalArgs, altId, CardContentReader));

                    // Revision (REV:1995-10-31T22:27:10Z or REV:19951031T222710)
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(VcardConstants._revSpecifier))
                    {
                        // Get the value
                        string revValue = _value.Substring(VcardConstants._revSpecifier.Length);

                        // Populate field
                        _rev = DateTime.Parse(revValue);
                    }

                    // Nickname (NICKNAME;TYPE=cell,home:495-522-3560)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._nicknameSpecifierWithType))
                        _nicks.Add(NicknameInfo.FromStringVcardFourWithType(_value, finalArgs, altId));

                    // Nickname (NICKNAME:495-522-3560)
                    // ALTID is supported. See above.
                    if (_value.StartsWith(VcardConstants._nicknameSpecifier))
                        _nicks.Add(NicknameInfo.FromStringVcardFour(_value, altId));

                    // Birthdate (BDAY:19950415 or BDAY:1953-10-15T23:10:00Z)
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(VcardConstants._birthSpecifier))
                    {
                        // Get the value
                        string bdayValue = _value.Substring(VcardConstants._birthSpecifier.Length);

                        // Populate field
                        _bday = DateTime.Parse(bdayValue);
                    }

                    // Role (ROLE:Programmer)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._roleSpecifier))
                        _roles.Add(RoleInfo.FromStringVcardFour(_value, altId));

                    // Role (ROLE;ALTID=1;LANGUAGE=en:Programmer)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._roleSpecifierWithType))
                        _roles.Add(RoleInfo.FromStringVcardFourWithType(_value, finalArgs, altId));

                    // Categories (CATEGORIES:INTERNET or CATEGORIES:INTERNET,IETF,INDUSTRY,INFORMATION TECHNOLOGY)
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(VcardConstants._categoriesSpecifier))
                    {
                        // Get the value
                        string categoriesValue = _value.Substring(VcardConstants._categoriesSpecifier.Length);

                        // Populate field
                        _categories.AddRange(Regex.Unescape(categoriesValue).Split(','));
                    }

                    // Product ID (PRODID:-//ONLINE DIRECTORY//NONSGML Version 1//EN)
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(VcardConstants._productIdSpecifier))
                    {
                        // Get the value
                        string prodIdValue = _value.Substring(VcardConstants._productIdSpecifier.Length);

                        // Populate field
                        _prodId = Regex.Unescape(prodIdValue);
                    }

                    // Sort string (SORT-STRING:Harten)
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(VcardConstants._sortStringSpecifier))
                    {
                        // Get the value
                        string sortStringValue = _value.Substring(VcardConstants._sortStringSpecifier.Length);

                        // Populate field
                        _sortString = Regex.Unescape(sortStringValue);
                    }

                    // Time Zone (TZ;VALUE=text:-05:00; EST; Raleigh/North America)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._timeZoneSpecifierWithType))
                        _timezones.Add(TimeZoneInfo.FromStringVcardFourWithType(_value, finalArgs, altId));

                    // Time Zone (TZ:-05:00)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._timeZoneSpecifier))
                        _timezones.Add(TimeZoneInfo.FromStringVcardFour(_value, altId));

                    // Geo (GEO;VALUE=uri:https://...)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._geoSpecifierWithType))
                        _geos.Add(GeoInfo.FromStringVcardFourWithType(_value, finalArgs, altId));

                    // Geo (GEO:geo:37.386013,-122.082932)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._geoSpecifier))
                        _geos.Add(GeoInfo.FromStringVcardFour(_value, altId));

                    // IMPP information (IMPP;TYPE=home:sip:test)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._imppSpecifierWithType))
                        _impps.Add(ImppInfo.FromStringVcardFourWithType(_value, finalArgs, altId));

                    // IMPP information (IMPP:sip:test)
                    // ALTID is supported. See above.
                    if (_value.StartsWith(VcardConstants._imppSpecifier))
                        _impps.Add(ImppInfo.FromStringVcardFour(_value, altId));

                    // X-nonstandard (X-AIM:john.s or X-DL;Design Work Group:List Item 1;List Item 2;List Item 3)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._xSpecifier))
                        _xes.Add(XNameInfo.FromStringVcardFour(_value, finalArgs, altId));
                }
                catch (Exception ex)
                {
                    throw new VCardParseException(ex.Message, _value, lineNumber, ex);
                }
            }

            // Requirement checks
            if (!fullNameSpecifierSpotted)
                throw new InvalidDataException("The full name specifier, \"FN:\", is required.");

            // Make a new instance of the card
            return new Card(this, CardVersion, _names.ToArray(), _fullName, _telephones.ToArray(), _addresses.ToArray(), _orgs.ToArray(), _titles.ToArray(), _url, _note, _emails.ToArray(), _xes.ToArray(), _kind, _photos.ToArray(), _rev, _nicks.ToArray(), _bday, "", _roles.ToArray(), _categories.ToArray(), _logos.ToArray(), _prodId, _sortString, _timezones.ToArray(), _geos.ToArray(), _sounds.ToArray(), _impps.ToArray());
        }

        internal override string SaveToString(Card card)
        {
            // Check the version to ensure that we're really dealing with VCard 4.0 contact
            if (CardVersion != "4.0")
                throw new InvalidDataException($"Card version {CardVersion} doesn't match expected \"4.0\".");

            // Check the content to ensure that we really have data
            if (string.IsNullOrEmpty(CardContent))
                throw new InvalidDataException($"Card content is empty.");

            // Initialize the card builder
            var cardBuilder = new StringBuilder();

            // First, write the header
            cardBuilder.AppendLine("BEGIN:VCARD");
            cardBuilder.AppendLine($"VERSION:{CardVersion}");
            cardBuilder.AppendLine($"{VcardConstants._kindSpecifier}{card.CardKind}");

            // Then, write the full name and the name
            if (!string.IsNullOrWhiteSpace(card.ContactFullName))
                cardBuilder.AppendLine($"{VcardConstants._fullNameSpecifier}{card.ContactFullName}");
            foreach (NameInfo name in card.ContactNames)
                cardBuilder.AppendLine(name.ToStringVcardFour());

            // Now, start filling in the rest...
            foreach (TelephoneInfo telephone in card.ContactTelephones)
                cardBuilder.AppendLine(telephone.ToStringVcardFour());
            foreach (AddressInfo address in card.ContactAddresses)
                cardBuilder.AppendLine(address.ToStringVcardFour());
            foreach (EmailInfo email in card.ContactMails)
                cardBuilder.AppendLine(email.ToStringVcardFour());
            foreach (OrganizationInfo organization in card.ContactOrganizations)
                cardBuilder.AppendLine(organization.ToStringVcardFour());
            foreach (TitleInfo title in card.ContactTitles)
                cardBuilder.AppendLine(title.ToStringVcardFour());
            if (!string.IsNullOrWhiteSpace(card.ContactURL))
                cardBuilder.AppendLine($"{VcardConstants._urlSpecifier}{card.ContactURL}");
            if (!string.IsNullOrWhiteSpace(card.ContactNotes))
                cardBuilder.AppendLine($"{VcardConstants._noteSpecifier}{card.ContactNotes}");
            foreach (PhotoInfo photo in card.ContactPhotos)
                cardBuilder.AppendLine(photo.ToStringVcardFour());
            foreach (LogoInfo logo in card.ContactLogos)
                cardBuilder.AppendLine(logo.ToStringVcardFour());
            foreach (SoundInfo sound in card.ContactSounds)
                cardBuilder.AppendLine(sound.ToStringVcardFour());
            if (card.CardRevision is not null && card.CardRevision != DateTime.MinValue)
                cardBuilder.AppendLine($"{VcardConstants._revSpecifier}{card.CardRevision:dd-MM-yyyy_HH-mm-ss}");
            foreach (NicknameInfo nickname in card.ContactNicknames)
                cardBuilder.AppendLine(nickname.ToStringVcardFour());
            if (card.ContactBirthdate is not null && card.ContactBirthdate != DateTime.MinValue)
                cardBuilder.AppendLine($"{VcardConstants._birthSpecifier}{card.ContactBirthdate:dd-MM-yyyy}");
            foreach (RoleInfo role in card.ContactRoles)
                cardBuilder.AppendLine(role.ToStringVcardFour());
            if (card.ContactCategories is not null && card.ContactCategories.Length > 0)
                cardBuilder.AppendLine($"{VcardConstants._categoriesSpecifier}{string.Join(",", card.ContactCategories)}");
            if (!string.IsNullOrWhiteSpace(card.ContactProdId))
                cardBuilder.AppendLine($"{VcardConstants._productIdSpecifier}{card.ContactProdId}");
            if (!string.IsNullOrWhiteSpace(card.ContactSortString))
                cardBuilder.AppendLine($"{VcardConstants._sortStringSpecifier}{card.ContactSortString}");
            foreach (TimeZoneInfo timeZone in card.ContactTimeZone)
                cardBuilder.AppendLine(timeZone.ToStringVcardFour());
            foreach (GeoInfo geo in card.ContactGeo)
                cardBuilder.AppendLine(geo.ToStringVcardFour());
            foreach (ImppInfo impp in card.ContactImpps)
                cardBuilder.AppendLine(impp.ToStringVcardFour());
            foreach (XNameInfo xname in card.ContactXNames)
                cardBuilder.AppendLine(xname.ToStringVcardFour());

            // Finally, end the card and return it
            cardBuilder.AppendLine("END:VCARD");
            return cardBuilder.ToString();
        }

        internal override void SaveTo(string path, Card card)
        {
            // Check the version to ensure that we're really dealing with VCard 4.0 contact
            if (CardVersion != "4.0")
                throw new InvalidDataException($"Card version {CardVersion} doesn't match expected \"4.0\".");

            // Check the content to ensure that we really have data
            if (string.IsNullOrEmpty(CardContent))
                throw new InvalidDataException($"Card content is empty.");

            // Save all the changes to the file
            var cardString = SaveToString(card);
            File.WriteAllText(path, cardString);
        }

        internal VcardFour(string cardContent, string cardVersion)
        {
            CardContent = cardContent;
            CardVersion = cardVersion;
        }
    }
}
