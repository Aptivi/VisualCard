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

namespace VisualCard.Parsers
{
    /// <summary>
    /// Parser for VCard version 4.0. Consult the vcard-40-rfc6350.txt file in source for the specification.
    /// </summary>
    public class VcardFour : BaseVcardParser, IVcardParser
    {
        public override string CardPath { get; }
        public override string CardContent { get; }
        public override string CardVersion { get; }

        // Some VCard 4.0 constants
        const char _fieldDelimiter                  = ';';
        const char _valueDelimiter                  = ',';
        const char _argumentDelimiter               = ':';
        const string _kindSpecifier                 = "KIND:";
        const string _nameSpecifierWithType         = "N;";
        const string _nameSpecifier                 = "N:";
        const string _fullNameSpecifier             = "FN:";
        const string _telephoneSpecifierWithType    = "TEL;";
        const string _telephoneSpecifier            = "TEL:";
        const string _addressSpecifierWithType      = "ADR;";
        const string _emailSpecifier                = "EMAIL;";
        const string _orgSpecifier                  = "ORG:";
        const string _titleSpecifier                = "TITLE:";
        const string _titleSpecifierWithArguments   = "TITLE;";
        const string _urlSpecifier                  = "URL:";
        const string _noteSpecifier                 = "NOTE:";
        const string _photoSpecifierWithType        = "PHOTO;";
        const string _soundSpecifierWithType        = "SOUND;";
        const string _revSpecifier                  = "REV:";
        const string _nicknameSpecifier             = "NICKNAME:";
        const string _nicknameSpecifierWithType     = "NICKNAME;";
        const string _birthSpecifier                = "BDAY:";
        const string _roleSpecifier                 = "ROLE:";
        const string _roleSpecifierWithType         = "ROLE;";
        const string _categoriesSpecifier           = "CATEGORIES:";
        const string _xSpecifier                    = "X-";
        const string _typeArgumentSpecifier         = "TYPE=";
        const string _altIdArgumentSpecifier        = "ALTID=";

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
            DateTime _rev                   = DateTime.MinValue;
            DateTime _bday                  = DateTime.MinValue;
            List<NameInfo> _names           = new();
            List<TelephoneInfo> _telephones = new();
            List<EmailInfo> _emails         = new();
            List<AddressInfo> _addresses    = new();
            List<OrganizationInfo> _orgs    = new();
            List<TitleInfo> _titles         = new();
            List<PhotoInfo> _photos         = new();
            List<SoundInfo> _sounds         = new();
            List<NicknameInfo> _nicks       = new();
            List<RoleInfo> _roles           = new();
            List<string> _categories        = new();
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
                    string[] splitValueParts = _value.Split(_argumentDelimiter);
                    string[] splitArgs = splitValueParts[0].Split(_fieldDelimiter);
                    splitArgs = splitArgs.Except(new string[] { splitArgs[0] }).ToArray();
                    string[] splitValues = splitValueParts[1].Split(_fieldDelimiter);
                    List<string> finalArgs = new();
                    int altId = 0;

                    if (splitArgs.Length > 0)
                    {
                        // If we have more than one argument, check for ALTID
                        if (splitArgs[0].StartsWith(_altIdArgumentSpecifier))
                        {
                            if (!int.TryParse(splitArgs[0].Substring(_altIdArgumentSpecifier.Length), out altId))
                                throw new InvalidDataException("ALTID must be numeric");

                            // Here, we require arguments for ALTID
                            if (splitArgs.Length <= 1)
                                throw new InvalidDataException("ALTID must have one or more arguments to specify why is this instance an alternative");
                        }

                        // Finalize the arguments
                        if (splitArgs[0].StartsWith(_altIdArgumentSpecifier))
                            finalArgs.AddRange(splitArgs.Except(new string[] { splitArgs[0] }));
                        else
                            finalArgs.AddRange(splitArgs);
                    }

                    // Card type (KIND:individual, KIND:group, KIND:org, KIND:location, ...)
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(_kindSpecifier))
                    {
                        // Get the value
                        string kindValue = _value.Substring(_kindSpecifier.Length);

                        // Populate field
                        if (!string.IsNullOrEmpty(kindValue))
                            _kind = Regex.Unescape(kindValue);
                    }

