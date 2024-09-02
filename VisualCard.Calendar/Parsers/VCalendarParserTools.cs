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
using System.Text;
using VisualCard.Calendar.Parts;
using VisualCard.Calendar.Parts.Implementations;
using VisualCard.Calendar.Parts.Enums;
using VisualCard.Calendar.Parts.Implementations.Event;
using VisualCard.Parts.Enums;

namespace VisualCard.Calendar.Parsers
{
    internal class VCalendarParserTools
    {
#pragma warning disable IDE0060
        internal static string GetTypesString(string[] args, string @default, bool isSpecifierRequired = true)
        {
            // We're given an array of split arguments of an element delimited by the colon, such as: "...TYPE=home..."
            // Filter list of arguments with the arguments that start with the type argument specifier, or, if specifier is not required,
            // that doesn't have an equals sign
            var ArgType = args.Where((arg) => arg.StartsWith(VCalendarConstants._typeArgumentSpecifier) || !arg.Contains("=")).ToArray();

            // Trying to specify type without TYPE= is illegal according to RFC2426 in vCard 3.0 and 4.0
            if (ArgType.Count() > 0 && !ArgType[0].StartsWith(VCalendarConstants._typeArgumentSpecifier) && isSpecifierRequired)
                throw new InvalidDataException("Type must be prepended with TYPE=");

            // Get the type from the split argument
            string Type = "";
            if (isSpecifierRequired)
                // Attempt to get the value from the key strictly
                Type =
                    ArgType.Count() > 0 ?
                    string.Join(VCalendarConstants._valueDelimiter.ToString(), ArgType.Select((arg) => arg.Substring(VCalendarConstants._typeArgumentSpecifier.Length))) :
                    @default;
            else
                // Attempt to get the value from the key
                Type =
                    ArgType.Count() > 0 ?
                    string.Join(VCalendarConstants._valueDelimiter.ToString(), ArgType.Select((arg) => arg.StartsWith(VCalendarConstants._typeArgumentSpecifier) ? arg.Substring(VCalendarConstants._typeArgumentSpecifier.Length) : arg)) :
                    @default;

            // Return the type
            return Type;
        }

        internal static string[] GetTypes(string[] args, string @default, bool isSpecifierRequired = true) =>
            GetTypesString(args, @default, isSpecifierRequired).Split([VCalendarConstants._valueDelimiter], StringSplitOptions.RemoveEmptyEntries);

        internal static string GetValuesString(string[] args, string @default, string argSpecifier)
        {
            // We're given an array of split arguments of an element delimited by the colon, such as: "...TYPE=home..."
            // Filter list of arguments with the arguments that start with the specified specifier (key)
            var argFromSpecifier = args.Where((arg) => arg.StartsWith(argSpecifier));

            // Attempt to get the value from the key
            string argString =
                    argFromSpecifier.Count() > 0 ?
                    string.Join(VCalendarConstants._valueDelimiter.ToString(), argFromSpecifier.Select((arg) => arg.Substring(argSpecifier.Length))) :
                    @default;
            return argString;
        }

        internal static string[] GetValues(string[] args, string @default, string argSpecifier) =>
            GetValuesString(args, @default, argSpecifier).Split([VCalendarConstants._valueDelimiter], StringSplitOptions.RemoveEmptyEntries);

        internal static bool StringSupported(CalendarStringsEnum stringsEnum, Version calendarVersion) =>
            stringsEnum switch
            {
                CalendarStringsEnum.ProductId => true,
                CalendarStringsEnum.Uid => true,
                CalendarStringsEnum.Organizer => true,
                CalendarStringsEnum.Status => true,
                CalendarStringsEnum.Summary => true,
                CalendarStringsEnum.Description => true,
                _ =>
                    throw new InvalidOperationException("Invalid string enumeration type to get supported value"),
            };

        internal static bool EnumArrayTypeSupported(CalendarPartsArrayEnum partsArrayEnum, Version calendarVersion) =>
            partsArrayEnum switch
            {
                CalendarPartsArrayEnum.DateStart => true,
                CalendarPartsArrayEnum.DateEnd => true,
                CalendarPartsArrayEnum.DateStamp => calendarVersion.Major == 2,
                CalendarPartsArrayEnum.Categories => true,
                CalendarPartsArrayEnum.NonstandardNames => true,
                _ =>
                    throw new InvalidOperationException("Invalid parts array enumeration type to get supported value"),
            };

