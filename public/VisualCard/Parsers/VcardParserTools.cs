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
using VisualCard.Parts.Enums;
using VisualCard.Parts.Implementations;

namespace VisualCard.Parsers
{
    internal class VcardParserTools
    {
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
                StringsEnum.Expertise => VcardConstants._expertiseSpecifier,
                StringsEnum.Hobby => VcardConstants._hobbySpecifier,
                StringsEnum.Interest => VcardConstants._interestSpecifier,
                StringsEnum.OrgDirectory => VcardConstants._orgDirectorySpecifier,
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

        internal static PartsArrayEnum GetPartsArrayEnumFromType(Type? partsArrayType, Version cardVersion, string cardKindStr)
        {
            if (partsArrayType is null)
                throw new NotImplementedException("Type is not provided.");

            // Enumerate through all parts array enums
            var enums = Enum.GetValues(typeof(PartsArrayEnum));
            foreach (PartsArrayEnum part in enums)
            {
                string prefix = GetPrefixFromPartsArrayEnum(part);
                var type = GetPartType(prefix, cardVersion, cardKindStr);
                if (type.enumType == partsArrayType)
                    return part;
            }
            return PartsArrayEnum.IanaNames;
        }

        internal static VcardPartType GetPartType(string prefix, Version cardVersion, string kindStr)
        {
            var kind = VcardCommonTools.GetKindEnum(kindStr);
            return prefix switch
            {
                VcardConstants._nameSpecifier => new(PartType.PartsArray, PartsArrayEnum.Names, cardVersion.Major == 4 ? PartCardinality.MayBeOne : PartCardinality.ShouldBeOne, null, typeof(NameInfo), NameInfo.FromStringVcardStatic, "", "", "text", [], []),
                VcardConstants._addressSpecifier => new(PartType.PartsArray, PartsArrayEnum.Addresses, PartCardinality.Any, null, typeof(AddressInfo), AddressInfo.FromStringVcardStatic, "HOME", "", "text", ["DOM", "INTL", "PARCEL", "POSTAL"], []),
                VcardConstants._agentSpecifier => new(PartType.PartsArray, PartsArrayEnum.Agents, PartCardinality.Any, (ver) => ver.Major != 4, typeof(AgentInfo), AgentInfo.FromStringVcardStatic, "", "", "inline", [], []),
                VcardConstants._orgSpecifier => new(PartType.PartsArray, PartsArrayEnum.Organizations, PartCardinality.Any, null, typeof(OrganizationInfo), OrganizationInfo.FromStringVcardStatic, "WORK", "", "text", [], []),
                VcardConstants._photoSpecifier => new(PartType.PartsArray, PartsArrayEnum.Photos, PartCardinality.Any, null, typeof(PhotoInfo), PhotoInfo.FromStringVcardStatic, "JPEG", "", "inline", ["JPG", "GIF", "CGM", "WMF", "BMP", "MET", "PMB", "DIB", "PICT", "TIFF", "PS", "PDF", "JPEG", "MPEG", "MPEG2", "AVI", "QTIME", "PNG", "WEBP"], []),
                VcardConstants._logoSpecifier => new(PartType.PartsArray, PartsArrayEnum.Logos, PartCardinality.Any, null, typeof(LogoInfo), LogoInfo.FromStringVcardStatic, "JPEG", "", "inline", ["JPG", "GIF", "CGM", "WMF", "BMP", "MET", "PMB", "DIB", "PICT", "TIFF", "PS", "PDF", "JPEG", "MPEG", "MPEG2", "AVI", "QTIME", "PNG", "WEBP"], []),
                VcardConstants._soundSpecifier => new(PartType.PartsArray, PartsArrayEnum.Sounds, PartCardinality.Any, null, typeof(SoundInfo), SoundInfo.FromStringVcardStatic, "MP3", "", "inline", ["MP3", "WAVE", "PCM", "AIFF", "AAC"], []),
                VcardConstants._categoriesSpecifier => new(PartType.PartsArray, PartsArrayEnum.Categories, PartCardinality.Any, null, typeof(CategoryInfo), CategoryInfo.FromStringVcardStatic, "", "", "text", [], []),
                VcardConstants._xmlSpecifier => new(PartType.PartsArray, PartsArrayEnum.Xml, PartCardinality.Any, (ver) => ver.Major == 4, typeof(XmlInfo), XmlInfo.FromStringVcardStatic, "", "", "text", [], []),
                VcardConstants._keySpecifier => new(PartType.PartsArray, PartsArrayEnum.Key, PartCardinality.Any, null, typeof(KeyInfo), KeyInfo.FromStringVcardStatic, "", "", "text", [], []),
                VcardConstants._revSpecifier => new(PartType.PartsArray, PartsArrayEnum.Revision, PartCardinality.MayBeOneNoAltId, null, typeof(RevisionInfo), RevisionInfo.FromStringVcardStatic, "", "", "timestamp", [], []),
                VcardConstants._birthSpecifier => new(PartType.PartsArray, PartsArrayEnum.Birthdate, PartCardinality.MayBeOne, null, typeof(BirthDateInfo), BirthDateInfo.FromStringVcardStatic, "", "", "date-and-or-time", [], []),
                VcardConstants._anniversarySpecifier => new(PartType.PartsArray, PartsArrayEnum.Anniversary, PartCardinality.MayBeOne, (ver) => ver.Major >= 4, typeof(AnniversaryInfo), AnniversaryInfo.FromStringVcardStatic, "", "", "date-and-or-time", [], []),
                VcardConstants._genderSpecifier => new(PartType.PartsArray, PartsArrayEnum.Gender, PartCardinality.MayBeOneNoAltId, (ver) => ver.Major >= 4, typeof(GenderInfo), GenderInfo.FromStringVcardStatic, "", "", "text", [], []),
                VcardConstants._clientPidMapSpecifier => new(PartType.PartsArray, PartsArrayEnum.ClientPidMap, PartCardinality.Any, (ver) => ver.Major == 4, typeof(ClientPidMapInfo), ClientPidMapInfo.FromStringVcardStatic, "", "", "text", [], []),
                VcardConstants._kindSpecifier => new(PartType.Strings, StringsEnum.Kind, PartCardinality.MayBeOneNoAltId, (ver) => ver.Major >= 4, null, null, "", "", "text", [], []),
                VcardConstants._mailerSpecifier => new(PartType.Strings, StringsEnum.Mailer, PartCardinality.MayBeOneNoAltId, (ver) => ver.Major != 4, null, null, "", "", "text", [], []),
                VcardConstants._productIdSpecifier => new(PartType.Strings, StringsEnum.ProductId, PartCardinality.MayBeOneNoAltId, (ver) => ver.Major >= 3, null, null, "", "", "text", [], []),
                VcardConstants._sortStringSpecifier => new(PartType.Strings, StringsEnum.SortString, PartCardinality.MayBeOneNoAltId, (ver) => ver.Major == 3 || ver.Major == 5, null, null, "", "", "text", [], []),
                VcardConstants._classSpecifier => new(PartType.Strings, StringsEnum.AccessClassification, PartCardinality.MayBeOneNoAltId, (ver) => ver.Major == 3 || ver.Major == 5, null, null, "", "", "text", [], []),
                VcardConstants._uidSpecifier => new(PartType.Strings, StringsEnum.Uid, PartCardinality.MayBeOneNoAltId, (ver) => ver.Major <= 4, null, null, "", "", "text", [], []),
                VcardConstants._srcNameSpecifier => new(PartType.Strings, StringsEnum.SourceName, PartCardinality.Any, (ver) => ver.Major == 3, null, null, "", "", "text", [], []),
                VcardConstants._profileSpecifier => new(PartType.Strings, StringsEnum.Profile, PartCardinality.MayBeOneNoAltId, (ver) => ver.Major == 3, null, null, "", "", "text", [], []),
                VcardConstants._telephoneSpecifier => new(PartType.Strings, StringsEnum.Telephones, PartCardinality.Any, null, null, null, "CELL", "", "text", ["TEXT", "VOICE", "FAX", "CELL", "VIDEO", "PAGER", "TEXTPHONE", "ISDN", "CAR", "MODEM", "BBS", "MSG", "PREF", "TLX", "MMS"], []),
                VcardConstants._labelSpecifier => new(PartType.Strings, StringsEnum.Labels, PartCardinality.Any, (ver) => ver.Major != 4, null, null, "HOME", "", "text", ["DOM", "INTL", "PARCEL", "POSTAL"], []),
                VcardConstants._emailSpecifier => new(PartType.Strings, StringsEnum.Mails, PartCardinality.Any, null, null, null, "HOME", "", "text", ["AOL", "APPLELINK", "ATTMAIL", "CIS", "EWORLD", "INTERNET", "IBMMAIL", "MCIMAIL", "POWERSHARE", "PRODIGY", "TLX", "X400", "CELL"], []),
                VcardConstants._titleSpecifier => new(PartType.Strings, StringsEnum.Titles, PartCardinality.Any, null, null, null, "", "", "text", [], []),
                VcardConstants._nicknameSpecifier => new(PartType.Strings, StringsEnum.Nicknames, PartCardinality.Any, (ver) => ver.Major >= 3, null, null, "HOME", "", "text", [], []),
                VcardConstants._roleSpecifier => new(PartType.Strings, StringsEnum.Roles, PartCardinality.Any, null, null, null, "", "", "text", [], []),
                VcardConstants._timeZoneSpecifier => new(PartType.Strings, StringsEnum.TimeZone, PartCardinality.Any, null, null, null, "", "", "utc-offset", [], []),
                VcardConstants._geoSpecifier => new(PartType.Strings, StringsEnum.Geo, PartCardinality.Any, null, null, null, "", "", "text", [], []),
                VcardConstants._imppSpecifier => new(PartType.Strings, StringsEnum.Impps, PartCardinality.Any, (ver) => ver.Major >= 3, null, null, "SIP", "", "uri", ["SIP"], []),
                VcardConstants._langSpecifier => new(PartType.Strings, StringsEnum.Langs, PartCardinality.Any, (ver) => ver.Major >= 4, null, null, "HOME", "", "language-tag", [], []),
                VcardConstants._fullNameSpecifier => new(PartType.Strings, StringsEnum.FullName, cardVersion.Major >= 3 ? PartCardinality.AtLeastOne : PartCardinality.Any, null, null, null, "", "", "text", [], []),
                VcardConstants._urlSpecifier => new(PartType.Strings, StringsEnum.Url, PartCardinality.Any, null, null, null, "", "", "uri", [], []),
                VcardConstants._noteSpecifier => new(PartType.Strings, StringsEnum.Notes, PartCardinality.Any, null, null, null, "", "", "text", [], []),
                VcardConstants._sourceSpecifier => new(PartType.Strings, StringsEnum.Source, PartCardinality.Any, null, null, null, "", "", "uri", [], []),
                VcardConstants._fbUrlSpecifier => new(PartType.Strings, StringsEnum.FreeBusyUrl, PartCardinality.Any, (ver) => ver.Major >= 4, null, null, "", "", "uri", [], []),
                VcardConstants._calUriSpecifier => new(PartType.Strings, StringsEnum.CalendarUrl, PartCardinality.Any, (ver) => ver.Major >= 4, null, null, "", "", "uri", [], []),
                VcardConstants._caladrUriSpecifier => new(PartType.Strings, StringsEnum.CalendarSchedulingRequestUrl, PartCardinality.Any, (ver) => ver.Major >= 4, null, null, "", "", "uri", [], []),
                VcardConstants._contactUriSpecifier => new(PartType.Strings, StringsEnum.ContactUri, PartCardinality.Any, (ver) => ver.Major >= 4, null, null, "", "", "uri", [], []),
                VcardConstants._memberSpecifier => new(PartType.Strings, StringsEnum.Member, PartCardinality.Any, (ver) => kind == CardKind.Group && ver.Major == 4, null, null, "", "", "uri", [], []),
                VcardConstants._relatedSpecifier => new(PartType.Strings, StringsEnum.Related, PartCardinality.Any, (ver) => ver.Major == 4, null, null, "", "", "uri", [], []),
                VcardConstants._expertiseSpecifier => new(PartType.Strings, StringsEnum.Expertise, PartCardinality.Any, (ver) => ver.Major >= 4, null, null, "", "", "text", [], []),
                VcardConstants._hobbySpecifier => new(PartType.Strings, StringsEnum.Hobby, PartCardinality.Any, (ver) => ver.Major >= 4, null, null, "", "", "text", [], []),
                VcardConstants._interestSpecifier => new(PartType.Strings, StringsEnum.Interest, PartCardinality.Any, (ver) => ver.Major >= 4, null, null, "", "", "text", [], []),
                VcardConstants._orgDirectorySpecifier => new(PartType.Strings, StringsEnum.OrgDirectory, PartCardinality.Any, (ver) => ver.Major >= 4, null, null, "", "", "uri", [], []),

                // Extensions are allowed
                VcardConstants._xSpecifier => new(PartType.PartsArray, PartsArrayEnum.NonstandardNames, PartCardinality.Any, null, typeof(XNameInfo), XNameInfo.FromStringVcardStatic, "", "", "", [], []),
                _ => new(PartType.PartsArray, PartsArrayEnum.IanaNames, PartCardinality.Any, null, typeof(ExtraInfo), ExtraInfo.FromStringVcardStatic, "", "", "", [], []),
            };
        }
    }
}
