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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Textify.General;
using VisualCard.Common.Diagnostics;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Parsers.Recurrence;

namespace VisualCard.Common.Parsers
{
    /// <summary>
    /// Common tools for vCard parsing
    /// </summary>
    public static class CommonTools
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
                LoggingTools.Info("Got date {0}", date);
                return true;
            }
            catch (Exception ex)
            {
                LoggingTools.Error(ex, "POSIX date {0} failed to parse: {1}", date, ex.Message);
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
                LoggingTools.Info("Got date {0}", date);
                return true;
            }
            catch (Exception ex)
            {
                LoggingTools.Error(ex, "POSIX date {0} failed to parse: {1}", date, ex.Message);
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
                LoggingTools.Info("Got time {0}", date);
                return true;
            }
            catch (Exception ex)
            {
                LoggingTools.Error(ex, "POSIX time {0} failed to parse: {1}", date, ex.Message);
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
                LoggingTools.Info("Got timestamp {0}", date);
                return true;
            }
            catch (Exception ex)
            {
                LoggingTools.Error(ex, "POSIX timestamp {0} failed to parse: {1}", date, ex.Message);
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
            LoggingTools.Debug("Built date {0}", posixDateBuilder.ToString());
            if (!dateOnly)
            {
                posixDateBuilder.Append(
                    $"T" +
                    $"{posixDateRepresentation.Hour:00}" +
                    $"{posixDateRepresentation.Minute:00}" +
                    $"{posixDateRepresentation.Second:00}" +
                    $"{(posixDateRepresentation.Offset == new TimeSpan() ? "Z" : "")}"
                );
                LoggingTools.Debug("Built time {0}", posixDateBuilder.ToString());
            }
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
            LoggingTools.Debug("Parsing UTC offset {0}", utcOffsetRepresentation);
            string designatorStr = utcOffsetRepresentation.Substring(0, 1);
            string offsetNoSign = utcOffsetRepresentation.Substring(1);
            LoggingTools.Debug("Designator {0}, offset {1}", designatorStr, offsetNoSign);
            if (designatorStr != "+" && designatorStr != "-")
                throw new ArgumentException($"Designator {designatorStr} is invalid.");
            if (TimeSpan.TryParseExact(offsetNoSign, supportedTimeFormats, CultureInfo.InvariantCulture, out TimeSpan offset))
            {
                bool useNegative = designatorStr == "-" && offset != new TimeSpan();
                LoggingTools.Debug("Using negative {0}", useNegative);
                return useNegative ? -offset : offset;
            }
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
            LoggingTools.Debug("Built offset {0}", utcOffsetBuilder.ToString());
            if (utcOffsetRepresentation.Seconds != 0)
            {
                utcOffsetBuilder.Append(
                    $"{Math.Abs(utcOffsetRepresentation.Seconds):00}"
                );
                LoggingTools.Debug("Built offset with seconds {0}", utcOffsetBuilder.ToString());
            }
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
            LoggingTools.Debug("Processing duration {0}", duration);
            if (string.IsNullOrEmpty(duration))
                throw new ArgumentException($"Duration is not provided");

            // Check to see if we've been provided with a sign
            bool isNegative = duration[0] == '-';
            LoggingTools.Debug("Sign {0} [negative: {1}]", duration[0], isNegative);
            if (duration[0] == '+' || isNegative)
                duration = duration.Substring(1);
            if (duration[0] != 'P')
                throw new ArgumentException($"Duration is invalid: {duration}");
            duration = duration.Substring(1);
            LoggingTools.Debug("Final duration: {0}", duration);

            // Populate the date time offset accordingly
            DateTimeOffset rightNow =
                source is DateTimeOffset sourceDate ?
                sourceDate :
                utc ? DateTimeOffset.UtcNow : DateTimeOffset.Now;
            DateTimeOffset offset = rightNow;
            bool inDate = true;
            while (!string.IsNullOrEmpty(duration))
            {
                // Get the designator index
                int designatorIndex;
                for (designatorIndex = 0; designatorIndex < duration.Length - 1; designatorIndex++)
                {
                    LoggingTools.Debug("Checking {0} at idx {1} if it's a number", duration[designatorIndex], designatorIndex);
                    if (!char.IsNumber(duration[designatorIndex]))
                        break;
                }

                // Split the duration according to the designator index
                string digits = duration.Substring(0, designatorIndex);
                string type = duration.Substring(designatorIndex, 1);
                int length = digits.Length + type.Length;
                LoggingTools.Debug("Digits {0}, type {1} [length: {2}]", digits, type, length);

                // Add according to type, but check first for the time designator
                if (type == "T")
                {
                    duration = duration.Substring(length);
                    inDate = false;
                    LoggingTools.Info("Time designator found in duration! Continuing...");
                    continue;
                }
                if (!int.TryParse(digits, out int value))
                    throw new ArgumentException($"Digits are not numeric: {digits}, {duration}");
                value = isNegative ? -value : value;
                LoggingTools.Debug("Value is {0}", value);
                switch (type)
                {
                    // Year and Month types are only supported in vCalendar 1.0
                    case "Y":
                        if (modern)
                            throw new ArgumentException($"Year specifier is disabled in vCalendar 2.0, {duration}");
                        offset = offset.AddYears(value);
                        LoggingTools.Debug("Added {0} years", value);
                        break;
                    case "M":
                        if (modern && inDate)
                            throw new ArgumentException($"Month specifier is disabled in vCalendar 2.0, {duration}");
                        if (inDate)
                        {
                            offset = offset.AddMonths(value);
                            LoggingTools.Debug("Added {0} months", value);
                        }
                        else
                        {
                            offset = offset.AddMinutes(value);
                            LoggingTools.Debug("Added {0} minutes", value);
                        }
                        break;

                    // Supported in all vCalendars
                    case "W":
                        offset = offset.AddDays(value * 7);
                        LoggingTools.Debug("Added {0} days ({1} weeks)", value * 7, value);
                        break;
                    case "D":
                        offset = offset.AddDays(value);
                        LoggingTools.Debug("Added {0} days", value);
                        break;
                    case "H":
                        offset = offset.AddHours(value);
                        LoggingTools.Debug("Added {0} hours", value);
                        break;
                    case "S":
                        offset = offset.AddSeconds(value);
                        LoggingTools.Debug("Added {0} seconds", value);
                        break;
                    default:
                        LoggingTools.Error("Type is invalid! {0}, {1}, {2}", type, duration, value);
                        throw new ArgumentException($"Type is invalid: {type}, {duration}");
                }
                duration = duration.Substring(length);
                LoggingTools.Debug("Cut duration: {0}", duration);
            }

            // Return the result
            LoggingTools.Info("Returning offset {0} and {1}", offset, offset - rightNow);
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
            LoggingTools.Debug("Time period: {0}", period);
            if (string.IsNullOrEmpty(period))
                throw new ArgumentException("Time period is not specified");

            // Parse the time period by splitting with the slash character to two string variables
            string[] splits = period.Split('/');
            LoggingTools.Debug("Expected two splits: {0}", splits.Length);
            if (splits.Length != 2)
                throw new ArgumentException($"After splitting, got {splits.Length} instead of 2: {period}");
            string startStr = splits[0];
            string endStr = splits[1];

            // Now, parse them
            LoggingTools.Debug("Parsing start: {0}", startStr);
            if (!TryParsePosixDateTime(startStr, out DateTimeOffset start))
                throw new ArgumentException($"Invalid start date {startStr}: {period}");
            LoggingTools.Debug("Parsing end: {0}", startStr);
            if (!TryParsePosixDateTime(endStr, out DateTimeOffset end))
            {
                LoggingTools.Warning("End date couldn't be parsed: {0}, parsing as duration...", endStr);
                end = GetDurationSpan(endStr, source: start).result;
            }

            // Return a new object
            LoggingTools.Info("Returning time period: {0}, {1}", start, end);
            return new TimePeriod(start, end);
        }

        internal static string GetTypesString(ArgumentInfo[] args, string @default, bool isSpecifierRequired = true)
        {
            // We're given an array of split arguments of an element delimited by the colon, such as: "...TYPE=home..."
            // Filter list of arguments with the arguments that start with the type argument specifier, or, if specifier is not required,
            // that doesn't have an equals sign
            var ArgType = args.Where((arg) => arg.Key == CommonConstants._typeArgumentSpecifier || string.IsNullOrEmpty(arg.Key)).ToArray();
            LoggingTools.Debug("{0} argument types", ArgType.Length);

            // Trying to specify type without TYPE= is illegal according to RFC2426 in vCard 3.0 and 4.0
            if (ArgType.Length > 0 && string.IsNullOrEmpty(ArgType[0].Key) && isSpecifierRequired)
                throw new InvalidDataException("Type must be prepended with TYPE=");

            // Flatten the strings
            var stringArrays = ArgType.Select((arg) => arg.AllValues);
            LoggingTools.Debug("Flattening {0} arrays", stringArrays.Count());
            List<string> flattened = [];
            foreach (var stringArray in stringArrays)
            {
                LoggingTools.Debug("Adding {0} strings [{1}]", stringArray.Length, string.Join(", ", stringArray));
                flattened.AddRange(stringArray);
            }

            // Get the type from the split argument
            string Type =
                ArgType.Length > 0 ?
                string.Join(CommonConstants._valueDelimiter.ToString(), flattened) :
                @default;
            LoggingTools.Debug("Type is {0}", Type);

            // Return the type
            return Type;
        }

        internal static string[] GetTypes(ArgumentInfo[] args, string @default, bool isSpecifierRequired = true) =>
            GetTypesString(args, @default, isSpecifierRequired).Split([CommonConstants._valueDelimiter], StringSplitOptions.RemoveEmptyEntries);

        internal static string GetValuesString(ArgumentInfo[] args, string @default, string argSpecifier)
        {
            // We're given an array of split arguments of an element delimited by the colon, such as: "...TYPE=home..."
            // Filter list of arguments with the arguments that start with the specified specifier (key)
            string finalSpecifierName = argSpecifier.EndsWith("=") ? argSpecifier.Substring(0, argSpecifier.Length - 1) : argSpecifier;
            var argFromSpecifier = args.Where((arg) => arg.Key.Equals(finalSpecifierName, StringComparison.OrdinalIgnoreCase));
            LoggingTools.Debug("Specifier name: {0}, {1} arguments", finalSpecifierName, argFromSpecifier.Count());

            // Flatten the strings
            var stringArrays = argFromSpecifier.Select((arg) => arg.AllValues);
            LoggingTools.Debug("Flattening {0} arrays", stringArrays.Count());
            List<string> flattened = [];
            foreach (var stringArray in stringArrays)
            {
                LoggingTools.Debug("Adding {0} strings [{1}]", stringArray.Length, string.Join(", ", stringArray));
                flattened.AddRange(stringArray);
            }

            // Attempt to get the value from the key
            string argString =
                flattened.Count() > 0 ?
                string.Join(CommonConstants._valueDelimiter.ToString(), flattened) :
                @default;
            LoggingTools.Debug("Values string: {0}", argString);
            return argString;
        }

        internal static string[] GetValues(ArgumentInfo[] args, string @default, string argSpecifier) =>
            GetValuesString(args, @default, argSpecifier).Split([CommonConstants._valueDelimiter], StringSplitOptions.RemoveEmptyEntries);

        internal static string GetFirstValue(ArgumentInfo[] args, string @default, string argSpecifier)
        {
            // We're given an array of split arguments of an element delimited by the colon, such as: "...TYPE=home..."
            // Filter list of arguments with the arguments that start with the specified specifier (key)
            var argFromSpecifier = args.SingleOrDefault((arg) => arg.Key == argSpecifier);
            LoggingTools.Debug("Argument info for {0} [default is {1}] exists: {2}", argSpecifier, @default, argFromSpecifier is not null);
            if (argFromSpecifier is null)
                return @default;

            // Attempt to get the value from the key
            string argString =
                argFromSpecifier.Values.FirstOrDefault() != default ?
                argFromSpecifier.Values.First().value :
                @default;
            LoggingTools.Debug("Value string: {0}", argString);
            return argString;
        }

        internal static string MakeStringBlock(string target, int firstLength = 0, bool writeSpace = true, string encoding = "")
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

                    // If we're dealing with quoted printable, put the equal sign before the new line
                    if (encoding == CommonConstants._quotedPrintable)
                        block.Append('=');
                    block.Append('\n');

                    // If we're not dealing with quoted printable, add space.
                    if (writeSpace && encoding != CommonConstants._quotedPrintable)
                        block.Append(' ');
                }
                if (target[currCharNum] != '\n' && target[currCharNum] != '\r')
                {
                    block.Append(target[currCharNum]);
                    processed++;

                    // Check to see if the current character is an equal sign and the string is a quoted printable
                    if (target[currCharNum] == '=' && encoding == CommonConstants._quotedPrintable)
                    {
                        // We need two characters to write the encoded character
                        for (int step = 1; step <= 2; step++)
                        {
                            block.Append(target[currCharNum + 1]);
                            processed++;
                            currCharNum++;
                        }
                    }
                }
            }
            return block.ToString();
        }

        internal static bool IsEncodingBlob(ArgumentInfo[]? args, string? keyEncoded)
        {
            args ??= [];
            string encoding = GetValuesString(args, "b", CommonConstants._encodingArgumentSpecifier);
            bool isValidUri = Uri.TryCreate(keyEncoded, UriKind.Absolute, out Uri uri);
            LoggingTools.Debug("Encoding: {0}, URI: {1}", encoding, isValidUri);
            if (isValidUri)
            {
                LoggingTools.Debug("URI scheme: {0}", uri.Scheme);
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
                LoggingTools.Debug("URI: {0}", isValidUri);
                string dataStr;
                if (isValidUri)
                {
                    LoggingTools.Debug("URI scheme: {0}", uri.Scheme);
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
                LoggingTools.Debug("Got blob stream with {0} bytes", blobStream.Length);
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
            LoggingTools.Info("Parsing POSIX date representation {0}", posixDateRepresentation);
            bool assumeUtc = posixDateRepresentation[posixDateRepresentation.Length - 1] == 'Z';
            if (DateTimeOffset.TryParseExact(posixDateRepresentation, formats, CultureInfo.InvariantCulture, assumeUtc ? DateTimeStyles.AssumeUniversal : DateTimeStyles.AssumeLocal, out var date))
            {
                LoggingTools.Info("Got {0}", date);
                return date;
            }
            LoggingTools.Error("Parsing failed for {0}", posixDateRepresentation);
            throw new ArgumentException($"Can't parse date {posixDateRepresentation}");
        }

        internal static string ProcessStringValue(string value, string valueType, char split = ';')
        {
            // Now, handle each type individually
            string finalValue = Regex.Unescape(value);
            var splitValues = finalValue.Split(split);
            LoggingTools.Debug("String value: {0}, split to {1} with {2} for {3}", finalValue, splitValues.Length, split, valueType);
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
                    LoggingTools.Error(e, "Error parsing string value {0}: {1}", finalValue, e.Message);
                    errors.Add(e);
                    continue;
                }
            }

            // Return the result
            if (!valid)
                throw new InvalidDataException($"String value {value} for {valueType} is invalid.\n\n  - {string.Join("\n  - ", errors.Select((ex) => ex.Message))}");
            LoggingTools.Info("Returning final value {0}", finalValue);
            return finalValue;
        }

        internal static string ConstructBlocks(string[] cardContent, ref int i)
        {
            StringBuilder valueBuilder = new();
            bool constructing;
            int idx;
            for (idx = i; idx < cardContent.Length; idx++)
            {
                // Get line
                var content = cardContent[idx];
                string _value = content;
                if (string.IsNullOrEmpty(_value))
                    continue;

                // First, check to see if we need to construct blocks
                string secondLine = idx + 1 < cardContent.Length ? cardContent[idx + 1] : "";
                bool firstConstructedLine = !_value.StartsWith(CommonConstants._spaceBreak) && !_value.StartsWith(CommonConstants._tabBreak);
                constructing = secondLine.StartsWithAnyOf([CommonConstants._spaceBreak, CommonConstants._tabBreak]);
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

        internal static string BuildRawValue(string prefix, string rawValue, string group, ArgumentInfo[] args)
        {
            var valueBuilder = new StringBuilder(prefix);
            if (!string.IsNullOrWhiteSpace(group))
                valueBuilder.Insert(0, $"{group}.");

            // Check to see if we've been provided arguments
            bool argsNeeded = args.Length > 0;
            if (argsNeeded)
            {
                valueBuilder.Append(CommonConstants._fieldDelimiter);
                for (int i = 0; i < args.Length; i++)
                {
                    // Get the argument and build it as a string
                    ArgumentInfo arg = args[i];
                    string argFinal = arg.BuildArguments();
                    valueBuilder.Append(argFinal);

                    // If not done, add another delimiter
                    if (i + 1 < args.Length)
                        valueBuilder.Append(CommonConstants._fieldDelimiter);
                }
            }

            // Now, add the raw value
            valueBuilder.Append(CommonConstants._argumentDelimiter);
            valueBuilder.Append(rawValue);
            return valueBuilder.ToString();
        }
    }
}