                    // The name (N:Sanders;John;;;)
                    // ALTID is supported. Refer below for info.
                    if (_value.StartsWith(_nameSpecifier))
                    {
                        // Check the line
                        if (splitValues.Length < 2)
                            throw new InvalidDataException("Name field must specify exactly five values (Last name, first name, alt names, prefixes, and suffixes)");

                        // Check to see if there are any names with altid
                        if (idReservedForName)
                            throw new InvalidDataException("Attempted to overwrite name under the main ID.");

                        // Populate fields
                        string _lastName   = Regex.Unescape(splitValues[0]);
                        string _firstName  = Regex.Unescape(splitValues[1]);
                        string[] _altNames = splitValues.Length >= 3 ? Regex.Unescape(splitValues[2]).Split(new char[] { _valueDelimiter }) : Array.Empty<string>();
                        string[] _prefixes = splitValues.Length >= 4 ? Regex.Unescape(splitValues[3]).Split(new char[] { _valueDelimiter }) : Array.Empty<string>();
                        string[] _suffixes = splitValues.Length >= 5 ? Regex.Unescape(splitValues[4]).Split(new char[] { _valueDelimiter }) : Array.Empty<string>();
                        NameInfo _name = new(0, Array.Empty<string>(), _firstName, _lastName, _altNames, _prefixes, _suffixes);
                        _names.Add(_name);

                        // Since we've reserved id 0, set the flag
                        idReservedForName = true;
                    }

                    // The name (N;ALTID=1;LANGUAGE=en:Sanders;John;;;)
                    // ALTID is supported.
                    if (_value.StartsWith(_nameSpecifierWithType))
                    {
                        // Check the line
                        string nameValue = _value.Substring(_nameSpecifierWithType.Length);
                        string[] splitNameParts = nameValue.Split(_argumentDelimiter);
                        string[] splitName = splitNameParts[1].Split(_fieldDelimiter);
                        if (splitName.Length < 2)
                            throw new InvalidDataException("Name field must specify exactly five values (Last name, first name, alt names, prefixes, and suffixes)");

                        // Check the ALTID
                        if (!splitArgs[0].StartsWith(_altIdArgumentSpecifier))
                            throw new InvalidDataException("ALTID must come exactly first");

                        // ALTID: N: has cardinality of *1
                        if (idReservedForName && _names.Count > 0 && _names[0].AltId != altId)
                            throw new InvalidDataException("ALTID may not be different from all the alternative argument names");

                        // Populate fields
                        string _lastName   = Regex.Unescape(splitName[0]);
                        string _firstName  = Regex.Unescape(splitName[1]);
                        string[] _altNames = splitName.Length >= 3 ? Regex.Unescape(splitName[2]).Split(new char[] { _valueDelimiter }) : Array.Empty<string>();
                        string[] _prefixes = splitName.Length >= 4 ? Regex.Unescape(splitName[3]).Split(new char[] { _valueDelimiter }) : Array.Empty<string>();
                        string[] _suffixes = splitName.Length >= 5 ? Regex.Unescape(splitName[4]).Split(new char[] { _valueDelimiter }) : Array.Empty<string>();
                        NameInfo _name = new(altId, finalArgs.ToArray(), _firstName, _lastName, _altNames, _prefixes, _suffixes);
                        _names.Add(_name);

                        // Since we've reserved a specific id, set the flag
                        idReservedForName = true;
                    }

