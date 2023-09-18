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
        /// <inheritdoc/>
        public override string CardContent { get; }
        /// <inheritdoc/>
        public override string CardVersion { get; }

        /// <inheritdoc/>
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
            string _source                  = "";
            string _xml                     = "";
            string _fbUrl                   = "";
            string _calUri                  = "";
            string _caladrUri               = "";
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

                // Check for type
                bool isWithType = false;
                var valueSplit = VcardParserTools.SplitToKeyAndValueFromString(_value);
                if (valueSplit[0].Contains(";"))
                    isWithType = true;
                var delimiter = isWithType ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter;

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
                    if (_value.StartsWith(VcardConstants._kindSpecifier + delimiter))
                    {
                        // Get the value
                        string kindValue = _value.Substring(VcardConstants._kindSpecifier.Length + 1);

                        // Populate field
                        if (!string.IsNullOrEmpty(kindValue))
                            _kind = Regex.Unescape(kindValue);
                    }

                    // The name (N:Sanders;John;;; or N;ALTID=1;LANGUAGE=en:Sanders;John;;;)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._nameSpecifier + delimiter))
                    {
                        // Get the name
                        if (isWithType)
                            _names.Add(NameInfo.FromStringVcardFourWithType(_value, splitArgs, finalArgs, altId, _names, idReservedForName));
                        else
                            _names.Add(NameInfo.FromStringVcardFour(splitValues, idReservedForName));

                        // Since we've reserved id 0, set the flag
                        idReservedForName = true;
                    }

                    // Full name (FN:John Sanders)
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(VcardConstants._fullNameSpecifier + delimiter))
                    {
                        // Get the value
                        string fullNameValue = _value.Substring(VcardConstants._fullNameSpecifier.Length + 1);

                        // Populate field
                        _fullName = Regex.Unescape(fullNameValue);

                        // Set flag to indicate that the required field is spotted
                        fullNameSpecifierSpotted = true;
                    }

                    // Telephone (TEL;CELL;TYPE=HOME:495-522-3560 or TEL;TYPE=cell,home:495-522-3560 or TEL:495-522-3560)
                    // Type is supported
                    if (_value.StartsWith(VcardConstants._telephoneSpecifier + delimiter))
                    {
                        if (isWithType)
                            _telephones.Add(TelephoneInfo.FromStringVcardFourWithType(_value, finalArgs, altId));
                        else
                            _telephones.Add(TelephoneInfo.FromStringVcardFour(_value, altId));
                    }

                    // Address (ADR;TYPE=HOME:;;Los Angeles, USA;;;; or ADR:;;Los Angeles, USA;;;;)
                    if (_value.StartsWith(VcardConstants._addressSpecifier + delimiter))
                    {
                        if (isWithType)
                            _addresses.Add(AddressInfo.FromStringVcardFourWithType(_value, finalArgs, altId));
                        else
                            _addresses.Add(AddressInfo.FromStringVcardFour(_value, altId));
                    }

                    // Email (EMAIL;TYPE=HOME,INTERNET:john.s@acme.co)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._emailSpecifier + delimiter))
                    {
                        if (isWithType)
                            _emails.Add(EmailInfo.FromStringVcardFourWithType(_value, finalArgs, altId));
                        else
                            _emails.Add(EmailInfo.FromStringVcardFour(_value, altId));
                    }

                    // Organization (ORG:Acme Co. or ORG:ABC, Inc.;North American Division;Marketing)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._orgSpecifier + delimiter))
                    {
                        if (isWithType)
                            _orgs.Add(OrganizationInfo.FromStringVcardFourWithType(_value, finalArgs, altId));
                        else
                            _orgs.Add(OrganizationInfo.FromStringVcardFour(_value, altId));
                    }

                    // Title (TITLE:Product Manager)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._titleSpecifier + delimiter))
                    {
                        if (isWithType)
                            _titles.Add(TitleInfo.FromStringVcardFourWithType(_value, finalArgs, altId));
                        else
                            _titles.Add(TitleInfo.FromStringVcardFour(_value, altId));
                    }

                    // Website link (URL:https://sso.org/)
                    // Here, we don't support ALTID.
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
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(VcardConstants._noteSpecifier + delimiter))
                    {
                        // Get the value
                        string noteValue = _value.Substring(VcardConstants._noteSpecifier.Length + 1);

                        // Populate field
                        _note = Regex.Unescape(noteValue);
                    }

                    // Photo (PHOTO;ENCODING=BASE64;JPEG:... or PHOTO;VALUE=URL:file:///jqpublic.gif or PHOTO;ENCODING=BASE64;TYPE=GIF:...)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._photoSpecifier + delimiter))
                    {
                        if (isWithType)
                            _photos.Add(PhotoInfo.FromStringVcardFourWithType(_value, finalArgs, altId, CardContentReader));
                        else
                            throw new InvalidDataException("Photo field must not have empty type.");
                    }

                    // Logo (LOGO;ENCODING=BASE64;JPEG:... or LOGO;VALUE=URL:file:///jqpublic.gif or LOGO;ENCODING=BASE64;TYPE=GIF:...)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._logoSpecifier + delimiter))
                    {
                        if (isWithType)
                            _logos.Add(LogoInfo.FromStringVcardFourWithType(_value, finalArgs, altId, CardContentReader));
                        else
                            throw new InvalidDataException("Photo field must not have empty type.");
                    }

                    // Sound (SOUND;VALUE=URL:file///multimed/audio/jqpublic.wav or SOUND;WAVE;BASE64:... or SOUND;TYPE=WAVE;ENCODING=BASE64:...)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._soundSpecifier + delimiter))
                    {
                        if (isWithType)
                            _sounds.Add(SoundInfo.FromStringVcardFourWithType(_value, finalArgs, altId, CardContentReader));
                        else
                            throw new InvalidDataException("Photo field must not have empty type.");
                    }

                    // Revision (REV:1995-10-31T22:27:10Z or REV:19951031T222710)
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(VcardConstants._revSpecifier + delimiter))
                    {
                        // Get the value
                        string revValue = _value.Substring(VcardConstants._revSpecifier.Length + 1);

                        // Populate field
                        _rev = DateTime.Parse(revValue);
                    }

                    // Nickname (NICKNAME;TYPE=cell,home:495-522-3560)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._nicknameSpecifier + delimiter))
                    {
                        if (isWithType)
                            _nicks.Add(NicknameInfo.FromStringVcardFourWithType(_value, finalArgs, altId));
                        else
                            _nicks.Add(NicknameInfo.FromStringVcardFour(_value, altId));
                    }

                    // Birthdate (BDAY:19950415 or BDAY:1953-10-15T23:10:00Z)
                    // Here, we don't support ALTID.
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

                    // Role (ROLE:Programmer)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._roleSpecifier + delimiter))
                    {
                        if (isWithType)
                            _roles.Add(RoleInfo.FromStringVcardFourWithType(_value, finalArgs, altId));
                        else
                            _roles.Add(RoleInfo.FromStringVcardFour(_value, altId));
                    }

                    // Categories (CATEGORIES:INTERNET or CATEGORIES:INTERNET,IETF,INDUSTRY,INFORMATION TECHNOLOGY)
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(VcardConstants._categoriesSpecifier + delimiter))
                    {
                        // Get the value
                        string categoriesValue = _value.Substring(VcardConstants._categoriesSpecifier.Length + 1);

                        // Populate field
                        _categories.AddRange(Regex.Unescape(categoriesValue).Split(','));
                    }

                    // Product ID (PRODID:-//ONLINE DIRECTORY//NONSGML Version 1//EN)
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(VcardConstants._productIdSpecifier + delimiter))
                    {
                        // Get the value
                        string prodIdValue = _value.Substring(VcardConstants._productIdSpecifier.Length + 1);

                        // Populate field
                        _prodId = Regex.Unescape(prodIdValue);
                    }

                    // Sort string (SORT-STRING:Harten)
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(VcardConstants._sortStringSpecifier + delimiter))
                    {
                        // Get the value
                        string sortStringValue = _value.Substring(VcardConstants._sortStringSpecifier.Length + 1);

                        // Populate field
                        _sortString = Regex.Unescape(sortStringValue);
                    }

                    // Time Zone (TZ;VALUE=text:-05:00; EST; Raleigh/North America)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._timeZoneSpecifier + delimiter))
                    {
                        if (isWithType)
                            _timezones.Add(TimeZoneInfo.FromStringVcardFourWithType(_value, finalArgs, altId));
                        else
                            _timezones.Add(TimeZoneInfo.FromStringVcardFour(_value, altId));
                    }

                    // Geo (GEO;VALUE=uri:https://...)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._geoSpecifier + delimiter))
                    {
                        if (isWithType)
                            _geos.Add(GeoInfo.FromStringVcardFourWithType(_value, finalArgs, altId));
                        else
                            _geos.Add(GeoInfo.FromStringVcardFour(_value, altId));
                    }

                    // IMPP information (IMPP;TYPE=home:sip:test)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._imppSpecifier + delimiter))
                    {
                        if (isWithType)
                            _impps.Add(ImppInfo.FromStringVcardFourWithType(_value, finalArgs, altId));
                        else
                            _impps.Add(ImppInfo.FromStringVcardFour(_value, altId));
                    }

                    // Source (SOURCE:http://johndoe.com/vcard.vcf)
                    // Here, we don't support ALTID.
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

                    // XML code (XML:<b>Not an xCard XML element</b>)
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(VcardConstants._xmlSpecifier + delimiter))
                    {
                        // Get the value
                        string xmlStringValue = _value.Substring(VcardConstants._xmlSpecifier.Length + 1);

                        // Populate field
                        _xml = Regex.Unescape(xmlStringValue);
                    }

                    // Free/busy URL (FBURL:http://example.com/fb/jdoe)
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(VcardConstants._fbUrlSpecifier + delimiter))
                    {
                        // Get the value
                        string fbUrlStringValue = _value.Substring(VcardConstants._fbUrlSpecifier.Length + 1);

                        // Try to parse the URL to ensure that it conforms to IETF RFC 1738: Uniform Resource Locators
                        if (!Uri.TryCreate(fbUrlStringValue, UriKind.Absolute, out Uri uri))
                            throw new InvalidDataException($"URL {fbUrlStringValue} is invalid");

                        // Populate field
                        _fbUrl = uri.ToString();
                    }

                    // Calendar URL (CALURI:http://example.com/calendar/jdoe)
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(VcardConstants._calUriSpecifier + delimiter))
                    {
                        // Get the value
                        string calUriStringValue = _value.Substring(VcardConstants._calUriSpecifier.Length + 1);

                        // Try to parse the URL to ensure that it conforms to IETF RFC 1738: Uniform Resource Locators
                        if (!Uri.TryCreate(calUriStringValue, UriKind.Absolute, out Uri uri))
                            throw new InvalidDataException($"URL {calUriStringValue} is invalid");

                        // Populate field
                        _calUri = uri.ToString();
                    }

                    // Calendar Request URL (CALADRURI:http://example.com/calendar/jdoe)
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(VcardConstants._caladrUriSpecifier + delimiter))
                    {
                        // Get the value
                        string caladrUriStringValue = _value.Substring(VcardConstants._caladrUriSpecifier.Length + 1);

                        // Try to parse the URL to ensure that it conforms to IETF RFC 1738: Uniform Resource Locators
                        if (!Uri.TryCreate(caladrUriStringValue, UriKind.Absolute, out Uri uri))
                            throw new InvalidDataException($"URL {caladrUriStringValue} is invalid");

                        // Populate field
                        _caladrUri = uri.ToString();
                    }

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
            return new Card(this, CardVersion, _kind)
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
                ContactMailer = "",
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
                ContactXml = _xml,
                ContactFreeBusyUrl = _fbUrl,
                ContactCalendarUrl = _calUri,
                ContactCalendarSchedulingRequestUrl = _caladrUri
            };
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
            cardBuilder.AppendLine($"{VcardConstants._kindSpecifier}:{card.CardKind}");

            // Then, write the full name and the name
            if (!string.IsNullOrWhiteSpace(card.ContactFullName))
                cardBuilder.AppendLine($"{VcardConstants._fullNameSpecifier}:{card.ContactFullName}");
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
                cardBuilder.AppendLine($"{VcardConstants._urlSpecifier}:{card.ContactURL}");
            if (!string.IsNullOrWhiteSpace(card.ContactNotes))
                cardBuilder.AppendLine($"{VcardConstants._noteSpecifier}:{card.ContactNotes}");
            foreach (PhotoInfo photo in card.ContactPhotos)
                cardBuilder.AppendLine(photo.ToStringVcardFour());
            foreach (LogoInfo logo in card.ContactLogos)
                cardBuilder.AppendLine(logo.ToStringVcardFour());
            foreach (SoundInfo sound in card.ContactSounds)
                cardBuilder.AppendLine(sound.ToStringVcardFour());
            if (card.CardRevision is not null && card.CardRevision != DateTime.MinValue)
                cardBuilder.AppendLine($"{VcardConstants._revSpecifier}:{card.CardRevision:yyyy-MM-dd HH:mm:ss}");
            foreach (NicknameInfo nickname in card.ContactNicknames)
                cardBuilder.AppendLine(nickname.ToStringVcardFour());
            if (card.ContactBirthdate is not null && card.ContactBirthdate != DateTime.MinValue)
                cardBuilder.AppendLine($"{VcardConstants._birthSpecifier}:{card.ContactBirthdate:yyyy-MM-dd}");
            foreach (RoleInfo role in card.ContactRoles)
                cardBuilder.AppendLine(role.ToStringVcardFour());
            if (card.ContactCategories is not null && card.ContactCategories.Length > 0)
                cardBuilder.AppendLine($"{VcardConstants._categoriesSpecifier}:{string.Join(",", card.ContactCategories)}");
            if (!string.IsNullOrWhiteSpace(card.ContactProdId))
                cardBuilder.AppendLine($"{VcardConstants._productIdSpecifier}:{card.ContactProdId}");
            if (!string.IsNullOrWhiteSpace(card.ContactSortString))
                cardBuilder.AppendLine($"{VcardConstants._sortStringSpecifier}:{card.ContactSortString}");
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
