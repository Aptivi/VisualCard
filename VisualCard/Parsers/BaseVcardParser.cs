//
// VisualCard  Copyright (C) 2021-2024  Aptivi
//
// This file is part of VisualCard
//
// VisualCard is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// VisualCard is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VisualCard.Exceptions;
using VisualCard.Parts;
using VisualCard.Parts.Enums;
using VisualCard.Parts.Implementations;

namespace VisualCard.Parsers
{
    /// <summary>
    /// The base vCard parser
    /// </summary>
    [DebuggerDisplay("vCard contact, version {CardVersion.ToString()}, expected {ExpectedCardVersion.ToString()}, {CardContent.Length} bytes")]
    public abstract class BaseVcardParser : IVcardParser
    {
        /// <summary>
        /// VCard card content
        /// </summary>
        public virtual string CardContent { get; internal set; } = "";
        /// <summary>
        /// VCard card version
        /// </summary>
        public virtual Version CardVersion { get; internal set; } = new();
        /// <summary>
        /// VCard expected card version
        /// </summary>
        public virtual Version ExpectedCardVersion => new();

        /// <summary>
        /// Parses a VCard contact
        /// </summary>
        /// <returns>A strongly-typed <see cref="Card"/> instance holding information about the card</returns>
        public virtual Card Parse()
        {
            // Verify the card data
            VerifyCardData();

            // Now, make a stream out of card content
            byte[] CardContentData = Encoding.Default.GetBytes(CardContent);
            MemoryStream CardContentStream = new(CardContentData, false);
            StreamReader CardContentReader = new(CardContentStream);

            // Make a new vCard
            var card = new Card(this);

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

                // Helper function to wrap things around
                bool StartsWithPrefix(string prefix) =>
                    _value.StartsWith(prefix + delimiter);

                try
                {
                    // Variables
                    string[] splitValueParts = _value.Split(VcardConstants._argumentDelimiter);
                    string[] splitArgs = splitValueParts[0].Split(VcardConstants._fieldDelimiter);
                    splitArgs = splitArgs.Except([splitArgs[0]]).ToArray();
                    string[] splitValues = splitValueParts[1].Split(VcardConstants._fieldDelimiter);
                    List<string> finalArgs = [];
                    int altId = 0;

                    if (splitArgs.Length > 0)
                    {
                        // If we have more than one argument, check for ALTID
                        if (splitArgs[0].StartsWith(VcardConstants._altIdArgumentSpecifier) && CardVersion.Major >= 4)
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

                    // The name (N:Sanders;John;;;)
                    // ALTID is supported.
                    if (StartsWithPrefix(VcardConstants._nameSpecifier))
                    {
                        // Get the name
                        var partInfo =
                            isWithType ?
                            NameInfo.FromStringVcardWithTypeStatic(_value, [.. finalArgs], altId, CardVersion, CardContentReader) :
                            NameInfo.FromStringVcardStatic(_value, altId, CardVersion, CardContentReader);
                        card.AddPartToArray(PartsArrayEnum.Names, partInfo);
                    }

                    // Full name (FN:John Sanders)
                    // Here, we don't support ALTID.
                    if (StartsWithPrefix(VcardConstants._fullNameSpecifier))
                    {
                        // Get the value
                        string fullNameValue = _value.Substring(VcardConstants._fullNameSpecifier.Length + 1);
                        fullNameValue = Regex.Unescape(fullNameValue);

                        // Populate field
                        card.SetString(StringsEnum.FullName, fullNameValue);
                    }

                    // Telephone (TEL;CELL;HOME:495-522-3560 or TEL;TYPE=cell,home:495-522-3560 or TEL:495-522-3560)
                    // ALTID is supported.
                    if (StartsWithPrefix(VcardConstants._telephoneSpecifier))
                    {
                        // Get the name
                        var partInfo =
                            isWithType ?
                            TelephoneInfo.FromStringVcardWithTypeStatic(_value, [.. finalArgs], altId, CardVersion, CardContentReader) :
                            TelephoneInfo.FromStringVcardStatic(_value, altId, CardVersion, CardContentReader);
                        card.AddPartToArray(PartsArrayEnum.Telephones, partInfo);
                    }

                    // Address (ADR;HOME:;;Los Angeles, USA;;;; or ADR:;;Los Angeles, USA;;;;)
                    // ALTID is supported.
                    if (StartsWithPrefix(VcardConstants._addressSpecifier))
                    {
                        // Get the name
                        var partInfo =
                            isWithType ?
                            AddressInfo.FromStringVcardWithTypeStatic(_value, [.. finalArgs], altId, CardVersion, CardContentReader) :
                            AddressInfo.FromStringVcardStatic(_value, altId, CardVersion, CardContentReader);
                        card.AddPartToArray(PartsArrayEnum.Addresses, partInfo);
                    }

                    // Email (EMAIL;HOME;INTERNET:john.s@acme.co or EMAIL;TYPE=HOME,INTERNET:john.s@acme.co)
                    // ALTID is supported.
                    if (StartsWithPrefix(VcardConstants._emailSpecifier))
                    {
                        // Get the name
                        var partInfo =
                            isWithType ?
                            EmailInfo.FromStringVcardWithTypeStatic(_value, [.. finalArgs], altId, CardVersion, CardContentReader) :
                            EmailInfo.FromStringVcardStatic(_value, altId, CardVersion, CardContentReader);
                        card.AddPartToArray(PartsArrayEnum.Mails, partInfo);
                    }

                    // Organization (ORG:Acme Co. or ORG:ABC, Inc.;North American Division;Marketing)
                    // ALTID is supported.
                    if (StartsWithPrefix(VcardConstants._orgSpecifier))
                    {
                        // Get the name
                        var partInfo =
                            isWithType ?
                            OrganizationInfo.FromStringVcardWithTypeStatic(_value, [.. finalArgs], altId, CardVersion, CardContentReader) :
                            OrganizationInfo.FromStringVcardStatic(_value, altId, CardVersion, CardContentReader);
                        card.AddPartToArray(PartsArrayEnum.Organizations, partInfo);
                    }

                    // Title (TITLE:Product Manager)
                    // ALTID is supported.
                    if (StartsWithPrefix(VcardConstants._titleSpecifier))
                    {
                        // Get the name
                        var partInfo =
                            isWithType ?
                            TitleInfo.FromStringVcardWithTypeStatic(_value, [.. finalArgs], altId, CardVersion, CardContentReader) :
                            TitleInfo.FromStringVcardStatic(_value, altId, CardVersion, CardContentReader);
                        card.AddPartToArray(PartsArrayEnum.Titles, partInfo);
                    }

                    // Website link (URL:https://sso.org/)
                    // Here, we don't support ALTID.
                    if (StartsWithPrefix(VcardConstants._urlSpecifier))
                    {
                        // Get the value
                        string urlValue = _value.Substring(VcardConstants._urlSpecifier.Length + 1);

                        // Try to parse the URL to ensure that it conforms to IETF RFC 1738: Uniform Resource Locators
                        if (!Uri.TryCreate(urlValue, UriKind.Absolute, out Uri uri))
                            throw new InvalidDataException($"URL {urlValue} is invalid");

                        // Populate field
                        card.SetString(StringsEnum.Url, uri.ToString());
                    }

                    // Note (NOTE:Product Manager)
                    // Here, we don't support ALTID.
                    if (StartsWithPrefix(VcardConstants._noteSpecifier))
                    {
                        // Get the value
                        string noteValue = _value.Substring(VcardConstants._noteSpecifier.Length + 1);
                        noteValue = Regex.Unescape(noteValue);

                        // Populate field
                        card.SetString(StringsEnum.Notes, noteValue);
                    }

                    // Photo (PHOTO;ENCODING=BASE64;JPEG:... or PHOTO;VALUE=URL:file:///jqpublic.gif or PHOTO;ENCODING=BASE64;TYPE=GIF:...)
                    // ALTID is supported.
                    if (StartsWithPrefix(VcardConstants._photoSpecifier))
                    {
                        if (isWithType)
                        {
                            var partInfo = PhotoInfo.FromStringVcardWithTypeStatic(_value, [.. finalArgs], altId, CardVersion, CardContentReader);
                            card.AddPartToArray(PartsArrayEnum.Photos, partInfo);
                        }
                        else
                            throw new InvalidDataException("Photo field must not have empty type.");
                    }

                    // Logo (LOGO;ENCODING=BASE64;JPEG:... or LOGO;VALUE=URL:file:///jqpublic.gif or LOGO;ENCODING=BASE64;TYPE=GIF:...)
                    // ALTID is supported.
                    if (StartsWithPrefix(VcardConstants._logoSpecifier))
                    {
                        if (isWithType)
                        {
                            var partInfo = LogoInfo.FromStringVcardWithTypeStatic(_value, [.. finalArgs], altId, CardVersion, CardContentReader);
                            card.AddPartToArray(PartsArrayEnum.Logos, partInfo);
                        }
                        else
                            throw new InvalidDataException("Logo field must not have empty type.");
                    }

                    // Sound (SOUND;VALUE=URL:file///multimed/audio/jqpublic.wav or SOUND;WAVE;BASE64:... or SOUND;TYPE=WAVE;ENCODING=BASE64:...)
                    // ALTID is supported.
                    if (StartsWithPrefix(VcardConstants._soundSpecifier))
                    {
                        if (isWithType)
                        {
                            var partInfo = SoundInfo.FromStringVcardWithTypeStatic(_value, [.. finalArgs], altId, CardVersion, CardContentReader);
                            card.AddPartToArray(PartsArrayEnum.Sounds, partInfo);
                        }
                        else
                            throw new InvalidDataException("Sound field must not have empty type.");
                    }

                    // Revision (REV:1995-10-31T22:27:10Z or REV:19951031T222710)
                    // Here, we don't support ALTID.
                    if (StartsWithPrefix(VcardConstants._revSpecifier))
                    {
                        var partInfo = RevisionInfo.FromStringVcardStatic(_value, altId, CardVersion, CardContentReader);
                        card.SetPart(PartsEnum.Revision, partInfo);
                    }

                    // Birthdate (BDAY:19950415 or BDAY:1953-10-15T23:10:00Z)
                    // Here, we don't support ALTID.
                    if (StartsWithPrefix(VcardConstants._birthSpecifier))
                    {
                        var partInfo =
                            isWithType ?
                            BirthDateInfo.FromStringVcardWithTypeStatic(_value, [.. finalArgs], altId, CardVersion, CardContentReader) :
                            BirthDateInfo.FromStringVcardStatic(_value, altId, CardVersion, CardContentReader);
                        card.SetPart(PartsEnum.Birthdate, partInfo);
                    }

                    // Role (ROLE:Programmer)
                    // ALTID is supported.
                    if (StartsWithPrefix(VcardConstants._roleSpecifier))
                    {
                        // Get the name
                        var partInfo =
                            isWithType ?
                            RoleInfo.FromStringVcardWithTypeStatic(_value, [.. finalArgs], altId, CardVersion, CardContentReader) :
                            RoleInfo.FromStringVcardStatic(_value, altId, CardVersion, CardContentReader);
                        card.AddPartToArray(PartsArrayEnum.Roles, partInfo);
                    }

                    // Categories (CATEGORIES:INTERNET or CATEGORIES:INTERNET,IETF,INDUSTRY,INFORMATION TECHNOLOGY)
                    // Here, we don't support ALTID.
                    if (StartsWithPrefix(VcardConstants._categoriesSpecifier))
                    {
                        // Get the value
                        string categoriesValue = _value.Substring(VcardConstants._categoriesSpecifier.Length + 1);

                        // Populate field
                        var categories = CategoryInfo.FromStringVcardStatic(_value, altId, CardVersion, CardContentReader);
                        card.AddPartToArray(PartsArrayEnum.Categories, categories);
                    }

                    // Time Zone (TZ;VALUE=text:-05:00; EST; Raleigh/North America)
                    // ALTID is supported.
                    if (StartsWithPrefix(VcardConstants._timeZoneSpecifier))
                    {
                        // Get the name
                        var partInfo =
                            isWithType ?
                            TimeDateZoneInfo.FromStringVcardWithTypeStatic(_value, [.. finalArgs], altId, CardVersion, CardContentReader) :
                            TimeDateZoneInfo.FromStringVcardStatic(_value, altId, CardVersion, CardContentReader);
                        card.AddPartToArray(PartsArrayEnum.TimeZone, partInfo);
                    }

                    // Geo (GEO;VALUE=uri:https://...)
                    // ALTID is supported.
                    if (StartsWithPrefix(VcardConstants._geoSpecifier))
                    {
                        // Get the name
                        var partInfo =
                            isWithType ?
                            GeoInfo.FromStringVcardWithTypeStatic(_value, [.. finalArgs], altId, CardVersion, CardContentReader) :
                            GeoInfo.FromStringVcardStatic(_value, altId, CardVersion, CardContentReader);
                        card.AddPartToArray(PartsArrayEnum.Geo, partInfo);
                    }

                    // Source (SOURCE:http://johndoe.com/vcard.vcf)
                    // Here, we don't support ALTID.
                    if (StartsWithPrefix(VcardConstants._sourceSpecifier))
                    {
                        // Get the value
                        string sourceStringValue = _value.Substring(VcardConstants._sourceSpecifier.Length + 1);

                        // Try to parse the URL to ensure that it conforms to IETF RFC 1738: Uniform Resource Locators
                        if (!Uri.TryCreate(sourceStringValue, UriKind.Absolute, out Uri uri))
                            throw new InvalidDataException($"URL {sourceStringValue} is invalid");

                        // Populate field
                        string sourceString = uri.ToString();
                        card.SetString(StringsEnum.Source, sourceString);
                    }

                    // Non-standard names (X-AIM:john.s or X-DL;Design Work Group:List Item 1;List Item 2;List Item 3)
                    // ALTID is supported.
                    if (StartsWithPrefix(VcardConstants._xSpecifier))
                    {
                        // Get the name
                        var partInfo =
                            isWithType ?
                            XNameInfo.FromStringVcardWithTypeStatic(_value, [.. finalArgs], altId, CardVersion, CardContentReader) :
                            XNameInfo.FromStringVcardStatic(_value, altId, CardVersion, CardContentReader);
                        card.AddPartToArray(PartsArrayEnum.NonstandardNames, partInfo);
                    }

                    // Now, the keys that are only available in specific versions of vCard

                    // Card type (KIND:individual, KIND:group, KIND:org, KIND:location, ...)
                    // Here, we don't support ALTID.
                    if (StartsWithPrefix(VcardConstants._kindSpecifier) &&
                        StringSupported(StringsEnum.Kind, CardVersion))
                    {
                        // Get the value
                        string kindValue = _value.Substring(VcardConstants._kindSpecifier.Length + 1);

                        // Populate field
                        if (!string.IsNullOrEmpty(kindValue))
                            kindValue = Regex.Unescape(kindValue);
                        card.SetString(StringsEnum.Kind, kindValue);

                        // Let VisualCard know that we've explicitly specified a kind.
                        card.kindExplicitlySpecified = true;
                    }

                    // Label (LABEL;TYPE=dom,home,postal,parcel:Mr.John Q. Public\, Esq.\nMail Drop: TNE QB\n123 Main Street\nAny Town\, CA  91921 - 1234\nU.S.A.)
                    // ALTID is supported.
                    if (StartsWithPrefix(VcardConstants._labelSpecifier) &&
                        EnumArrayTypeSupported(PartsArrayEnum.Labels, CardVersion))
                    {
                        // Get the name
                        var partInfo =
                            isWithType ?
                            LabelAddressInfo.FromStringVcardWithTypeStatic(_value, [.. finalArgs], altId, CardVersion, CardContentReader) :
                            LabelAddressInfo.FromStringVcardStatic(_value, altId, CardVersion, CardContentReader);
                        card.AddPartToArray(PartsArrayEnum.Labels, partInfo);
                    }

                    // Agent (AGENT:BEGIN:VCARD\nFN:Joe Friday\nTEL:+1...)
                    // ALTID is supported.
                    if (StartsWithPrefix(VcardConstants._agentSpecifier) &&
                        EnumArrayTypeSupported(PartsArrayEnum.Agents, CardVersion))
                    {
                        // Get the name
                        var partInfo =
                            isWithType ?
                            AgentInfo.FromStringVcardWithTypeStatic(_value, [.. finalArgs], altId, CardVersion, CardContentReader) :
                            AgentInfo.FromStringVcardStatic(_value, altId, CardVersion, CardContentReader);
                        card.AddPartToArray(PartsArrayEnum.Agents, partInfo);
                    }

                    // Nickname (NICKNAME;TYPE=cell,home:495-522-3560)
                    // ALTID is supported.
                    if (StartsWithPrefix(VcardConstants._nicknameSpecifier) &&
                        EnumArrayTypeSupported(PartsArrayEnum.Nicknames, CardVersion))
                    {
                        // Get the name
                        var partInfo =
                            isWithType ?
                            NicknameInfo.FromStringVcardWithTypeStatic(_value, [.. finalArgs], altId, CardVersion, CardContentReader) :
                            NicknameInfo.FromStringVcardStatic(_value, altId, CardVersion, CardContentReader);
                        card.AddPartToArray(PartsArrayEnum.Nicknames, partInfo);
                    }

                    // Mailer (MAILER:ccMail 2.2 or MAILER:PigeonMail 2.1)
                    // Here, we don't support ALTID.
                    if (StartsWithPrefix(VcardConstants._mailerSpecifier) &&
                        StringSupported(StringsEnum.Mailer, CardVersion))
                    {
                        // Get the value
                        string mailerValue = _value.Substring(VcardConstants._mailerSpecifier.Length + 1);

                        // Populate field
                        mailerValue = Regex.Unescape(mailerValue);
                        card.SetString(StringsEnum.Mailer, mailerValue);
                    }

                    // Product ID (PRODID:-//ONLINE DIRECTORY//NONSGML Version 1//EN)
                    // Here, we don't support ALTID.
                    if (StartsWithPrefix(VcardConstants._productIdSpecifier) &&
                        StringSupported(StringsEnum.ProductId, CardVersion))
                    {
                        // Get the value
                        string prodIdValue = _value.Substring(VcardConstants._productIdSpecifier.Length + 1);

                        // Populate field
                        prodIdValue = Regex.Unescape(prodIdValue);
                        card.SetString(StringsEnum.ProductId, prodIdValue);
                    }

                    // Sort string (SORT-STRING:Harten)
                    // Here, we don't support ALTID.
                    if (StartsWithPrefix(VcardConstants._sortStringSpecifier) &&
                        StringSupported(StringsEnum.SortString, CardVersion))
                    {
                        // Get the value
                        string sortStringValue = _value.Substring(VcardConstants._sortStringSpecifier.Length + 1);

                        // Populate field
                        sortStringValue = Regex.Unescape(sortStringValue);
                        card.SetString(StringsEnum.SortString, sortStringValue);
                    }

                    // IMPP information (IMPP;TYPE=home:sip:test)
                    // ALTID is supported.
                    if (StartsWithPrefix(VcardConstants._imppSpecifier) &&
                        EnumArrayTypeSupported(PartsArrayEnum.Impps, CardVersion))
                    {
                        // Get the name
                        var partInfo =
                            isWithType ?
                            ImppInfo.FromStringVcardWithTypeStatic(_value, [.. finalArgs], altId, CardVersion, CardContentReader) :
                            ImppInfo.FromStringVcardStatic(_value, altId, CardVersion, CardContentReader);
                        card.AddPartToArray(PartsArrayEnum.Impps, partInfo);
                    }

                    // Access classification (CLASS:PUBLIC, CLASS:PRIVATE, or CLASS:CONFIDENTIAL)
                    // Here, we don't support ALTID.
                    if (StartsWithPrefix(VcardConstants._classSpecifier) &&
                        StringSupported(StringsEnum.AccessClassification, CardVersion))
                    {
                        // Get the value
                        string classValue = _value.Substring(VcardConstants._classSpecifier.Length + 1);

                        // Populate field
                        classValue = Regex.Unescape(classValue);
                        card.SetString(StringsEnum.AccessClassification, classValue);
                    }

                    // XML code (XML:<b>Not an xCard XML element</b>)
                    // Here, we don't support ALTID.
                    if (StartsWithPrefix(VcardConstants._xmlSpecifier) &&
                        StringSupported(StringsEnum.Xml, CardVersion))
                    {
                        // Get the value
                        string xmlStringValue = _value.Substring(VcardConstants._xmlSpecifier.Length + 1);

                        // Populate field
                        xmlStringValue = Regex.Unescape(xmlStringValue);
                        card.SetString(StringsEnum.Xml, xmlStringValue);
                    }

                    // Free/busy URL (FBURL:http://example.com/fb/jdoe)
                    // Here, we don't support ALTID.
                    if (StartsWithPrefix(VcardConstants._fbUrlSpecifier) &&
                        StringSupported(StringsEnum.FreeBusyUrl, CardVersion))
                    {
                        // Get the value
                        string fbUrlStringValue = _value.Substring(VcardConstants._fbUrlSpecifier.Length + 1);

                        // Try to parse the URL to ensure that it conforms to IETF RFC 1738: Uniform Resource Locators
                        if (!Uri.TryCreate(fbUrlStringValue, UriKind.Absolute, out Uri uri))
                            throw new InvalidDataException($"URL {fbUrlStringValue} is invalid");

                        // Populate field
                        fbUrlStringValue = uri.ToString();
                        card.SetString(StringsEnum.FreeBusyUrl, fbUrlStringValue);
                    }

                    // Calendar URL (CALURI:http://example.com/calendar/jdoe)
                    // Here, we don't support ALTID.
                    if (StartsWithPrefix(VcardConstants._calUriSpecifier) &&
                        StringSupported(StringsEnum.CalendarUrl, CardVersion))
                    {
                        // Get the value
                        string calUriStringValue = _value.Substring(VcardConstants._calUriSpecifier.Length + 1);

                        // Try to parse the URL to ensure that it conforms to IETF RFC 1738: Uniform Resource Locators
                        if (!Uri.TryCreate(calUriStringValue, UriKind.Absolute, out Uri uri))
                            throw new InvalidDataException($"URL {calUriStringValue} is invalid");

                        // Populate field
                        calUriStringValue = uri.ToString();
                        card.SetString(StringsEnum.CalendarUrl, calUriStringValue);
                    }

                    // Calendar Request URL (CALADRURI:http://example.com/calendar/jdoe/request)
                    // Here, we don't support ALTID.
                    if (StartsWithPrefix(VcardConstants._caladrUriSpecifier) &&
                        StringSupported(StringsEnum.CalendarSchedulingRequestUrl, CardVersion))
                    {
                        // Get the value
                        string caladrUriStringValue = _value.Substring(VcardConstants._caladrUriSpecifier.Length + 1);

                        // Try to parse the URL to ensure that it conforms to IETF RFC 1738: Uniform Resource Locators
                        if (!Uri.TryCreate(caladrUriStringValue, UriKind.Absolute, out Uri uri))
                            throw new InvalidDataException($"URL {caladrUriStringValue} is invalid");

                        // Populate field
                        caladrUriStringValue = uri.ToString();
                        card.SetString(StringsEnum.CalendarSchedulingRequestUrl, caladrUriStringValue);
                    }

                    // Wedding anniversary (ANNIVERSARY:19960415)
                    // Here, we don't support ALTID.
                    if (StartsWithPrefix(VcardConstants._anniversarySpecifier) &&
                        EnumTypeSupported(PartsEnum.Anniversary, CardVersion))
                    {
                        var partInfo = AnniversaryInfo.FromStringVcardStatic(_value, altId, CardVersion, CardContentReader);
                        card.SetPart(PartsEnum.Anniversary, partInfo);
                    }

                    // IMPP information (IMPP;TYPE=home:sip:test)
                    // ALTID is supported.
                    if (StartsWithPrefix(VcardConstants._genderSpecifier) &&
                        EnumTypeSupported(PartsEnum.Gender, CardVersion))
                    {
                        // Get the name
                        var partInfo = GenderInfo.FromStringVcardStatic(_value, altId, CardVersion, CardContentReader);
                        card.SetPart(PartsEnum.Gender, partInfo);
                    }
                }
                catch (Exception ex)
                {
                    throw new VCardParseException(ex.Message, _value, lineNumber, ex);
                }
            }

            // Validate this card before returning it.
            ValidateCard(card);
            return card;
        }

