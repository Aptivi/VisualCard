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
using System.IO;
using VisualCard.Parts.Enums;
using VisualCard.Parts.Implementations;
using VisualCard.Parts;
using System.Text.RegularExpressions;
using VisualCard.Parsers.Recurrence;
using VisualCard.Parsers.Arguments;

namespace VisualCard.Parsers
{
    internal class VcardParserTools
    {
        internal static bool StringSupported(StringsEnum stringsEnum, Version cardVersion, string kind) =>
            stringsEnum switch
            {
                StringsEnum.Kind => cardVersion.Major >= 4,
                StringsEnum.Mailer => cardVersion.Major != 4,
                StringsEnum.ProductId => cardVersion.Major >= 3,
                StringsEnum.SortString => cardVersion.Major == 3 || cardVersion.Major == 5,
                StringsEnum.AccessClassification => cardVersion.Major != 2 || cardVersion.Major != 4,
                StringsEnum.Uid => cardVersion.Major <= 4,
                StringsEnum.SourceName => cardVersion.Major == 3,
                StringsEnum.Profile => cardVersion.Major == 3,
                StringsEnum.Telephones => true,
                StringsEnum.Mails => true,
                StringsEnum.Titles => true,
                StringsEnum.Roles => true,
                StringsEnum.TimeZone => true,
                StringsEnum.Geo => true,
                StringsEnum.FullName => true,
                StringsEnum.Url => true,
                StringsEnum.Notes => true,
                StringsEnum.Source => true,
                StringsEnum.Impps => cardVersion.Major >= 3,
                StringsEnum.Nicknames => cardVersion.Major >= 3,
                StringsEnum.Labels => cardVersion.Major != 4,
                StringsEnum.Langs => cardVersion.Major >= 4,
                StringsEnum.FreeBusyUrl => cardVersion.Major >= 4,
                StringsEnum.CalendarUrl => cardVersion.Major >= 4,
                StringsEnum.CalendarSchedulingRequestUrl => cardVersion.Major >= 4,
                StringsEnum.ContactUri => cardVersion.Major >= 4,
                StringsEnum.Member => kind == "group" && cardVersion.Major == 4,
                StringsEnum.Related => cardVersion.Major == 4,
                _ =>
                    throw new InvalidOperationException("Invalid string enumeration type to get supported value"),
            };

        internal static bool EnumArrayTypeSupported(PartsArrayEnum partsArrayEnum, Version cardVersion) =>
            partsArrayEnum switch
            {
                PartsArrayEnum.Names => true,
                PartsArrayEnum.Addresses => true,
                PartsArrayEnum.Organizations => true,
                PartsArrayEnum.Photos => true,
                PartsArrayEnum.Logos => true,
                PartsArrayEnum.Sounds => true,
                PartsArrayEnum.Categories => true,
                PartsArrayEnum.Key => true,
                PartsArrayEnum.Revision => true,
                PartsArrayEnum.Birthdate => true,
                PartsArrayEnum.Agents => cardVersion.Major != 4,
                PartsArrayEnum.Xml => cardVersion.Major == 4,
                PartsArrayEnum.Anniversary => cardVersion.Major >= 4,
                PartsArrayEnum.Gender => cardVersion.Major >= 4,
                PartsArrayEnum.ClientPidMap => cardVersion.Major == 4,

                // Extensions are allowed
                _ => true,
            };

