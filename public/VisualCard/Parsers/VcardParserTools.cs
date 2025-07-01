//
// VisualCard  Copyright (C) 2021-2025  Aptivi
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
using System.Linq;
using Textify.General;
using VisualCard.Common.Diagnostics;
using VisualCard.Common.Parsers;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts.Enums;
using VisualCard.Common.Parts.Implementations;
using VisualCard.Parts.Enums;
using VisualCard.Parts.Implementations;

namespace VisualCard.Parsers
{
    internal class VcardParserTools
    {
        internal static Version[] supportedVersions =
        [
            new(2, 1),
            new(3, 0),
            new(4, 0),
            new(5, 0),
        ];

        internal static bool VerifySupportedVersion(Version version)
        {
            int major = version.Major;
            int minor = version.Minor;
            foreach (var supportedVersion in supportedVersions)
            {
                if (supportedVersion.Major == major && supportedVersion.Minor == minor)
                    return true;
            }
            LoggingTools.Warning("Major version {0} and minor version {1} isn't in supported version list", major, minor);
            return false;
        }

        internal static string GetPrefixFromStringsEnum(CardStringsEnum stringsEnum) =>
            stringsEnum switch
            {
                CardStringsEnum.AccessClassification => VcardConstants._classSpecifier,
                CardStringsEnum.Kind => VcardConstants._kindSpecifier,
                CardStringsEnum.Mailer => VcardConstants._mailerSpecifier,
                CardStringsEnum.ProductId => VcardConstants._productIdSpecifier,
                CardStringsEnum.SortString => VcardConstants._sortStringSpecifier,
                CardStringsEnum.Uid => VcardConstants._uidSpecifier,
                CardStringsEnum.SourceName => VcardConstants._srcNameSpecifier,
                CardStringsEnum.Profile => VcardConstants._profileSpecifier,
                CardStringsEnum.Telephones => VcardConstants._telephoneSpecifier,
                CardStringsEnum.Labels => VcardConstants._labelSpecifier,
                CardStringsEnum.Mails => VcardConstants._emailSpecifier,
                CardStringsEnum.Titles => VcardConstants._titleSpecifier,
                CardStringsEnum.Nicknames => VcardConstants._nicknameSpecifier,
                CardStringsEnum.Roles => VcardConstants._roleSpecifier,
                CardStringsEnum.TimeZone => VcardConstants._timeZoneSpecifier,
                CardStringsEnum.Geo => VcardConstants._geoSpecifier,
                CardStringsEnum.Impps => VcardConstants._imppSpecifier,
                CardStringsEnum.Langs => VcardConstants._langSpecifier,
                CardStringsEnum.CalendarSchedulingRequestUrl => VcardConstants._caladrUriSpecifier,
                CardStringsEnum.CalendarUrl => VcardConstants._calUriSpecifier,
                CardStringsEnum.FreeBusyUrl => VcardConstants._fbUrlSpecifier,
                CardStringsEnum.FullName => VcardConstants._fullNameSpecifier,
                CardStringsEnum.Notes => VcardConstants._noteSpecifier,
                CardStringsEnum.Source => VcardConstants._sourceSpecifier,
                CardStringsEnum.Url => VcardConstants._urlSpecifier,
                CardStringsEnum.ContactUri => VcardConstants._contactUriSpecifier,
                CardStringsEnum.Member => VcardConstants._memberSpecifier,
                CardStringsEnum.Related => VcardConstants._relatedSpecifier,
                CardStringsEnum.Expertise => VcardConstants._expertiseSpecifier,
                CardStringsEnum.Hobby => VcardConstants._hobbySpecifier,
                CardStringsEnum.Interest => VcardConstants._interestSpecifier,
                CardStringsEnum.OrgDirectory => VcardConstants._orgDirectorySpecifier,
                _ =>
                    throw new NotImplementedException("String enumeration {0} is not implemented.".FormatString(stringsEnum))
            };