        /// <summary>
        /// Saves a parsed card to the string
        /// </summary>
        /// <param name="card">Parsed card</param>
        public virtual string SaveToString(Card card)
        {
            // Verify the card data
            VerifyCardData();

            // Initialize the card builder
            var cardBuilder = new StringBuilder();
            var version = card.CardVersion;

            // First, write the header
            cardBuilder.AppendLine("BEGIN:VCARD");
            cardBuilder.AppendLine($"VERSION:{version}");

            // Then, enumerate all the strings
            StringsEnum[] stringEnums = (StringsEnum[])Enum.GetValues(typeof(StringsEnum));
            foreach (StringsEnum stringEnum in stringEnums)
            {
                // Get the string value
                string stringValue = card.GetString(stringEnum);
                if (string.IsNullOrEmpty(stringValue))
                    continue;

                // Check to see if kind is specified
                if (!card.kindExplicitlySpecified && stringEnum == StringsEnum.Kind)
                    continue;

                // Now, locate the prefix and assemble the line
                string prefix = stringEnum switch
                {
                    StringsEnum.AccessClassification => VcardConstants._classSpecifier,
                    StringsEnum.CalendarSchedulingRequestUrl => VcardConstants._caladrUriSpecifier,
                    StringsEnum.CalendarUrl => VcardConstants._calUriSpecifier,
                    StringsEnum.FreeBusyUrl => VcardConstants._fbUrlSpecifier,
                    StringsEnum.FullName => VcardConstants._fullNameSpecifier,
                    StringsEnum.Kind => VcardConstants._kindSpecifier,
                    StringsEnum.Mailer => VcardConstants._mailerSpecifier,
                    StringsEnum.Notes => VcardConstants._noteSpecifier,
                    StringsEnum.ProductId => VcardConstants._productIdSpecifier,
                    StringsEnum.SortString => VcardConstants._sortStringSpecifier,
                    StringsEnum.Source => VcardConstants._sourceSpecifier,
                    StringsEnum.Url => VcardConstants._urlSpecifier,
                    StringsEnum.Xml => VcardConstants._xmlSpecifier,
                    _ => throw new NotImplementedException($"String enumeration {stringEnum} is not implemented.")
                };
                cardBuilder.AppendLine($"{prefix}:{stringValue}");
            }

            // Next, enumerate all the arrays
            PartsArrayEnum[] partsArrayEnums = (PartsArrayEnum[])Enum.GetValues(typeof(PartsArrayEnum));
            foreach (PartsArrayEnum partsArrayEnum in partsArrayEnums)
            {
                // Get the array value
                var array = card.GetPartsArray(partsArrayEnum);
                if (array is null || array.Length == 0)
                    continue;

                // Now, assemble the line
                foreach (var part in array)
                    cardBuilder.AppendLine($"{part.ToStringVcardInternal(version)}");
            }

            // Finally, enumerate all the parts
            PartsEnum[] partsEnums = (PartsEnum[])Enum.GetValues(typeof(PartsEnum));
            foreach (PartsEnum partsEnum in partsEnums)
            {
                // Get the part value
                var part = card.GetPart(partsEnum);
                if (part is null)
                    continue;

                // Now, assemble the line
                cardBuilder.AppendLine($"{part.ToStringVcardInternal(version)}");
            }

            // End the card and return it
            cardBuilder.AppendLine("END:VCARD");
            return cardBuilder.ToString();
        }

