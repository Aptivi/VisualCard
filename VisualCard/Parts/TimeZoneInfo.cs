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

        internal string ToStringVcardFive() =>
            ToStringVcardFour();

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

        internal static TimeZoneInfo FromStringVcardFive(string value, int altId) =>
            FromStringVcardFour(value, altId);

        internal static TimeZoneInfo FromStringVcardFiveWithType(string value, List<string> finalArgs, int altId) =>
            FromStringVcardFourWithType(value, finalArgs, altId);

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
