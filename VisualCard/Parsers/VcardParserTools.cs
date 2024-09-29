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

using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using VisualCard.Parts.Enums;
using VisualCard.Parts.Implementations;
using System.Text;
using VisualCard.Parts;
using System.Globalization;
using System.Text.RegularExpressions;

namespace VisualCard.Parsers
{
    internal class VcardParserTools
    {
        internal static string GetTypesString(string[] args, string @default, bool isSpecifierRequired = true)
        {
            // We're given an array of split arguments of an element delimited by the colon, such as: "...TYPE=home..."
            // Filter list of arguments with the arguments that start with the type argument specifier, or, if specifier is not required,
            // that doesn't have an equals sign
            var ArgType = args.Where((arg) => arg.StartsWith(VcardConstants._typeArgumentSpecifier) || !arg.Contains("=")).ToArray();

            // Trying to specify type without TYPE= is illegal according to RFC2426 in vCard 3.0 and 4.0
            if (ArgType.Count() > 0 && !ArgType[0].StartsWith(VcardConstants._typeArgumentSpecifier) && isSpecifierRequired)
                throw new InvalidDataException("Type must be prepended with TYPE=");

            // Get the type from the split argument
            string Type = "";
            if (isSpecifierRequired)
                // Attempt to get the value from the key strictly
                Type =
                    ArgType.Count() > 0 ?
                    string.Join(VcardConstants._valueDelimiter.ToString(), ArgType.Select((arg) => arg.Substring(VcardConstants._typeArgumentSpecifier.Length))) :
                    @default;
            else
                // Attempt to get the value from the key
                Type =
                    ArgType.Count() > 0 ?
                    string.Join(VcardConstants._valueDelimiter.ToString(), ArgType.Select((arg) => arg.StartsWith(VcardConstants._typeArgumentSpecifier) ? arg.Substring(VcardConstants._typeArgumentSpecifier.Length) : arg)) :
                    @default;

            // Return the type
            return Type;
        }

        internal static string[] GetTypes(string[] args, string @default, bool isSpecifierRequired = true) =>
            GetTypesString(args, @default, isSpecifierRequired).Split([VcardConstants._valueDelimiter], StringSplitOptions.RemoveEmptyEntries);

        internal static string GetValuesString(string[] args, string @default, string argSpecifier)
        {
            // We're given an array of split arguments of an element delimited by the colon, such as: "...TYPE=home..."
            // Filter list of arguments with the arguments that start with the specified specifier (key)
            var argFromSpecifier = args.Where((arg) => arg.StartsWith(argSpecifier));

            // Attempt to get the value from the key
            string argString =
                    argFromSpecifier.Count() > 0 ?
                    string.Join(VcardConstants._valueDelimiter.ToString(), argFromSpecifier.Select((arg) => arg.Substring(argSpecifier.Length))) :
                    @default;
            return argString;
        }

        internal static string[] GetValues(string[] args, string @default, string argSpecifier) =>
            GetValuesString(args, @default, argSpecifier).Split([VcardConstants._valueDelimiter], StringSplitOptions.RemoveEmptyEntries);

        internal static bool StringSupported(StringsEnum stringsEnum, Version cardVersion) =>
            stringsEnum switch
            {
                StringsEnum.Kind => cardVersion.Major >= 4,
                StringsEnum.Mailer => cardVersion.Major != 4,
                StringsEnum.ProductId => cardVersion.Major >= 3,
                StringsEnum.SortString => cardVersion.Major == 3 || cardVersion.Major == 5,
                StringsEnum.AccessClassification => cardVersion.Major != 2 || cardVersion.Major != 4,
                StringsEnum.Uid => cardVersion.Major <= 4,
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
                PartsArrayEnum.Key => true,
                PartsArrayEnum.Revision => true,
                PartsArrayEnum.Birthdate => true,
                PartsArrayEnum.FullName => true,
                PartsArrayEnum.Url => true,
                PartsArrayEnum.Notes => true,
                PartsArrayEnum.Source => true,
                PartsArrayEnum.Impps => cardVersion.Major >= 3,
                PartsArrayEnum.Nicknames => cardVersion.Major >= 3,
                PartsArrayEnum.Labels => cardVersion.Major != 4,
                PartsArrayEnum.Agents => cardVersion.Major != 4,
                PartsArrayEnum.Langs => cardVersion.Major >= 4,
                PartsArrayEnum.Xml => cardVersion.Major == 4,
                PartsArrayEnum.Anniversary => cardVersion.Major >= 4,
                PartsArrayEnum.Gender => cardVersion.Major >= 4,
                PartsArrayEnum.FreeBusyUrl => cardVersion.Major >= 4,
                PartsArrayEnum.CalendarUrl => cardVersion.Major >= 4,
                PartsArrayEnum.CalendarSchedulingRequestUrl => cardVersion.Major >= 4,
                PartsArrayEnum.ContactUri => cardVersion.Major >= 4,

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
                _ =>
                    throw new NotImplementedException($"String enumeration {stringsEnum} is not implemented.")
            };