        internal static string GetPrefixFromStringsEnum(StringsEnum stringsEnum) =>
            stringsEnum switch
            {
                StringsEnum.AccessClassification => VcardConstants._classSpecifier,
                StringsEnum.Kind => VcardConstants._kindSpecifier,
                StringsEnum.Mailer => VcardConstants._mailerSpecifier,
                StringsEnum.ProductId => VcardConstants._productIdSpecifier,
                StringsEnum.SortString => VcardConstants._sortStringSpecifier,
                StringsEnum.Uid => VcardConstants._uidSpecifier,
                StringsEnum.SourceName => VcardConstants._srcNameSpecifier,
                StringsEnum.Profile => VcardConstants._profileSpecifier,
                StringsEnum.Telephones => VcardConstants._telephoneSpecifier,
                StringsEnum.Labels => VcardConstants._labelSpecifier,
                StringsEnum.Mails => VcardConstants._emailSpecifier,
                StringsEnum.Titles => VcardConstants._titleSpecifier,
                StringsEnum.Nicknames => VcardConstants._nicknameSpecifier,
                StringsEnum.Roles => VcardConstants._roleSpecifier,
                StringsEnum.TimeZone => VcardConstants._timeZoneSpecifier,
                StringsEnum.Geo => VcardConstants._geoSpecifier,
                StringsEnum.Impps => VcardConstants._imppSpecifier,
                StringsEnum.Langs => VcardConstants._langSpecifier,
                StringsEnum.CalendarSchedulingRequestUrl => VcardConstants._caladrUriSpecifier,
                StringsEnum.CalendarUrl => VcardConstants._calUriSpecifier,
                StringsEnum.FreeBusyUrl => VcardConstants._fbUrlSpecifier,
                StringsEnum.FullName => VcardConstants._fullNameSpecifier,
                StringsEnum.Notes => VcardConstants._noteSpecifier,
                StringsEnum.Source => VcardConstants._sourceSpecifier,
                StringsEnum.Url => VcardConstants._urlSpecifier,
                StringsEnum.ContactUri => VcardConstants._contactUriSpecifier,
                StringsEnum.Member => VcardConstants._memberSpecifier,
                StringsEnum.Related => VcardConstants._relatedSpecifier,
                _ =>
                    throw new NotImplementedException($"String enumeration {stringsEnum} is not implemented.")
            };

        internal static string GetPrefixFromPartsArrayEnum(PartsArrayEnum partsArrayEnum) =>
            partsArrayEnum switch
            {
                PartsArrayEnum.Names => VcardConstants._nameSpecifier,
                PartsArrayEnum.Addresses => VcardConstants._addressSpecifier,
                PartsArrayEnum.Agents => VcardConstants._agentSpecifier,
                PartsArrayEnum.Organizations => VcardConstants._orgSpecifier,
                PartsArrayEnum.Photos => VcardConstants._photoSpecifier,
                PartsArrayEnum.Logos => VcardConstants._logoSpecifier,
                PartsArrayEnum.Sounds => VcardConstants._soundSpecifier,
                PartsArrayEnum.Categories => VcardConstants._categoriesSpecifier,
                PartsArrayEnum.Xml => VcardConstants._xmlSpecifier,
                PartsArrayEnum.Key => VcardConstants._keySpecifier,
                PartsArrayEnum.Birthdate => VcardConstants._birthSpecifier,
                PartsArrayEnum.Revision => VcardConstants._revSpecifier,
                PartsArrayEnum.Anniversary => VcardConstants._anniversarySpecifier,
                PartsArrayEnum.Gender => VcardConstants._genderSpecifier,
                PartsArrayEnum.ClientPidMap => VcardConstants._clientPidMapSpecifier,

                // Extensions are allowed
                PartsArrayEnum.NonstandardNames => VcardConstants._xSpecifier,
                _ => ""
            };

