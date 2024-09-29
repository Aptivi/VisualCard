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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using VisualCard.Parts.Enums;

namespace VisualCard.Parsers
{
    internal static class VcardCommonTools
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

        internal static bool TryParsePosixDate(string posixDateRepresentation, out DateTimeOffset date)
        {
            try
            {
                date = ParsePosixDate(posixDateRepresentation);
                return true;
            }
            catch
            {
                return false;
            }
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
    }
}
