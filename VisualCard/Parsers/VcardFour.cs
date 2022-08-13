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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0057:Use range operator", Justification = "Trying to maintain .NET Framework compatibility as it doesn't have System.Index")]
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
            List<NameInfo> _names           = new();
            List<TelephoneInfo> _telephones = new();
            List<EmailInfo> _emails         = new();
            List<AddressInfo> _addresses    = new();
            List<OrganizationInfo> _orgs    = new();
            List<TitleInfo> _titles         = new();
            List<XNameInfo> _xes            = new();

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
            const string _xSpecifier                    = "X-";
            const string _typeArgumentSpecifier         = "TYPE=";
            const string _altIdArgumentSpecifier        = "ALTID=";

            // Full Name specifier is required
            bool fullNameSpecifierSpotted = false;

            // Flags
            bool idReservedForName = false;

            // Iterate through all the lines
            while (!CardContentReader.EndOfStream)
            {
                // Get line
                string? _value = CardContentReader.ReadLine();

                // Card type (KIND:individual, KIND:group, KIND:org, KIND:location, ...)
                // Here, we don't support ALTID.
                if (_value.StartsWith(_kindSpecifier))
                {
                    // Get the value
                    string? kindValue = _value.Substring(_kindSpecifier.Length);

                    // Populate field
                    if (!string.IsNullOrEmpty(kindValue))
                        _kind = Regex.Unescape(kindValue);
                }

                // The name (N:Sanders;John;;;)
                // ALTID is supported. Refer below for info.
                if (_value.StartsWith(_nameSpecifier))
                {
                    // Check the line
                    string? nameValue = _value.Substring(_nameSpecifier.Length);
                    string[] splitName = nameValue.Split(_fieldDelimiter);
                    if (splitName.Length < 2)
                        throw new InvalidDataException("Name field must specify exactly five values (Last name, first name, alt names, prefixes, and suffixes)");

                    // Check to see if there are any names with altid
                    if (idReservedForName)
                        throw new InvalidDataException("Attempted to overwrite name under the main ID.");

                    // Populate fields
                    string _lastName   = Regex.Unescape(splitName[0]);
                    string _firstName  = Regex.Unescape(splitName[1]);
                    string[] _altNames = splitName.Length >= 3 ? Regex.Unescape(splitName[2]).Split(new char[] { _valueDelimiter }) : Array.Empty<string>();
                    string[] _prefixes = splitName.Length >= 4 ? Regex.Unescape(splitName[3]).Split(new char[] { _valueDelimiter }) : Array.Empty<string>();
                    string[] _suffixes = splitName.Length >= 5 ? Regex.Unescape(splitName[4]).Split(new char[] { _valueDelimiter }) : Array.Empty<string>();
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
                    string? nameValue = _value.Substring(_nameSpecifierWithType.Length);
                    string[] splitNameParts = nameValue.Split(_argumentDelimiter);
                    string[] splitName = splitNameParts[1].Split(_fieldDelimiter);
                    string[] splitArgs = splitNameParts[0].Split(_fieldDelimiter);
                    if (splitName.Length < 2)
                        throw new InvalidDataException("Name field must specify exactly five values (Last name, first name, alt names, prefixes, and suffixes)");

                    // Check the ALTID
                    if (!splitArgs[0].StartsWith(_altIdArgumentSpecifier))
                        throw new InvalidDataException("ALTID must come exactly first");
                    if (!int.TryParse(splitArgs[0].Substring(_altIdArgumentSpecifier.Length), out int altId))
                        throw new InvalidDataException("ALTID must be numeric");

                    // ALTID: N: has cardinality of *1
                    if (idReservedForName && _names.Count > 0 && _names[0].AltId != altId)
                        throw new InvalidDataException("ALTID may not be different from all the alternative argument names");

                    // Here, we require arguments for ALTID
                    if (splitArgs.Length <= 1)
                        throw new InvalidDataException("ALTID must have one or more arguments to specify why is this instance an alternative");

                    // Finalize the arguments
                    string[] finalArgs = Array.Empty<string>();
                    splitArgs.CopyTo(finalArgs, 1);

                    // Populate fields
                    string _lastName   = Regex.Unescape(splitName[0]);
                    string _firstName  = Regex.Unescape(splitName[1]);
                    string[] _altNames = splitName.Length >= 3 ? Regex.Unescape(splitName[2]).Split(new char[] { _valueDelimiter }) : Array.Empty<string>();
                    string[] _prefixes = splitName.Length >= 4 ? Regex.Unescape(splitName[3]).Split(new char[] { _valueDelimiter }) : Array.Empty<string>();
                    string[] _suffixes = splitName.Length >= 5 ? Regex.Unescape(splitName[4]).Split(new char[] { _valueDelimiter }) : Array.Empty<string>();
                    NameInfo _name = new(altId, finalArgs, _firstName, _lastName, _altNames, _prefixes, _suffixes);
                    _names.Add(_name);

                    // Since we've reserved a specific id, set the flag
                    idReservedForName = true;
                }

                // Full name (FN:John Sanders)
                // Here, we don't support ALTID.
                if (_value.StartsWith(_fullNameSpecifier))
                {
                    // Get the value
                    string? fullNameValue = _value.Substring(_fullNameSpecifier.Length);

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
                    string? telValue = _value.Substring(_telephoneSpecifierWithType.Length);
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
                    string? telValue = _value.Substring(_telephoneSpecifier.Length);

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
                    string? adrValue = _value.Substring(_addressSpecifierWithType.Length);
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
                    string? mailValue = _value.Substring(_emailSpecifier.Length);
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
                    string? orgValue = _value.Substring(_orgSpecifier.Length);
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
                    string? titleValue = _value.Substring(_titleSpecifier.Length);

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
                    string? titleValue = _value.Substring(_titleSpecifierWithArguments.Length);
                    string[] splitTitleParts = titleValue.Split(_argumentDelimiter);
                    string[] splitArgs = splitTitleParts[0].Split(_fieldDelimiter);
                    int altId = 0;

                    // Check the ALTID
                    if (splitArgs[0].StartsWith(_altIdArgumentSpecifier))
                    {
                        if (!int.TryParse(splitArgs[0].Substring(_altIdArgumentSpecifier.Length), out altId))
                            throw new InvalidDataException("ALTID must be numeric");

                        // Here, we require arguments for ALTID
                        if (splitArgs.Length <= 1)
                            throw new InvalidDataException("ALTID must have one or more arguments to specify why is this instance an alternative");
                    }

                    // Finalize the arguments
                    string[] finalArgs = Array.Empty<string>();
                    if (splitArgs[0].StartsWith(_altIdArgumentSpecifier))
                        splitArgs.CopyTo(finalArgs, 1);
                    else
                        finalArgs = splitArgs;

                    // Populate field
                    string _title = Regex.Unescape(splitTitleParts[1]);
                    TitleInfo title = new(altId, finalArgs, _title);
                    _titles.Add(title);
                }

                // Website link (URL:https://sso.org/)
                // Here, we don't support ALTID.
                if (_value.StartsWith(_urlSpecifier))
                {
                    // Get the value
                    string? urlValue = _value.Substring(_urlSpecifier.Length);

                    // Try to parse the URL to ensure that it conforms to IETF RFC 1738: Uniform Resource Locators
                    if (!Uri.TryCreate(urlValue, UriKind.Absolute, out Uri? uri))
                        throw new InvalidDataException($"URL {urlValue} is invalid");

                    // Populate field
                    _url = uri.ToString();
                }

                // Note (NOTE:Product Manager)
                // Here, we don't support ALTID.
                if (_value.StartsWith(_noteSpecifier))
                {
                    // Get the value
                    string? noteValue = _value.Substring(_noteSpecifier.Length);

                    // Populate field
                    _note = Regex.Unescape(noteValue);
                }

                // X-nonstandard (X-AIM:john.s or X-DL;Design Work Group:List Item 1;List Item 2;List Item 3)
                // Here, we don't support ALTID.
                if (_value.StartsWith(_xSpecifier))
                {
                    // Get the value
                    string? xValue = _value.Substring(_xSpecifier.Length);
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

            // Requirement checks
            if (!fullNameSpecifierSpotted)
                throw new InvalidDataException("The full name specifier, \"FN:\", is required.");

            // Make a new instance of the card
            return new Card(CardVersion, _names.ToArray(), _fullName, _telephones.ToArray(), _addresses.ToArray(), _orgs.ToArray(), _titles.ToArray(), _url, _note, _emails.ToArray(), _xes.ToArray(), _kind);
        }

        internal VcardFour(string cardPath, string cardContent, string cardVersion)
        {
            CardPath = cardPath;
            CardContent = cardContent;
            CardVersion = cardVersion;
        }
    }
}