        internal static string GetPrefixFromPartsArrayEnum(PartsArrayEnum partsArrayEnum) =>
            partsArrayEnum switch
            {
                PartsArrayEnum.Names => VcardConstants._nameSpecifier,
                PartsArrayEnum.Telephones => VcardConstants._telephoneSpecifier,
                PartsArrayEnum.Addresses => VcardConstants._addressSpecifier,
                PartsArrayEnum.Labels => VcardConstants._labelSpecifier,
                PartsArrayEnum.Agents => VcardConstants._agentSpecifier,
                PartsArrayEnum.Mails => VcardConstants._emailSpecifier,
                PartsArrayEnum.Organizations => VcardConstants._orgSpecifier,
                PartsArrayEnum.Titles => VcardConstants._titleSpecifier,
                PartsArrayEnum.Photos => VcardConstants._photoSpecifier,
                PartsArrayEnum.Nicknames => VcardConstants._nicknameSpecifier,
                PartsArrayEnum.Roles => VcardConstants._roleSpecifier,
                PartsArrayEnum.Logos => VcardConstants._logoSpecifier,
                PartsArrayEnum.TimeZone => VcardConstants._timeZoneSpecifier,
                PartsArrayEnum.Geo => VcardConstants._geoSpecifier,
                PartsArrayEnum.Sounds => VcardConstants._soundSpecifier,
                PartsArrayEnum.Impps => VcardConstants._imppSpecifier,
                PartsArrayEnum.Categories => VcardConstants._categoriesSpecifier,
                PartsArrayEnum.Langs => VcardConstants._langSpecifier,
                PartsArrayEnum.Xml => VcardConstants._xmlSpecifier,
                PartsArrayEnum.Key => VcardConstants._keySpecifier,
                PartsArrayEnum.Birthdate => VcardConstants._birthSpecifier,
                PartsArrayEnum.Revision => VcardConstants._revSpecifier,
                PartsArrayEnum.Anniversary => VcardConstants._anniversarySpecifier,
                PartsArrayEnum.Gender => VcardConstants._genderSpecifier,
                PartsArrayEnum.CalendarSchedulingRequestUrl => VcardConstants._caladrUriSpecifier,
                PartsArrayEnum.CalendarUrl => VcardConstants._calUriSpecifier,
                PartsArrayEnum.FreeBusyUrl => VcardConstants._fbUrlSpecifier,
                PartsArrayEnum.FullName => VcardConstants._fullNameSpecifier,
                PartsArrayEnum.Notes => VcardConstants._noteSpecifier,
                PartsArrayEnum.Source => VcardConstants._sourceSpecifier,
                PartsArrayEnum.Url => VcardConstants._urlSpecifier,
                PartsArrayEnum.ContactUri => VcardConstants._contactUriSpecifier,

                // Extensions are allowed
                PartsArrayEnum.NonstandardNames => VcardConstants._xSpecifier,
                _ => ""
            };

