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
                        // Check the line
                        string nameValue = _value.Substring(VcardConstants._nameSpecifier.Length);
                        string[] splitName = nameValue.Split(VcardConstants._fieldDelimiter);
                        if (splitName.Length < 2)
                            throw new InvalidDataException("Name field must specify the first two or more of the five values (Last name, first name, alt names, prefixes, and suffixes)");

                        // Populate fields
                        string _lastName = Regex.Unescape(splitName[0]);
                        string _firstName = Regex.Unescape(splitName[1]);
                        string[] _altNames = splitName.Length >= 3 ? Regex.Unescape(splitName[2]).Split(new char[] { VcardConstants._valueDelimiter }) : Array.Empty<string>();
                        string[] _prefixes = splitName.Length >= 4 ? Regex.Unescape(splitName[3]).Split(new char[] { VcardConstants._valueDelimiter }) : Array.Empty<string>();
                        string[] _suffixes = splitName.Length >= 5 ? Regex.Unescape(splitName[4]).Split(new char[] { VcardConstants._valueDelimiter }) : Array.Empty<string>();
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
                    }

                    // Telephone (TEL;CELL;HOME:495-522-3560 or TEL;TYPE=cell,home:495-522-3560)
                    if (_value.StartsWith(VcardConstants._telephoneSpecifierWithType))
                    {
                        // Get the value
                        string telValue = _value.Substring(VcardConstants._telephoneSpecifierWithType.Length);
                        string[] splitTel = telValue.Split(VcardConstants._argumentDelimiter);
                        if (splitTel.Length != 2)
                            throw new InvalidDataException("Telephone field must specify exactly two values (Type (optionally prepended with TYPE=), and phone number)");

                        // Check to see if the type is prepended with the TYPE= argument
                        string[] splitTypes = splitTel[0].StartsWith(VcardConstants._typeArgumentSpecifier) ?
                                              splitTel[0].Substring(VcardConstants._typeArgumentSpecifier.Length).Split(VcardConstants._valueDelimiter) :
                                              splitTel[0].Split(VcardConstants._fieldDelimiter);

                        // Populate the fields
                        string[] _telephoneTypes = splitTypes;
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

                    // Address (ADR;HOME:;;Los Angeles, USA;;;;)
                    if (_value.StartsWith(VcardConstants._addressSpecifierWithType))
                    {
                        // Get the value
                        string adrValue = _value.Substring(VcardConstants._addressSpecifierWithType.Length);
                        string[] splitAdr = adrValue.Split(VcardConstants._argumentDelimiter);
                        if (splitAdr.Length != 2)
                            throw new InvalidDataException("Address field must specify exactly two values (Type (optionally prepended with TYPE=), and address information)");

                        // Check to see if the type is prepended with the TYPE= argument
                        string[] splitTypes = splitAdr[0].StartsWith(VcardConstants._typeArgumentSpecifier) ?
                                              splitAdr[0].Substring(VcardConstants._typeArgumentSpecifier.Length).Split(VcardConstants._valueDelimiter) :
                                              splitAdr[0].Split(VcardConstants._fieldDelimiter);

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
                        AddressInfo _address = new(0, Array.Empty<string>(), _addressTypes, _addressPOBox, _addressExtended, _addressStreet, _addressLocality, _addressRegion, _addressPostalCode, _addressCountry);
                        _addresses.Add(_address);
                    }

                    // Email (EMAIL;HOME;INTERNET:john.s@acme.co or EMAIL;TYPE=HOME,INTERNET:john.s@acme.co)
                    if (_value.StartsWith(VcardConstants._emailSpecifier))
                    {
                        // Get the value
                        string mailValue = _value.Substring(VcardConstants._emailSpecifier.Length);
                        string[] splitMail = mailValue.Split(VcardConstants._argumentDelimiter);
                        MailAddress mail;
                        if (splitMail.Length != 2)
                            throw new InvalidDataException("E-mail field must specify exactly two values (Type (optionally prepended with TYPE=), and a valid e-mail address)");

                        // Check to see if the type is prepended with the TYPE= argument
                        string[] splitTypes = splitMail[0].StartsWith(VcardConstants._typeArgumentSpecifier) ?
                                              splitMail[0].Substring(VcardConstants._typeArgumentSpecifier.Length).Split(VcardConstants._valueDelimiter) :
                                              splitMail[0].Split(VcardConstants._fieldDelimiter);

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
                        if (splitOrg.Length != 2)
                            throw new InvalidDataException("Organization field must specify exactly two values (Type (must be prepended with TYPE=), and address information)");

                        // Check to see if the type is prepended with the TYPE= argument
                        string[] splitTypes = splitOrg[0].StartsWith(VcardConstants._typeArgumentSpecifier) ?
                                              splitOrg[0].Substring(VcardConstants._typeArgumentSpecifier.Length).Split(VcardConstants._valueDelimiter) :
                                              splitOrg[0].Split(VcardConstants._fieldDelimiter);

                        // Check the provided organization
                        string[] splitOrganizationValues = splitOrg[1].Split(VcardConstants._fieldDelimiter);
                        if (splitOrganizationValues.Length != 3)
                            throw new InvalidDataException("Organization information must specify exactly three values (name, unit, and role)");

                        // Populate the fields
                        string _orgName = Regex.Unescape(splitOrganizationValues[0]);
                        string _orgUnit = Regex.Unescape(splitOrganizationValues.Length >= 2 ? splitOrganizationValues[1] : "");
                        string _orgUnitRole = Regex.Unescape(splitOrganizationValues.Length >= 3 ? splitOrganizationValues[2] : "");
                        OrganizationInfo _org = new(0, Array.Empty<string>(), _orgName, _orgUnit, _orgUnitRole, splitTypes);
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
                        string[] splitPhotoArgs = photoValue.Split(VcardConstants._fieldDelimiter);

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
                            photoType = splitPhotoArgs[1].StartsWith(VcardConstants._typeArgumentSpecifier) ?
                                        splitPhotoArgs[1].Substring(VcardConstants._typeArgumentSpecifier.Length).Substring(0, splitPhotoArgs[1].IndexOf(VcardConstants._argumentDelimiter)) :
                                        splitPhotoArgs[1].Substring(0, splitPhotoArgs[1].IndexOf(VcardConstants._argumentDelimiter));
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
                    if (_value.StartsWith(VcardConstants._logoSpecifierWithType))
                    {
                        // Get the value
                        string logoValue = _value.Substring(VcardConstants._logoSpecifierWithType.Length);
                        string[] splitLogo = logoValue.Split(VcardConstants._argumentDelimiter);
                        string[] splitLogoArgs = logoValue.Split(VcardConstants._fieldDelimiter);

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
                            logoType = splitLogoArgs[1].StartsWith(VcardConstants._typeArgumentSpecifier) ?
                                       splitLogoArgs[1].Substring(VcardConstants._typeArgumentSpecifier.Length).Substring(0, splitLogoArgs[1].IndexOf(VcardConstants._argumentDelimiter)) :
                                       splitLogoArgs[1].Substring(0, splitLogoArgs[1].IndexOf(VcardConstants._argumentDelimiter));
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
                    if (_value.StartsWith(VcardConstants._soundSpecifierWithType))
                    {
                        // Get the value
                        string soundValue = _value.Substring(VcardConstants._soundSpecifierWithType.Length);
                        string[] splitSound = soundValue.Split(VcardConstants._argumentDelimiter);
                        string[] splitSoundArgs = soundValue.Split(VcardConstants._fieldDelimiter);

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
                            soundType = splitSoundArgs[0].StartsWith(VcardConstants._typeArgumentSpecifier) ?
                                        splitSoundArgs[0].Substring(VcardConstants._typeArgumentSpecifier.Length) :
                                        splitSoundArgs[0];
                        }

                        // Check to see if the value is prepended by the ENCODING= argument
                        string soundEncoding = "";
                        if (splitSoundArgs.Length > 1)
                        {
                            const string _encodingArgumentSpecifier = "ENCODING=";
                            soundEncoding = splitSoundArgs[1].StartsWith(_encodingArgumentSpecifier) ?
                                            splitSoundArgs[1].Substring(_encodingArgumentSpecifier.Length).Substring(0, splitSoundArgs[1].IndexOf(VcardConstants._argumentDelimiter)) :
                                            splitSoundArgs[1].Substring(0, splitSoundArgs[1].IndexOf(VcardConstants._argumentDelimiter));
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
                    {
                        string roleValue = _value.Substring(VcardConstants._roleSpecifier.Length);
                        RoleInfo _role = new(0, Array.Empty<string>(), roleValue);
                        _roles.Add(_role);
                    }

                    // Time Zone (TZ;VALUE=text:-05:00; EST; Raleigh/North America)
                    if (_value.StartsWith(VcardConstants._timeZoneSpecifierWithType))
                    {
                        string tzValue = _value.Substring(VcardConstants._timeZoneSpecifierWithType.Length);
                        string[] splitTz = tzValue.Split(VcardConstants._argumentDelimiter);
                        if (splitTz.Length != 1)
                            throw new InvalidDataException("TimeZone field must specify exactly one value (VALUE=\"text\" / \"uri\" / \"utc-offset\")");
                        string[] splitTypes = splitTz[0].StartsWith(VcardConstants._valueArgumentSpecifier) ? splitTz[0].Substring(VcardConstants._valueArgumentSpecifier.Length).Split(VcardConstants._valueDelimiter) : splitTz[0].Split(VcardConstants._fieldDelimiter);
                        string[] _timeZoneTypes = splitTypes;
                        string _timeZoneNumber = Regex.Unescape(splitTz[1]);
                        TimeZoneInfo _timeZone = new(0, Array.Empty<string>(), _timeZoneTypes, _timeZoneNumber);
                        _timezones.Add(_timeZone);
                    }

                    // Time Zone (TZ:-05:00)
                    if (_value.StartsWith(VcardConstants._timeZoneSpecifier))
                    {
                        string tzValue = _value.Substring(VcardConstants._timeZoneSpecifier.Length);
                        string _timeZoneStr = Regex.Unescape(tzValue);
                        TimeZoneInfo _timeZone = new(0, Array.Empty<string>(), Array.Empty<string>(), _timeZoneStr);
                        _timezones.Add(_timeZone);
                    }

                    // Geo (GEO;VALUE=uri:https://...)
                    if (_value.StartsWith(VcardConstants._geoSpecifierWithType))
                    {
                        string geoValue = _value.Substring(VcardConstants._geoSpecifierWithType.Length);
                        string[] splitGeo = geoValue.Split(VcardConstants._argumentDelimiter);
                        if (splitGeo.Length != 1)
                            throw new InvalidDataException("Geo field must specify exactly one value (VALUE=\"uri\")");
                        string[] splitTypes = splitGeo[0].StartsWith(VcardConstants._valueArgumentSpecifier) ? splitGeo[0].Substring(VcardConstants._valueArgumentSpecifier.Length).Split(VcardConstants._valueDelimiter) : splitGeo[0].Split(VcardConstants._fieldDelimiter);
                        string[] _geoTypes = splitTypes;
                        string _geoNumber = Regex.Unescape(splitGeo[1]);
                        GeoInfo _geo = new(0, Array.Empty<string>(), _geoTypes, _geoNumber);
                        _geos.Add(_geo);
                    }

                    // Geo (GEO:geo:37.386013,-122.082932)
                    if (_value.StartsWith(VcardConstants._geoSpecifier))
                    {
                        string geoValue = _value.Substring(VcardConstants._geoSpecifier.Length);
                        string _geoStr = Regex.Unescape(geoValue);
                        GeoInfo _geo = new(0, Array.Empty<string>(), Array.Empty<string>(), _geoStr);
                        _geos.Add(_geo);
                    }

                    // IMPP information (IMPP;TYPE=home:sip:test)
                    if (_value.StartsWith(VcardConstants._imppSpecifierWithType))
                    {
                        string imppValue = _value.Substring(VcardConstants._imppSpecifierWithType.Length);
                        string[] splitImpp = imppValue.Split(VcardConstants._argumentDelimiter);
                        string[] splitTypes;
                        if (splitImpp.Length < 2)
                            throw new InvalidDataException("IMPP information field must specify exactly two values (Type (must be prepended with TYPE=), and impp)");
                        if (splitImpp[0].StartsWith(VcardConstants._typeArgumentSpecifier))
                            splitTypes = splitImpp[0].Substring(VcardConstants._typeArgumentSpecifier.Length).Split(VcardConstants._valueDelimiter);
                        else if (string.IsNullOrEmpty(splitImpp[0]))
                            splitTypes = new string[] { "HOME" };
                        else
                            throw new InvalidDataException("IMPP type must be prepended with TYPE=");
                        string[] _imppTypes = splitTypes;
                        string _impp = Regex.Unescape(imppValue.Substring(imppValue.IndexOf(":") + 1));
                        ImppInfo _imppInstance = new(0, Array.Empty<string>(), _impp, _imppTypes);
                        _impps.Add(_imppInstance);
                    }

                    // IMPP information (IMPP:sip:test)
                    if (_value.StartsWith(VcardConstants._imppSpecifier))
                    {
                        string imppValue = _value.Substring(VcardConstants._imppSpecifier.Length);
                        string[] _imppTypes = new string[] { "HOME" };
                        string _impp = Regex.Unescape(imppValue);
                        ImppInfo _imppInstance = new(0, Array.Empty<string>(), _impp, _imppTypes);
                        _impps.Add(_imppInstance);
                    }

                    // X-nonstandard (X-AIM:john.s or X-DL;Design Work Group:List Item 1;List Item 2;List Item 3)
                    if (_value.StartsWith(VcardConstants._xSpecifier))
                    {
                        string xValue = _value.Substring(VcardConstants._xSpecifier.Length);
                        string[] splitX = xValue.Split(VcardConstants._argumentDelimiter);
                        string _xName = splitX[0].Contains(VcardConstants._fieldDelimiter.ToString()) ? splitX[0].Substring(0, splitX[0].IndexOf(VcardConstants._fieldDelimiter)) : splitX[0];
                        string[] _xTypes = splitX[0].Contains(VcardConstants._fieldDelimiter.ToString()) ? splitX[0].Substring(splitX[0].IndexOf(VcardConstants._fieldDelimiter) + 1).Split(VcardConstants._fieldDelimiter) : Array.Empty<string>();
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

            // Make a new instance of the card
            return new Card(this, CardVersion, _names.ToArray(), _fullName, _telephones.ToArray(), _addresses.ToArray(), _orgs.ToArray(), _titles.ToArray(), _url, _note, _emails.ToArray(), _xes.ToArray(), "individual", _photos.ToArray(), _rev, Array.Empty<NicknameInfo>(), _bday, _mailer, _roles.ToArray(), Array.Empty<string>(), _logos.ToArray(), "", "", _timezones.ToArray(), _geos.ToArray(), _sounds.ToArray(), _impps.ToArray());
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
