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
                        if (splitArgs[0].StartsWith(VcardConstants._altIdArgumentSpecifier))
                            finalArgs.AddRange(splitArgs.Except(new string[] { splitArgs[0] }));
                        else
                            finalArgs.AddRange(splitArgs);
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
                        // Check the line
                        if (splitValues.Length < 2)
                            throw new InvalidDataException("Name field must specify exactly five values (Last name, first name, alt names, prefixes, and suffixes)");

                        // Check to see if there are any names with altid
                        if (idReservedForName)
                            throw new InvalidDataException("Attempted to overwrite name under the main ID.");

                        // Populate fields
                        string _lastName   = Regex.Unescape(splitValues[0]);
                        string _firstName  = Regex.Unescape(splitValues[1]);
                        string[] _altNames = splitValues.Length >= 3 ? Regex.Unescape(splitValues[2]).Split(new char[] { VcardConstants._valueDelimiter }, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                        string[] _prefixes = splitValues.Length >= 4 ? Regex.Unescape(splitValues[3]).Split(new char[] { VcardConstants._valueDelimiter }, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                        string[] _suffixes = splitValues.Length >= 5 ? Regex.Unescape(splitValues[4]).Split(new char[] { VcardConstants._valueDelimiter }, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                        NameInfo _name = new(0, Array.Empty<string>(), _firstName, _lastName, _altNames, _prefixes, _suffixes);
                        _names.Add(_name);

                        // Since we've reserved id 0, set the flag
                        idReservedForName = true;
                    }

                    // The name (N;ALTID=1;LANGUAGE=en:Sanders;John;;;)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._nameSpecifierWithType))
                    {
                        // Check the line
                        string nameValue = _value.Substring(VcardConstants._nameSpecifierWithType.Length);
                        string[] splitNameParts = nameValue.Split(VcardConstants._argumentDelimiter);
                        string[] splitName = splitNameParts[1].Split(VcardConstants._fieldDelimiter);
                        if (splitName.Length < 2)
                            throw new InvalidDataException("Name field must specify exactly five values (Last name, first name, alt names, prefixes, and suffixes)");

                        // Check the ALTID
                        if (!splitArgs[0].StartsWith(VcardConstants._altIdArgumentSpecifier))
                            throw new InvalidDataException("ALTID must come exactly first");

                        // ALTID: N: has cardinality of *1
                        if (idReservedForName && _names.Count > 0 && _names[0].AltId != altId)
                            throw new InvalidDataException("ALTID may not be different from all the alternative argument names");

                        // Populate fields
                        string _lastName   = Regex.Unescape(splitName[0]);
                        string _firstName  = Regex.Unescape(splitName[1]);
                        string[] _altNames = splitName.Length >= 3 ? Regex.Unescape(splitName[2]).Split(new char[] { VcardConstants._valueDelimiter }, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                        string[] _prefixes = splitName.Length >= 4 ? Regex.Unescape(splitName[3]).Split(new char[] { VcardConstants._valueDelimiter }, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                        string[] _suffixes = splitName.Length >= 5 ? Regex.Unescape(splitName[4]).Split(new char[] { VcardConstants._valueDelimiter }, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                        NameInfo _name = new(altId, finalArgs.ToArray(), _firstName, _lastName, _altNames, _prefixes, _suffixes);
                        _names.Add(_name);

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
                    {
                        // Get the value
                        string telValue = _value.Substring(VcardConstants._telephoneSpecifierWithType.Length);
                        string[] splitTel = telValue.Split(VcardConstants._argumentDelimiter);
                        string[] splitTypes;
                        if (splitTel.Length != 2)
                            throw new InvalidDataException("Telephone field must specify exactly two values (Type (must be prepended with TYPE=), and phone number)");

                        // Check to see if the type is prepended with the TYPE= argument
                        if (splitTel[0].StartsWith(VcardConstants._typeArgumentSpecifier))
                            // Get the types
                            splitTypes = splitTel[0].Substring(VcardConstants._typeArgumentSpecifier.Length).Split(VcardConstants._valueDelimiter);
                        else if (string.IsNullOrEmpty(splitTel[0]))
                            // We're confronted with an empty type. Assume that it's a CELL.
                            splitTypes = new string[] { "CELL" };
                        else
                            // Trying to specify type without TYPE= is illegal according to RFC2426
                            throw new InvalidDataException("Telephone type must be prepended with TYPE=");

                        // Populate the fields
                        string[] _telephoneTypes = splitTypes;
                        string _telephoneNumber = Regex.Unescape(splitTel[1]);
                        TelephoneInfo _telephone = new(altId, finalArgs.ToArray(), _telephoneTypes, _telephoneNumber);
                        _telephones.Add(_telephone);
                    }

                    // Telephone (TEL:495-522-3560)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._telephoneSpecifier))
                    {
                        // Get the value
                        string telValue = _value.Substring(VcardConstants._telephoneSpecifier.Length);

                        // Populate the fields
                        string[] _telephoneTypes = new string[] { "CELL" };
                        string _telephoneNumber = Regex.Unescape(telValue);
                        TelephoneInfo _telephone = new(altId, Array.Empty<string>(), _telephoneTypes, _telephoneNumber);
                        _telephones.Add(_telephone);
                    }

                    // Address (ADR;TYPE=HOME:;;Los Angeles, USA;;;;)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._addressSpecifierWithType))
                    {
                        // Get the value
                        string adrValue = _value.Substring(VcardConstants._addressSpecifierWithType.Length);
                        string[] splitAdr = adrValue.Split(VcardConstants._argumentDelimiter);
                        string[] splitTypes;
                        if (splitAdr.Length != 2)
                            throw new InvalidDataException("Address field must specify exactly two values (Type (must be prepended with TYPE=), and address information)");

                        // Check to see if the type is prepended with the TYPE= argument
                        if (splitAdr[0].StartsWith(VcardConstants._typeArgumentSpecifier))
                            // Get the types
                            splitTypes = splitAdr[0].Substring(VcardConstants._typeArgumentSpecifier.Length).Split(VcardConstants._valueDelimiter);
                        else if (string.IsNullOrEmpty(splitAdr[0]))
                            // We're confronted with an empty type. Assume that it's a HOME address.
                            splitTypes = new string[] { "HOME" };
                        else
                            // Trying to specify type without TYPE= is illegal according to RFC2426
                            throw new InvalidDataException("Address type must be prepended with TYPE=");

                        // Check the provided address
                        string[] splitAddressValues = splitAdr[1].Split(VcardConstants._fieldDelimiter);
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
                        AddressInfo _address = new(altId, finalArgs.ToArray(), _addressTypes, _addressPOBox, _addressExtended, _addressStreet, _addressLocality, _addressRegion, _addressPostalCode, _addressCountry);
                        _addresses.Add(_address);
                    }

                    // Email (EMAIL;TYPE=HOME,INTERNET:john.s@acme.co)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._emailSpecifier))
                    {
                        // Get the value
                        string mailValue = _value.Substring(VcardConstants._emailSpecifier.Length);
                        string[] splitMail = mailValue.Split(VcardConstants._argumentDelimiter);
                        string[] splitTypes;
                        MailAddress mail;
                        if (splitMail.Length != 2)
                            throw new InvalidDataException("E-mail field must specify exactly two values (Type (must be prepended with TYPE=), and a valid e-mail address)");

                        // Check to see if the type is prepended with the TYPE= argument
                        if (splitMail[0].StartsWith(VcardConstants._typeArgumentSpecifier))
                            // Get the types
                            splitTypes = splitMail[0].Substring(VcardConstants._typeArgumentSpecifier.Length).Split(VcardConstants._valueDelimiter);
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
                        EmailInfo _email = new(altId, finalArgs.ToArray(), _emailTypes, _emailAddress);
                        _emails.Add(_email);
                    }

                    // Organization (ORG:Acme Co. or ORG:ABC, Inc.;North American Division;Marketing)
                    // ALTID is supported.
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
                        OrganizationInfo _org = new(altId, Array.Empty<string>(), _orgName, _orgUnit, _orgUnitRole, splitTypes);
                        _orgs.Add(_org);
                    }

                    // Organization (ORG;TYPE=WORK:Acme Co. or ORG:ABC, Inc.;North American Division;Marketing)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._orgSpecifierWithType))
                    {
                        // Get the value
                        string orgValue = _value.Substring(VcardConstants._orgSpecifierWithType.Length);
                        string[] splitOrg = orgValue.Split(VcardConstants._argumentDelimiter);
                        string[] splitTypes;
                        if (splitOrg.Length != 2)
                            throw new InvalidDataException("Organization field must specify exactly two values (Type (must be prepended with TYPE=), and address information)");

                        // Check to see if the type is prepended with the TYPE= argument
                        if (splitOrg[0].StartsWith(VcardConstants._typeArgumentSpecifier))
                            // Get the types
                            splitTypes = splitOrg[0].Substring(VcardConstants._typeArgumentSpecifier.Length).Split(VcardConstants._valueDelimiter);
                        else if (string.IsNullOrEmpty(splitOrg[0]))
                            // We're confronted with an empty type. Assume that it's a WORK organization.
                            splitTypes = new string[] { "WORK" };
                        else
                            // Trying to specify type without TYPE= is illegal according to RFC2426
                            throw new InvalidDataException("Organization type must be prepended with TYPE=");

                        // Check the provided organization
                        string[] splitOrganizationValues = splitOrg[1].Split(VcardConstants._fieldDelimiter);
                        if (splitOrganizationValues.Length != 3)
                            throw new InvalidDataException("Organization information must specify exactly three values (name, unit, and role)");

                        // Populate the fields
                        string _orgName = Regex.Unescape(splitOrganizationValues[0]);
                        string _orgUnit = Regex.Unescape(splitOrganizationValues.Length >= 2 ? splitOrganizationValues[1] : "");
                        string _orgUnitRole = Regex.Unescape(splitOrganizationValues.Length >= 3 ? splitOrganizationValues[2] : "");
                        OrganizationInfo _org = new(altId, finalArgs.ToArray(), _orgName, _orgUnit, _orgUnitRole, splitTypes);
                        _orgs.Add(_org);
                    }

                    // Title (TITLE:Product Manager)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._titleSpecifier))
                    {
                        // Get the value
                        string titleValue = _value.Substring(VcardConstants._titleSpecifier.Length);

                        // Populate field
                        string _title = Regex.Unescape(titleValue);
                        TitleInfo title = new(altId, Array.Empty<string>(), _title);
                        _titles.Add(title);
                    }

                    // Title (TITLE;ALTID=1;LANGUAGE=fr:Patron or TITLE;LANGUAGE=fr:Patron)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._titleSpecifierWithArguments))
                    {
                        // Get the value
                        string titleValue = _value.Substring(VcardConstants._titleSpecifierWithArguments.Length);
                        string[] splitTitleParts = titleValue.Split(VcardConstants._argumentDelimiter);

                        // Populate field
                        string _title = Regex.Unescape(splitTitleParts[1]);
                        TitleInfo title = new(altId, finalArgs.ToArray(), _title);
                        _titles.Add(title);
                    }

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
                    {
                        // Get the value
                        string photoValue = _value.Substring(VcardConstants._photoSpecifierWithType.Length);
                        string[] splitPhoto = photoValue.Split(VcardConstants._argumentDelimiter);

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
                            photoType = splitArgs[1].StartsWith(VcardConstants._typeArgumentSpecifier) ?
                                        splitArgs[1].Substring(VcardConstants._typeArgumentSpecifier.Length).Substring(0, splitArgs[1].IndexOf(VcardConstants._argumentDelimiter)) :
                                        splitArgs[1].Substring(0, splitArgs[1].IndexOf(VcardConstants._argumentDelimiter));
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

                    // Logo (LOGO;ENCODING=BASE64;JPEG:... or LOGO;VALUE=URL:file:///jqpublic.gif or LOGO;ENCODING=BASE64;TYPE=GIF:...)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._logoSpecifierWithType))
                    {
                        // Get the value
                        string logoValue = _value.Substring(VcardConstants._logoSpecifierWithType.Length);
                        string[] splitLogo = logoValue.Split(VcardConstants._argumentDelimiter);

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
                        string logoEncoding = "";
                        if (splitArgs.Length >= 1)
                        {
                            const string _encodingArgumentSpecifier = "ENCODING=";
                            logoEncoding = splitArgs[0].Substring(_encodingArgumentSpecifier.Length);
                        }

                        // Check to see if the value is prepended with the TYPE= argument
                        string logoType = "";
                        if (splitArgs.Length >= 1)
                        {
                            logoType = splitArgs[1].StartsWith(VcardConstants._typeArgumentSpecifier) ?
                                       splitArgs[1].Substring(VcardConstants._typeArgumentSpecifier.Length).Substring(0, splitArgs[1].IndexOf(VcardConstants._argumentDelimiter)) :
                                       splitArgs[1].Substring(0, splitArgs[1].IndexOf(VcardConstants._argumentDelimiter));
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
                        LogoInfo _logo = new(altId, finalArgs.ToArray(), valueType, logoEncoding, logoType, encodedLogo.ToString());
                        _logos.Add(_logo);
                    }

                    // Sound (SOUND;VALUE=URL:file///multimed/audio/jqpublic.wav or SOUND;WAVE;BASE64:... or SOUND;TYPE=WAVE;ENCODING=BASE64:...)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._soundSpecifierWithType))
                    {
                        // Get the value
                        string soundValue = _value.Substring(VcardConstants._soundSpecifierWithType.Length);
                        string[] splitSound = soundValue.Split(VcardConstants._argumentDelimiter);

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
                            soundType = splitArgs[0].StartsWith(VcardConstants._typeArgumentSpecifier) ?
                                        splitArgs[0].Substring(VcardConstants._typeArgumentSpecifier.Length) :
                                        splitArgs[0];
                        }

                        // Check to see if the value is prepended by the ENCODING= argument
                        string soundEncoding = "";
                        if (splitArgs.Length > 1)
                        {
                            const string _encodingArgumentSpecifier = "ENCODING=";
                            soundEncoding = splitArgs[1].StartsWith(_encodingArgumentSpecifier) ?
                                            splitArgs[1].Substring(_encodingArgumentSpecifier.Length).Substring(0, splitArgs[1].IndexOf(VcardConstants._argumentDelimiter)) :
                                            splitArgs[1].Substring(0, splitArgs[1].IndexOf(VcardConstants._argumentDelimiter));
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
                    {
                        // Get the value
                        string nickValue = _value.Substring(VcardConstants._nicknameSpecifierWithType.Length);
                        string[] splitNick = nickValue.Split(VcardConstants._argumentDelimiter);

                        string[] splitTypes;
                        if (splitNick.Length != 2)
                            throw new InvalidDataException("Nickname field must specify exactly two values (Type (must be prepended with TYPE=), and nickname)");

                        // Check to see if the type is prepended with the TYPE= argument
                        if (splitNick[0].StartsWith(VcardConstants._typeArgumentSpecifier))
                            // Get the types
                            splitTypes = splitNick[0].Substring(VcardConstants._typeArgumentSpecifier.Length).Split(VcardConstants._valueDelimiter);
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
                    {
                        // Get the value
                        string roleValue = _value.Substring(VcardConstants._roleSpecifier.Length);

                        // Populate the fields
                        RoleInfo _role = new(0, Array.Empty<string>(), roleValue);
                        _roles.Add(_role);
                    }

                    // Role (ROLE;ALTID=1;LANGUAGE=en:Programmer)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._roleSpecifierWithType))
                    {
                        // Get the value
                        string roleValue = _value.Substring(VcardConstants._roleSpecifier.Length);

                        // Populate the fields
                        RoleInfo _role = new(altId, finalArgs.ToArray(), roleValue);
                        _roles.Add(_role);
                    }

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
                    {
                        // Get the value
                        string tzValue = _value.Substring(VcardConstants._timeZoneSpecifierWithType.Length);
                        string[] splitTz = tzValue.Split(VcardConstants._argumentDelimiter);
                        string[] splitTypes;
                        if (splitTz.Length != 1)
                            throw new InvalidDataException("Time Zone field must specify exactly one value (VALUE=\"text\" / \"uri\" / \"utc-offset\")");

                        // Check to see if the type is prepended with the VALUE= argument
                        if (splitTz[0].StartsWith(VcardConstants._valueArgumentSpecifier))
                            // Get the types
                            splitTypes = splitTz[0].Substring(VcardConstants._valueArgumentSpecifier.Length).Split(VcardConstants._valueDelimiter);
                        else if (string.IsNullOrEmpty(splitTz[0]))
                            // We're confronted with an empty type. Assume that it's a uri-offset.
                            splitTypes = new string[] { "uri-offset" };
                        else
                            // Trying to specify type without VALUE= is illegal according to RFC2426
                            throw new InvalidDataException("Time Zone type must be prepended with VALUE=");

                        // Populate the fields
                        string[] _timeZoneTypes = splitTypes;
                        string _timeZoneNumber = Regex.Unescape(splitTz[1]);
                        TimeZoneInfo _timeZone = new(altId, finalArgs.ToArray(), _timeZoneTypes, _timeZoneNumber);
                        _timezones.Add(_timeZone);
                    }

                    // Time Zone (TZ:-05:00)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._timeZoneSpecifier))
                    {
                        // Get the value
                        string tzValue = _value.Substring(VcardConstants._timeZoneSpecifier.Length);

                        // Populate the fields
                        string[] _timeZoneTypes = new string[] { "uri-offset" };
                        string _timeZoneNumber = Regex.Unescape(tzValue);
                        TimeZoneInfo _timeZone = new(altId, Array.Empty<string>(), _timeZoneTypes, _timeZoneNumber);
                        _timezones.Add(_timeZone);
                    }

                    // Geo (GEO;VALUE=uri:https://...)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._geoSpecifierWithType))
                    {
                        // Get the value
                        string geoValue = _value.Substring(VcardConstants._geoSpecifierWithType.Length);
                        string[] splitGeo = geoValue.Split(VcardConstants._argumentDelimiter);
                        string[] splitTypes;
                        if (splitGeo.Length != 1)
                            throw new InvalidDataException("Geo field must specify exactly one value (VALUE=\"uri\")");

                        // Check to see if the type is prepended with the VALUE= argument
                        if (splitGeo[0].StartsWith(VcardConstants._valueArgumentSpecifier))
                            // Get the types
                            splitTypes = splitGeo[0].Substring(VcardConstants._valueArgumentSpecifier.Length).Split(VcardConstants._valueDelimiter);
                        else if (string.IsNullOrEmpty(splitGeo[0]))
                            // We're confronted with an empty type. Assume that it's a uri.
                            splitTypes = new string[] { "uri" };
                        else
                            // Trying to specify type without VALUE= is illegal according to RFC2426
                            throw new InvalidDataException("Geo type must be prepended with VALUE=");

                        // Populate the fields
                        string[] _geoTypes = splitTypes;
                        string _geoNumber = Regex.Unescape(splitGeo[1]);
                        GeoInfo _geo = new(altId, finalArgs.ToArray(), _geoTypes, _geoNumber);
                        _geos.Add(_geo);
                    }

                    // Geo (GEO:geo:37.386013,-122.082932)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._geoSpecifier))
                    {
                        // Get the value
                        string geoValue = _value.Substring(VcardConstants._geoSpecifier.Length);

                        // Populate the fields
                        string[] _geoTypes = new string[] { "uri" };
                        string _geoNumber = Regex.Unescape(geoValue);
                        GeoInfo _geo = new(altId, Array.Empty<string>(), _geoTypes, _geoNumber);
                        _geos.Add(_geo);
                    }

                    // IMPP information (IMPP;TYPE=home:sip:test)
                    // ALTID is supported.
                    if (_value.StartsWith(VcardConstants._imppSpecifierWithType))
                    {
                        // Get the value
                        string imppValue = _value.Substring(VcardConstants._imppSpecifierWithType.Length);
                        string[] splitImpp = imppValue.Split(VcardConstants._argumentDelimiter);

                        string[] splitTypes;
                        if (splitImpp.Length < 2)
                            throw new InvalidDataException("IMPP information field must specify exactly two values (Type (must be prepended with TYPE=), and impp)");

                        // Check to see if the type is prepended with the TYPE= argument
                        if (splitImpp[0].StartsWith(VcardConstants._typeArgumentSpecifier))
                            // Get the types
                            splitTypes = splitImpp[0].Substring(VcardConstants._typeArgumentSpecifier.Length).Split(VcardConstants._valueDelimiter);
                        else if (string.IsNullOrEmpty(splitImpp[0]))
                            // We're confronted with an empty type. Assume that it's HOME.
                            splitTypes = new string[] { "HOME" };
                        else
                            // Trying to specify type without TYPE= is illegal according to RFC2426
                            throw new InvalidDataException("IMPP type must be prepended with TYPE=");

                        // Populate the fields
                        string[] _imppTypes = splitTypes;
                        string _impp = Regex.Unescape(imppValue.Substring(imppValue.IndexOf(":") + 1));
                        ImppInfo _imppInstance = new(altId, finalArgs.ToArray(), _impp, _imppTypes);
                        _impps.Add(_imppInstance);
                    }

                    // IMPP information (IMPP:sip:test)
                    // ALTID is supported. See above.
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
                    // ALTID is supported.
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
                        XNameInfo _x = new(altId, finalArgs.ToArray(), _xName, _xValues, _xTypes);
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