        internal static (PartsArrayEnum, PartCardinality) GetPartsArrayEnumFromType(Type? partsArrayType, Version cardVersion)
        {
            if (partsArrayType is null)
                throw new NotImplementedException("Type is not provided.");

            // Now, iterate through every type
            if (partsArrayType == typeof(NameInfo))
                return (PartsArrayEnum.Names, cardVersion.Major == 4 ? PartCardinality.MayBeOne : PartCardinality.ShouldBeOne);
            else if (partsArrayType == typeof(TelephoneInfo))
                return (PartsArrayEnum.Telephones, PartCardinality.Any);
            else if (partsArrayType == typeof(AddressInfo))
                return (PartsArrayEnum.Addresses, PartCardinality.Any);
            else if (partsArrayType == typeof(LabelAddressInfo))
                return (PartsArrayEnum.Labels, PartCardinality.Any);
            else if (partsArrayType == typeof(AgentInfo))
                return (PartsArrayEnum.Agents, PartCardinality.Any);
            else if (partsArrayType == typeof(EmailInfo))
                return (PartsArrayEnum.Mails, PartCardinality.Any);
            else if (partsArrayType == typeof(OrganizationInfo))
                return (PartsArrayEnum.Organizations, PartCardinality.Any);
            else if (partsArrayType == typeof(TitleInfo))
                return (PartsArrayEnum.Titles, PartCardinality.Any);
            else if (partsArrayType == typeof(PhotoInfo))
                return (PartsArrayEnum.Photos, PartCardinality.Any);
            else if (partsArrayType == typeof(NicknameInfo))
                return (PartsArrayEnum.Nicknames, PartCardinality.Any);
            else if (partsArrayType == typeof(RoleInfo))
                return (PartsArrayEnum.Roles, PartCardinality.Any);
            else if (partsArrayType == typeof(LogoInfo))
                return (PartsArrayEnum.Logos, PartCardinality.Any);
            else if (partsArrayType == typeof(TimeDateZoneInfo))
                return (PartsArrayEnum.TimeZone, PartCardinality.Any);
            else if (partsArrayType == typeof(GeoInfo))
                return (PartsArrayEnum.Geo, PartCardinality.Any);
            else if (partsArrayType == typeof(SoundInfo))
                return (PartsArrayEnum.Sounds, PartCardinality.Any);
            else if (partsArrayType == typeof(ImppInfo))
                return (PartsArrayEnum.Impps, PartCardinality.Any);
            else if (partsArrayType == typeof(CategoryInfo))
                return (PartsArrayEnum.Categories, PartCardinality.Any);
            else if (partsArrayType == typeof(LangInfo))
                return (PartsArrayEnum.Langs, PartCardinality.Any);
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
            else if (partsArrayType == typeof(FullNameInfo))
                return (PartsArrayEnum.FullName, cardVersion.Major >= 3 ? PartCardinality.AtLeastOne : PartCardinality.Any);
            else if (partsArrayType == typeof(UrlInfo))
                return (PartsArrayEnum.Url, PartCardinality.Any);
            else if (partsArrayType == typeof(NoteInfo))
                return (PartsArrayEnum.Notes, PartCardinality.Any);
            else if (partsArrayType == typeof(SourceInfo))
                return (PartsArrayEnum.Source, PartCardinality.Any);
            else if (partsArrayType == typeof(FreeBusyInfo))
                return (PartsArrayEnum.FreeBusyUrl, PartCardinality.Any);
            else if (partsArrayType == typeof(CalendarUrlInfo))
                return (PartsArrayEnum.CalendarUrl, PartCardinality.Any);
            else if (partsArrayType == typeof(CalendarSchedulingRequestUrlInfo))
                return (PartsArrayEnum.CalendarSchedulingRequestUrl, PartCardinality.Any);
            else if (partsArrayType == typeof(ContactUriInfo))
                return (PartsArrayEnum.ContactUri, PartCardinality.Any);

            // Extensions are allowed
            else if (partsArrayType == typeof(XNameInfo))
                return (PartsArrayEnum.NonstandardNames, PartCardinality.Any);
            return (PartsArrayEnum.IanaNames, PartCardinality.Any);
        }