        /// <summary>
        /// Saves a parsed card to a file path
        /// </summary>
        /// <param name="path">File path to save the card to</param>
        /// <param name="card">Parsed card</param>
        public void SaveTo(string path, Card card)
        {
            // Verify the card data
            VerifyCardData();

            // Save all the changes to the file
            var cardString = SaveToString(card);
            File.WriteAllText(path, cardString);
        }

        internal void VerifyCardData()
        {
            // Check the version to ensure that we're really dealing with VCard 2.1 contact
            if (CardVersion != ExpectedCardVersion)
                throw new InvalidDataException($"Card version {CardVersion} doesn't match expected {ExpectedCardVersion}.");

            // Check the content to ensure that we really have data
            if (string.IsNullOrEmpty(CardContent))
                throw new InvalidDataException($"Card content is empty.");
        }

        internal static string MakeStringBlock(string target, int firstLength)
        {
            const int maxChars = 74;
            int maxCharsFirst = maxChars - firstLength + 1;

            // Construct the block
            StringBuilder block = new();
            int selectedMax = maxCharsFirst;
            int processed = 0;
            for (int currCharNum = 0; currCharNum < target.Length; currCharNum++)
            {
                block.Append(target[currCharNum]);
                processed++;
                if (processed >= selectedMax)
                {
                    // Append a new line because we reached the maximum limit
                    selectedMax = maxChars;
                    processed = 0;
                    block.Append("\n ");
                }
            }
            return block.ToString();
        }