        internal static string GetPrefixFromPartsArrayEnum(CardPartsArrayEnum partsArrayEnum) =>
            partsArrayEnum switch
            {
                CardPartsArrayEnum.Names => VcardConstants._nameSpecifier,
                CardPartsArrayEnum.Addresses => VcardConstants._addressSpecifier,
                CardPartsArrayEnum.Agents => VcardConstants._agentSpecifier,
                CardPartsArrayEnum.Organizations => VcardConstants._orgSpecifier,
                CardPartsArrayEnum.Photos => VcardConstants._photoSpecifier,
                CardPartsArrayEnum.Logos => VcardConstants._logoSpecifier,
                CardPartsArrayEnum.Sounds => VcardConstants._soundSpecifier,
                CardPartsArrayEnum.Categories => VcardConstants._categoriesSpecifier,
                CardPartsArrayEnum.Xml => VcardConstants._xmlSpecifier,
                CardPartsArrayEnum.Key => VcardConstants._keySpecifier,
                CardPartsArrayEnum.Birthdate => VcardConstants._birthSpecifier,
                CardPartsArrayEnum.Revision => VcardConstants._revSpecifier,
                CardPartsArrayEnum.Anniversary => VcardConstants._anniversarySpecifier,
                CardPartsArrayEnum.Gender => VcardConstants._genderSpecifier,
                CardPartsArrayEnum.ClientPidMap => VcardConstants._clientPidMapSpecifier,

                // Extensions are allowed
                CardPartsArrayEnum.NonstandardNames => CommonConstants._xSpecifier,
                _ => ""
            };

        internal static CardPartsArrayEnum GetPartsArrayEnumFromType(Type? partsArrayType, Version cardVersion, string cardKindStr)
        {
            if (partsArrayType is null)
            {
                LoggingTools.Error("Part type not provided [version {0}, kind {1}]", cardVersion.ToString(), cardKindStr);
                throw new NotImplementedException("Type is not provided.");
            }

            // Enumerate through all parts array enums
            var enums = Enum.GetValues(typeof(CardPartsArrayEnum));
            LoggingTools.Debug("Processing {0} enums...", enums.Length);
            foreach (CardPartsArrayEnum part in enums)
            {
                string prefix = GetPrefixFromPartsArrayEnum(part);
                var type = GetPartType(prefix, cardVersion, cardKindStr);
                if (type.enumType == partsArrayType)
                {
                    LoggingTools.Debug("Returning {0} based on {1}...", part, partsArrayType.Name);
                    return part;
                }
            }
            LoggingTools.Warning("Returning IANA name enum");
            return CardPartsArrayEnum.IanaNames;
        }