        internal static (PartType type, object enumeration, Type? enumType, Func<string, string[], int, string[], string, Version, BaseCardPartInfo>? fromStringFunc, string defaultType, string defaultValue, string[] allowedExtraTypes) GetPartType(string prefix) =>
            prefix switch
            {
                VcardConstants._nameSpecifier => (PartType.PartsArray, PartsArrayEnum.Names, typeof(NameInfo), NameInfo.FromStringVcardStatic, "", "", []),
                VcardConstants._telephoneSpecifier => (PartType.PartsArray, PartsArrayEnum.Telephones, typeof(TelephoneInfo), TelephoneInfo.FromStringVcardStatic, "CELL", "", ["TEXT", "VOICE", "FAX", "CELL", "VIDEO", "PAGER", "TEXTPHONE", "ISDN", "CAR", "MODEM", "BBS", "MSG", "PREF", "TLX", "MMS"]),
                VcardConstants._addressSpecifier => (PartType.PartsArray, PartsArrayEnum.Addresses, typeof(AddressInfo), AddressInfo.FromStringVcardStatic, "HOME", "", ["DOM", "INTL", "PARCEL", "POSTAL"]),
                VcardConstants._labelSpecifier => (PartType.PartsArray, PartsArrayEnum.Labels, typeof(LabelAddressInfo), LabelAddressInfo.FromStringVcardStatic, "HOME", "", ["DOM", "INTL", "PARCEL", "POSTAL"]),
                VcardConstants._agentSpecifier => (PartType.PartsArray, PartsArrayEnum.Agents, typeof(AgentInfo), AgentInfo.FromStringVcardStatic, "", "", []),
                VcardConstants._emailSpecifier => (PartType.PartsArray, PartsArrayEnum.Mails, typeof(EmailInfo), EmailInfo.FromStringVcardStatic, "HOME", "", ["AOL", "APPLELINK", "ATTMAIL", "CIS", "EWORLD", "INTERNET", "IBMMAIL", "MCIMAIL", "POWERSHARE", "PRODIGY", "TLX", "X400", "CELL"]),
                VcardConstants._orgSpecifier => (PartType.PartsArray, PartsArrayEnum.Organizations, typeof(OrganizationInfo), OrganizationInfo.FromStringVcardStatic, "WORK", "", []),
                VcardConstants._titleSpecifier => (PartType.PartsArray, PartsArrayEnum.Titles, typeof(TitleInfo), TitleInfo.FromStringVcardStatic, "", "", []),
                VcardConstants._photoSpecifier => (PartType.PartsArray, PartsArrayEnum.Photos, typeof(PhotoInfo), PhotoInfo.FromStringVcardStatic, "JPEG", "", ["JPG", "GIF", "CGM", "WMF", "BMP", "MET", "PMB", "DIB", "PICT", "TIFF", "PS", "PDF", "JPEG", "MPEG", "MPEG2", "AVI", "QTIME", "PNG", "WEBP"]),
                VcardConstants._nicknameSpecifier => (PartType.PartsArray, PartsArrayEnum.Nicknames, typeof(NicknameInfo), NicknameInfo.FromStringVcardStatic, "HOME", "", []),
                VcardConstants._roleSpecifier => (PartType.PartsArray, PartsArrayEnum.Roles, typeof(RoleInfo), RoleInfo.FromStringVcardStatic, "", "", []),
                VcardConstants._logoSpecifier => (PartType.PartsArray, PartsArrayEnum.Logos, typeof(LogoInfo), LogoInfo.FromStringVcardStatic, "JPEG", "", ["JPG", "GIF", "CGM", "WMF", "BMP", "MET", "PMB", "DIB", "PICT", "TIFF", "PS", "PDF", "JPEG", "MPEG", "MPEG2", "AVI", "QTIME", "PNG", "WEBP"]),
                VcardConstants._timeZoneSpecifier => (PartType.PartsArray, PartsArrayEnum.TimeZone, typeof(TimeDateZoneInfo), TimeDateZoneInfo.FromStringVcardStatic, "", "", []),
                VcardConstants._geoSpecifier => (PartType.PartsArray, PartsArrayEnum.Geo, typeof(GeoInfo), GeoInfo.FromStringVcardStatic, "", "uri", []),
                VcardConstants._soundSpecifier => (PartType.PartsArray, PartsArrayEnum.Sounds, typeof(SoundInfo), SoundInfo.FromStringVcardStatic, "MP3", "", ["MP3", "WAVE", "PCM", "AIFF", "AAC"]),
                VcardConstants._imppSpecifier => (PartType.PartsArray, PartsArrayEnum.Impps, typeof(ImppInfo), ImppInfo.FromStringVcardStatic, "SIP", "", ["SIP"]),
                VcardConstants._categoriesSpecifier => (PartType.PartsArray, PartsArrayEnum.Categories, typeof(CategoryInfo), CategoryInfo.FromStringVcardStatic, "", "", []),
                VcardConstants._langSpecifier => (PartType.PartsArray, PartsArrayEnum.Langs, typeof(LangInfo), LangInfo.FromStringVcardStatic, "HOME", "", []),
                VcardConstants._xmlSpecifier => (PartType.PartsArray, PartsArrayEnum.Xml, typeof(XmlInfo), XmlInfo.FromStringVcardStatic, "", "", []),
                VcardConstants._keySpecifier => (PartType.PartsArray, PartsArrayEnum.Key, typeof(KeyInfo), KeyInfo.FromStringVcardStatic, "", "", []),
                VcardConstants._revSpecifier => (PartType.PartsArray, PartsArrayEnum.Revision, typeof(RevisionInfo), RevisionInfo.FromStringVcardStatic, "", "", []),
                VcardConstants._birthSpecifier => (PartType.PartsArray, PartsArrayEnum.Birthdate, typeof(BirthDateInfo), BirthDateInfo.FromStringVcardStatic, "", "", []),
                VcardConstants._anniversarySpecifier => (PartType.PartsArray, PartsArrayEnum.Anniversary, typeof(AnniversaryInfo), AnniversaryInfo.FromStringVcardStatic, "", "", []),
                VcardConstants._genderSpecifier => (PartType.PartsArray, PartsArrayEnum.Gender, typeof(GenderInfo), GenderInfo.FromStringVcardStatic, "", "", []),
                VcardConstants._fullNameSpecifier => (PartType.PartsArray, PartsArrayEnum.FullName, typeof(FullNameInfo), FullNameInfo.FromStringVcardStatic, "", "", []),
                VcardConstants._urlSpecifier => (PartType.PartsArray, PartsArrayEnum.Url, typeof(UrlInfo), UrlInfo.FromStringVcardStatic, "", "", []),
                VcardConstants._noteSpecifier => (PartType.PartsArray, PartsArrayEnum.Notes, typeof(NoteInfo), NoteInfo.FromStringVcardStatic, "", "", []),
                VcardConstants._sourceSpecifier => (PartType.PartsArray, PartsArrayEnum.Source, typeof(SourceInfo), SourceInfo.FromStringVcardStatic, "", "", []),
                VcardConstants._fbUrlSpecifier => (PartType.PartsArray, PartsArrayEnum.FreeBusyUrl, typeof(FreeBusyInfo), FreeBusyInfo.FromStringVcardStatic, "", "", []),
                VcardConstants._calUriSpecifier => (PartType.PartsArray, PartsArrayEnum.CalendarUrl, typeof(CalendarUrlInfo), CalendarUrlInfo.FromStringVcardStatic, "", "", []),
                VcardConstants._caladrUriSpecifier => (PartType.PartsArray, PartsArrayEnum.CalendarSchedulingRequestUrl, typeof(CalendarSchedulingRequestUrlInfo), CalendarSchedulingRequestUrlInfo.FromStringVcardStatic, "", "", []),
                VcardConstants._contactUriSpecifier => (PartType.PartsArray, PartsArrayEnum.ContactUri, typeof(ContactUriInfo), ContactUriInfo.FromStringVcardStatic, "", "", []),
                VcardConstants._kindSpecifier => (PartType.Strings, StringsEnum.Kind, null, null, "", "", []),
                VcardConstants._mailerSpecifier => (PartType.Strings, StringsEnum.Mailer, null, null, "", "", []),
                VcardConstants._productIdSpecifier => (PartType.Strings, StringsEnum.ProductId, null, null, "", "", []),
                VcardConstants._sortStringSpecifier => (PartType.Strings, StringsEnum.SortString, null, null, "", "", []),
                VcardConstants._classSpecifier => (PartType.Strings, StringsEnum.AccessClassification, null, null, "", "", []),
                VcardConstants._uidSpecifier => (PartType.Strings, StringsEnum.Uid, null, null, "", "", []),

                // Extensions are allowed
                VcardConstants._xSpecifier => (PartType.PartsArray, PartsArrayEnum.NonstandardNames, typeof(XNameInfo), XNameInfo.FromStringVcardStatic, "", "", []),
                _ => (PartType.PartsArray, PartsArrayEnum.IanaNames, typeof(ExtraInfo), ExtraInfo.FromStringVcardStatic, "", "", []),
            };

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
                if (target[currCharNum] != '\n' && target[currCharNum] != '\r')
                {
                    block.Append(target[currCharNum]);
                    processed++;
                }
                if (processed >= selectedMax || target[currCharNum] == '\n')
                {
                    // Append a new line because we reached the maximum limit
                    selectedMax = maxChars;
                    processed = 0;
                    block.Append("\n ");
                }
            }
            return block.ToString();
        }

        internal static IEnumerable<int> GetDigits(int num)
        {
            int individualFactor = 0;
            int tennerFactor = Convert.ToInt32(Math.Pow(10, num.ToString().Length));
            while (tennerFactor > 1)
            {
                num -= tennerFactor * individualFactor;
                tennerFactor /= 10;
                individualFactor = num / tennerFactor;
                yield return individualFactor;
            }
        }

        internal static bool IsEncodingBlob(string[]? args, string? keyEncoded)
        {
            args ??= [];
            string encoding = GetValuesString(args, "b", VcardConstants._encodingArgumentSpecifier);
            bool isValidUri = Uri.TryCreate(keyEncoded, UriKind.Absolute, out Uri uri);
            if (isValidUri)
            {
                if (uri.Scheme == "data")
                    return true;
                return false;
            }
            return
                encoding.Equals("b", StringComparison.OrdinalIgnoreCase) ||
                encoding.Equals("BASE64", StringComparison.OrdinalIgnoreCase) ||
                encoding.Equals("BLOB", StringComparison.OrdinalIgnoreCase);
        }

        internal static Stream GetBlobData(string[]? args, string? keyEncoded)
        {
            args ??= [];
            if (IsEncodingBlob(args, keyEncoded))
            {
                bool isValidUri = Uri.TryCreate(keyEncoded, UriKind.Absolute, out Uri uri);
                string dataStr;
                if (isValidUri)
                {
                    if (uri.Scheme == "data")
                        dataStr = uri.AbsolutePath.Substring(uri.AbsolutePath.IndexOf(",") + 1);
                    else
                        throw new InvalidDataException("Contains a valid URL; you should fetch that URL manually and convert the response to the stream.");
                }
                else
                    dataStr = keyEncoded ??
                        throw new InvalidDataException("There is no encoded data.");
                byte[] dataBytes = Convert.FromBase64String(dataStr);
                Stream blobStream = new MemoryStream(dataBytes);
                return blobStream;
            }
            else
                throw new InvalidOperationException("Not a blob. You should somehow handle it.");
        }

        internal static DateTimeOffset ParsePosixDate(string posixDateRepresentation)
        {
            // Check to see if this date and time representation is supported by .NET
            if (DateTimeOffset.TryParse(posixDateRepresentation, out DateTimeOffset date))
                return date;

            // Now, this date might be a POSIX date that follows the vCard specification, but check it
            if (posixDateRepresentation.Length == 8)
            {
                // It might be yyyyMMdd, but check again
                string yearStr = posixDateRepresentation.Substring(0, 4);
                string monthStr = posixDateRepresentation.Substring(4, 2);
                string dayStr = posixDateRepresentation.Substring(6, 2);
                if (DateTimeOffset.TryParseExact($"{yearStr}/{monthStr}/{dayStr}", "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    return date;
            }
            else if (posixDateRepresentation.Length == 15 || posixDateRepresentation.Length == 16)
            {
                // It might be yyyyMMdd + "T" + HHmmss + ["Z"], but check again
                string yearStr = posixDateRepresentation.Substring(0, 4);
                string monthStr = posixDateRepresentation.Substring(4, 2);
                string dayStr = posixDateRepresentation.Substring(6, 2);
                char timeIndicator = posixDateRepresentation[8];
                string hourStr = posixDateRepresentation.Substring(9, 2);
                string minuteStr = posixDateRepresentation.Substring(11, 2);
                string secondStr = posixDateRepresentation.Substring(13, 2);
                if (timeIndicator != 'T')
                    throw new ArgumentException($"Time indicator is invalid.");
                if (posixDateRepresentation.Length == 16 && posixDateRepresentation[15] != 'Z')
                    throw new ArgumentException($"UTC indicator is invalid.");
                bool assumeUtc = posixDateRepresentation.Length == 16 && posixDateRepresentation[15] == 'Z';
                var utcOffset = assumeUtc ? DateTimeOffset.UtcNow.Offset : DateTimeOffset.Now.Offset;
                string renderedOffset = SaveUtcOffset(utcOffset);
                if (DateTimeOffset.TryParseExact($"{yearStr}/{monthStr}/{dayStr} {hourStr}:{minuteStr}:{secondStr} {renderedOffset}", "yyyy/MM/dd HH:mm:ss zzz", CultureInfo.InvariantCulture, assumeUtc ? DateTimeStyles.AssumeUniversal : DateTimeStyles.AssumeLocal, out date))
                    return date;
            }
            throw new ArgumentException($"Can't parse date {posixDateRepresentation}");
        }

        internal static string SavePosixDate(DateTimeOffset posixDateRepresentation, bool dateOnly = false)
        {
            StringBuilder posixDateBuilder = new(
                $"{posixDateRepresentation.Year:0000}" +
                $"{posixDateRepresentation.Month:00}" +
                $"{posixDateRepresentation.Day:00}"
            );
            if (!dateOnly)
                posixDateBuilder.Append(
                    $"T" +
                    $"{posixDateRepresentation.Hour:00}" +
                    $"{posixDateRepresentation.Minute:00}" +
                    $"{posixDateRepresentation.Second:00}" +
                    $"{(posixDateRepresentation.Offset == new TimeSpan() ? "Z" : "")}"
                );
            return posixDateBuilder.ToString();
        }

        internal static TimeSpan ParseUtcOffset(string utcOffsetRepresentation)
        {
            // Check for sanity
            if (utcOffsetRepresentation.Length != 3 && utcOffsetRepresentation.Length != 5 && utcOffsetRepresentation.Length != 7)
                throw new ArgumentException($"UTC offset representation [{utcOffsetRepresentation}] is invalid.");
            bool hasMinutes = utcOffsetRepresentation.Length >= 5;
            bool hasSeconds = utcOffsetRepresentation.Length == 7;

            // Now, this representation might be a POSIX offset that follows the vCard specification, but check it,
            // because it might be either <+/->HHmmss, <+/->HHmm, or <+/->HH.
            string designatorStr = utcOffsetRepresentation.Substring(0, 1);
            string hourStr = utcOffsetRepresentation.Substring(1, 2);
            string minuteStr = hasMinutes ? utcOffsetRepresentation.Substring(3, 2) : "";
            string secondStr = hasSeconds ? utcOffsetRepresentation.Substring(5, 2) : "";
            if (designatorStr != "+" && designatorStr != "-")
                throw new ArgumentException($"Designator {designatorStr} is invalid.");
            if (hourStr == "00" && (!hasMinutes || (hasMinutes && minuteStr == "00")) && (!hasSeconds || (hasSeconds && secondStr == "00")))
            {
                if (designatorStr == "-")
                    throw new ArgumentException($"Can't specify negative zero offset.");
                return new();
            }
            if (TimeSpan.TryParseExact($"{hourStr}:{(hasMinutes ? minuteStr : "00")}:{(hasSeconds ? secondStr : "00")}", "hh\\:mm\\:ss", CultureInfo.InvariantCulture, out TimeSpan offset))
                return designatorStr == "-" ? -offset : offset;
            throw new ArgumentException($"Can't parse offset {utcOffsetRepresentation}");
        }

        internal static string SaveUtcOffset(TimeSpan utcOffsetRepresentation)
        {
            StringBuilder utcOffsetBuilder = new(
                $"{(utcOffsetRepresentation < new TimeSpan() ? "-" : "+")}" +
                $"{Math.Abs(utcOffsetRepresentation.Hours):00}" +
                $"{Math.Abs(utcOffsetRepresentation.Minutes):00}"
            );
            if (utcOffsetRepresentation.Seconds != 0)
                utcOffsetBuilder.Append(
                    $"{Math.Abs(utcOffsetRepresentation.Seconds):00}"
                );
            return utcOffsetBuilder.ToString();
        }

        internal static string ProcessStringValue(string value, string values, StringsEnum stringType)
        {
            // Now, handle each type individually
            string finalValue;
            switch (stringType)
            {
                case StringsEnum.Mailer:
                case StringsEnum.ProductId:
                case StringsEnum.SortString:
                case StringsEnum.AccessClassification:
                    // Unescape the value
                    finalValue = Regex.Unescape(value);
                    break;
                case StringsEnum.Uid:
                    // Unescape the value
                    finalValue = Regex.Unescape(value);
                    if (!Uri.TryCreate(finalValue, UriKind.Absolute, out Uri uri) && values.Equals("uri", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException($"URL {finalValue} is invalid");
                    finalValue = uri is not null ? uri.ToString() : finalValue;
                    break;
                case StringsEnum.Kind:
                    // Get the kind
                    if (!string.IsNullOrEmpty(value))
                        finalValue = Regex.Unescape(value);
                    else
                        finalValue = "individual";
                    break;
                default:
                    throw new InvalidDataException($"The string enum type {stringType} is invalid. Are you sure that you've specified the correct type in your vCard representation?");
            }

            // Return the result
            return finalValue;
        }
    }
}
