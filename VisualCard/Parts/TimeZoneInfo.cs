/*
 * MIT License
 *
 * Copyright (c) 2021-2022 Aptivi
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VisualCard.Parsers;

namespace VisualCard.Parts
{
    /// <summary>
    /// Contact time zone info
    /// </summary>
    [DebuggerDisplay("Time zone = {TimeZone}")]
    public class TimeZoneInfo : IEquatable<TimeZoneInfo>
    {
        /// <summary>
        /// Alternative ID. Zero if unspecified.
        /// </summary>
        public int AltId { get; }
        /// <summary>
        /// Arguments that follow the AltId
        /// </summary>
        public string[] AltArguments { get; }
        /// <summary>
        /// The contact's time zone types
        /// </summary>
        public string[] TimeZoneTypes { get; }
        /// <summary>
        /// The contact's time zone
        /// </summary>
        public string TimeZone { get; }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            base.Equals(obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="TimeZoneInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(TimeZoneInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="TimeZoneInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="TimeZoneInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(TimeZoneInfo source, TimeZoneInfo target)
        {
            // We can't perform this operation on null.
            if (source is null)
                return false;

            // Check all the properties
            return
                source.AltArguments.SequenceEqual(target.AltArguments) &&
                source.TimeZoneTypes.SequenceEqual(target.TimeZoneTypes) &&
                source.AltId == target.AltId &&
                source.TimeZone == target.TimeZone
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 1304261678;
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AltArguments);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(TimeZoneTypes);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TimeZone);
            return hashCode;
        }

        internal string ToStringVcardTwo()
        {
            return
                $"{VcardConstants._timeZoneSpecifier}:" +
                $"{TimeZone}";
        }

        internal string ToStringVcardThree()
        {
            return
                $"{VcardConstants._timeZoneSpecifier}:" +
                $"{TimeZone}";
        }

        internal string ToStringVcardFour()
        {
            bool installAltId = AltId >= 0 && AltArguments.Length > 0;
            return
                $"{VcardConstants._timeZoneSpecifier}{(installAltId ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter)}" +
                $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                $"{(installAltId ? string.Join(VcardConstants._fieldDelimiter.ToString(), AltArguments) + VcardConstants._argumentDelimiter : "")}" +
                $"{TimeZone}";
        }

        internal static TimeZoneInfo FromStringVcardTwo(string value)
        {
            string tzValue = value.Substring(VcardConstants._timeZoneSpecifier.Length + 1);
            string _timeZoneStr = Regex.Unescape(tzValue);
            TimeZoneInfo _timeZone = new(0, [], [], _timeZoneStr);
            return _timeZone;
        }

        internal static TimeZoneInfo FromStringVcardTwoWithType(string value)
        {
            string tzValue = value.Substring(VcardConstants._timeZoneSpecifier.Length + 1);
            string[] splitTz = tzValue.Split(VcardConstants._argumentDelimiter);
            if (splitTz.Length < 2)
                throw new InvalidDataException("Time Zone field must specify exactly two values (VALUE=\"text\" / \"uri\" / \"utc-offset\", and time zone info)");

            // Get the types and the number
            string[] _timeZoneTypes = VcardParserTools.GetValues(splitTz, "", VcardConstants._valueArgumentSpecifier);
            string _timeZoneNumber = Regex.Unescape(splitTz[1]);

            // Add the fetched information
            TimeZoneInfo _timeZone = new(0, [], _timeZoneTypes, _timeZoneNumber);
            return _timeZone;
        }

        internal static TimeZoneInfo FromStringVcardThree(string value)
        {
            // Get the value
            string tzValue = value.Substring(VcardConstants._timeZoneSpecifier.Length + 1);

            // Populate the fields
            string[] _timeZoneTypes = ["uri-offset"];
            string _timeZoneNumber = Regex.Unescape(tzValue);
            TimeZoneInfo _timeZone = new(0, [], _timeZoneTypes, _timeZoneNumber);
            return _timeZone;
        }

        internal static TimeZoneInfo FromStringVcardThreeWithType(string value)
        {
            // Get the value
            string tzValue = value.Substring(VcardConstants._timeZoneSpecifier.Length + 1);
            string[] splitTz = tzValue.Split(VcardConstants._argumentDelimiter);
            if (splitTz.Length < 2)
                throw new InvalidDataException("Time Zone field must specify exactly two values (VALUE=\"text\" / \"uri\" / \"utc-offset\", and time zone info)");

            // Get the types and the number
            string[] _timeZoneTypes = VcardParserTools.GetValues(splitTz, "", VcardConstants._valueArgumentSpecifier);
            string _timeZoneNumber = Regex.Unescape(splitTz[1]);

            // Add the fetched information
            TimeZoneInfo _timeZone = new(0, [], _timeZoneTypes, _timeZoneNumber);
            return _timeZone;
        }

        internal static TimeZoneInfo FromStringVcardFour(string value, int altId)
        {
            // Get the value
            string tzValue = value.Substring(VcardConstants._timeZoneSpecifier.Length + 1);

            // Populate the fields
            string[] _timeZoneTypes = ["uri-offset"];
            string _timeZoneNumber = Regex.Unescape(tzValue);
            TimeZoneInfo _timeZone = new(altId, [], _timeZoneTypes, _timeZoneNumber);
            return _timeZone;
        }

        internal static TimeZoneInfo FromStringVcardFourWithType(string value, List<string> finalArgs, int altId)
        {
            // Get the value
            string tzValue = value.Substring(VcardConstants._timeZoneSpecifier.Length + 1);
            string[] splitTz = tzValue.Split(VcardConstants._argumentDelimiter);
            if (splitTz.Length < 2)
                throw new InvalidDataException("Time Zone field must specify exactly two values (VALUE=\"text\" / \"uri\" / \"utc-offset\", and time zone info)");

            // Get the types and the number
            string[] _timeZoneTypes = VcardParserTools.GetValues(splitTz, "", VcardConstants._valueArgumentSpecifier);
            string _timeZoneNumber = Regex.Unescape(splitTz[1]);

            // Add the fetched information
            TimeZoneInfo _timeZone = new(altId, [.. finalArgs], _timeZoneTypes, _timeZoneNumber);
            return _timeZone;
        }

        internal TimeZoneInfo() { }

        internal TimeZoneInfo(int altId, string[] altArguments, string[] timeZoneTypes, string timeZone)
        {
            AltId = altId;
            AltArguments = altArguments;
            TimeZoneTypes = timeZoneTypes;
            TimeZone = timeZone;
        }
    }
}