        internal static bool StringSupported(StringsEnum stringsEnum, Version cardVersion) =>
            stringsEnum switch
            {
                StringsEnum.FullName => true,
                StringsEnum.Url => true,
                StringsEnum.Notes => true,
                StringsEnum.Source => true,
                StringsEnum.Kind => cardVersion.Major >= 4,
                StringsEnum.Mailer => cardVersion.Major != 4,
                StringsEnum.ProductId => cardVersion.Major >= 3,
                StringsEnum.SortString => cardVersion.Major == 3 || cardVersion.Major == 5,
                StringsEnum.AccessClassification => cardVersion.Major != 2 || cardVersion.Major != 4,
                StringsEnum.Xml => cardVersion.Major == 4,
                StringsEnum.FreeBusyUrl => cardVersion.Major >= 4,
                StringsEnum.CalendarUrl => cardVersion.Major >= 4,
                StringsEnum.CalendarSchedulingRequestUrl => cardVersion.Major >= 4,
                _ =>
                    throw new InvalidOperationException("Invalid string enumeration type to get supported value"),
            };

        internal static bool EnumArrayTypeSupported(PartsArrayEnum partsArrayEnum, Version cardVersion) =>
            partsArrayEnum switch
            {
                PartsArrayEnum.Names => true,
                PartsArrayEnum.Telephones => true,
                PartsArrayEnum.Addresses => true,
                PartsArrayEnum.Mails => true,
                PartsArrayEnum.Organizations => true,
                PartsArrayEnum.Titles => true,
                PartsArrayEnum.Photos => true,
                PartsArrayEnum.Roles => true,
                PartsArrayEnum.Logos => true,
                PartsArrayEnum.TimeZone => true,
                PartsArrayEnum.Geo => true,
                PartsArrayEnum.Sounds => true,
                PartsArrayEnum.Categories => true,
                PartsArrayEnum.NonstandardNames => true,
                PartsArrayEnum.Impps => cardVersion.Major >= 3,
                PartsArrayEnum.Nicknames => cardVersion.Major >= 3,
                PartsArrayEnum.Labels => cardVersion.Major != 4,
                PartsArrayEnum.Agents => cardVersion.Major != 4,
                _ =>
                    throw new InvalidOperationException("Invalid parts array enumeration type to get supported value"),
            };

