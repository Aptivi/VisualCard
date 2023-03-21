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
using System.Data;
using System.IO;
using System.IO.Pipes;
using System.Net;
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
        public override string CardContent { get; }
        public override string CardVersion { get; }

        // Some VCard 3.0 constants
        const char _fieldDelimiter = ';';
        const char _valueDelimiter = ',';
        const char _argumentDelimiter = ':';
        const string _nameSpecifier = "N:";
        const string _fullNameSpecifier = "FN:";
        const string _telephoneSpecifierWithType = "TEL;";
        const string _telephoneSpecifier = "TEL:";
        const string _addressSpecifierWithType = "ADR;";
        const string _emailSpecifier = "EMAIL;";
        const string _orgSpecifier = "ORG:";
        const string _orgSpecifierWithType = "ORG;";
        const string _titleSpecifier = "TITLE:";
        const string _urlSpecifier = "URL:";
        const string _noteSpecifier = "NOTE:";
        const string _photoSpecifierWithType = "PHOTO;";
        const string _logoSpecifierWithType = "LOGO;";
        const string _soundSpecifierWithType = "SOUND;";
        const string _revSpecifier = "REV:";
        const string _nicknameSpecifier = "NICKNAME:";
        const string _nicknameSpecifierWithType = "NICKNAME;";
        const string _birthSpecifier = "BDAY:";
        const string _mailerSpecifier = "MAILER:";
        const string _roleSpecifier = "ROLE:";
        const string _categoriesSpecifier = "CATEGORIES:";
        const string _productIdSpecifier = "PRODID:";
        const string _sortStringSpecifier = "SORT-STRING:";
        const string _timeZoneSpecifier = "TZ:";
        const string _geoSpecifier = "GEO:";
        const string _timeZoneSpecifierWithType = "TZ;";
        const string _geoSpecifierWithType = "GEO;";
        const string _imppSpecifier = "IMPP:";
        const string _imppSpecifierWithType = "IMPP;";
        const string _xSpecifier = "X-";
        const string _typeArgumentSpecifier = "TYPE=";
        const string _valueArgumentSpecifier = "VALUE=";

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

                try
                {
                    // The name (N:Sanders;John;;;)
                    if (_value.StartsWith(_nameSpecifier))
                    {
                        // Check the line
                        string nameValue = _value.Substring(_nameSpecifier.Length);
                        string[] splitName = nameValue.Split(_fieldDelimiter);
                        if (splitName.Length < 2)
                            throw new InvalidDataException("Name field must specify exactly five values (Last name, first name, alt names, prefixes, and suffixes)");

                        // Populate fields
                        string _lastName = Regex.Unescape(splitName[0]);
                        string _firstName = Regex.Unescape(splitName[1]);
                        string[] _altNames = splitName.Length >= 3 ? Regex.Unescape(splitName[2]).Split(new char[] { _valueDelimiter }) : Array.Empty<string>();
                        string[] _prefixes = splitName.Length >= 4 ? Regex.Unescape(splitName[3]).Split(new char[] { _valueDelimiter }) : Array.Empty<string>();
                        string[] _suffixes = splitName.Length >= 5 ? Regex.Unescape(splitName[4]).Split(new char[] { _valueDelimiter }) : Array.Empty<string>();
                        NameInfo _name = new(0, Array.Empty<string>(), _firstName, _lastName, _altNames, _prefixes, _suffixes);
                        _names.Add(_name);

                        // Set flag to indicate that the required field is spotted
                        nameSpecifierSpotted = true;
                    }

                    // Full name (FN:John Sanders)
                    if (_value.StartsWith(_fullNameSpecifier))
                    {
                        // Get the value
                        string fullNameValue = _value.Substring(_fullNameSpecifier.Length);

                        // Populate field
                        _fullName = Regex.Unescape(fullNameValue);

                        // Set flag to indicate that the required field is spotted
                        fullNameSpecifierSpotted = true;
                    }

                    // Telephone (TEL;TYPE=cell,home:495-522-3560)
                    if (_value.StartsWith(_telephoneSpecifierWithType))
                    {
                        // Get the value
                        string telValue = _value.Substring(_telephoneSpecifierWithType.Length);
                        string[] splitTel = telValue.Split(_argumentDelimiter);
                        string[] splitTypes;
                        if (splitTel.Length != 2)
                            throw new InvalidDataException("Telephone field must specify exactly two values (Type (must be prepended with TYPE=), and phone number)");

                        // Check to see if the type is prepended with the TYPE= argument
                        if (splitTel[0].StartsWith(_typeArgumentSpecifier))
                            // Get the types
                            splitTypes = splitTel[0].Substring(_typeArgumentSpecifier.Length).Split(_valueDelimiter);
                        else if (string.IsNullOrEmpty(splitTel[0]))
                            // We're confronted with an empty type. Assume that it's a CELL.
                            splitTypes = new string[] { "CELL" };
                        else
                            // Trying to specify type without TYPE= is illegal according to RFC2426
                            throw new InvalidDataException("Telephone type must be prepended with TYPE=");

                        // Populate the fields
                        string[] _telephoneTypes = splitTypes;
                        string _telephoneNumber = Regex.Unescape(splitTel[1]);
                        TelephoneInfo _telephone = new(0, _telephoneTypes, _telephoneNumber);
                        _telephones.Add(_telephone);
                    }

                    // Telephone (TEL:495-522-3560)
                    if (_value.StartsWith(_telephoneSpecifier))
                    {
                        // Get the value
                        string telValue = _value.Substring(_telephoneSpecifier.Length);

                        // Populate the fields
                        string[] _telephoneTypes = new string[] { "CELL" };
                        string _telephoneNumber = Regex.Unescape(telValue);
                        TelephoneInfo _telephone = new(0, _telephoneTypes, _telephoneNumber);
                        _telephones.Add(_telephone);
                    }

                    // Address (ADR;TYPE=HOME:;;Los Angeles, USA;;;;)
                    if (_value.StartsWith(_addressSpecifierWithType))
                    {
                        // Get the value
                        string adrValue = _value.Substring(_addressSpecifierWithType.Length);
                        string[] splitAdr = adrValue.Split(_argumentDelimiter);
                        string[] splitTypes;
                        if (splitAdr.Length != 2)
                            throw new InvalidDataException("Address field must specify exactly two values (Type (must be prepended with TYPE=), and address information)");

                        // Check to see if the type is prepended with the TYPE= argument
                        if (splitAdr[0].StartsWith(_typeArgumentSpecifier))
                            // Get the types
                            splitTypes = splitAdr[0].Substring(_typeArgumentSpecifier.Length).Split(_valueDelimiter);
                        else if (string.IsNullOrEmpty(splitAdr[0]))
                            // We're confronted with an empty type. Assume that it's a HOME address.
                            splitTypes = new string[] { "HOME" };
                        else
                            // Trying to specify type without TYPE= is illegal according to RFC2426
                            throw new InvalidDataException("Address type must be prepended with TYPE=");

                        // Check the provided address
                        string[] splitAddressValues = splitAdr[1].Split(_fieldDelimiter);
                        if (splitAddressValues.Length != 7)
                            throw new InvalidDataException("Address information must specify exactly seven values (P.O. Box, extended address, street address, locality, region, postal code, and country)");

                        // Populate the fields
                        string[] _addressTypes = splitTypes;
                        string _addressPOBox = Regex.Unescape(splitAddressValues[0]);
                        string _addressExtended = Regex.Unescape(splitAddressValues[1]);
                        string _addressStreet = Regex.Unescape(splitAddressValues[2]);
                        string _addressLocality = Regex.Unescape(splitAddressValues[3]);
                        string _addressRegion = Regex.Unescape(splitAddressValues[4]);
                        string _addressPostalCode = Regex.Unescape(splitAddressValues[5]);
                        string _addressCountry = Regex.Unescape(splitAddressValues[6]);
                        AddressInfo _address = new(0, _addressTypes, _addressPOBox, _addressExtended, _addressStreet, _addressLocality, _addressRegion, _addressPostalCode, _addressCountry);
                        _addresses.Add(_address);
                    }

                    // Email (EMAIL;TYPE=HOME,INTERNET:john.s@acme.co)
                    if (_value.StartsWith(_emailSpecifier))
                    {
                        // Get the value
                        string mailValue = _value.Substring(_emailSpecifier.Length);
                        string[] splitMail = mailValue.Split(_argumentDelimiter);
                        string[] splitTypes;
                        MailAddress mail;
                        if (splitMail.Length != 2)
                            throw new InvalidDataException("E-mail field must specify exactly two values (Type (must be prepended with TYPE=), and a valid e-mail address)");

                        // Check to see if the type is prepended with the TYPE= argument
                        if (splitMail[0].StartsWith(_typeArgumentSpecifier))
                            // Get the types
                            splitTypes = splitMail[0].Substring(_typeArgumentSpecifier.Length).Split(_valueDelimiter);
                        else if (string.IsNullOrEmpty(splitMail[0]))
                            // We're confronted with an empty type. Assume that it's an INTERNET mail.
                            splitTypes = new string[] { "INTERNET" };
                        else
                            // Trying to specify type without TYPE= is illegal according to RFC2426
                            throw new InvalidDataException("E-mail type must be prepended with TYPE=");

                        // Try to create mail address
                        try
                        {
                            mail = new MailAddress(splitMail[1]);
                        }
                        catch (ArgumentException aex)
                        {
                            throw new InvalidDataException("E-mail address is invalid", aex);
                        }

                        // Populate the fields
                        string[] _emailTypes = splitTypes;
                        string _emailAddress = mail.Address;
                        EmailInfo _email = new(0, _emailTypes, _emailAddress);
                        _emails.Add(_email);
                    }

                    // Organization (ORG:Acme Co. or ORG:ABC, Inc.;North American Division;Marketing)
                    if (_value.StartsWith(_orgSpecifier))
                    {
                        // Get the value
                        string orgValue = _value.Substring(_orgSpecifier.Length);
                        string[] splitOrg = orgValue.Split(_fieldDelimiter);

                        // Populate the fields
                        string[] splitTypes = new string[] { "WORK" };
                        string _orgName = Regex.Unescape(splitOrg[0]);
                        string _orgUnit = Regex.Unescape(splitOrg.Length >= 2 ? splitOrg[1] : "");
                        string _orgUnitRole = Regex.Unescape(splitOrg.Length >= 3 ? splitOrg[2] : "");
                        OrganizationInfo _org = new(0, _orgName, _orgUnit, _orgUnitRole, splitTypes);
                        _orgs.Add(_org);
                    }

                    // Organization (ORG;TYPE=WORK:Acme Co. or ORG:ABC, Inc.;North American Division;Marketing)
                    if (_value.StartsWith(_orgSpecifierWithType))
                    {
                        // Get the value
                        string orgValue = _value.Substring(_orgSpecifierWithType.Length);
                        string[] splitOrg = orgValue.Split(_argumentDelimiter);
                        string[] splitTypes;
                        if (splitOrg.Length != 2)
                            throw new InvalidDataException("Organization field must specify exactly two values (Type (must be prepended with TYPE=), and address information)");

                        // Check to see if the type is prepended with the TYPE= argument
                        if (splitOrg[0].StartsWith(_typeArgumentSpecifier))
                            // Get the types
                            splitTypes = splitOrg[0].Substring(_typeArgumentSpecifier.Length).Split(_valueDelimiter);
                        else if (string.IsNullOrEmpty(splitOrg[0]))
                            // We're confronted with an empty type. Assume that it's a WORK organization.
                            splitTypes = new string[] { "WORK" };
                        else
                            // Trying to specify type without TYPE= is illegal according to RFC2426
                            throw new InvalidDataException("Organization type must be prepended with TYPE=");

                        // Check the provided organization
                        string[] splitOrganizationValues = splitOrg[1].Split(_fieldDelimiter);
                        if (splitOrganizationValues.Length != 3)
                            throw new InvalidDataException("Organization information must specify exactly three values (name, unit, and role)");

                        // Populate the fields
                        string _orgName = Regex.Unescape(splitOrganizationValues[0]);
                        string _orgUnit = Regex.Unescape(splitOrganizationValues.Length >= 2 ? splitOrganizationValues[1] : "");
                        string _orgUnitRole = Regex.Unescape(splitOrganizationValues.Length >= 3 ? splitOrganizationValues[2] : "");
                        OrganizationInfo _org = new(0, _orgName, _orgUnit, _orgUnitRole, splitTypes);
                        _orgs.Add(_org);
                    }

                    // Title (TITLE:Product Manager)
                    if (_value.StartsWith(_titleSpecifier))
                    {
                        // Get the value
                        string titleValue = _value.Substring(_titleSpecifier.Length);

                        // Populate field
                        string _title = Regex.Unescape(titleValue);
                        TitleInfo title = new(0, Array.Empty<string>(), _title);
                        _titles.Add(title);
                    }

                    // Website link (URL:https://sso.org/)
                    if (_value.StartsWith(_urlSpecifier))
                    {
                        // Get the value
                        string urlValue = _value.Substring(_urlSpecifier.Length);

                        // Try to parse the URL to ensure that it conforms to IETF RFC 1738: Uniform Resource Locators
                        if (!Uri.TryCreate(urlValue, UriKind.Absolute, out Uri uri))
                            throw new InvalidDataException($"URL {urlValue} is invalid");

                        // Populate field
                        _url = uri.ToString();
                    }

                    // Note (NOTE:Product Manager)
                    if (_value.StartsWith(_noteSpecifier))
                    {
                        // Get the value
                        string noteValue = _value.Substring(_noteSpecifier.Length);

                        // Populate field
                        _note = Regex.Unescape(noteValue);
                    }

                    // Photo (PHOTO;ENCODING=BASE64;JPEG:... or PHOTO;VALUE=URL:file:///jqpublic.gif or PHOTO;ENCODING=BASE64;TYPE=GIF:...)
                    if (_value.StartsWith(_photoSpecifierWithType))
                    {
                        // Get the value
                        string photoValue = _value.Substring(_photoSpecifierWithType.Length);
                        string[] splitPhoto = photoValue.Split(_argumentDelimiter);
                        string[] splitPhotoArgs = photoValue.Split(_fieldDelimiter);

                        // Check to see if the value is prepended by the VALUE= argument
                        bool isUrl = false;
                        string valueType = "";
                        if (splitPhotoArgs.Length == 1)
                        {
                            const string _valueArgumentSpecifier = "VALUE=";
                            valueType = splitPhotoArgs[0].Substring(_valueArgumentSpecifier.Length).ToLower();
                            isUrl = valueType == "url" || valueType == "uri";
                        }

                        // Check to see if the value is prepended by the ENCODING= argument
                        string photoEncoding = "";
                        if (splitPhotoArgs.Length >= 1)
                        {
                            const string _encodingArgumentSpecifier = "ENCODING=";
                            photoEncoding = splitPhotoArgs[0].Substring(_encodingArgumentSpecifier.Length);
                        }

                        // Check to see if the value is prepended with the TYPE= argument
                        string photoType = "";
                        if (splitPhotoArgs.Length >= 1)
                        {
                            photoType = splitPhotoArgs[1].StartsWith(_typeArgumentSpecifier) ?
                                        splitPhotoArgs[1].Substring(_typeArgumentSpecifier.Length) :
                                        splitPhotoArgs[1];
                        }

                        // Now, get the encoded photo
                        StringBuilder encodedPhoto = new();
                        if (splitPhoto.Length == 2)
                            encodedPhoto.Append(splitPhoto[1]);

                        // Make sure to get all the blocks until we reach an empty line
                        if (!isUrl)
                        {
                            string lineToBeAppended = CardContentReader.ReadLine();
                            while (!string.IsNullOrWhiteSpace(lineToBeAppended) && lineToBeAppended.StartsWith(" "))
                            {
                                encodedPhoto.Append(lineToBeAppended.Trim());
                                lineToBeAppended = CardContentReader.ReadLine();
                            }
                        }

                        // Populate the fields
                        PhotoInfo _photo = new(0, Array.Empty<string>(), valueType, photoEncoding, photoType, encodedPhoto.ToString());
                        _photos.Add(_photo);
                    }

                    // Logo (LOGO;ENCODING=BASE64;JPEG:... or LOGO;VALUE=URL:file:///jqpublic.gif or LOGO;ENCODING=BASE64;TYPE=GIF:...)
                    if (_value.StartsWith(_logoSpecifierWithType))
                    {
                        // Get the value
                        string logoValue = _value.Substring(_logoSpecifierWithType.Length);
                        string[] splitLogo = logoValue.Split(_argumentDelimiter);
                        string[] splitLogoArgs = logoValue.Split(_fieldDelimiter);

                        // Check to see if the value is prepended by the VALUE= argument
                        bool isUrl = false;
                        string valueType = "";
                        if (splitLogoArgs.Length == 1)
                        {
                            const string _valueArgumentSpecifier = "VALUE=";
                            valueType = splitLogoArgs[0].Substring(_valueArgumentSpecifier.Length).ToLower();
                            isUrl = valueType == "url" || valueType == "uri";
                        }

                        // Check to see if the value is prepended by the ENCODING= argument
                        string logoEncoding = "";
                        if (splitLogoArgs.Length >= 1)
                        {
                            const string _encodingArgumentSpecifier = "ENCODING=";
                            logoEncoding = splitLogoArgs[0].Substring(_encodingArgumentSpecifier.Length);
                        }

                        // Check to see if the value is prepended with the TYPE= argument
                        string logoType = "";
                        if (splitLogoArgs.Length >= 1)
                        {
                            logoType = splitLogoArgs[1].StartsWith(_typeArgumentSpecifier) ?
                                       splitLogoArgs[1].Substring(_typeArgumentSpecifier.Length) :
                                       splitLogoArgs[1];
                        }

                        // Now, get the encoded logo
                        StringBuilder encodedLogo = new();
                        if (splitLogo.Length == 2)
                            encodedLogo.Append(splitLogo[1]);

                        // Make sure to get all the blocks until we reach an empty line
                        if (!isUrl)
                        {
                            string lineToBeAppended = CardContentReader.ReadLine();
                            while (!string.IsNullOrWhiteSpace(lineToBeAppended) && lineToBeAppended.StartsWith(" "))
                            {
                                encodedLogo.Append(lineToBeAppended.Trim());
                                lineToBeAppended = CardContentReader.ReadLine();
                            }
                        }

                        // Populate the fields
                        LogoInfo _logo = new(0, Array.Empty<string>(), valueType, logoEncoding, logoType, encodedLogo.ToString());
                        _logos.Add(_logo);
                    }

                    // Sound (SOUND;VALUE=URL:file///multimed/audio/jqpublic.wav or SOUND;WAVE;BASE64:... or SOUND;TYPE=WAVE;ENCODING=BASE64:...)
                    if (_value.StartsWith(_soundSpecifierWithType))
                    {
                        // Get the value
                        string soundValue = _value.Substring(_soundSpecifierWithType.Length);
                        string[] splitSound = soundValue.Split(_argumentDelimiter);
                        string[] splitSoundArgs = soundValue.Split(_fieldDelimiter);

                        // Check to see if the value is prepended by the VALUE= argument
                        bool isUrl = false;
                        string valueType = "";
                        if (splitSoundArgs.Length == 1)
                        {
                            const string _valueArgumentSpecifier = "VALUE=";
                            valueType = splitSoundArgs[0].Substring(_valueArgumentSpecifier.Length).ToLower();
                            isUrl = valueType == "url" || valueType == "uri";
                        }

                        // Check to see if the value is prepended with the TYPE= argument
                        string soundType = "";
                        if (splitSoundArgs.Length > 1)
                        {
                            soundType = splitSoundArgs[0].StartsWith(_typeArgumentSpecifier) ?
                                        splitSoundArgs[0].Substring(_typeArgumentSpecifier.Length) :
                                        splitSoundArgs[0];
                        }

                        // Check to see if the value is prepended by the ENCODING= argument
                        string soundEncoding = "";
                        if (splitSoundArgs.Length > 1)
                        {
                            const string _encodingArgumentSpecifier = "ENCODING=";
                            soundEncoding = splitSoundArgs[1].StartsWith(_encodingArgumentSpecifier) ?
                                            splitSoundArgs[1].Substring(_encodingArgumentSpecifier.Length).Substring(0, splitSoundArgs[1].IndexOf(_argumentDelimiter)) :
                                            splitSoundArgs[1].Substring(0, splitSoundArgs[1].IndexOf(_argumentDelimiter));
                        }

                        // Now, get the encoded sound
                        StringBuilder encodedSound = new();
                        if (splitSound.Length == 2)
                            encodedSound.Append(splitSound[1]);

                        // Make sure to get all the blocks until we reach an empty line
                        if (!isUrl)
                        {
                            string lineToBeAppended = CardContentReader.ReadLine();
                            while (!string.IsNullOrWhiteSpace(lineToBeAppended))
                            {
                                encodedSound.Append(lineToBeAppended);
                                lineToBeAppended = CardContentReader.ReadLine()?.Trim();
                            }
                        }

                        // Populate the fields
                        SoundInfo _sound = new(0, Array.Empty<string>(), valueType, soundEncoding, soundType, encodedSound.ToString());
                        _sounds.Add(_sound);
                    }

                    // Revision (REV:1995-10-31T22:27:10Z or REV:19951031T222710)
                    if (_value.StartsWith(_revSpecifier))
                    {
                        // Get the value
                        string revValue = _value.Substring(_revSpecifier.Length);

                        // Populate field
                        _rev = DateTime.Parse(revValue);
                    }

                    // Nickname (NICKNAME;TYPE=work:Boss)
                    if (_value.StartsWith(_nicknameSpecifierWithType))
                    {
                        // Get the value
                        string nickValue = _value.Substring(_nicknameSpecifierWithType.Length);
                        string[] splitNick = nickValue.Split(_argumentDelimiter);
                        string[] splitTypes;
                        if (splitNick.Length != 2)
                            throw new InvalidDataException("Nickname field must specify exactly two values (Type (must be prepended with TYPE=), and nickname)");

                        // Check to see if the type is prepended with the TYPE= argument
                        if (splitNick[0].StartsWith(_typeArgumentSpecifier))
                            // Get the types
                            splitTypes = splitNick[0].Substring(_typeArgumentSpecifier.Length).Split(_valueDelimiter);
                        else if (string.IsNullOrEmpty(splitNick[0]))
                            // We're confronted with an empty type. Assume that it's HOME.
                            splitTypes = new string[] { "HOME" };
                        else
                            // Trying to specify type without TYPE= is illegal according to RFC2426
                            throw new InvalidDataException("Nick type must be prepended with TYPE=");

                        // Populate the fields
                        string[] _nicknameTypes = splitTypes;
                        string _nick = Regex.Unescape(splitNick[1]);
                        NicknameInfo _nickInstance = new(0, Array.Empty<string>(), _nick, _nicknameTypes);
                        _nicks.Add(_nickInstance);
                    }

                    // Nickname (NICKNAME:Jim)
                    if (_value.StartsWith(_nicknameSpecifier))
                    {
                        // Get the value
                        string nickValue = _value.Substring(_nicknameSpecifier.Length);

                        // Populate the fields
                        string[] _nicknameTypes = new string[] { "HOME" };
                        string _nick = Regex.Unescape(nickValue);
                        NicknameInfo _nickInstance = new(0, Array.Empty<string>(), _nick, _nicknameTypes);
                        _nicks.Add(_nickInstance);
                    }

                    // Birthdate (BDAY:19950415 or BDAY:1953-10-15T23:10:00Z)
                    if (_value.StartsWith(_birthSpecifier))
                    {
                        // Get the value
                        string bdayValue = _value.Substring(_birthSpecifier.Length);

                        // Populate field
                        _bday = DateTime.Parse(bdayValue);
                    }

                    // Mailer (MAILER:ccMail 2.2 or MAILER:PigeonMail 2.1)
                    if (_value.StartsWith(_mailerSpecifier))
                    {
                        // Get the value
                        string mailerValue = _value.Substring(_mailerSpecifier.Length);

                        // Populate field
                        _mailer = Regex.Unescape(mailerValue);
                    }

                    // Role (ROLE:Programmer)
                    if (_value.StartsWith(_roleSpecifier))
                    {
                        // Get the value
                        string roleValue = _value.Substring(_roleSpecifier.Length);

                        // Populate the fields
                        RoleInfo _role = new(0, Array.Empty<string>(), roleValue);
                        _roles.Add(_role);
                    }

                    // Categories (CATEGORIES:INTERNET or CATEGORIES:INTERNET,IETF,INDUSTRY,INFORMATION TECHNOLOGY)
                    if (_value.StartsWith(_categoriesSpecifier))
                    {
                        // Get the value
                        string categoriesValue = _value.Substring(_categoriesSpecifier.Length);

                        // Populate field
                        _categories.AddRange(Regex.Unescape(categoriesValue).Split(','));
                    }

                    // Product ID (PRODID:-//ONLINE DIRECTORY//NONSGML Version 1//EN)
                    if (_value.StartsWith(_productIdSpecifier))
                    {
                        // Get the value
                        string prodIdValue = _value.Substring(_productIdSpecifier.Length);

                        // Populate field
                        _prodId = Regex.Unescape(prodIdValue);
                    }

                    // Sort string (SORT-STRING:Harten)
                    if (_value.StartsWith(_sortStringSpecifier))
                    {
                        // Get the value
                        string sortStringValue = _value.Substring(_sortStringSpecifier.Length);

                        // Populate field
                        _sortString = Regex.Unescape(sortStringValue);
                    }

                    // Time Zone (TZ;VALUE=text:-05:00; EST; Raleigh/North America)
                    if (_value.StartsWith(_timeZoneSpecifierWithType))
                    {
                        // Get the value
                        string tzValue = _value.Substring(_timeZoneSpecifierWithType.Length);
                        string[] splitTz = tzValue.Split(_argumentDelimiter);
                        string[] splitTypes;
                        if (splitTz.Length != 1)
                            throw new InvalidDataException("Time Zone field must specify exactly one value (VALUE=\"text\" / \"uri\" / \"utc-offset\")");

                        // Check to see if the type is prepended with the VALUE= argument
                        if (splitTz[0].StartsWith(_valueArgumentSpecifier))
                            // Get the types
                            splitTypes = splitTz[0].Substring(_valueArgumentSpecifier.Length).Split(_valueDelimiter);
                        else if (string.IsNullOrEmpty(splitTz[0]))
                            // We're confronted with an empty type. Assume that it's a uri-offset.
                            splitTypes = new string[] { "uri-offset" };
                        else
                            // Trying to specify type without VALUE= is illegal according to RFC2426
                            throw new InvalidDataException("Time Zone type must be prepended with VALUE=");

                        // Populate the fields
                        string[] _timeZoneTypes = splitTypes;
                        string _timeZoneNumber = Regex.Unescape(splitTz[1]);
                        TimeZoneInfo _timeZone = new(0, _timeZoneTypes, _timeZoneNumber);
                        _timezones.Add(_timeZone);
                    }

                    // Time Zone (TZ:-05:00)
                    if (_value.StartsWith(_timeZoneSpecifier))
                    {
                        // Get the value
                        string tzValue = _value.Substring(_timeZoneSpecifier.Length);

                        // Populate the fields
                        string[] _timeZoneTypes = new string[] { "uri-offset" };
                        string _timeZoneNumber = Regex.Unescape(tzValue);
                        TimeZoneInfo _timeZone = new(0, _timeZoneTypes, _timeZoneNumber);
                        _timezones.Add(_timeZone);
                    }

                    // Geo (GEO;VALUE=uri:https://...)
                    if (_value.StartsWith(_geoSpecifierWithType))
                    {
                        // Get the value
                        string geoValue = _value.Substring(_geoSpecifierWithType.Length);
                        string[] splitGeo = geoValue.Split(_argumentDelimiter);
                        string[] splitTypes;
                        if (splitGeo.Length != 1)
                            throw new InvalidDataException("Geo field must specify exactly one value (VALUE=\"uri\")");

                        // Check to see if the type is prepended with the VALUE= argument
                        if (splitGeo[0].StartsWith(_valueArgumentSpecifier))
                            // Get the types
                            splitTypes = splitGeo[0].Substring(_valueArgumentSpecifier.Length).Split(_valueDelimiter);
                        else if (string.IsNullOrEmpty(splitGeo[0]))
                            // We're confronted with an empty type. Assume that it's a uri.
                            splitTypes = new string[] { "uri" };
                        else
                            // Trying to specify type without VALUE= is illegal according to RFC2426
                            throw new InvalidDataException("Geo type must be prepended with VALUE=");

                        // Populate the fields
                        string[] _geoTypes = splitTypes;
                        string _geoNumber = Regex.Unescape(splitGeo[1]);
                        GeoInfo _geo = new(0, _geoTypes, _geoNumber);
                        _geos.Add(_geo);
                    }

                    // Geo (GEO:geo:37.386013,-122.082932)
                    if (_value.StartsWith(_geoSpecifier))
                    {
                        // Get the value
                        string geoValue = _value.Substring(_geoSpecifier.Length);

                        // Populate the fields
                        string[] _geoTypes = new string[] { "uri" };
                        string _geoNumber = Regex.Unescape(geoValue);
                        GeoInfo _geo = new(0, _geoTypes, _geoNumber);
                        _geos.Add(_geo);
                    }

                    // IMPP information (IMPP;TYPE=home:sip:test)
                    if (_value.StartsWith(_imppSpecifierWithType))
                    {
                        // Get the value
                        string imppValue = _value.Substring(_imppSpecifierWithType.Length);
                        string[] splitImpp = imppValue.Split(_argumentDelimiter);

                        string[] splitTypes;
                        if (splitImpp.Length < 2)
                            throw new InvalidDataException("IMPP information field must specify exactly two values (Type (must be prepended with TYPE=), and impp)");

                        // Check to see if the type is prepended with the TYPE= argument
                        if (splitImpp[0].StartsWith(_typeArgumentSpecifier))
                            // Get the types
                            splitTypes = splitImpp[0].Substring(_typeArgumentSpecifier.Length).Split(_valueDelimiter);
                        else if (string.IsNullOrEmpty(splitImpp[0]))
                            // We're confronted with an empty type. Assume that it's HOME.
                            splitTypes = new string[] { "HOME" };
                        else
                            // Trying to specify type without TYPE= is illegal according to RFC2426
                            throw new InvalidDataException("IMPP type must be prepended with TYPE=");

                        // Populate the fields
                        string[] _imppTypes = splitTypes;
                        string _impp = Regex.Unescape(imppValue.Substring(imppValue.IndexOf(":") + 1));
                        ImppInfo _imppInstance = new(0, Array.Empty<string>(), _impp, _imppTypes);
                        _impps.Add(_imppInstance);
                    }

                    // IMPP information (IMPP:sip:test)
                    if (_value.StartsWith(_imppSpecifier))
                    {
                        // Get the value
                        string imppValue = _value.Substring(_imppSpecifier.Length);

                        // Populate the fields
                        string[] _imppTypes = new string[] { "HOME" };
                        string _impp = Regex.Unescape(imppValue);
                        ImppInfo _imppInstance = new(0, Array.Empty<string>(), _impp, _imppTypes);
                        _impps.Add(_imppInstance);
                    }

                    // X-nonstandard (X-AIM:john.s or X-DL;Design Work Group:List Item 1;List Item 2;List Item 3)
                    if (_value.StartsWith(_xSpecifier))
                    {
                        // Get the value
                        string xValue = _value.Substring(_xSpecifier.Length);
                        string[] splitX = xValue.Split(_argumentDelimiter);

                        // Populate the name
                        string _xName = splitX[0].Contains(_fieldDelimiter.ToString()) ?
                                        splitX[0].Substring(0, splitX[0].IndexOf(_fieldDelimiter)) :
                                        splitX[0];

                        // Populate the fields
                        string[] _xTypes = splitX[0].Contains(_fieldDelimiter.ToString()) ?
                                           splitX[0].Substring(splitX[0].IndexOf(_fieldDelimiter) + 1)
                                                    .Split(_fieldDelimiter) :
                                           Array.Empty<string>();
                        string[] _xValues = splitX[1].Split(_fieldDelimiter);
                        XNameInfo _x = new(0, _xName, _xValues, _xTypes);
                        _xes.Add(_x);
                    }
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
            return new Card(this, CardVersion, _names.ToArray(), _fullName, _telephones.ToArray(), _addresses.ToArray(), _orgs.ToArray(), _titles.ToArray(), _url, _note, _emails.ToArray(), _xes.ToArray(), "individual", _photos.ToArray(), _rev, _nicks.ToArray(), _bday, _mailer, _roles.ToArray(), _categories.ToArray(), _logos.ToArray(), _prodId, _sortString, _timezones.ToArray(), _geos.ToArray(), _sounds.ToArray(), _impps.ToArray());
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
                cardBuilder.AppendLine($"{_fullNameSpecifier}{card.ContactFullName}");
            foreach (NameInfo name in card.ContactNames)
            {
                string altNamesStr = string.Join(_valueDelimiter.ToString(), name.AltNames);
                string prefixesStr = string.Join(_valueDelimiter.ToString(), name.Prefixes);
                string suffixesStr = string.Join(_valueDelimiter.ToString(), name.Suffixes);
                cardBuilder.AppendLine(
                    $"{_nameSpecifier}" +
                    $"{name.ContactLastName}{_fieldDelimiter}" +
                    $"{name.ContactFirstName}{_fieldDelimiter}" +
                    $"{altNamesStr}{_fieldDelimiter}" +
                    $"{prefixesStr}{_fieldDelimiter}" +
                    $"{suffixesStr}{_fieldDelimiter}"
                );
            }

            // Now, start filling in the rest...
            foreach (TelephoneInfo telephone in card.ContactTelephones)
                cardBuilder.AppendLine(
                    $"{_telephoneSpecifierWithType}" +
                    $"TYPE={string.Join(",", telephone.ContactPhoneTypes)}{_argumentDelimiter}" +
                    $"{telephone.ContactPhoneNumber}"
                );
            foreach (AddressInfo address in card.ContactAddresses)
                cardBuilder.AppendLine(
                    $"{_addressSpecifierWithType}" +
                    $"TYPE={string.Join(",", address.AddressTypes)}{_argumentDelimiter}" +
                    $"{address.PostOfficeBox}{_fieldDelimiter}" +
                    $"{address.ExtendedAddress}{_fieldDelimiter}" +
                    $"{address.StreetAddress}{_fieldDelimiter}" +
                    $"{address.Locality}{_fieldDelimiter}" +
                    $"{address.Region}{_fieldDelimiter}" +
                    $"{address.PostalCode}{_fieldDelimiter}" +
                    $"{address.Country}"
                );
            foreach (EmailInfo email in card.ContactMails)
                cardBuilder.AppendLine(
                    $"{_emailSpecifier}" +
                    $"TYPE={string.Join(",", email.ContactEmailTypes)}{_argumentDelimiter}" +
                    $"{email.ContactEmailAddress}"
                );
            foreach (OrganizationInfo organization in card.ContactOrganizations)
                cardBuilder.AppendLine(
                    $"{_orgSpecifierWithType}" +
                    $"TYPE={string.Join(",", organization.OrgTypes)}{_argumentDelimiter}" +
                    $"{organization.Name}{_fieldDelimiter}" +
                    $"{organization.Unit}{_fieldDelimiter}" +
                    $"{organization.Role}"
                );
            foreach (TitleInfo title in card.ContactTitles)
                cardBuilder.AppendLine(
                    $"{_titleSpecifier}" +
                    $"{title.ContactTitle}"
                );
            if (!string.IsNullOrWhiteSpace(card.ContactURL))
                cardBuilder.AppendLine($"{_urlSpecifier}{card.ContactURL}");
            if (!string.IsNullOrWhiteSpace(card.ContactNotes))
                cardBuilder.AppendLine($"{_noteSpecifier}{card.ContactNotes}");
            foreach (PhotoInfo photo in card.ContactPhotos)
                cardBuilder.AppendLine(
                    $"{_photoSpecifierWithType}" +
                    $"VALUE={photo.ValueType}{_fieldDelimiter}" +
                    $"ENCODING={photo.Encoding}{_fieldDelimiter}" +
                    $"TYPE={photo.PhotoType}{_argumentDelimiter}" +
                    $"{photo.PhotoEncoded}"
                );
            foreach (LogoInfo logo in card.ContactLogos)
                cardBuilder.AppendLine(
                    $"{_logoSpecifierWithType}" +
                    $"VALUE={logo.ValueType}{_fieldDelimiter}" +
                    $"ENCODING={logo.Encoding}{_fieldDelimiter}" +
                    $"TYPE={logo.LogoType}{_argumentDelimiter}" +
                    $"{logo.LogoEncoded}"
                );
            foreach (SoundInfo sound in card.ContactSounds)
                cardBuilder.AppendLine(
                    $"{_soundSpecifierWithType}" +
                    $"VALUE={sound.ValueType}{_fieldDelimiter}" +
                    $"ENCODING={sound.Encoding}{_fieldDelimiter}" +
                    $"TYPE={sound.SoundType}{_argumentDelimiter}" +
                    $"{sound.SoundEncoded}"
                );
            if (card.CardRevision is not null && card.CardRevision != DateTime.MinValue)
                cardBuilder.AppendLine($"{_revSpecifier}{card.CardRevision:dd-MM-yyyy_HH-mm-ss}");
            foreach (NicknameInfo nickname in card.ContactNicknames)
                cardBuilder.AppendLine(
                    $"{_nicknameSpecifierWithType}" +
                    $"TYPE={string.Join(",", nickname.NicknameTypes)}{_argumentDelimiter}" +
                    $"{nickname.ContactNickname}"
                );
            if (card.ContactBirthdate is not null && card.ContactBirthdate != DateTime.MinValue)
                cardBuilder.AppendLine($"{_birthSpecifier}{card.ContactBirthdate:dd-MM-yyyy}");
            if (!string.IsNullOrWhiteSpace(card.ContactMailer))
                cardBuilder.AppendLine($"{_mailerSpecifier}{card.ContactMailer}");
            foreach (RoleInfo role in card.ContactRoles)
                cardBuilder.AppendLine(
                    $"{_roleSpecifier}" +
                    $"{role.ContactRole}"
                );
            if (card.ContactCategories is not null && card.ContactCategories.Length > 0)
                cardBuilder.AppendLine($"{_categoriesSpecifier}{string.Join(",", card.ContactCategories)}");
            if (!string.IsNullOrWhiteSpace(card.ContactProdId))
                cardBuilder.AppendLine($"{_productIdSpecifier}{card.ContactProdId}");
            if (!string.IsNullOrWhiteSpace(card.ContactSortString))
                cardBuilder.AppendLine($"{_sortStringSpecifier}{card.ContactSortString}");
            foreach (TimeZoneInfo timeZone in card.ContactTimeZone)
                cardBuilder.AppendLine(
                    $"{_timeZoneSpecifier}" +
                    $"{timeZone.TimeZone}"
                );
            foreach (GeoInfo geo in card.ContactGeo)
                cardBuilder.AppendLine(
                    $"{_geoSpecifier}" +
                    $"{geo.Geo}"
                );
            foreach (XNameInfo xname in card.ContactXNames)
                cardBuilder.AppendLine(
                    $"{_xSpecifier}" +
                    $"{xname.XKeyName}{(xname.XKeyTypes.Length > 0 ? _fieldDelimiter : _argumentDelimiter)}" +
                    $"{(xname.XKeyTypes.Length > 0 ? string.Join(_fieldDelimiter.ToString(), xname.XKeyTypes) + _argumentDelimiter : "")}" +
                    $"{string.Join(_fieldDelimiter.ToString(), xname.XValues)}"
                );

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