        internal static PartCardinality GetStringsEnumFromType(StringsEnum strings, Version cardVersion)
        {
            // Now, iterate through every type
            return strings switch
            {
                StringsEnum.Telephones => PartCardinality.Any,
                StringsEnum.Labels => PartCardinality.Any,
                StringsEnum.Mails => PartCardinality.Any,
                StringsEnum.Titles => PartCardinality.Any,
                StringsEnum.Nicknames => PartCardinality.Any,
                StringsEnum.Roles => PartCardinality.Any,
                StringsEnum.TimeZone => PartCardinality.Any,
                StringsEnum.Geo => PartCardinality.Any,
                StringsEnum.Impps => PartCardinality.Any,
                StringsEnum.Langs => PartCardinality.Any,
                StringsEnum.FullName => cardVersion.Major >= 3 ? PartCardinality.AtLeastOne : PartCardinality.Any,
                StringsEnum.Url => PartCardinality.Any,
                StringsEnum.Notes => PartCardinality.Any,
                StringsEnum.Source => PartCardinality.Any,
                StringsEnum.FreeBusyUrl => PartCardinality.Any,
                StringsEnum.CalendarUrl => PartCardinality.Any,
                StringsEnum.CalendarSchedulingRequestUrl => PartCardinality.Any,
                StringsEnum.ContactUri => PartCardinality.Any,
                StringsEnum.Member => PartCardinality.Any,
                StringsEnum.Related => PartCardinality.Any,
                _ => throw new ArgumentException($"There is no string enum info for {strings}"),
            };
        }

        internal static (PartsArrayEnum, PartCardinality) GetPartsArrayEnumFromType(Type? partsArrayType, Version cardVersion)
        {
            if (partsArrayType is null)
                throw new NotImplementedException("Type is not provided.");

            // Now, iterate through every type
            if (partsArrayType == typeof(NameInfo))
                return (PartsArrayEnum.Names, cardVersion.Major == 4 ? PartCardinality.MayBeOne : PartCardinality.ShouldBeOne);
            else if (partsArrayType == typeof(AddressInfo))
                return (PartsArrayEnum.Addresses, PartCardinality.Any);
            else if (partsArrayType == typeof(AgentInfo))
                return (PartsArrayEnum.Agents, PartCardinality.Any);
            else if (partsArrayType == typeof(OrganizationInfo))
                return (PartsArrayEnum.Organizations, PartCardinality.Any);
            else if (partsArrayType == typeof(PhotoInfo))
                return (PartsArrayEnum.Photos, PartCardinality.Any);
            else if (partsArrayType == typeof(LogoInfo))
                return (PartsArrayEnum.Logos, PartCardinality.Any);
            else if (partsArrayType == typeof(SoundInfo))
                return (PartsArrayEnum.Sounds, PartCardinality.Any);
            else if (partsArrayType == typeof(CategoryInfo))
                return (PartsArrayEnum.Categories, PartCardinality.Any);
            else if (partsArrayType == typeof(XmlInfo))
                return (PartsArrayEnum.Xml, PartCardinality.Any);
            else if (partsArrayType == typeof(KeyInfo))
                return (PartsArrayEnum.Key, PartCardinality.Any);
            else if (partsArrayType == typeof(RevisionInfo))
                return (PartsArrayEnum.Revision, PartCardinality.MayBeOneNoAltId);
            else if (partsArrayType == typeof(BirthDateInfo))
                return (PartsArrayEnum.Birthdate, PartCardinality.MayBeOne);
            else if (partsArrayType == typeof(AnniversaryInfo))
                return (PartsArrayEnum.Anniversary, PartCardinality.MayBeOne);
            else if (partsArrayType == typeof(GenderInfo))
                return (PartsArrayEnum.Gender, PartCardinality.MayBeOneNoAltId);
            else if (partsArrayType == typeof(ClientPidMapInfo))
                return (PartsArrayEnum.ClientPidMap, PartCardinality.Any);

            // Extensions are allowed
            else if (partsArrayType == typeof(XNameInfo))
                return (PartsArrayEnum.NonstandardNames, PartCardinality.Any);
            return (PartsArrayEnum.IanaNames, PartCardinality.Any);
        }

