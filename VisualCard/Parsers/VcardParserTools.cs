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

namespace VisualCard.Parsers
{
    internal class VcardParserTools
    {
        internal static string GetTypesString(string[] args, string @default, bool isSpecifierRequired = true)
        {
            // We're given a split of this: "IMPP;TYPE=home:sip:test" delimited by the colon. Split by semicolon to get list of args.
            string[] splitArgs = args[0].Split(VcardConstants._fieldDelimiter);

            // Filter list of arguments with the arguments that start with the type argument specifier, or, if specifier is not required,
            // that doesn't have an equals sign
            var ArgType = splitArgs.Where((arg) => arg.StartsWith(VcardConstants._typeArgumentSpecifier) || !arg.Contains("=")).ToArray();

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
                        ArgType[0].StartsWith(VcardConstants._typeArgumentSpecifier) ?
                        string.Join(VcardConstants._valueDelimiter.ToString(), ArgType.Select((arg) => arg.Substring(VcardConstants._typeArgumentSpecifier.Length))) :
                        ArgType[0]
                    :
                        @default;

            // Return the type
            return Type;
        }

        internal static string[] GetTypes(string[] args, string @default, bool isSpecifierRequired = true) =>
            GetTypesString(args, @default, isSpecifierRequired).Split(VcardConstants._valueDelimiter);

        internal static string GetValuesString(string[] args, string @default, string argSpecifier)
        {
            // We're given a split of this: "IMPP;ARGSPECIFIER=etc;TYPE=home:sip:test" delimited by the colon. Split by semicolon to get list of args.
            string[] splitArgs = args[0].Split(VcardConstants._fieldDelimiter);

            // Filter list of arguments with the arguments that start with the specified specifier (key)
            var argFromSpecifier = splitArgs.Where((arg) => arg.StartsWith(argSpecifier));

            // Attempt to get the value from the key
            string argString =
                    argFromSpecifier.Count() > 0 ?
                    string.Join(VcardConstants._valueDelimiter.ToString(), argFromSpecifier.Select((arg) => arg.Substring(argSpecifier.Length))) :
                    @default;
            return argString;
        }

        internal static string[] GetValues(string[] args, string @default, string argSpecifier) =>
            GetValuesString(args, @default, argSpecifier).Split(VcardConstants._valueDelimiter);

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

        internal static string GetPrefixFromStringsEnum(StringsEnum stringsEnum) =>
            stringsEnum switch
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
                _ =>
                    throw new NotImplementedException($"String enumeration {stringsEnum} is not implemented.")
            };

        internal static string GetPrefixFromPartsEnum(PartsEnum partsEnum) =>
            partsEnum switch
            {
                PartsEnum.Birthdate => VcardConstants._birthSpecifier,
                PartsEnum.Revision => VcardConstants._revSpecifier,
                PartsEnum.Anniversary => VcardConstants._anniversarySpecifier,
                PartsEnum.Gender => VcardConstants._genderSpecifier,
                _ =>
                    throw new NotImplementedException($"String enumeration {partsEnum} is not implemented.")
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
                PartsArrayEnum.NonstandardNames => VcardConstants._xSpecifier,
                _ =>
                    throw new NotImplementedException($"String enumeration {partsArrayEnum} is not implemented.")
            };

