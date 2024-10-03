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
using System.Text.RegularExpressions;
using Textify.General;
using VisualCard.Parsers.Arguments;
using VisualCard.Parsers.Recurrence;
using VisualCard.Parts.Enums;

namespace VisualCard.Parsers
{
    /// <summary>
    /// Common tools for vCard parsing
    /// </summary>
    public static class VcardCommonTools
    {
        private static readonly string[] supportedDateTimeFormats =
        [
            @"yyyyMMdd",
            @"yyyy-MM-dd",
            @"yyyyMM",
            @"yyyy-MM",
            @"yyyy",
            @"--MMdd",
            @"----dd",
            @"yyyyMMdd\THHmmss\Z",
            @"yyyyMMdd\THHmmss",
            @"yyyy-MM-dd\THH\:mm\:ss\Z",
            @"yyyy-MM-dd\THH\:mm\:ss",
        ];

        private static readonly string[] supportedDateFormats =
        [
            @"yyyyMMdd",
            @"yyyy-MM-dd",
            @"yyyyMM",
            @"yyyy-MM",
            @"yyyy",
            @"--MMdd",
            @"----dd",
        ];

        private static readonly string[] supportedTimeFormats =
        [
            @"hh",
            @"hhmm",
            @"hh\:mm",
            @"hhmmss",
            @"hh\:mm\:ss",
            @"-mmss",
            @"--ss",
            @"\Thh",
            @"\Thhmm",
            @"\Thh\:mm",
            @"\Thhmmss",
            @"\Thh\:mm\:ss",
            @"\T-mmss",
            @"\T--ss",
            @"hh\Z",
            @"hhmm\Z",
            @"hh\:mm\Z",
            @"hhmmss\Z",
            @"hh\:mm\:ss\Z",
            @"-mmss\Z",
            @"--ss\Z",
            @"\Thh\Z",
            @"\Thhmm\Z",
            @"\Thh\:mm\Z",
            @"\Thhmmss\Z",
            @"\Thh\:mm\:ss\Z",
            @"\T-mmss\Z",
            @"\T--ss\Z",
            @"hhzz",
            @"hhmmzz",
            @"hh\:mmzz",
            @"hhmmsszz",
            @"hh\:mm\:sszz",
            @"-mmsszz",
            @"--sszz",
            @"\Thhzz",
            @"\Thhmmzz",
            @"\Thh\:mmzz",
            @"\Thhmmsszz",
            @"\Thh\:mm\:sszz",
            @"\T-mmsszz",
            @"\T--sszz",
            @"hhzzz",
            @"hhmmzzz",
            @"hh\:mmzzz",
            @"hhmmsszzz",
            @"hh\:mm\:sszzz",
            @"-mmsszzz",
            @"--sszzz",
            @"\Thhzzz",
            @"\Thhmmzzz",
            @"\Thh\:mmzzz",
            @"\Thhmmsszzz",
            @"\Thh\:mm\:sszzz",
            @"\T-mmsszzz",
            @"\T--sszzz",
        ];

        private static readonly string[] supportedTimestampFormats =
        [
            @"yyyyMMdd\THHmmsszz",
            @"yyyyMMdd\THHmmsszzz",
            @"yyyyMMdd\THHmmss\Z",
            @"yyyyMMdd\THHmmss",
            @"yyyy-MM-dd\THH\:mm\:sszz",
            @"yyyy-MM-dd\THH\:mm\:sszzz",
            @"yyyy-MM-dd\THH\:mm\:ss\Z",
            @"yyyy-MM-dd\THH\:mm\:ss",
        ];

        /// <summary>
        /// Parses the POSIX date formatted with the representation according to the vCard and vCalendar specifications
        /// </summary>
        /// <param name="posixDateRepresentation">Date representation in basic or extended format of ISO 8601</param>
        /// <returns>An instance of <see cref="DateTimeOffset"/> that matches the representation</returns>
        /// <exception cref="ArgumentException"></exception>
        public static DateTimeOffset ParsePosixDateTime(string posixDateRepresentation) =>
            ParsePosixRepresentation(posixDateRepresentation, supportedDateTimeFormats);