        internal static string GetPrefixFromStringsEnum(CalendarStringsEnum stringsEnum) =>
            stringsEnum switch
            {
                CalendarStringsEnum.ProductId => VCalendarConstants._productIdSpecifier,
                CalendarStringsEnum.Uid => VCalendarConstants._uidSpecifier,
                CalendarStringsEnum.Organizer => VCalendarConstants._organizerSpecifier,
                CalendarStringsEnum.Status => VCalendarConstants._statusSpecifier,
                CalendarStringsEnum.Summary => VCalendarConstants._summarySpecifier,
                CalendarStringsEnum.Description => VCalendarConstants._descriptionSpecifier,
                _ =>
                    throw new NotImplementedException($"String enumeration {stringsEnum} is not implemented.")
            };

        internal static string GetPrefixFromPartsArrayEnum(CalendarPartsArrayEnum partsArrayEnum) =>
            partsArrayEnum switch
            {
                CalendarPartsArrayEnum.DateStart => VCalendarConstants._dateStartSpecifier,
                CalendarPartsArrayEnum.DateEnd => VCalendarConstants._dateEndSpecifier,
                CalendarPartsArrayEnum.DateStamp => VCalendarConstants._dateStampSpecifier,
                CalendarPartsArrayEnum.Categories => VCalendarConstants._categoriesSpecifier,
                _ =>
                    throw new NotImplementedException($"String enumeration {partsArrayEnum} is not implemented.")
            };

        internal static (CalendarPartsArrayEnum, PartCardinality) GetPartsArrayEnumFromType(Type partsArrayType, Version calendarVersion)
        {
            if (partsArrayType == null)
                throw new NotImplementedException("Type is not provided.");

            // Now, iterate through every type
            if (partsArrayType == typeof(DateStartInfo))
                return (CalendarPartsArrayEnum.DateStart, PartCardinality.ShouldBeOne);
            else if (partsArrayType == typeof(DateEndInfo))
                return (CalendarPartsArrayEnum.DateEnd, PartCardinality.MayBeOne);
            else if (partsArrayType == typeof(DateStampInfo))
                return (CalendarPartsArrayEnum.DateStamp, PartCardinality.MayBeOne);
            else if (partsArrayType == typeof(CategoriesInfo))
                return (CalendarPartsArrayEnum.Categories, PartCardinality.MayBeOne);
            throw new NotImplementedException($"Type {partsArrayType.Name} doesn't represent any part array.");
        }

        internal static (PartType type, object enumeration, Type enumType, Func<string, string[], string[], string, Version, BaseCalendarPartInfo> fromStringFunc, string defaultType, string defaultValue, string[] allowedExtraTypes) GetPartType(string prefix) =>
            prefix switch
            {
                VCalendarConstants._dateStartSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.DateStart, typeof(DateStartInfo), DateStartInfo.FromStringVcalendarStatic, "", "", []),
                VCalendarConstants._dateEndSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.DateEnd, typeof(DateEndInfo), DateEndInfo.FromStringVcalendarStatic, "", "", []),
                VCalendarConstants._dateStampSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.DateStamp, typeof(DateStampInfo), DateStampInfo.FromStringVcalendarStatic, "", "", []),
                VCalendarConstants._categoriesSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.Categories, typeof(CategoriesInfo), CategoriesInfo.FromStringVcalendarStatic, "", "", []),
                VCalendarConstants._xSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.NonstandardNames, typeof(XNameInfo), XNameInfo.FromStringVcalendarStatic, "", "", []),
                VCalendarConstants._productIdSpecifier => (PartType.Strings, CalendarStringsEnum.ProductId, null, null, "", "", []),
                VCalendarConstants._uidSpecifier => (PartType.Strings, CalendarStringsEnum.Uid, null, null, "", "", []),
                VCalendarConstants._organizerSpecifier => (PartType.Strings, CalendarStringsEnum.Organizer, null, null, "", "", []),
                VCalendarConstants._statusSpecifier => (PartType.Strings, CalendarStringsEnum.Status, null, null, "", "", []),
                VCalendarConstants._summarySpecifier => (PartType.Strings, CalendarStringsEnum.Summary, null, null, "", "", []),
                VCalendarConstants._descriptionSpecifier => (PartType.Strings, CalendarStringsEnum.Description, null, null, "", "", []),
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

        internal static bool IsEncodingBlob(string[] args, string keyEncoded)
        {
            string encoding = GetValuesString(args, "b", VCalendarConstants._encodingArgumentSpecifier);
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

        internal static Stream GetBlobData(string[] args, string keyEncoded)
        {
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
                    dataStr = keyEncoded;
                byte[] dataBytes = Convert.FromBase64String(dataStr);
                Stream blobStream = new MemoryStream(dataBytes);
                return blobStream;
            }
            else
                throw new InvalidOperationException("Not a blob. You should somehow handle it.");
        }
#pragma warning restore IDE0060
    }
}