        internal static (PartType type, object enumeration, Type enumType, Func<string, int, Version, BaseCardPartInfo> fromStringFunc, Func<string, string[], int, Version, BaseCardPartInfo> fromStringWithTypeFunc) GetPartType(string prefix) =>
            prefix switch
            {
                VcardConstants._nameSpecifier => (PartType.PartsArray, PartsArrayEnum.Names, typeof(NameInfo), NameInfo.FromStringVcardStatic, NameInfo.FromStringVcardWithTypeStatic),
                VcardConstants._telephoneSpecifier => (PartType.PartsArray, PartsArrayEnum.Telephones, typeof(TelephoneInfo), TelephoneInfo.FromStringVcardStatic, TelephoneInfo.FromStringVcardWithTypeStatic),
                VcardConstants._addressSpecifier => (PartType.PartsArray, PartsArrayEnum.Addresses, typeof(AddressInfo), AddressInfo.FromStringVcardStatic, AddressInfo.FromStringVcardWithTypeStatic),
                VcardConstants._labelSpecifier => (PartType.PartsArray, PartsArrayEnum.Labels, typeof(LabelAddressInfo), LabelAddressInfo.FromStringVcardStatic, LabelAddressInfo.FromStringVcardWithTypeStatic),
                VcardConstants._agentSpecifier => (PartType.PartsArray, PartsArrayEnum.Agents, typeof(AgentInfo), AgentInfo.FromStringVcardStatic, AgentInfo.FromStringVcardWithTypeStatic),
                VcardConstants._emailSpecifier => (PartType.PartsArray, PartsArrayEnum.Mails, typeof(EmailInfo), EmailInfo.FromStringVcardStatic, EmailInfo.FromStringVcardWithTypeStatic),
                VcardConstants._orgSpecifier => (PartType.PartsArray, PartsArrayEnum.Organizations, typeof(OrganizationInfo), OrganizationInfo.FromStringVcardStatic, OrganizationInfo.FromStringVcardWithTypeStatic),
                VcardConstants._titleSpecifier => (PartType.PartsArray, PartsArrayEnum.Titles, typeof(TitleInfo), TitleInfo.FromStringVcardStatic, TitleInfo.FromStringVcardWithTypeStatic),
                VcardConstants._photoSpecifier => (PartType.PartsArray, PartsArrayEnum.Photos, typeof(PhotoInfo), PhotoInfo.FromStringVcardStatic, PhotoInfo.FromStringVcardWithTypeStatic),
                VcardConstants._nicknameSpecifier => (PartType.PartsArray, PartsArrayEnum.Nicknames, typeof(NicknameInfo), NicknameInfo.FromStringVcardStatic, NicknameInfo.FromStringVcardWithTypeStatic),
                VcardConstants._roleSpecifier => (PartType.PartsArray, PartsArrayEnum.Roles, typeof(RoleInfo), RoleInfo.FromStringVcardStatic, RoleInfo.FromStringVcardWithTypeStatic),
                VcardConstants._logoSpecifier => (PartType.PartsArray, PartsArrayEnum.Logos, typeof(LogoInfo), LogoInfo.FromStringVcardStatic, LogoInfo.FromStringVcardWithTypeStatic),
                VcardConstants._timeZoneSpecifier => (PartType.PartsArray, PartsArrayEnum.TimeZone, typeof(TimeDateZoneInfo), TimeDateZoneInfo.FromStringVcardStatic, TimeDateZoneInfo.FromStringVcardWithTypeStatic),
                VcardConstants._geoSpecifier => (PartType.PartsArray, PartsArrayEnum.Geo, typeof(GeoInfo), GeoInfo.FromStringVcardStatic, GeoInfo.FromStringVcardWithTypeStatic),
                VcardConstants._soundSpecifier => (PartType.PartsArray, PartsArrayEnum.Sounds, typeof(SoundInfo), SoundInfo.FromStringVcardStatic, SoundInfo.FromStringVcardWithTypeStatic),
                VcardConstants._imppSpecifier => (PartType.PartsArray, PartsArrayEnum.Impps, typeof(ImppInfo), ImppInfo.FromStringVcardStatic, ImppInfo.FromStringVcardWithTypeStatic),
                VcardConstants._categoriesSpecifier => (PartType.PartsArray, PartsArrayEnum.Categories, typeof(CategoryInfo), CategoryInfo.FromStringVcardStatic, CategoryInfo.FromStringVcardWithTypeStatic),
                VcardConstants._xSpecifier => (PartType.PartsArray, PartsArrayEnum.NonstandardNames, typeof(XNameInfo), XNameInfo.FromStringVcardStatic, XNameInfo.FromStringVcardWithTypeStatic),
                VcardConstants._revSpecifier => (PartType.Parts, PartsEnum.Revision, typeof(RevisionInfo), RevisionInfo.FromStringVcardStatic, RevisionInfo.FromStringVcardWithTypeStatic),
                VcardConstants._birthSpecifier => (PartType.Parts, PartsEnum.Birthdate, typeof(BirthDateInfo), BirthDateInfo.FromStringVcardStatic, BirthDateInfo.FromStringVcardWithTypeStatic),
                VcardConstants._anniversarySpecifier => (PartType.Parts, PartsEnum.Anniversary, typeof(AnniversaryInfo), AnniversaryInfo.FromStringVcardStatic, AnniversaryInfo.FromStringVcardWithTypeStatic),
                VcardConstants._genderSpecifier => (PartType.Parts, PartsEnum.Gender, typeof(GenderInfo), GenderInfo.FromStringVcardStatic, GenderInfo.FromStringVcardWithTypeStatic),
                VcardConstants._fullNameSpecifier => (PartType.Strings, StringsEnum.FullName, null, null, null),
                VcardConstants._urlSpecifier => (PartType.Strings, StringsEnum.Url, null, null, null),
                VcardConstants._noteSpecifier => (PartType.Strings, StringsEnum.Notes, null, null, null),
                VcardConstants._sourceSpecifier => (PartType.Strings, StringsEnum.Source, null, null, null),
                VcardConstants._kindSpecifier => (PartType.Strings, StringsEnum.Kind, null, null, null),
                VcardConstants._mailerSpecifier => (PartType.Strings, StringsEnum.Mailer, null, null, null),
                VcardConstants._productIdSpecifier => (PartType.Strings, StringsEnum.ProductId, null, null, null),
                VcardConstants._sortStringSpecifier => (PartType.Strings, StringsEnum.SortString, null, null, null),
                VcardConstants._classSpecifier => (PartType.Strings, StringsEnum.AccessClassification, null, null, null),
                VcardConstants._xmlSpecifier => (PartType.Strings, StringsEnum.Xml, null, null, null),
                VcardConstants._fbUrlSpecifier => (PartType.Strings, StringsEnum.FreeBusyUrl, null, null, null),
                VcardConstants._calUriSpecifier => (PartType.Strings, StringsEnum.CalendarUrl, null, null, null),
                VcardConstants._caladrUriSpecifier => (PartType.Strings, StringsEnum.CalendarSchedulingRequestUrl, null, null, null),
                _ =>
                    throw new InvalidOperationException($"Unknown prefix {prefix}"),
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
    }
}