                    // Full name (FN:John Sanders)
                    // Here, we don't support ALTID.
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
                    // Here, we don't support ALTID.
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
                        TelephoneInfo _telephone = new(_telephoneTypes, _telephoneNumber);
                        _telephones.Add(_telephone);
                    }

                    // Telephone (TEL:495-522-3560)
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(_telephoneSpecifier))
                    {
                        // Get the value
                        string telValue = _value.Substring(_telephoneSpecifier.Length);

                        // Populate the fields
                        string[] _telephoneTypes = new string[] { "CELL" };
                        string _telephoneNumber = Regex.Unescape(telValue);
                        TelephoneInfo _telephone = new(_telephoneTypes, _telephoneNumber);
                        _telephones.Add(_telephone);
                    }

                    // Address (ADR;TYPE=HOME:;;Los Angeles, USA;;;;)
                    // Here, we don't support ALTID.
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
                        AddressInfo _address = new(_addressTypes, _addressPOBox, _addressExtended, _addressStreet, _addressLocality, _addressRegion, _addressPostalCode, _addressCountry);
                        _addresses.Add(_address);
                    }

                    // Email (EMAIL;TYPE=HOME,INTERNET:john.s@acme.co)
                    // Here, we don't support ALTID.
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
                        EmailInfo _email = new(_emailTypes, _emailAddress);
                        _emails.Add(_email);
                    }

                    // Organization (ORG:Acme Co. or ORG:ABC, Inc.;North American Division;Marketing)
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(_orgSpecifier))
                    {
                        // Get the value
                        string orgValue = _value.Substring(_orgSpecifier.Length);
                        string[] splitOrg = orgValue.Split(_fieldDelimiter);

                        // Populate the fields
                        string _orgName = Regex.Unescape(splitOrg[0]);
                        string _orgUnit = Regex.Unescape(splitOrg.Length >= 2 ? splitOrg[1] : "");
                        string _orgUnitRole = Regex.Unescape(splitOrg.Length >= 3 ? splitOrg[2] : "");
                        OrganizationInfo _org = new(_orgName, _orgUnit, _orgUnitRole);
                        _orgs.Add(_org);
                    }

                    // Title (TITLE:Product Manager)
                    // ALTID is supported. Refer below for info.
                    if (_value.StartsWith(_titleSpecifier))
                    {
                        // Get the value
                        string titleValue = _value.Substring(_titleSpecifier.Length);

                        // Populate field
                        string _title = Regex.Unescape(titleValue);
                        TitleInfo title = new(0, Array.Empty<string>(), _title);
                        _titles.Add(title);
                    }

                    // Title (TITLE;ALTID=1;LANGUAGE=fr:Patron or TITLE;LANGUAGE=fr:Patron)
                    // ALTID is supported.
                    if (_value.StartsWith(_titleSpecifierWithArguments))
                    {
                        // Get the value
                        string titleValue = _value.Substring(_titleSpecifierWithArguments.Length);
                        string[] splitTitleParts = titleValue.Split(_argumentDelimiter);

                        // Populate field
                        string _title = Regex.Unescape(splitTitleParts[1]);
                        TitleInfo title = new(altId, finalArgs.ToArray(), _title);
                        _titles.Add(title);
                    }

                    // Website link (URL:https://sso.org/)
                    // Here, we don't support ALTID.
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
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(_noteSpecifier))
                    {
                        // Get the value
                        string noteValue = _value.Substring(_noteSpecifier.Length);

                        // Populate field
                        _note = Regex.Unescape(noteValue);
                    }

                    // Photo (PHOTO;ENCODING=BASE64;JPEG:... or PHOTO;VALUE=URL:file:///jqpublic.gif or PHOTO;ENCODING=BASE64;TYPE=GIF:...)
                    // ALTID is supported.
                    if (_value.StartsWith(_photoSpecifierWithType))
                    {
                        // Get the value
                        string photoValue = _value.Substring(_photoSpecifierWithType.Length);
                        string[] splitPhoto = photoValue.Split(_argumentDelimiter);

                        // Check to see if the value is prepended by the VALUE= argument
                        bool isUrl = false;
                        string valueType = "";
                        if (splitArgs.Length == 1)
                        {
                            const string _valueArgumentSpecifier = "VALUE=";
                            valueType = splitArgs[0].Substring(_valueArgumentSpecifier.Length).ToLower();
                            isUrl = valueType == "url" || valueType == "uri";
                        }

                        // Check to see if the value is prepended by the ENCODING= argument
                        string photoEncoding = "";
                        if (splitArgs.Length >= 1)
                        {
                            const string _encodingArgumentSpecifier = "ENCODING=";
                            photoEncoding = splitArgs[0].Substring(_encodingArgumentSpecifier.Length);
                        }

                        // Check to see if the value is prepended with the TYPE= argument
                        string photoType = "";
                        if (splitArgs.Length >= 1)
                        {
                            photoType = splitArgs[1].StartsWith(_typeArgumentSpecifier) ?
                                        splitArgs[1].Substring(_typeArgumentSpecifier.Length) :
                                        splitArgs[1];
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
                        PhotoInfo _photo = new(altId, finalArgs.ToArray(), valueType, photoEncoding, photoType, encodedPhoto.ToString());
                        _photos.Add(_photo);
                    }

                    // Sound (SOUND;VALUE=URL:file///multimed/audio/jqpublic.wav or SOUND;WAVE;BASE64:... or SOUND;TYPE=WAVE;ENCODING=BASE64:...)
                    // ALTID is supported.
                    if (_value.StartsWith(_soundSpecifierWithType))
                    {
                        // Get the value
                        string soundValue = _value.Substring(_soundSpecifierWithType.Length);
                        string[] splitSound = soundValue.Split(_argumentDelimiter);

                        // Check to see if the value is prepended by the VALUE= argument
                        bool isUrl = false;
                        string valueType = "";
                        if (splitArgs.Length == 1)
                        {
                            const string _valueArgumentSpecifier = "VALUE=";
                            valueType = splitArgs[0].Substring(_valueArgumentSpecifier.Length).ToLower();
                            isUrl = valueType == "url" || valueType == "uri";
                        }

                        // Check to see if the value is prepended with the TYPE= argument
                        string soundType = "";
                        if (splitArgs.Length > 1)
                        {
                            soundType = splitArgs[0].StartsWith(_typeArgumentSpecifier) ?
                                        splitArgs[0].Substring(_typeArgumentSpecifier.Length) :
                                        splitArgs[0];
                        }

                        // Check to see if the value is prepended by the ENCODING= argument
                        string soundEncoding = "";
                        if (splitArgs.Length > 1)
                        {
                            const string _encodingArgumentSpecifier = "ENCODING=";
                            soundEncoding = splitArgs[1].StartsWith(_encodingArgumentSpecifier) ?
                                            splitArgs[1].Substring(_encodingArgumentSpecifier.Length).Substring(0, splitArgs[1].IndexOf(_argumentDelimiter)) :
                                            splitArgs[1].Substring(0, splitArgs[1].IndexOf(_argumentDelimiter));
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
                        SoundInfo _sound = new(altId, finalArgs.ToArray(), valueType, soundEncoding, soundType, encodedSound.ToString());
                        _sounds.Add(_sound);
                    }

                    // Revision (REV:1995-10-31T22:27:10Z or REV:19951031T222710)
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(_revSpecifier))
                    {
                        // Get the value
                        string revValue = _value.Substring(_revSpecifier.Length);

                        // Populate field
                        _rev = DateTime.Parse(revValue);
                    }

                    // Nickname (NICKNAME;TYPE=cell,home:495-522-3560)
                    // ALTID is supported.
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
                        NicknameInfo _nickInstance = new(altId, finalArgs.ToArray(), _nick, _nicknameTypes);
                        _nicks.Add(_nickInstance);
                    }

                    // Nickname (NICKNAME:495-522-3560)
                    // ALTID is supported. See above.
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
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(_birthSpecifier))
                    {
                        // Get the value
                        string bdayValue = _value.Substring(_birthSpecifier.Length);

                        // Populate field
                        _bday = DateTime.Parse(bdayValue);
                    }

                    // Role (ROLE:Programmer)
                    // ALTID is supported. See below.
                    if (_value.StartsWith(_roleSpecifier))
                    {
                        // Get the value
                        string roleValue = _value.Substring(_roleSpecifier.Length);

                        // Populate the fields
                        RoleInfo _role = new(0, Array.Empty<string>(), roleValue);
                        _roles.Add(_role);
                    }

                    // Role (ROLE;ALTID=1;LANGUAGE=en:Programmer)
                    // ALTID is supported. See below.
                    if (_value.StartsWith(_roleSpecifierWithType))
                    {
                        // Get the value
                        string roleValue = _value.Substring(_roleSpecifier.Length);

                        // Populate the fields
                        RoleInfo _role = new(altId, finalArgs.ToArray(), roleValue);
                        _roles.Add(_role);
                    }

                    // Categories (CATEGORIES:INTERNET or CATEGORIES:INTERNET,IETF,INDUSTRY,INFORMATION TECHNOLOGY)
                    // Here, we don't support ALTID.
                    if (_value.StartsWith(_categoriesSpecifier))
                    {
                        // Get the value
                        string categoriesValue = _value.Substring(_categoriesSpecifier.Length);

                        // Populate field
                        _categories.AddRange(Regex.Unescape(categoriesValue).Split(','));
                    }

                    // X-nonstandard (X-AIM:john.s or X-DL;Design Work Group:List Item 1;List Item 2;List Item 3)
                    // Here, we don't support ALTID.
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
                        XNameInfo _x = new(_xName, _xValues, _xTypes);
                        _xes.Add(_x);
                    }
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
            return new Card(CardVersion, _names.ToArray(), _fullName, _telephones.ToArray(), _addresses.ToArray(), _orgs.ToArray(), _titles.ToArray(), _url, _note, _emails.ToArray(), _xes.ToArray(), _kind, _photos.ToArray(), _rev, _nicks.ToArray(), _bday, "", _roles.ToArray(), _categories.ToArray());
        }

        public override void SaveTo(string path)
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
            var fileStream = File.OpenWrite(path);
            CardContentStream.CopyTo(fileStream);
            fileStream.Flush();
            fileStream.Close();
        }

        internal VcardFour(string cardPath, string cardContent, string cardVersion)
        {
            CardPath = cardPath;
            CardContent = cardContent;
            CardVersion = cardVersion;
        }
    }
}