        internal static (PartType type, object enumeration, Type? enumType, Func<string, ArgumentInfo[], int, string[], string, string, Version, BaseCardPartInfo>? fromStringFunc, string defaultType, string defaultValue, string defaultValueType, string[] allowedExtraTypes, string[] allowedValues) GetPartType(string prefix) =>
            prefix switch
            {
                VcardConstants._nameSpecifier => (PartType.PartsArray, PartsArrayEnum.Names, typeof(NameInfo), NameInfo.FromStringVcardStatic, "", "", "text", [], []),
                VcardConstants._addressSpecifier => (PartType.PartsArray, PartsArrayEnum.Addresses, typeof(AddressInfo), AddressInfo.FromStringVcardStatic, "HOME", "", "text", ["DOM", "INTL", "PARCEL", "POSTAL"], []),
                VcardConstants._agentSpecifier => (PartType.PartsArray, PartsArrayEnum.Agents, typeof(AgentInfo), AgentInfo.FromStringVcardStatic, "", "", "inline", [], []),
                VcardConstants._orgSpecifier => (PartType.PartsArray, PartsArrayEnum.Organizations, typeof(OrganizationInfo), OrganizationInfo.FromStringVcardStatic, "WORK", "", "text", [], []),
                VcardConstants._photoSpecifier => (PartType.PartsArray, PartsArrayEnum.Photos, typeof(PhotoInfo), PhotoInfo.FromStringVcardStatic, "JPEG", "", "inline", ["JPG", "GIF", "CGM", "WMF", "BMP", "MET", "PMB", "DIB", "PICT", "TIFF", "PS", "PDF", "JPEG", "MPEG", "MPEG2", "AVI", "QTIME", "PNG", "WEBP"], []),
                VcardConstants._logoSpecifier => (PartType.PartsArray, PartsArrayEnum.Logos, typeof(LogoInfo), LogoInfo.FromStringVcardStatic, "JPEG", "", "inline", ["JPG", "GIF", "CGM", "WMF", "BMP", "MET", "PMB", "DIB", "PICT", "TIFF", "PS", "PDF", "JPEG", "MPEG", "MPEG2", "AVI", "QTIME", "PNG", "WEBP"], []),
                VcardConstants._soundSpecifier => (PartType.PartsArray, PartsArrayEnum.Sounds, typeof(SoundInfo), SoundInfo.FromStringVcardStatic, "MP3", "", "inline", ["MP3", "WAVE", "PCM", "AIFF", "AAC"], []),
                VcardConstants._categoriesSpecifier => (PartType.PartsArray, PartsArrayEnum.Categories, typeof(CategoryInfo), CategoryInfo.FromStringVcardStatic, "", "", "text", [], []),
                VcardConstants._xmlSpecifier => (PartType.PartsArray, PartsArrayEnum.Xml, typeof(XmlInfo), XmlInfo.FromStringVcardStatic, "", "", "text", [], []),
                VcardConstants._keySpecifier => (PartType.PartsArray, PartsArrayEnum.Key, typeof(KeyInfo), KeyInfo.FromStringVcardStatic, "", "", "text", [], []),
                VcardConstants._revSpecifier => (PartType.PartsArray, PartsArrayEnum.Revision, typeof(RevisionInfo), RevisionInfo.FromStringVcardStatic, "", "", "timestamp", [], []),
                VcardConstants._birthSpecifier => (PartType.PartsArray, PartsArrayEnum.Birthdate, typeof(BirthDateInfo), BirthDateInfo.FromStringVcardStatic, "", "", "date-and-or-time", [], []),
                VcardConstants._anniversarySpecifier => (PartType.PartsArray, PartsArrayEnum.Anniversary, typeof(AnniversaryInfo), AnniversaryInfo.FromStringVcardStatic, "", "", "date-and-or-time", [], []),
                VcardConstants._genderSpecifier => (PartType.PartsArray, PartsArrayEnum.Gender, typeof(GenderInfo), GenderInfo.FromStringVcardStatic, "", "", "text", [], []),
                VcardConstants._clientPidMapSpecifier => (PartType.PartsArray, PartsArrayEnum.ClientPidMap, typeof(ClientPidMapInfo), ClientPidMapInfo.FromStringVcardStatic, "", "", "text", [], []),
                VcardConstants._kindSpecifier => (PartType.Strings, StringsEnum.Kind, null, null, "", "", "text", [], []),
                VcardConstants._mailerSpecifier => (PartType.Strings, StringsEnum.Mailer, null, null, "", "", "text", [], []),
                VcardConstants._productIdSpecifier => (PartType.Strings, StringsEnum.ProductId, null, null, "", "", "text", [], []),
                VcardConstants._sortStringSpecifier => (PartType.Strings, StringsEnum.SortString, null, null, "", "", "text", [], []),
                VcardConstants._classSpecifier => (PartType.Strings, StringsEnum.AccessClassification, null, null, "", "", "text", [], []),
                VcardConstants._uidSpecifier => (PartType.Strings, StringsEnum.Uid, null, null, "", "", "text", [], []),
                VcardConstants._srcNameSpecifier => (PartType.Strings, StringsEnum.SourceName, null, null, "", "", "text", [], []),
                VcardConstants._profileSpecifier => (PartType.Strings, StringsEnum.Profile, null, null, "", "", "text", [], []),
                VcardConstants._telephoneSpecifier => (PartType.Strings, StringsEnum.Telephones, null, null, "CELL", "", "text", ["TEXT", "VOICE", "FAX", "CELL", "VIDEO", "PAGER", "TEXTPHONE", "ISDN", "CAR", "MODEM", "BBS", "MSG", "PREF", "TLX", "MMS"], []),
                VcardConstants._labelSpecifier => (PartType.Strings, StringsEnum.Labels, null, null, "HOME", "", "text", ["DOM", "INTL", "PARCEL", "POSTAL"], []),
                VcardConstants._emailSpecifier => (PartType.Strings, StringsEnum.Mails, null, null, "HOME", "", "text", ["AOL", "APPLELINK", "ATTMAIL", "CIS", "EWORLD", "INTERNET", "IBMMAIL", "MCIMAIL", "POWERSHARE", "PRODIGY", "TLX", "X400", "CELL"], []),
                VcardConstants._titleSpecifier => (PartType.Strings, StringsEnum.Titles, null, null, "", "", "text", [], []),
                VcardConstants._nicknameSpecifier => (PartType.Strings, StringsEnum.Nicknames, null, null, "HOME", "", "text", [], []),
                VcardConstants._roleSpecifier => (PartType.Strings, StringsEnum.Roles, null, null, "", "", "text", [], []),
                VcardConstants._timeZoneSpecifier => (PartType.Strings, StringsEnum.TimeZone, null, null, "", "", "utc-offset", [], []),
                VcardConstants._geoSpecifier => (PartType.Strings, StringsEnum.Geo, null, null, "", "", "text", [], []),
                VcardConstants._imppSpecifier => (PartType.Strings, StringsEnum.Impps, null, null, "SIP", "", "uri", ["SIP"], []),
                VcardConstants._langSpecifier => (PartType.Strings, StringsEnum.Langs, null, null, "HOME", "", "language-tag", [], []),
                VcardConstants._fullNameSpecifier => (PartType.Strings, StringsEnum.FullName, null, null, "", "", "text", [], []),
                VcardConstants._urlSpecifier => (PartType.Strings, StringsEnum.Url, null, null, "", "", "uri", [], []),
                VcardConstants._noteSpecifier => (PartType.Strings, StringsEnum.Notes, null, null, "", "", "text", [], []),
                VcardConstants._sourceSpecifier => (PartType.Strings, StringsEnum.Source, null, null, "", "", "uri", [], []),
                VcardConstants._fbUrlSpecifier => (PartType.Strings, StringsEnum.FreeBusyUrl, null, null, "", "", "uri", [], []),
                VcardConstants._calUriSpecifier => (PartType.Strings, StringsEnum.CalendarUrl, null, null, "", "", "uri", [], []),
                VcardConstants._caladrUriSpecifier => (PartType.Strings, StringsEnum.CalendarSchedulingRequestUrl, null, null, "", "", "uri", [], []),
                VcardConstants._contactUriSpecifier => (PartType.Strings, StringsEnum.ContactUri, null, null, "", "", "uri", [], []),
                VcardConstants._memberSpecifier => (PartType.Strings, StringsEnum.Member, null, null, "", "", "uri", [], []),
                VcardConstants._relatedSpecifier => (PartType.Strings, StringsEnum.Related, null, null, "", "", "uri", [], []),

                // Extensions are allowed
                VcardConstants._xSpecifier => (PartType.PartsArray, PartsArrayEnum.NonstandardNames, typeof(XNameInfo), XNameInfo.FromStringVcardStatic, "", "", "", [], []),
                _ => (PartType.PartsArray, PartsArrayEnum.IanaNames, typeof(ExtraInfo), ExtraInfo.FromStringVcardStatic, "", "", "", [], []),
            };