        internal static bool EnumTypeSupported(PartsEnum partsEnum, Version cardVersion) =>
            partsEnum switch
            {
                PartsEnum.Revision => true,
                PartsEnum.Birthdate => true,
                PartsEnum.Anniversary => cardVersion.Major >= 4,
                PartsEnum.Gender => cardVersion.Major >= 4,
                _ =>
                    throw new InvalidOperationException("Invalid parts enumeration type to get supported value"),
            };

        internal static Type GetEnumArrayType(PartsArrayEnum partsArrayEnum) =>
            partsArrayEnum switch
            {
                PartsArrayEnum.Names => typeof(NameInfo),
                PartsArrayEnum.Telephones => typeof(TelephoneInfo),
                PartsArrayEnum.Addresses => typeof(AddressInfo),
                PartsArrayEnum.Labels => typeof(LabelAddressInfo),
                PartsArrayEnum.Agents => typeof(AgentInfo),
                PartsArrayEnum.Mails => typeof(EmailInfo),
                PartsArrayEnum.Organizations => typeof(OrganizationInfo),
                PartsArrayEnum.Titles => typeof(TitleInfo),
                PartsArrayEnum.Photos => typeof(PhotoInfo),
                PartsArrayEnum.Nicknames => typeof(NicknameInfo),
                PartsArrayEnum.Roles => typeof(RoleInfo),
                PartsArrayEnum.Logos => typeof(LogoInfo),
                PartsArrayEnum.TimeZone => typeof(TimeDateZoneInfo),
                PartsArrayEnum.Geo => typeof(GeoInfo),
                PartsArrayEnum.Sounds => typeof(SoundInfo),
                PartsArrayEnum.Impps => typeof(ImppInfo),
                PartsArrayEnum.Categories => typeof(CategoryInfo),
                PartsArrayEnum.NonstandardNames => typeof(XNameInfo),
                _ =>
                    throw new InvalidOperationException("Invalid parts array enumeration type"),
            };