        internal static VcardPartType GetPartType(string prefix, Version cardVersion, string kindStr)
        {
            var kind = GetKindEnum(kindStr);
            LoggingTools.Debug("Got kind {0}", kind);
            return prefix switch
            {
                VcardConstants._nameSpecifier => new(PartType.PartsArray, CardPartsArrayEnum.Names, cardVersion.Major == 4 ? PartCardinality.MayBeOne : PartCardinality.ShouldBeOne, null, typeof(NameInfo), NameInfo.FromStringStatic, "", "", "text", [], []),
                VcardConstants._addressSpecifier => new(PartType.PartsArray, CardPartsArrayEnum.Addresses, PartCardinality.Any, null, typeof(AddressInfo), AddressInfo.FromStringStatic, "HOME", "", "text", ["DOM", "INTL", "PARCEL", "POSTAL"], []),
                VcardConstants._agentSpecifier => new(PartType.PartsArray, CardPartsArrayEnum.Agents, PartCardinality.Any, (ver) => ver.Major != 4, typeof(AgentInfo), AgentInfo.FromStringStatic, "", "", "inline", [], []),
                VcardConstants._orgSpecifier => new(PartType.PartsArray, CardPartsArrayEnum.Organizations, PartCardinality.Any, null, typeof(OrganizationInfo), OrganizationInfo.FromStringStatic, "WORK", "", "text", [], []),
                VcardConstants._photoSpecifier => new(PartType.PartsArray, CardPartsArrayEnum.Photos, PartCardinality.Any, null, typeof(PhotoInfo), PhotoInfo.FromStringStatic, "JPEG", "", "inline", ["JPG", "GIF", "CGM", "WMF", "BMP", "MET", "PMB", "DIB", "PICT", "TIFF", "PS", "PDF", "JPEG", "MPEG", "MPEG2", "AVI", "QTIME", "PNG", "WEBP"], []),
                VcardConstants._logoSpecifier => new(PartType.PartsArray, CardPartsArrayEnum.Logos, PartCardinality.Any, null, typeof(LogoInfo), LogoInfo.FromStringStatic, "JPEG", "", "inline", ["JPG", "GIF", "CGM", "WMF", "BMP", "MET", "PMB", "DIB", "PICT", "TIFF", "PS", "PDF", "JPEG", "MPEG", "MPEG2", "AVI", "QTIME", "PNG", "WEBP"], []),
                VcardConstants._soundSpecifier => new(PartType.PartsArray, CardPartsArrayEnum.Sounds, PartCardinality.Any, null, typeof(SoundInfo), SoundInfo.FromStringStatic, "MP3", "", "inline", ["MP3", "WAVE", "PCM", "AIFF", "AAC"], []),
                VcardConstants._categoriesSpecifier => new(PartType.PartsArray, CardPartsArrayEnum.Categories, PartCardinality.Any, null, typeof(CategoryInfo), CategoryInfo.FromStringStatic, "", "", "text", [], []),
                VcardConstants._xmlSpecifier => new(PartType.PartsArray, CardPartsArrayEnum.Xml, PartCardinality.Any, (ver) => ver.Major == 4, typeof(XmlInfo), XmlInfo.FromStringStatic, "", "", "text", [], []),
                VcardConstants._keySpecifier => new(PartType.PartsArray, CardPartsArrayEnum.Key, PartCardinality.Any, null, typeof(KeyInfo), KeyInfo.FromStringStatic, "", "", "text", [], []),
                VcardConstants._revSpecifier => new(PartType.PartsArray, CardPartsArrayEnum.Revision, PartCardinality.MayBeOneNoAltId, null, typeof(RevisionInfo), RevisionInfo.FromStringStatic, "", "", "timestamp", [], []),
                VcardConstants._birthSpecifier => new(PartType.PartsArray, CardPartsArrayEnum.Birthdate, PartCardinality.MayBeOne, null, typeof(BirthDateInfo), BirthDateInfo.FromStringStatic, "", "", "date-and-or-time", [], []),
                VcardConstants._anniversarySpecifier => new(PartType.PartsArray, CardPartsArrayEnum.Anniversary, PartCardinality.MayBeOne, (ver) => ver.Major >= 4, typeof(AnniversaryInfo), AnniversaryInfo.FromStringStatic, "", "", "date-and-or-time", [], []),
                VcardConstants._genderSpecifier => new(PartType.PartsArray, CardPartsArrayEnum.Gender, PartCardinality.MayBeOneNoAltId, (ver) => ver.Major >= 4, typeof(GenderInfo), GenderInfo.FromStringStatic, "", "", "text", [], []),
                VcardConstants._clientPidMapSpecifier => new(PartType.PartsArray, CardPartsArrayEnum.ClientPidMap, PartCardinality.Any, (ver) => ver.Major == 4, typeof(ClientPidMapInfo), ClientPidMapInfo.FromStringStatic, "", "", "text", [], []),
                VcardConstants._kindSpecifier => new(PartType.Strings, CardStringsEnum.Kind, PartCardinality.MayBeOneNoAltId, (ver) => ver.Major >= 4, null, null, "", "", "text", [], []),
                VcardConstants._mailerSpecifier => new(PartType.Strings, CardStringsEnum.Mailer, PartCardinality.MayBeOneNoAltId, (ver) => ver.Major != 4, null, null, "", "", "text", [], []),
                VcardConstants._productIdSpecifier => new(PartType.Strings, CardStringsEnum.ProductId, PartCardinality.MayBeOneNoAltId, (ver) => ver.Major >= 3, null, null, "", "", "text", [], []),
                VcardConstants._sortStringSpecifier => new(PartType.Strings, CardStringsEnum.SortString, PartCardinality.MayBeOneNoAltId, (ver) => ver.Major == 3 || ver.Major == 5, null, null, "", "", "text", [], []),
                VcardConstants._classSpecifier => new(PartType.Strings, CardStringsEnum.AccessClassification, PartCardinality.MayBeOneNoAltId, (ver) => ver.Major == 3 || ver.Major == 5, null, null, "", "", "text", [], []),
                VcardConstants._uidSpecifier => new(PartType.Strings, CardStringsEnum.Uid, PartCardinality.MayBeOneNoAltId, (ver) => ver.Major <= 4, null, null, "", "", "text", [], []),
                VcardConstants._srcNameSpecifier => new(PartType.Strings, CardStringsEnum.SourceName, PartCardinality.Any, (ver) => ver.Major == 3, null, null, "", "", "text", [], []),
                VcardConstants._profileSpecifier => new(PartType.Strings, CardStringsEnum.Profile, PartCardinality.MayBeOneNoAltId, (ver) => ver.Major == 3, null, null, "", "", "text", [], []),
                VcardConstants._telephoneSpecifier => new(PartType.Strings, CardStringsEnum.Telephones, PartCardinality.Any, null, null, null, "CELL", "", "text", ["TEXT", "VOICE", "FAX", "CELL", "VIDEO", "PAGER", "TEXTPHONE", "ISDN", "CAR", "MODEM", "BBS", "MSG", "PREF", "TLX", "MMS"], []),
                VcardConstants._labelSpecifier => new(PartType.Strings, CardStringsEnum.Labels, PartCardinality.Any, (ver) => ver.Major != 4, null, null, "HOME", "", "text", ["DOM", "INTL", "PARCEL", "POSTAL"], []),
                VcardConstants._emailSpecifier => new(PartType.Strings, CardStringsEnum.Mails, PartCardinality.Any, null, null, null, "HOME", "", "text", ["AOL", "APPLELINK", "ATTMAIL", "CIS", "EWORLD", "INTERNET", "IBMMAIL", "MCIMAIL", "POWERSHARE", "PRODIGY", "TLX", "X400", "CELL"], []),
                VcardConstants._titleSpecifier => new(PartType.Strings, CardStringsEnum.Titles, PartCardinality.Any, null, null, null, "", "", "text", [], []),
                VcardConstants._nicknameSpecifier => new(PartType.Strings, CardStringsEnum.Nicknames, PartCardinality.Any, (ver) => ver.Major >= 3, null, null, "HOME", "", "text", [], []),
                VcardConstants._roleSpecifier => new(PartType.Strings, CardStringsEnum.Roles, PartCardinality.Any, null, null, null, "", "", "text", [], []),
                VcardConstants._timeZoneSpecifier => new(PartType.Strings, CardStringsEnum.TimeZone, PartCardinality.Any, null, null, null, "", "", "utc-offset", [], []),
                VcardConstants._geoSpecifier => new(PartType.Strings, CardStringsEnum.Geo, PartCardinality.Any, null, null, null, "", "", "text", [], []),
                VcardConstants._imppSpecifier => new(PartType.Strings, CardStringsEnum.Impps, PartCardinality.Any, (ver) => ver.Major >= 3, null, null, "SIP", "", "uri", ["SIP"], []),
                VcardConstants._langSpecifier => new(PartType.Strings, CardStringsEnum.Langs, PartCardinality.Any, (ver) => ver.Major >= 4, null, null, "HOME", "", "language-tag", [], []),
                VcardConstants._fullNameSpecifier => new(PartType.Strings, CardStringsEnum.FullName, cardVersion.Major >= 3 ? PartCardinality.AtLeastOne : PartCardinality.Any, null, null, null, "", "", "text", [], []),
                VcardConstants._urlSpecifier => new(PartType.Strings, CardStringsEnum.Url, PartCardinality.Any, null, null, null, "", "", "uri", [], []),
                VcardConstants._noteSpecifier => new(PartType.Strings, CardStringsEnum.Notes, PartCardinality.Any, null, null, null, "", "", "text", [], []),
                VcardConstants._sourceSpecifier => new(PartType.Strings, CardStringsEnum.Source, PartCardinality.Any, null, null, null, "", "", "uri", [], []),
                VcardConstants._fbUrlSpecifier => new(PartType.Strings, CardStringsEnum.FreeBusyUrl, PartCardinality.Any, (ver) => ver.Major >= 4, null, null, "", "", "uri", [], []),
                VcardConstants._calUriSpecifier => new(PartType.Strings, CardStringsEnum.CalendarUrl, PartCardinality.Any, (ver) => ver.Major >= 4, null, null, "", "", "uri", [], []),
                VcardConstants._caladrUriSpecifier => new(PartType.Strings, CardStringsEnum.CalendarSchedulingRequestUrl, PartCardinality.Any, (ver) => ver.Major >= 4, null, null, "", "", "uri", [], []),
                VcardConstants._contactUriSpecifier => new(PartType.Strings, CardStringsEnum.ContactUri, PartCardinality.Any, (ver) => ver.Major >= 4, null, null, "", "", "uri", [], []),
                VcardConstants._memberSpecifier => new(PartType.Strings, CardStringsEnum.Member, PartCardinality.Any, (ver) => kind == CardKind.Group && ver.Major == 4, null, null, "", "", "uri", [], []),
                VcardConstants._relatedSpecifier => new(PartType.Strings, CardStringsEnum.Related, PartCardinality.Any, (ver) => ver.Major == 4, null, null, "", "", "uri", [], []),
                VcardConstants._expertiseSpecifier => new(PartType.Strings, CardStringsEnum.Expertise, PartCardinality.Any, (ver) => ver.Major >= 4, null, null, "", "", "text", [], []),
                VcardConstants._hobbySpecifier => new(PartType.Strings, CardStringsEnum.Hobby, PartCardinality.Any, (ver) => ver.Major >= 4, null, null, "", "", "text", [], []),
                VcardConstants._interestSpecifier => new(PartType.Strings, CardStringsEnum.Interest, PartCardinality.Any, (ver) => ver.Major >= 4, null, null, "", "", "text", [], []),
                VcardConstants._orgDirectorySpecifier => new(PartType.Strings, CardStringsEnum.OrgDirectory, PartCardinality.Any, (ver) => ver.Major >= 4, null, null, "", "", "uri", [], []),

                // Extensions are allowed
                CommonConstants._xSpecifier => new(PartType.PartsArray, CardPartsArrayEnum.NonstandardNames, PartCardinality.Any, null, typeof(XNameInfo), null, "", "", "", [], []),
                _ => new(PartType.PartsArray, CardPartsArrayEnum.IanaNames, PartCardinality.Any, null, typeof(ExtraInfo), null, "", "", "", [], []),
            };
        }