        internal static string ProcessStringValue(string value, string valueType, char split = ';')
        {
            // Now, handle each type individually
            string finalValue;
            finalValue = Regex.Unescape(value);
            foreach (string finalValuePart in finalValue.Split(split))
            {
                switch (valueType.ToUpper())
                {
                    case "URI":
                    case "URL":
                        // Check the URI
                        if (!Uri.TryCreate(finalValue, UriKind.Absolute, out Uri uri))
                            throw new InvalidDataException($"URL {finalValue} is invalid");
                        finalValue = uri is not null ? uri.ToString() : finalValue;
                        break;
                    case "UTC-OFFSET":
                        // Check the UTC offset
                        VcardCommonTools.ParseUtcOffset(finalValue);
                        break;
                    case "DATE":
                        // Check the date
                        if (!VcardCommonTools.TryParsePosixDate(finalValue, out _))
                            throw new InvalidDataException($"Date {finalValue} is invalid");
                        break;
                    case "TIME":
                        // Check the time
                        if (!VcardCommonTools.TryParsePosixTime(finalValue, out _))
                            throw new InvalidDataException($"Time {finalValue} is invalid");
                        break;
                    case "DATE-TIME":
                        // Check the date and time
                        if (!VcardCommonTools.TryParsePosixDateTime(finalValue, out _))
                            throw new InvalidDataException($"Date and time {finalValue} is invalid");
                        break;
                    case "DATE-AND-OR-TIME":
                        // Check the date and/or time
                        if (!VcardCommonTools.TryParsePosixDateTime(finalValue, out _) &&
                            !VcardCommonTools.TryParsePosixTime(finalValue, out _))
                            throw new InvalidDataException($"Date and/or time {finalValue} is invalid");
                        break;
                    case "TIMESTAMP":
                        // Check the timestamp
                        if (!VcardCommonTools.TryParsePosixTimestamp(finalValue, out _))
                            throw new InvalidDataException($"Timestamp {finalValue} is invalid");
                        break;
                    case "BOOLEAN":
                        // Check the boolean
                        if (!finalValue.Equals("true", StringComparison.OrdinalIgnoreCase) &&
                            !finalValue.Equals("false", StringComparison.OrdinalIgnoreCase))
                            throw new InvalidDataException($"Boolean {finalValue} is invalid");
                        break;
                    case "INTEGER":
                        // Check the integer
                        if (!int.TryParse(finalValue, out _))
                            throw new InvalidDataException($"Integer {finalValue} is invalid");
                        break;
                    case "FLOAT":
                        // Check the float
                        string[] floatParts = finalValuePart.Split(split == ';' ? ',' : ';');
                        foreach (var floatPart in floatParts)
                            if (!double.TryParse(floatPart, out _))
                                throw new InvalidDataException($"Float {floatPart} in {finalValue} is invalid");
                        break;
                    case "DURATION":
                        // Check the duration
                        VcardCommonTools.GetDurationSpan(finalValue);
                        break;
                    case "PERIOD":
                        // Check the period
                        VcardCommonTools.GetTimePeriod(finalValue);
                        break;
                    case "RECUR":
                        // Check the recursion rules
                        try
                        {
                            RecurrenceParser.ParseRuleV1(finalValue);
                        }
                        catch
                        {
                            RecurrenceParser.ParseRuleV2(finalValue);
                        }
                        break;
                }
            }

            // Return the result
            return finalValue;
        }
    }
}
