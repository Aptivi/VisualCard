/*
 * MIT License
 *
 * Copyright (c) 2021-2022 EoflaOE and its companies
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
    /// Parser for VCard version 2.1. Consult the vcard-21.txt file in source for the specification.
    /// </summary>
    public class VcardTwo : BaseVcardParser, IVcardParser
    {
        public override string CardPath { get; }
        public override string CardContent { get; }
        public override string CardVersion { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0057:Use range operator", Justification = "Trying to maintain .NET Framework compatibility as it doesn't have System.Index")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1847:Use char literal for a single character lookup", Justification = "Trying to maintain .NET Framework compatibility")]
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
            string _lastName                = "";
            string _firstName               = "";
            string _fullName                = "";
            string _title                   = "";
            string _url                     = "";
            string _note                    = "";
            List<TelephoneInfo> _telephones = new();
            List<EmailInfo> _emails         = new();
            List<AddressInfo> _addresses    = new();
            List<OrganizationInfo> _orgs    = new();
            List<XNameInfo> _xes            = new();

            // Some VCard 2.1 constants
            const char _fieldDelimiter                  = ';';
            const char _valueDelimiter                  = ',';
            const char _argumentDelimiter               = ':';
            const string _nameSpecifier                 = "N:";
            const string _fullNameSpecifier             = "FN:";
            const string _telephoneSpecifierWithType    = "TEL;";
            const string _telephoneSpecifier            = "TEL:";
            const string _addressSpecifierWithType      = "ADR;";
            const string _emailSpecifier                = "EMAIL;";
            const string _orgSpecifier                  = "ORG:";
            const string _titleSpecifier                = "TITLE:";
            const string _urlSpecifier                  = "URL:";
            const string _noteSpecifier                 = "NOTE:";
            const string _xSpecifier                    = "X-";
            const string _typeArgumentSpecifier         = "TYPE=";

            // Name specifier is required
            bool nameSpecifierSpotted = false;

            // Iterate through all the lines
            while (!CardContentReader.EndOfStream)
            {
                // Get line
                string? _value = CardContentReader.ReadLine();

                // The name (N:Sanders;John;;;)
                if (_value.StartsWith(_nameSpecifier))
                {
                    // Check the line
                    string? nameValue = _value.Substring(_nameSpecifier.Length);
                    string[] splitName = nameValue.Split(_fieldDelimiter);
                    if (splitName.Length != 5)
                        throw new InvalidDataException("Name field must specify exactly five values (Last name, first name, alt names, prefixes, and suffixes)");

                    // Populate fields
                    _lastName =  Regex.Unescape(splitName[0]);
                    _firstName = Regex.Unescape(splitName[1]);

                    // Set flag to indicate that the required field is spotted
                    nameSpecifierSpotted = true;
                }

                // Full name (FN:John Sanders)
                if (_value.StartsWith(_fullNameSpecifier))
                {
                    // Get the value
                    string? fullNameValue = _value.Substring(_fullNameSpecifier.Length);

                    // Populate field
                    _fullName = Regex.Unescape(fullNameValue);
                }

                // Telephone (TEL;CELL;HOME:495-522-3560 or TEL;TYPE=cell,home:495-522-3560)
                if (_value.StartsWith(_telephoneSpecifierWithType))
                {
                    // Get the value
                    string? telValue = _value.Substring(_telephoneSpecifierWithType.Length);
                    string[] splitTel = telValue.Split(_argumentDelimiter);
                    if (splitTel.Length != 2)
                        throw new InvalidDataException("Telephone field must specify exactly two values (Type (optionally prepended with TYPE=), and phone number)");

                    // Check to see if the type is prepended with the TYPE= argument
                    string[] splitTypes = splitTel[0].StartsWith(_typeArgumentSpecifier) ?
                                          splitTel[0].Substring(_typeArgumentSpecifier.Length).Split(_valueDelimiter) :
                                          splitTel[0].Split(_fieldDelimiter);

                    // Populate the fields
                    string[] _telephoneTypes =  splitTypes;
                    string _telephoneNumber =   Regex.Unescape(splitTel[1]);
                    TelephoneInfo _telephone = new(_telephoneTypes, _telephoneNumber);
                    _telephones.Add(_telephone);
                }

                // Telephone (TEL:495-522-3560)
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

                // Address (ADR;HOME:;;Los Angeles, USA;;;;)
                if (_value.StartsWith(_addressSpecifierWithType))
                {
                    // Get the value
                    string? adrValue = _value.Substring(_addressSpecifierWithType.Length);
                    string[] splitAdr = adrValue.Split(_argumentDelimiter);
                    if (splitAdr.Length != 2)
                        throw new InvalidDataException("Address field must specify exactly two values (Type (optionally prepended with TYPE=), and address information)");

                    // Check to see if the type is prepended with the TYPE= argument
                    string[] splitTypes = splitAdr[0].StartsWith(_typeArgumentSpecifier) ?
                                          splitAdr[0].Substring(_typeArgumentSpecifier.Length).Split(_valueDelimiter) :
                                          splitAdr[0].Split(_fieldDelimiter);

                    // Check the provided address
                    string[] splitAddressValues = splitAdr[1].Split(_fieldDelimiter);
                    if (splitAddressValues.Length != 7)
                        throw new InvalidDataException("Address information must specify exactly seven values (P.O. Box, extended address, street address, locality, region, postal code, and country)");

                    // Populate the fields
                    string[] _addressTypes =    splitTypes;
                    string _addressPOBox =      Regex.Unescape(splitAddressValues[0]);
                    string _addressExtended =   Regex.Unescape(splitAddressValues[1]);
                    string _addressStreet =     Regex.Unescape(splitAddressValues[2]);
                    string _addressLocality =   Regex.Unescape(splitAddressValues[3]);
                    string _addressRegion =     Regex.Unescape(splitAddressValues[4]);
                    string _addressPostalCode = Regex.Unescape(splitAddressValues[5]);
                    string _addressCountry =    Regex.Unescape(splitAddressValues[6]);
                    AddressInfo _address = new(_addressTypes, _addressPOBox, _addressExtended, _addressStreet, _addressLocality, _addressRegion, _addressPostalCode, _addressCountry);
                    _addresses.Add(_address);
                }

                // Email (EMAIL;HOME;INTERNET:john.s@acme.co or EMAIL;TYPE=HOME,INTERNET:john.s@acme.co)
                if (_value.StartsWith(_emailSpecifier))
                {
                    // Get the value
                    string? mailValue = _value.Substring(_emailSpecifier.Length);
                    string[] splitMail = mailValue.Split(_argumentDelimiter);
                    MailAddress mail;
                    if (splitMail.Length != 2)
                        throw new InvalidDataException("E-mail field must specify exactly two values (Type (optionally prepended with TYPE=), and a valid e-mail address)");

                    // Check to see if the type is prepended with the TYPE= argument
                    string[] splitTypes = splitMail[0].StartsWith(_typeArgumentSpecifier) ?
                                          splitMail[0].Substring(_typeArgumentSpecifier.Length).Split(_valueDelimiter) :
                                          splitMail[0].Split(_fieldDelimiter);

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
                if (_value.StartsWith(_orgSpecifier))
                {
                    // Get the value
                    string? orgValue = _value.Substring(_orgSpecifier.Length);
                    string[] splitOrg = orgValue.Split(_fieldDelimiter);

                    // Populate the fields
                    string _orgName =     Regex.Unescape(splitOrg[0]);
                    string _orgUnit =     Regex.Unescape(splitOrg.Length >= 2 ? splitOrg[1] : "");
                    string _orgUnitRole = Regex.Unescape(splitOrg.Length >= 3 ? splitOrg[2] : "");
                    OrganizationInfo _org = new(_orgName, _orgUnit, _orgUnitRole);
                    _orgs.Add(_org);
                }

                // Title (TITLE:Product Manager)
                if (_value.StartsWith(_titleSpecifier))
                {
                    // Get the value
                    string? titleValue = _value.Substring(_titleSpecifier.Length);

                    // Populate field
                    _title = Regex.Unescape(titleValue);
                }

                // Website link (URL:https://sso.org/)
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
                if (_value.StartsWith(_noteSpecifier))
                {
                    // Get the value
                    string? noteValue = _value.Substring(_noteSpecifier.Length);

                    // Populate field
                    _note = Regex.Unescape(noteValue);
                }

                // X-nonstandard (X-AIM:john.s or X-DL;Design Work Group:List Item 1;List Item 2;List Item 3)
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
                    string[] _xTypes  = splitX[0].Contains(_fieldDelimiter.ToString()) ?
                                        splitX[0].Substring(splitX[0].IndexOf(_fieldDelimiter) + 1)
                                                 .Split(_fieldDelimiter) :
                                        Array.Empty<string>();
                    string[] _xValues = splitX[1].Split(_fieldDelimiter);
                    XNameInfo _x = new(_xName, _xValues, _xTypes);
                    _xes.Add(_x);
                }
            }

            // Requirement checks
            if (!nameSpecifierSpotted)
                throw new InvalidDataException("The name specifier, \"N:\", is required.");

            // Make a new instance of the card
            return new Card(CardVersion, _firstName, _lastName, _fullName, _telephones.ToArray(), _addresses.ToArray(), _orgs.ToArray(), _title, _url, _note, _emails.ToArray(), _xes.ToArray());
        }

        internal VcardTwo(string cardPath, string cardContent, string cardVersion)
        {
            CardPath = cardPath;
            CardContent = cardContent;
            CardVersion = cardVersion;
        }
    }
}