        internal static Type GetEnumType(PartsEnum partsEnum) =>
            partsEnum switch
            {
                PartsEnum.Revision => typeof(RevisionInfo),
                PartsEnum.Birthdate => typeof(BirthDateInfo),
                PartsEnum.Anniversary => typeof(AnniversaryInfo),
                PartsEnum.Gender => typeof(GenderInfo),
                _ =>
                    throw new InvalidOperationException("Invalid parts enumeration type"),
            };

        internal void ValidateCard(Card card)
        {
            // Track the required fields
            List<string> expectedFields = [];
            List<string> actualFields = [];
            switch (CardVersion.ToString(2))
            {
                case "2.1":
                    expectedFields.Add(VcardConstants._nameSpecifier);
                    break;
                case "4.0":
                    expectedFields.Add(VcardConstants._fullNameSpecifier);
                    break;
                case "3.0":
                case "5.0":
                    expectedFields.Add(VcardConstants._nameSpecifier);
                    expectedFields.Add(VcardConstants._fullNameSpecifier);
                    break;
            }

            // Requirement checks
            if (expectedFields.Contains(VcardConstants._nameSpecifier))
            {
                var names = card.GetPartsArray(PartsArrayEnum.Names);
                bool exists = names is not null && names.Length > 0;
                if (exists)
                    actualFields.Add(VcardConstants._nameSpecifier);
            }
            if (expectedFields.Contains(VcardConstants._fullNameSpecifier))
            {
                string fullName = card.GetString(StringsEnum.FullName);
                bool exists = !string.IsNullOrEmpty(fullName);
                if (exists)
                    actualFields.Add(VcardConstants._fullNameSpecifier);
            }
            expectedFields.Sort();
            actualFields.Sort();
            if (!actualFields.SequenceEqual(expectedFields))
                throw new InvalidDataException($"The following keys [{string.Join(", ", expectedFields)}] are required. Got [{string.Join(", ", actualFields)}].");
        }
    }
}