        internal static CardKind GetKindEnum(string kind) =>
            kind.ToLower() switch
            {
                "individual" => CardKind.Individual,
                "group" => CardKind.Group,
                "organization" => CardKind.Organization,
                "location" => CardKind.Location,
                _ => CardKind.Others,
            };

        internal static int GetAltIdFromArgs(Version version, PropertyInfo? property, VcardPartType partType)
        {
            var arguments = property?.Arguments ?? [];
            int altId = -1;
            if (arguments.Length > 0)
            {
                // If we have more than one argument, check for ALTID
                if (version.Major >= 4)
                {
                    var cardinality = partType.cardinality;
                    bool supportsAltId =
                        cardinality != PartCardinality.MayBeOneNoAltId && cardinality != PartCardinality.ShouldBeOneNoAltId &&
                        cardinality != PartCardinality.AtLeastOneNoAltId && cardinality != PartCardinality.AnyNoAltId;
                    var altIdArg = arguments.SingleOrDefault((arg) => arg.Key == VcardConstants._altIdArgumentSpecifier);
                    LoggingTools.Debug("Cardinality {0} with altid support: {1}", cardinality, supportsAltId);
                    if (supportsAltId)
                    {
                        // The type supports ALTID.
                        if (arguments[0].Key == VcardConstants._altIdArgumentSpecifier)
                        {
                            // We need ALTID to be numeric
                            if (!int.TryParse(altIdArg.Values[0].value, out altId))
                                throw new InvalidDataException("ALTID must be numeric");
                            LoggingTools.Debug("Parsed altid as {0}", altId);

                            // We need ALTID to be positive
                            if (altId < 0)
                                throw new InvalidDataException("ALTID must be positive");

                            // Here, we require arguments for ALTID
                            LoggingTools.Debug("Checking {0} arguments to find reasons as to why altid is {1}", arguments.Length, altId);
                            if (arguments.Length <= 1)
                                throw new InvalidDataException("ALTID must have one or more arguments to specify why this instance is an alternative");
                        }
                        else if (altIdArg is not null)
                            throw new InvalidDataException("ALTID must be exactly in the first position of the argument, because arguments that follow it are required to be specified");
                    }
                    else if (altIdArg is not null)
                        throw new InvalidDataException("ALTID must not be specified in the {0} type that expects a cardinality of {1}".FormatString(partType.enumeration, cardinality));
                }
            }
            LoggingTools.Info("Returning ALTID {0}", altId);
            return altId;
        }
    }
}