        /// <summary>
        /// Tries to parse the POSIX date formatted with the representation according to the vCard and vCalendar specifications
        /// </summary>
        /// <param name="posixDateRepresentation">Date representation in basic or extended format of ISO 8601</param>
        /// <param name="date">[<see langword="out"/>] Date output parsed from the representation</param>
        /// <returns>True if parsed successfully; false otherwise.</returns>
        public static bool TryParsePosixDateTime(string posixDateRepresentation, out DateTimeOffset date)
        {
            try
            {
                date = ParsePosixDateTime(posixDateRepresentation);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Parses the POSIX date formatted with the representation according to the vCard and vCalendar specifications
        /// </summary>
        /// <param name="posixDateRepresentation">Date representation in basic or extended format of ISO 8601</param>
        /// <returns>An instance of <see cref="DateTimeOffset"/> that matches the representation</returns>
        /// <exception cref="ArgumentException"></exception>
        public static DateTimeOffset ParsePosixDate(string posixDateRepresentation) =>
            ParsePosixRepresentation(posixDateRepresentation, supportedDateFormats);

        /// <summary>
        /// Tries to parse the POSIX date formatted with the representation according to the vCard and vCalendar specifications
        /// </summary>
        /// <param name="posixDateRepresentation">Date representation in basic or extended format of ISO 8601</param>
        /// <param name="date">[<see langword="out"/>] Date output parsed from the representation</param>
        /// <returns>True if parsed successfully; false otherwise.</returns>
        public static bool TryParsePosixDate(string posixDateRepresentation, out DateTimeOffset date)
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
        
        /// <summary>
        /// Parses the POSIX date formatted with the representation according to the vCard and vCalendar specifications
        /// </summary>
        /// <param name="posixDateRepresentation">Date representation in basic or extended format of ISO 8601</param>
        /// <returns>An instance of <see cref="DateTimeOffset"/> that matches the representation</returns>
        /// <exception cref="ArgumentException"></exception>
        public static DateTimeOffset ParsePosixTime(string posixDateRepresentation) =>
            ParsePosixRepresentation(posixDateRepresentation, supportedTimeFormats);

        /// <summary>
        /// Tries to parse the POSIX date formatted with the representation according to the vCard and vCalendar specifications
        /// </summary>
        /// <param name="posixDateRepresentation">Date representation in basic or extended format of ISO 8601</param>
        /// <param name="date">[<see langword="out"/>] Date output parsed from the representation</param>
        /// <returns>True if parsed successfully; false otherwise.</returns>
        public static bool TryParsePosixTime(string posixDateRepresentation, out DateTimeOffset date)
        {
            try
            {
                date = ParsePosixTime(posixDateRepresentation);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Parses the POSIX date formatted with the representation according to the vCard and vCalendar specifications
        /// </summary>
        /// <param name="posixDateRepresentation">Date representation in basic or extended format of ISO 8601</param>
        /// <returns>An instance of <see cref="DateTimeOffset"/> that matches the representation</returns>
        /// <exception cref="ArgumentException"></exception>
        public static DateTimeOffset ParsePosixTimestamp(string posixDateRepresentation) =>
            ParsePosixRepresentation(posixDateRepresentation, supportedTimestampFormats);

        /// <summary>
        /// Tries to parse the POSIX date formatted with the representation according to the vCard and vCalendar specifications
        /// </summary>
        /// <param name="posixDateRepresentation">Date representation in basic or extended format of ISO 8601</param>
        /// <param name="date">[<see langword="out"/>] Date output parsed from the representation</param>
        /// <returns>True if parsed successfully; false otherwise.</returns>
        public static bool TryParsePosixTimestamp(string posixDateRepresentation, out DateTimeOffset date)
        {
            try
            {
                date = ParsePosixTime(posixDateRepresentation);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Saves the date to a ISO 8601 formatted date
        /// </summary>
        /// <param name="posixDateRepresentation">Date to save</param>
        /// <param name="dateOnly">Whether to save only date</param>
        /// <returns>A string representation of a date formatted with the basic ISO 8601 format</returns>
        public static string SavePosixDate(DateTimeOffset posixDateRepresentation, bool dateOnly = false)
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

        /// <summary>
        /// Parses the POSIX UTC offset formatted with the representation according to the vCard and vCalendar specifications
        /// </summary>
        /// <param name="utcOffsetRepresentation">UTC offset representation in basic or extended format of ISO 8601, prefixed by either a plus or a minus sign</param>
        /// <returns>An instance of <see cref="TimeSpan"/> that matches the representation</returns>
        /// <exception cref="ArgumentException"></exception>
        public static TimeSpan ParseUtcOffset(string utcOffsetRepresentation)
        {
            // Check for sanity
            if (string.IsNullOrEmpty(utcOffsetRepresentation))
                throw new ArgumentException($"UTC offset representation is not provided.");

            // Now, this representation might be a POSIX offset that follows the vCard specification, but check the sign,
            // because it might be either <+/->HHmmss, <+/->HHmm, or <+/->HH.
            string designatorStr = utcOffsetRepresentation.Substring(0, 1);
            string offsetNoSign = utcOffsetRepresentation.Substring(1);
            if (designatorStr != "+" && designatorStr != "-")
                throw new ArgumentException($"Designator {designatorStr} is invalid.");
            if (TimeSpan.TryParseExact(offsetNoSign, supportedTimeFormats, CultureInfo.InvariantCulture, out TimeSpan offset))
                return designatorStr == "-" && offset != new TimeSpan() ? -offset : offset;
            throw new ArgumentException($"Can't parse offset {utcOffsetRepresentation}");
        }

        /// <summary>
        /// Saves the UTC offset to a ISO 8601 formatted time
        /// </summary>
        /// <param name="utcOffsetRepresentation">UTC offset to save</param>
        /// <returns>A string representation of a UTC offset formatted with the basic ISO 8601 format</returns>
        public static string SaveUtcOffset(TimeSpan utcOffsetRepresentation)
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
        /// <summary>
        /// Gets the date/time offset from the duration specifier that is compliant with the ISO-8601:2004 specification
        /// </summary>
        /// <param name="duration">Duration specifier in the ISO-8601:2004 format</param>
        /// <param name="modern">Whether to disable parsing years and months or not</param>
        /// <param name="utc">Whether to use UTC</param>
        /// <param name="source">Source date/time</param>
        /// <returns>A date/time offset instance and a time span instance from the duration specifier</returns>
        /// <exception cref="ArgumentException"></exception>
        public static (DateTimeOffset result, TimeSpan span) GetDurationSpan(string duration, bool modern = false, bool utc = true, DateTimeOffset? source = null)
        {
            // Sanity checks
            duration = duration.Trim();
            if (string.IsNullOrEmpty(duration))
                throw new ArgumentException($"Duration is not provided");

            // Check to see if we've been provided with a sign
            bool isNegative = duration[0] == '-';
            if (duration[0] == '+' || isNegative)
                duration = duration.Substring(1);
            if (duration[0] != 'P')
                throw new ArgumentException($"Duration is invalid: {duration}");
            duration = duration.Substring(1);

            // Populate the date time offset accordingly
            DateTimeOffset rightNow =
                source is DateTimeOffset sourceDate ?
                sourceDate :
                (utc ? DateTimeOffset.UtcNow : DateTimeOffset.Now);
            DateTimeOffset offset = rightNow;
            bool inDate = true;
            while (!string.IsNullOrEmpty(duration))
            {
                // Get the designator index
                int designatorIndex;
                for (designatorIndex = 0; designatorIndex < duration.Length - 1; designatorIndex++)
                    if (!char.IsNumber(duration[designatorIndex]))
                        break;

                // Split the duration according to the designator index
                string digits = duration.Substring(0, designatorIndex);
                string type = duration.Substring(designatorIndex, 1);
                int length = digits.Length + type.Length;

                // Add according to type, but check first for the time designator
                if (type == "T")
                {
                    duration = duration.Substring(length);
                    inDate = false;
                    continue;
                }
                if (!int.TryParse(digits, out int value))
                    throw new ArgumentException($"Digits are not numeric: {digits}, {duration}");
                value = isNegative ? -value : value;
                switch (type)
                {
                    // Year and Month types are only supported in vCalendar 1.0
                    case "Y":
                        if (modern)
                            throw new ArgumentException($"Year specifier is disabled in vCalendar 2.0, {duration}");
                        offset = offset.AddYears(value);
                        break;
                    case "M":
                        if (modern && inDate)
                            throw new ArgumentException($"Month specifier is disabled in vCalendar 2.0, {duration}");
                        if (inDate)
                            offset = offset.AddMonths(value);
                        else
                            offset = offset.AddMinutes(value);
                        break;

                    // Supported in all vCalendars
                    case "W":
                        offset = offset.AddDays(value * 7);
                        break;
                    case "D":
                        offset = offset.AddDays(value);
                        break;
                    case "H":
                        offset = offset.AddHours(value);
                        break;
                    case "S":
                        offset = offset.AddSeconds(value);
                        break;
                    default:
                        throw new ArgumentException($"Type is invalid: {type}, {duration}");
                }
                duration = duration.Substring(length);
            }

            // Return the result
            return (offset, offset - rightNow);
        }

        /// <summary>
        /// Gets the time period that contains dates or date/duration combination that satisfy the ISO-8601:2004 specification
        /// </summary>
        /// <param name="period">Either a date/date format or a date/duration format that conform with the ISO-8601:2004 specification</param>
        /// <returns>A <see cref="TimePeriod"/> instance that describes the time period</returns>
        /// <exception cref="ArgumentException"></exception>
        public static TimePeriod GetTimePeriod(string period)
        {
            // Sanity checks
            period = period.Trim();
            if (string.IsNullOrEmpty(period))
                throw new ArgumentException("Time period is not specified");

            // Parse the time period by splitting with the slash character to two string variables
            string[] splits = period.Split('/');
            if (splits.Length != 2)
                throw new ArgumentException($"After splitting, got {splits.Length} instead of 2: {period}");
            string startStr = splits[0];
            string endStr = splits[1];

            // Now, parse them
            if (!TryParsePosixDateTime(startStr, out DateTimeOffset start))
                throw new ArgumentException($"Invalid start date {startStr}: {period}");
            if (!TryParsePosixDateTime(endStr, out DateTimeOffset end))
                end = GetDurationSpan(endStr, source: start).result;

            // Return a new object
            return new TimePeriod(start, end);
        }

        internal static string GetTypesString(ArgumentInfo[] args, string @default, bool isSpecifierRequired = true)
        {
            // We're given an array of split arguments of an element delimited by the colon, such as: "...TYPE=home..."
            // Filter list of arguments with the arguments that start with the type argument specifier, or, if specifier is not required,
            // that doesn't have an equals sign
            var ArgType = args.Where((arg) => arg.Key == VcardConstants._typeArgumentSpecifier || string.IsNullOrEmpty(arg.Key)).ToArray();

            // Trying to specify type without TYPE= is illegal according to RFC2426 in vCard 3.0 and 4.0
            if (ArgType.Length > 0 && string.IsNullOrEmpty(ArgType[0].Key) && isSpecifierRequired)
                throw new InvalidDataException("Type must be prepended with TYPE=");

            // Flatten the strings
            var stringArrays = ArgType.Select((arg) => arg.AllValues);
            List<string> flattened = [];
            foreach (var stringArray in stringArrays)
                flattened.AddRange(stringArray);

            // Get the type from the split argument
            string Type =
                ArgType.Length > 0 ?
                string.Join(VcardConstants._valueDelimiter.ToString(), flattened) :
                @default;

            // Return the type
            return Type;
        }

        internal static string[] GetTypes(ArgumentInfo[] args, string @default, bool isSpecifierRequired = true) =>
            GetTypesString(args, @default, isSpecifierRequired).Split([VcardConstants._valueDelimiter], StringSplitOptions.RemoveEmptyEntries);

        internal static string GetValuesString(ArgumentInfo[] args, string @default, string argSpecifier)
        {
            // We're given an array of split arguments of an element delimited by the colon, such as: "...TYPE=home..."
            // Filter list of arguments with the arguments that start with the specified specifier (key)
            string finalSpecifierName = argSpecifier.EndsWith("=") ? argSpecifier.Substring(0, argSpecifier.Length - 1) : argSpecifier;
            var argFromSpecifier = args.Where((arg) => arg.Key.Equals(finalSpecifierName, StringComparison.OrdinalIgnoreCase));

            // Flatten the strings
            var stringArrays = argFromSpecifier.Select((arg) => arg.AllValues);
            List<string> flattened = [];
            foreach (var stringArray in stringArrays)
                flattened.AddRange(stringArray);

            // Attempt to get the value from the key
            string argString =
                flattened.Count() > 0 ?
                string.Join(VcardConstants._valueDelimiter.ToString(), flattened) :
                @default;
            return argString;
        }

        internal static string[] GetValues(ArgumentInfo[] args, string @default, string argSpecifier) =>
            GetValuesString(args, @default, argSpecifier).Split([VcardConstants._valueDelimiter], StringSplitOptions.RemoveEmptyEntries);

        internal static string GetFirstValue(ArgumentInfo[] args, string @default, string argSpecifier)
        {
            // We're given an array of split arguments of an element delimited by the colon, such as: "...TYPE=home..."
            // Filter list of arguments with the arguments that start with the specified specifier (key)
            var argFromSpecifier = args.SingleOrDefault((arg) => arg.Key == argSpecifier);

            // Attempt to get the value from the key
            string argString =
                argFromSpecifier is not null ?
                argFromSpecifier.Values.First().value :
                @default;
            return argString;
        }

        internal static string MakeStringBlock(string target, int firstLength = 0, bool writeSpace = true)
        {
            const int maxChars = 74;
            int maxCharsFirst = maxChars - firstLength;

            // Construct the block
            StringBuilder block = new();
            int selectedMax = maxCharsFirst;
            int processed = 0;
            for (int currCharNum = 0; currCharNum < target.Length; currCharNum++)
            {
                if (processed >= selectedMax || target[currCharNum] == '\n')
                {
                    // Append a new line because we reached the maximum limit
                    selectedMax = writeSpace ? maxChars - 1 : maxChars;
                    processed = 0;
                    block.Append("\n");
                    if (writeSpace)
                        block.Append(" ");
                }
                if (target[currCharNum] != '\n' && target[currCharNum] != '\r')
                {
                    block.Append(target[currCharNum]);
                    processed++;
                }
            }
            return block.ToString();
        }

        internal static bool IsEncodingBlob(ArgumentInfo[]? args, string? keyEncoded)
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

        internal static Stream GetBlobData(ArgumentInfo[]? args, string? keyEncoded)
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

        internal static DateTimeOffset ParsePosixRepresentation(string posixDateRepresentation, string[] formats)
        {
            // Check for sanity
            if (string.IsNullOrEmpty(posixDateRepresentation))
                throw new ArgumentException($"Date representation is not provided.");

            // Now, check the representation
            bool assumeUtc = posixDateRepresentation[posixDateRepresentation.Length - 1] == 'Z';
            if (DateTimeOffset.TryParseExact(posixDateRepresentation, formats, CultureInfo.InvariantCulture, assumeUtc ? DateTimeStyles.AssumeUniversal : DateTimeStyles.AssumeLocal, out var date))
                return date;
            throw new ArgumentException($"Can't parse date {posixDateRepresentation}");
        }

        internal static string ProcessStringValue(string value, string valueType, char split = ';')
        {
            // Now, handle each type individually
            string finalValue;
            finalValue = Regex.Unescape(value);
            var splitValues = finalValue.Split(split);
            bool valid = false;
            List<Exception> errors = [];
            foreach (string finalValuePart in splitValues)
            {
                try
                {
                    switch (valueType.ToUpper())
                    {
                        case "URI":
                        case "URL":
                            // Check the URI
                            if (!Uri.TryCreate(finalValuePart, UriKind.Absolute, out Uri uri))
                                throw new InvalidDataException($"URL {finalValuePart} is invalid");
                            break;
                        case "UTC-OFFSET":
                            // Check the UTC offset
                            ParseUtcOffset(finalValuePart);
                            break;
                        case "DATE":
                            // Check the date
                            if (!TryParsePosixDate(finalValuePart, out _))
                                throw new InvalidDataException($"Date {finalValuePart} is invalid");
                            break;
                        case "TIME":
                            // Check the time
                            if (!TryParsePosixTime(finalValuePart, out _))
                                throw new InvalidDataException($"Time {finalValuePart} is invalid");
                            break;
                        case "DATE-TIME":
                            // Check the date and time
                            if (!TryParsePosixDateTime(finalValuePart, out _))
                                throw new InvalidDataException($"Date and time {finalValuePart} is invalid");
                            break;
                        case "DATE-AND-OR-TIME":
                            // Check the date and/or time
                            if (!TryParsePosixDateTime(finalValuePart, out _) &&
                                !TryParsePosixTime(finalValuePart, out _))
                                throw new InvalidDataException($"Date and/or time {finalValuePart} is invalid");
                            break;
                        case "TIMESTAMP":
                            // Check the timestamp
                            if (!TryParsePosixTimestamp(finalValuePart, out _))
                                throw new InvalidDataException($"Timestamp {finalValuePart} is invalid");
                            break;
                        case "BOOLEAN":
                            // Check the boolean
                            if (!finalValuePart.Equals("true", StringComparison.OrdinalIgnoreCase) &&
                                !finalValuePart.Equals("false", StringComparison.OrdinalIgnoreCase))
                                throw new InvalidDataException($"Boolean {finalValuePart} is invalid");
                            break;
                        case "INTEGER":
                            // Check the integer
                            if (!int.TryParse(finalValuePart, out _))
                                throw new InvalidDataException($"Integer {finalValuePart} is invalid");
                            break;
                        case "FLOAT":
                            // Check the float
                            string[] floatParts = finalValuePart.Split(split == ';' ? ',' : ';');
                            foreach (var floatPart in floatParts)
                                if (!double.TryParse(floatPart, out _))
                                    throw new InvalidDataException($"Float {floatPart} in {finalValuePart} is invalid");
                            break;
                        case "DURATION":
                            // Check the duration
                            GetDurationSpan(finalValuePart);
                            break;
                        case "PERIOD":
                            // Check the period
                            GetTimePeriod(finalValuePart);
                            break;
                        case "RECUR":
                            // Check the recursion rules
                            try
                            {
                                RecurrenceParser.ParseRuleV1(finalValuePart);
                            }
                            catch
                            {
                                RecurrenceParser.ParseRuleV2(finalValuePart);
                            }
                            break;
                    }
                    valid = true;
                }
                catch (Exception e)
                {
                    errors.Add(e);
                    continue;
                }
            }

            // Return the result
            if (!valid)
                throw new InvalidDataException($"String value {value} for {valueType} is invalid.\n\n  - {string.Join("\n  - ", errors.Select((ex) => ex.Message))}");
            return finalValue;
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

        internal static string ConstructBlocks((int, string)[] cardContent, ref int i)
        {
            StringBuilder valueBuilder = new();
            bool constructing;
            int idx;
            for (idx = i; idx < cardContent.Length; idx++)
            {
                // Get line
                var content = cardContent[idx];
                string _value = content.Item2;
                if (string.IsNullOrEmpty(_value))
                    continue;

                // First, check to see if we need to construct blocks
                string secondLine = idx + 1 < cardContent.Length ? cardContent[idx + 1].Item2 : "";
                bool firstConstructedLine = !_value.StartsWith(VcardConstants._spaceBreak) && !_value.StartsWith(VcardConstants._tabBreak);
                constructing = secondLine.StartsWithAnyOf([VcardConstants._spaceBreak, VcardConstants._tabBreak]);
                secondLine = secondLine.Length > 1 ? secondLine.Substring(1) : "";
                if (constructing)
                {
                    if (firstConstructedLine)
                        valueBuilder.Append(_value);
                    valueBuilder.Append(secondLine);
                    continue;
                }
                else if (firstConstructedLine && !constructing)
                    valueBuilder.Append(_value);
                break;
            }
            i = idx;
            return valueBuilder.ToString();
        }
    }
}
