﻿/*
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
        public override string CardContent { get; }
        public override string CardVersion { get; }

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
                    if (_value.StartsWith(VcardConstants._nameSpecifier))
                    {
                        // Check the line
                        string nameValue = _value.Substring(VcardConstants._nameSpecifier.Length);
                        string[] splitName = nameValue.Split(VcardConstants._fieldDelimiter);
                        if (splitName.Length < 2)
                            throw new InvalidDataException("Name field must specify exactly five values (Last name, first name, alt names, prefixes, and suffixes)");

                        // Populate fields
                        string _lastName = Regex.Unescape(splitName[0]);
                        string _firstName = Regex.Unescape(splitName[1]);
                        string[] _altNames = splitName.Length >= 3 ? Regex.Unescape(splitName[2]).Split(new char[] { VcardConstants._valueDelimiter }, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                        string[] _prefixes = splitName.Length >= 4 ? Regex.Unescape(splitName[3]).Split(new char[] { VcardConstants._valueDelimiter }, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                        string[] _suffixes = splitName.Length >= 5 ? Regex.Unescape(splitName[4]).Split(new char[] { VcardConstants._valueDelimiter }, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                        NameInfo _name = new(0, Array.Empty<string>(), _firstName, _lastName, _altNames, _prefixes, _suffixes);
                        _names.Add(_name);

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

                        // Set flag to indicate that the required field is spotted
                        fullNameSpecifierSpotted = true;
                    }

                    // Telephone (TEL;TYPE=cell,home:495-522-3560)
                    if (_value.StartsWith(VcardConstants._telephoneSpecifierWithType))
                    {
                        // Get the value
                        string telValue = _value.Substring(VcardConstants._telephoneSpecifierWithType.Length);
                        string[] splitTel = telValue.Split(VcardConstants._argumentDelimiter);
                        if (splitTel.Length < 2)
                            throw new InvalidDataException("Telephone field must specify exactly two values (Type (must be prepended with TYPE=), and phone number)");

                        // Check to see if the type is prepended with the TYPE= argument
                        string[] splitTelArgs = splitTel[0].Split(VcardConstants._argumentDelimiter);
                        var telArgType = splitTelArgs.Where((arg) => arg.StartsWith(VcardConstants._typeArgumentSpecifier) || !arg.Contains("=")).ToArray();
                        if (telArgType.Count() > 0 && !telArgType[0].StartsWith(VcardConstants._typeArgumentSpecifier))
                            // Trying to specify type without TYPE= is illegal according to RFC2426
                            throw new InvalidDataException("Telephone type must be prepended with TYPE=");
                        string telType =
                            telArgType.Count() > 0 ?
                            string.Join(VcardConstants._valueDelimiter.ToString(), telArgType.Select((arg) => arg.Substring(VcardConstants._typeArgumentSpecifier.Length))) :
                            "CELL";

                        // Populate the fields
                        string[] _telephoneTypes = telType.Split(VcardConstants._valueDelimiter);
                        string _telephoneNumber = Regex.Unescape(splitTel[1]);
                        TelephoneInfo _telephone = new(0, Array.Empty<string>(), _telephoneTypes, _telephoneNumber);
                        _telephones.Add(_telephone);
                    }

                    // Telephone (TEL:495-522-3560)
                    if (_value.StartsWith(VcardConstants._telephoneSpecifier))
                    {
                        // Get the value
                        string telValue = _value.Substring(VcardConstants._telephoneSpecifier.Length);

                        // Populate the fields
                        string[] _telephoneTypes = new string[] { "CELL" };
                        string _telephoneNumber = Regex.Unescape(telValue);
                        TelephoneInfo _telephone = new(0, Array.Empty<string>(), _telephoneTypes, _telephoneNumber);
                        _telephones.Add(_telephone);
                    }

                    // Address (ADR;TYPE=HOME:;;Los Angeles, USA;;;;)
                    if (_value.StartsWith(VcardConstants._addressSpecifierWithType))
                    {
                        // Get the value
                        string adrValue = _value.Substring(VcardConstants._addressSpecifierWithType.Length);
                        string[] splitAdr = adrValue.Split(VcardConstants._argumentDelimiter);
                        if (splitAdr.Length < 2)
                            throw new InvalidDataException("Address field must specify exactly two values (Type (must be prepended with TYPE=), and address information)");

                        // Check to see if the type is prepended with the TYPE= argument
                        string[] splitAdrArgs = splitAdr[0].Split(VcardConstants._argumentDelimiter);
                        var adrArgType = splitAdrArgs.Where((arg) => arg.StartsWith(VcardConstants._typeArgumentSpecifier) || !arg.Contains("=")).ToArray();
                        if (adrArgType.Count() > 0 && !adrArgType[0].StartsWith(VcardConstants._typeArgumentSpecifier))
                            // Trying to specify type without TYPE= is illegal according to RFC2426
                            throw new InvalidDataException("Address type must be prepended with TYPE=");
                        string adrType =
                            adrArgType.Count() > 0 ?
                            string.Join(VcardConstants._valueDelimiter.ToString(), adrArgType.Select((arg) => arg.Substring(VcardConstants._typeArgumentSpecifier.Length))) :
                            "HOME";

                        // Check the provided address
                        string[] splitAddressValues = splitAdr[1].Split(VcardConstants._fieldDelimiter);
                        if (splitAddressValues.Length < 7)
                            throw new InvalidDataException("Address information must specify exactly seven values (P.O. Box, extended address, street address, locality, region, postal code, and country)");

                        // Populate the fields
                        string[] _addressTypes = adrType.Split(VcardConstants._valueDelimiter);
                        string _addressPOBox = Regex.Unescape(splitAddressValues[0]);
                        string _addressExtended = Regex.Unescape(splitAddressValues[1]);
                        string _addressStreet = Regex.Unescape(splitAddressValues[2]);
                        string _addressLocality = Regex.Unescape(splitAddressValues[3]);
                        string _addressRegion = Regex.Unescape(splitAddressValues[4]);
                        string _addressPostalCode = Regex.Unescape(splitAddressValues[5]);
                        string _addressCountry = Regex.Unescape(splitAddressValues[6]);
                        AddressInfo _address = new(0, Array.Empty<string>(), _addressTypes, _addressPOBox, _addressExtended, _addressStreet, _addressLocality, _addressRegion, _addressPostalCode, _addressCountry);
                        _addresses.Add(_address);
                    }

                    // Email (EMAIL;TYPE=HOME,INTERNET:john.s@acme.co)
                    if (_value.StartsWith(VcardConstants._emailSpecifier))
                    {
                        // Get the value
                        string mailValue = _value.Substring(VcardConstants._emailSpecifier.Length);
                        string[] splitMail = mailValue.Split(VcardConstants._argumentDelimiter);
                        MailAddress mail;
                        if (splitMail.Length < 2)
                            throw new InvalidDataException("E-mail field must specify exactly two values (Type (must be prepended with TYPE=), and a valid e-mail address)");

                        // Check to see if the type is prepended with the TYPE= argument
                        string[] splitMailArgs = splitMail[0].Split(VcardConstants._argumentDelimiter);
                        var mailArgType = splitMailArgs.Where((arg) => arg.StartsWith(VcardConstants._typeArgumentSpecifier) || !arg.Contains("=")).ToArray();
                        if (mailArgType.Count() > 0 && !mailArgType[0].StartsWith(VcardConstants._typeArgumentSpecifier))
                            // Trying to specify type without TYPE= is illegal according to RFC2426
                            throw new InvalidDataException("Mail type must be prepended with TYPE=");
                        string mailType =
                            mailArgType.Count() > 0 ?
                            string.Join(VcardConstants._valueDelimiter.ToString(), mailArgType.Select((arg) => arg.Substring(VcardConstants._typeArgumentSpecifier.Length))) :
                            "INTERNET";

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
                        string[] _emailTypes = mailType.Split(VcardConstants._valueDelimiter);
                        string _emailAddress = mail.Address;
                        EmailInfo _email = new(0, Array.Empty<string>(), _emailTypes, _emailAddress);
                        _emails.Add(_email);
                    }

                    // Organization (ORG:Acme Co. or ORG:ABC, Inc.;North American Division;Marketing)
                    if (_value.StartsWith(VcardConstants._orgSpecifier))
                    {
                        // Get the value
                        string orgValue = _value.Substring(VcardConstants._orgSpecifier.Length);
                        string[] splitOrg = orgValue.Split(VcardConstants._fieldDelimiter);

                        // Populate the fields
                        string[] splitTypes = new string[] { "WORK" };
                        string _orgName = Regex.Unescape(splitOrg[0]);
                        string _orgUnit = Regex.Unescape(splitOrg.Length >= 2 ? splitOrg[1] : "");
                        string _orgUnitRole = Regex.Unescape(splitOrg.Length >= 3 ? splitOrg[2] : "");
                        OrganizationInfo _org = new(0, Array.Empty<string>(), _orgName, _orgUnit, _orgUnitRole, splitTypes);
                        _orgs.Add(_org);
                    }

                    // Organization (ORG;TYPE=WORK:Acme Co. or ORG:ABC, Inc.;North American Division;Marketing)
                    if (_value.StartsWith(VcardConstants._orgSpecifierWithType))
                    {
                        // Get the value
                        string orgValue = _value.Substring(VcardConstants._orgSpecifierWithType.Length);
                        string[] splitOrg = orgValue.Split(VcardConstants._argumentDelimiter);
                        if (splitOrg.Length < 2)
                            throw new InvalidDataException("Organization field must specify exactly two values (Type (must be prepended with TYPE=), and address information)");

                        // Check to see if the type is prepended with the TYPE= argument
                        string[] splitOrgArgs = splitOrg[0].Split(VcardConstants._argumentDelimiter);
                        var orgArgType = splitOrgArgs.Where((arg) => arg.StartsWith(VcardConstants._typeArgumentSpecifier) || !arg.Contains("=")).ToArray();
                        if (orgArgType.Count() > 0 && !orgArgType[0].StartsWith(VcardConstants._typeArgumentSpecifier))
                            // Trying to specify type without TYPE= is illegal according to RFC2426
                            throw new InvalidDataException("Organization type must be prepended with TYPE=");
                        string orgType =
                            orgArgType.Count() > 0 ?
                            string.Join(VcardConstants._valueDelimiter.ToString(), orgArgType.Select((arg) => arg.Substring(VcardConstants._typeArgumentSpecifier.Length))) :
                            "WORK";

                        // Check the provided organization
                        string[] splitOrganizationValues = splitOrg[1].Split(VcardConstants._fieldDelimiter);
                        if (splitOrganizationValues.Length < 3)
                            throw new InvalidDataException("Organization information must specify exactly three values (name, unit, and role)");

                        // Populate the fields
                        string[] _orgTypes = orgType.Split(VcardConstants._valueDelimiter);
                        string _orgName = Regex.Unescape(splitOrganizationValues[0]);
                        string _orgUnit = Regex.Unescape(splitOrganizationValues.Length >= 2 ? splitOrganizationValues[1] : "");
                        string _orgUnitRole = Regex.Unescape(splitOrganizationValues.Length >= 3 ? splitOrganizationValues[2] : "");
                        OrganizationInfo _org = new(0, Array.Empty<string>(), _orgName, _orgUnit, _orgUnitRole, _orgTypes);
                        _orgs.Add(_org);
                    }

                    // Title (TITLE:Product Manager)
                    if (_value.StartsWith(VcardConstants._titleSpecifier))
                    {
                        // Get the value
                        string titleValue = _value.Substring(VcardConstants._titleSpecifier.Length);

                        // Populate field
                        string _title = Regex.Unescape(titleValue);
                        TitleInfo title = new(0, Array.Empty<string>(), _title);
                        _titles.Add(title);
                    }

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
                    {
                        // Get the value
                        string photoValue = _value.Substring(VcardConstants._photoSpecifierWithType.Length);
                        string[] splitPhoto = photoValue.Split(VcardConstants._argumentDelimiter);
                        if (splitPhoto.Length >= 2)
                            throw new InvalidDataException("Photo field must specify exactly two values (Type and arguments, and photo information)");
                        string[] splitPhotoArgs = splitPhoto[0].Split(VcardConstants._fieldDelimiter);

                        // Check to see if the value is prepended by the VALUE= argument
                        var photoArgValue = splitPhotoArgs.Where((arg) => arg.StartsWith(VcardConstants._valueArgumentSpecifier));
                        string valueType = string.Join(VcardConstants._valueDelimiter.ToString(), photoArgValue.Select((arg) => arg.Substring(VcardConstants._valueArgumentSpecifier.Length).ToLower()));
                        bool isUrl = valueType == "url" || valueType == "uri";

                        // Check to see if the value is prepended by the ENCODING= argument
                        var photoArgEncoding = splitPhotoArgs.Where((arg) => arg.StartsWith(VcardConstants._encodingArgumentSpecifier));
                        string photoEncoding = string.Join(VcardConstants._valueDelimiter.ToString(), photoArgEncoding.Select((arg) => arg.Substring(VcardConstants._encodingArgumentSpecifier.Length)));

                        // Check to see if the value is prepended with the TYPE= argument
                        var photoArgType = splitPhotoArgs.Where((arg) => arg.StartsWith(VcardConstants._typeArgumentSpecifier) || !arg.Contains("=")).ToArray();
                        string photoType =
                            photoArgType.Count() > 0
                            ?
                                photoArgType[0].StartsWith(VcardConstants._typeArgumentSpecifier)
                                ?
                                    string.Join(VcardConstants._valueDelimiter.ToString(), photoArgType.Select((arg) => arg.Substring(VcardConstants._typeArgumentSpecifier.Length)))
                                :
                                    photoArgType[0]
                            :
                                "JPEG";

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
                    if (_value.StartsWith(VcardConstants._logoSpecifierWithType))
                    {
                        // Get the value
                        string logoValue = _value.Substring(VcardConstants._logoSpecifierWithType.Length);
                        string[] splitLogo = logoValue.Split(VcardConstants._argumentDelimiter);
                        if (splitLogo.Length >= 2)
                            throw new InvalidDataException("Logo field must specify exactly two values (Type and arguments, and logo information)");
                        string[] splitLogoArgs = splitLogo[0].Split(VcardConstants._fieldDelimiter);

                        // Check to see if the value is prepended by the VALUE= argument
                        var logoArgValue = splitLogoArgs.Where((arg) => arg.StartsWith(VcardConstants._valueArgumentSpecifier));
                        string valueType = string.Join(VcardConstants._valueDelimiter.ToString(), logoArgValue.Select((arg) => arg.Substring(VcardConstants._valueArgumentSpecifier.Length).ToLower()));
                        bool isUrl = valueType == "url" || valueType == "uri";

                        // Check to see if the value is prepended by the ENCODING= argument
                        var logoArgEncoding = splitLogoArgs.Where((arg) => arg.StartsWith(VcardConstants._encodingArgumentSpecifier));
                        string logoEncoding = string.Join(VcardConstants._valueDelimiter.ToString(), logoArgEncoding.Select((arg) => arg.Substring(VcardConstants._encodingArgumentSpecifier.Length)));

                        // Check to see if the value is prepended with the TYPE= argument
                        var logoArgType = splitLogoArgs.Where((arg) => arg.StartsWith(VcardConstants._typeArgumentSpecifier) || !arg.Contains("=")).ToArray();
                        string logoType =
                            logoArgType.Count() > 0
                            ?
                                logoArgType[0].StartsWith(VcardConstants._typeArgumentSpecifier)
                                ?
                                    string.Join(VcardConstants._valueDelimiter.ToString(), logoArgType.Select((arg) => arg.Substring(VcardConstants._typeArgumentSpecifier.Length)))
                                :
                                    logoArgType[0]
                            :
                                "JPEG";

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
                    if (_value.StartsWith(VcardConstants._soundSpecifierWithType))
                    {
                        // Get the value
                        string soundValue = _value.Substring(VcardConstants._soundSpecifierWithType.Length);
                        string[] splitSound = soundValue.Split(VcardConstants._argumentDelimiter);
                        if (splitSound.Length >= 2)
                            throw new InvalidDataException("Sound field must specify exactly two values (Type and arguments, and sound information)");
                        string[] splitSoundArgs = splitSound[0].Split(VcardConstants._fieldDelimiter);

                        // Check to see if the value is prepended by the VALUE= argument
                        var soundArgValue = splitSoundArgs.Where((arg) => arg.StartsWith(VcardConstants._valueArgumentSpecifier));
                        string valueType = string.Join(VcardConstants._valueDelimiter.ToString(), soundArgValue.Select((arg) => arg.Substring(VcardConstants._valueArgumentSpecifier.Length).ToLower()));
                        bool isUrl = valueType == "url" || valueType == "uri";

                        // Check to see if the value is prepended by the ENCODING= argument
                        var soundArgEncoding = splitSoundArgs.Where((arg) => arg.StartsWith(VcardConstants._encodingArgumentSpecifier));
                        string soundEncoding = string.Join(VcardConstants._valueDelimiter.ToString(), soundArgEncoding.Select((arg) => arg.Substring(VcardConstants._encodingArgumentSpecifier.Length)));

                        // Check to see if the value is prepended with the TYPE= argument
                        var soundArgType = splitSoundArgs.Where((arg) => arg.StartsWith(VcardConstants._typeArgumentSpecifier) || !arg.Contains("=")).ToArray();
                        string soundType =
                            soundArgType.Count() > 0
                            ?
                                soundArgType[0].StartsWith(VcardConstants._typeArgumentSpecifier)
                                ?
                                    string.Join(VcardConstants._valueDelimiter.ToString(), soundArgType.Select((arg) => arg.Substring(VcardConstants._typeArgumentSpecifier.Length)))
                                :
                                    soundArgType[0]
                            :
                                "WAVE";

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
                    if (_value.StartsWith(VcardConstants._revSpecifier))
                    {
                        // Get the value
                        string revValue = _value.Substring(VcardConstants._revSpecifier.Length);

                        // Populate field
                        _rev = DateTime.Parse(revValue);
                    }

                    // Nickname (NICKNAME;TYPE=work:Boss)
                    if (_value.StartsWith(VcardConstants._nicknameSpecifierWithType))
                    {
                        // Get the value
                        string nickValue = _value.Substring(VcardConstants._nicknameSpecifierWithType.Length);
                        string[] splitNick = nickValue.Split(VcardConstants._argumentDelimiter);
                        if (splitNick.Length < 2)
                            throw new InvalidDataException("Nickname field must specify exactly two values (Type (must be prepended with TYPE=), and nickname)");

                        // Check to see if the type is prepended with the TYPE= argument
                        string[] splitNickArgs = splitNick[0].Split(VcardConstants._argumentDelimiter);
                        var nickArgType = splitNickArgs.Where((arg) => arg.StartsWith(VcardConstants._typeArgumentSpecifier) || !arg.Contains("=")).ToArray();
                        if (nickArgType.Count() > 0 && !nickArgType[0].StartsWith(VcardConstants._typeArgumentSpecifier))
                            // Trying to specify type without TYPE= is illegal according to RFC2426
                            throw new InvalidDataException("Nickname type must be prepended with TYPE=");
                        string nickType =
                            nickArgType.Count() > 0 ?
                            string.Join(VcardConstants._valueDelimiter.ToString(), nickArgType.Select((arg) => arg.Substring(VcardConstants._typeArgumentSpecifier.Length))) :
                            "WORK";

                        // Populate the fields
                        string[] _nicknameTypes = nickType.Split(VcardConstants._valueDelimiter);
                        string _nick = Regex.Unescape(splitNick[1]);
                        NicknameInfo _nickInstance = new(0, Array.Empty<string>(), _nick, _nicknameTypes);
                        _nicks.Add(_nickInstance);
                    }

                    // Nickname (NICKNAME:Jim)
                    if (_value.StartsWith(VcardConstants._nicknameSpecifier))
                    {
                        // Get the value
                        string nickValue = _value.Substring(VcardConstants._nicknameSpecifier.Length);

                        // Populate the fields
                        string[] _nicknameTypes = new string[] { "HOME" };
                        string _nick = Regex.Unescape(nickValue);
                        NicknameInfo _nickInstance = new(0, Array.Empty<string>(), _nick, _nicknameTypes);
                        _nicks.Add(_nickInstance);
                    }

                    // Birthdate (BDAY:19950415 or BDAY:1953-10-15T23:10:00Z)
                    if (_value.StartsWith(VcardConstants._birthSpecifier))
                    {
                        // Get the value
                        string bdayValue = _value.Substring(VcardConstants._birthSpecifier.Length);

                        // Populate field
                        _bday = DateTime.Parse(bdayValue);
                    }

                    // Mailer (MAILER:ccMail 2.2 or MAILER:PigeonMail 2.1)
                    if (_value.StartsWith(VcardConstants._mailerSpecifier))
                    {
                        // Get the value
                        string mailerValue = _value.Substring(VcardConstants._mailerSpecifier.Length);

                        // Populate field
                        _mailer = Regex.Unescape(mailerValue);
                    }

                    // Role (ROLE:Programmer)
                    if (_value.StartsWith(VcardConstants._roleSpecifier))
                    {
                        // Get the value
                        string roleValue = _value.Substring(VcardConstants._roleSpecifier.Length);

                        // Populate the fields
                        RoleInfo _role = new(0, Array.Empty<string>(), roleValue);
                        _roles.Add(_role);
                    }

                    // Categories (CATEGORIES:INTERNET or CATEGORIES:INTERNET,IETF,INDUSTRY,INFORMATION TECHNOLOGY)
                    if (_value.StartsWith(VcardConstants._categoriesSpecifier))
                    {
                        // Get the value
                        string categoriesValue = _value.Substring(VcardConstants._categoriesSpecifier.Length);

                        // Populate field
                        _categories.AddRange(Regex.Unescape(categoriesValue).Split(','));
                    }

                    // Product ID (PRODID:-//ONLINE DIRECTORY//NONSGML Version 1//EN)
                    if (_value.StartsWith(VcardConstants._productIdSpecifier))
                    {
                        // Get the value
                        string prodIdValue = _value.Substring(VcardConstants._productIdSpecifier.Length);

                        // Populate field
                        _prodId = Regex.Unescape(prodIdValue);
                    }

                    // Sort string (SORT-STRING:Harten)
                    if (_value.StartsWith(VcardConstants._sortStringSpecifier))
                    {
                        // Get the value
                        string sortStringValue = _value.Substring(VcardConstants._sortStringSpecifier.Length);

                        // Populate field
                        _sortString = Regex.Unescape(sortStringValue);
                    }

                    // Time Zone (TZ;VALUE=text:-05:00; EST; Raleigh/North America)
                    if (_value.StartsWith(VcardConstants._timeZoneSpecifierWithType))
                    {
                        // Get the value
                        string tzValue = _value.Substring(VcardConstants._timeZoneSpecifierWithType.Length);
                        string[] splitTz = tzValue.Split(VcardConstants._argumentDelimiter);
                        if (splitTz.Length < 2)
                            throw new InvalidDataException("Time Zone field must specify exactly two values (VALUE=\"text\" / \"uri\" / \"utc-offset\", and time zone info)");

                        // Check to see if the type is prepended with the VALUE= argument
                        string[] splitTzArgs = splitTz[0].Split(VcardConstants._argumentDelimiter);
                        var tzArgType = splitTzArgs.Where((arg) => arg.StartsWith(VcardConstants._valueArgumentSpecifier) || !arg.Contains("=")).ToArray();
                        if (tzArgType.Count() > 0 && !tzArgType[0].StartsWith(VcardConstants._valueArgumentSpecifier))
                            // Trying to specify type without VALUE= is illegal according to RFC2426
                            throw new InvalidDataException("Time Zone type must be prepended with VALUE=");
                        string tzType =
                            tzArgType.Count() > 0 ?
                            string.Join(VcardConstants._valueDelimiter.ToString(), tzArgType.Select((arg) => arg.Substring(VcardConstants._typeArgumentSpecifier.Length))) :
                            "uri-offset";

                        // Populate the fields
                        string[] _timeZoneTypes = tzType.Split(VcardConstants._valueDelimiter);
                        string _timeZoneNumber = Regex.Unescape(splitTz[1]);
                        TimeZoneInfo _timeZone = new(0, Array.Empty<string>(), _timeZoneTypes, _timeZoneNumber);
                        _timezones.Add(_timeZone);
                    }

                    // Time Zone (TZ:-05:00)
                    if (_value.StartsWith(VcardConstants._timeZoneSpecifier))
                    {
                        // Get the value
                        string tzValue = _value.Substring(VcardConstants._timeZoneSpecifier.Length);

                        // Populate the fields
                        string[] _timeZoneTypes = new string[] { "uri-offset" };
                        string _timeZoneNumber = Regex.Unescape(tzValue);
                        TimeZoneInfo _timeZone = new(0, Array.Empty<string>(), _timeZoneTypes, _timeZoneNumber);
                        _timezones.Add(_timeZone);
                    }

                    // Geo (GEO;VALUE=uri:https://...)
                    if (_value.StartsWith(VcardConstants._geoSpecifierWithType))
                    {
                        // Get the value
                        string geoValue = _value.Substring(VcardConstants._geoSpecifierWithType.Length);
                        string[] splitGeo = geoValue.Split(VcardConstants._argumentDelimiter);
                        if (splitGeo.Length < 2)
                            throw new InvalidDataException("Geo field must specify exactly two values (VALUE=\"uri\", and geo info)");

                        // Check to see if the type is prepended with the VALUE= argument
                        string[] splitGeoArgs = splitGeo[0].Split(VcardConstants._argumentDelimiter);
                        var geoArgType = splitGeoArgs.Where((arg) => arg.StartsWith(VcardConstants._valueArgumentSpecifier) || !arg.Contains("=")).ToArray();
                        if (geoArgType.Count() > 0 && !geoArgType[0].StartsWith(VcardConstants._valueArgumentSpecifier))
                            // Trying to specify type without VALUE= is illegal according to RFC2426
                            throw new InvalidDataException("Geo type must be prepended with VALUE=");
                        string geoType =
                            geoArgType.Count() > 0 ?
                            string.Join(VcardConstants._valueDelimiter.ToString(), geoArgType.Select((arg) => arg.Substring(VcardConstants._typeArgumentSpecifier.Length))) :
                            "uri";

                        // Populate the fields
                        string[] _geoTypes = geoType.Split(VcardConstants._valueDelimiter);
                        string _geoNumber = Regex.Unescape(splitGeo[1]);
                        GeoInfo _geo = new(0, Array.Empty<string>(), _geoTypes, _geoNumber);
                        _geos.Add(_geo);
                    }

                    // Geo (GEO:geo:37.386013,-122.082932)
                    if (_value.StartsWith(VcardConstants._geoSpecifier))
                    {
                        // Get the value
                        string geoValue = _value.Substring(VcardConstants._geoSpecifier.Length);

                        // Populate the fields
                        string[] _geoTypes = new string[] { "uri" };
                        string _geoNumber = Regex.Unescape(geoValue);
                        GeoInfo _geo = new(0, Array.Empty<string>(), _geoTypes, _geoNumber);
                        _geos.Add(_geo);
                    }

                    // IMPP information (IMPP;TYPE=home:sip:test)
                    if (_value.StartsWith(VcardConstants._imppSpecifierWithType))
                    {
                        // Get the value
                        string imppValue = _value.Substring(VcardConstants._imppSpecifierWithType.Length);
                        string[] splitImpp = imppValue.Split(VcardConstants._argumentDelimiter);
                        if (splitImpp.Length < 2)
                            throw new InvalidDataException("IMPP information field must specify exactly two values (Type (must be prepended with TYPE=), and impp)");

                        // Check to see if the type is prepended with the TYPE= argument
                        string[] splitImppArgs = splitImpp[0].Split(VcardConstants._argumentDelimiter);
                        var imppArgType = splitImppArgs.Where((arg) => arg.StartsWith(VcardConstants._typeArgumentSpecifier) || !arg.Contains("=")).ToArray();
                        if (imppArgType.Count() > 0 && !imppArgType[0].StartsWith(VcardConstants._typeArgumentSpecifier))
                            // Trying to specify type without TYPE= is illegal according to RFC2426
                            throw new InvalidDataException("IMPP type must be prepended with TYPE=");
                        string imppType =
                            imppArgType.Count() > 0 ?
                            string.Join(VcardConstants._valueDelimiter.ToString(), imppArgType.Select((arg) => arg.Substring(VcardConstants._typeArgumentSpecifier.Length))) :
                            "HOME";

                        // Populate the fields
                        string[] _imppTypes = imppType.Split(VcardConstants._valueDelimiter);
                        string _impp = Regex.Unescape(imppValue.Substring(imppValue.IndexOf(":") + 1));
                        ImppInfo _imppInstance = new(0, Array.Empty<string>(), _impp, _imppTypes);
                        _impps.Add(_imppInstance);
                    }

                    // IMPP information (IMPP:sip:test)
                    if (_value.StartsWith(VcardConstants._imppSpecifier))
                    {
                        // Get the value
                        string imppValue = _value.Substring(VcardConstants._imppSpecifier.Length);

                        // Populate the fields
                        string[] _imppTypes = new string[] { "HOME" };
                        string _impp = Regex.Unescape(imppValue);
                        ImppInfo _imppInstance = new(0, Array.Empty<string>(), _impp, _imppTypes);
                        _impps.Add(_imppInstance);
                    }

                    // X-nonstandard (X-AIM:john.s or X-DL;Design Work Group:List Item 1;List Item 2;List Item 3)
                    if (_value.StartsWith(VcardConstants._xSpecifier))
                    {
                        // Get the value
                        string xValue = _value.Substring(VcardConstants._xSpecifier.Length);
                        string[] splitX = xValue.Split(VcardConstants._argumentDelimiter);

                        // Populate the name
                        string _xName = splitX[0].Contains(VcardConstants._fieldDelimiter.ToString()) ?
                                        splitX[0].Substring(0, splitX[0].IndexOf(VcardConstants._fieldDelimiter)) :
                                        splitX[0];

                        // Populate the fields
                        string[] _xTypes = splitX[0].Contains(VcardConstants._fieldDelimiter.ToString()) ?
                                           splitX[0].Substring(splitX[0].IndexOf(VcardConstants._fieldDelimiter) + 1)
                                                    .Split(VcardConstants._fieldDelimiter) :
                                           Array.Empty<string>();
                        string[] _xValues = splitX[1].Split(VcardConstants._fieldDelimiter);
                        XNameInfo _x = new(0, Array.Empty<string>(), _xName, _xValues, _xTypes);
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
                cardBuilder.AppendLine($"{VcardConstants._fullNameSpecifier}{card.ContactFullName}");
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
                cardBuilder.AppendLine($"{VcardConstants._urlSpecifier}{card.ContactURL}");
            if (!string.IsNullOrWhiteSpace(card.ContactNotes))
                cardBuilder.AppendLine($"{VcardConstants._noteSpecifier}{card.ContactNotes}");
            foreach (PhotoInfo photo in card.ContactPhotos)
                cardBuilder.AppendLine(photo.ToStringVcardThree());
            foreach (LogoInfo logo in card.ContactLogos)
                cardBuilder.AppendLine(logo.ToStringVcardThree());
            foreach (SoundInfo sound in card.ContactSounds)
                cardBuilder.AppendLine(sound.ToStringVcardThree());
            if (card.CardRevision is not null && card.CardRevision != DateTime.MinValue)
                cardBuilder.AppendLine($"{VcardConstants._revSpecifier}{card.CardRevision:dd-MM-yyyy_HH-mm-ss}");
            foreach (NicknameInfo nickname in card.ContactNicknames)
                cardBuilder.AppendLine(nickname.ToStringVcardThree());
            if (card.ContactBirthdate is not null && card.ContactBirthdate != DateTime.MinValue)
                cardBuilder.AppendLine($"{VcardConstants._birthSpecifier}{card.ContactBirthdate:dd-MM-yyyy}");
            if (!string.IsNullOrWhiteSpace(card.ContactMailer))
                cardBuilder.AppendLine($"{VcardConstants._mailerSpecifier}{card.ContactMailer}");
            foreach (RoleInfo role in card.ContactRoles)
                cardBuilder.AppendLine(role.ToStringVcardThree());
            if (card.ContactCategories is not null && card.ContactCategories.Length > 0)
                cardBuilder.AppendLine($"{VcardConstants._categoriesSpecifier}{string.Join(",", card.ContactCategories)}");
            if (!string.IsNullOrWhiteSpace(card.ContactProdId))
                cardBuilder.AppendLine($"{VcardConstants._productIdSpecifier}{card.ContactProdId}");
            if (!string.IsNullOrWhiteSpace(card.ContactSortString))
                cardBuilder.AppendLine($"{VcardConstants._sortStringSpecifier}{card.ContactSortString}");
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
