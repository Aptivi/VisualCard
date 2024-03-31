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

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact time zone info
    /// </summary>
    [DebuggerDisplay("Time zone = {TimeZone}")]
    public class TimeDateZoneInfo : BaseCardPartInfo, IEquatable<TimeDateZoneInfo>
    {
        /// <summary>
        /// The contact's time zone types
        /// </summary>
        public string[] TimeZoneTypes { get; }
        /// <summary>
        /// The contact's time zone
        /// </summary>
        public string TimeZone { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion) =>
            new TimeDateZoneInfo().FromStringVcardInternal(value, finalArgs, altId, elementTypes, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;
            if (altIdSupported)
            {
                bool installAltId = AltId >= 0 && Arguments.Length > 0;
                return
                    $"{VcardConstants._timeZoneSpecifier}{(installAltId ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter)}" +
                    $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                    $"{(installAltId ? string.Join(VcardConstants._fieldDelimiter.ToString(), AltArguments) + VcardConstants._argumentDelimiter : "")}" +
                    $"{TimeZone}";
            }
            else
            {
                return
                    $"{VcardConstants._timeZoneSpecifier}:" +
                    $"{TimeZone}";
            }
        }

        internal override BaseCardPartInfo FromStringVcardInternal(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion)
        {
            // Get the value
            string tzValue = value.Substring(VcardConstants._timeZoneSpecifier.Length + 1);

            // Populate the fields
            return InstallInfo([tzValue], false, altId, cardVersion);
        }

        internal override BaseCardPartInfo FromStringVcardWithTypeInternal(string value, string[] finalArgs, int altId, Version cardVersion)
        {
            // Check the line
            string tzValue = value.Substring(VcardConstants._timeZoneSpecifier.Length + 1);
            string[] splitTz = tzValue.Split(VcardConstants._argumentDelimiter);
            if (splitTz.Length < 2)
                throw new InvalidDataException("Time Zone field must specify exactly two values (VALUE=\"text\" / \"uri\" / \"utc-offset\", and time zone info)");

            // Populate the fields
            return InstallInfo(splitTz, true, finalArgs, altId, cardVersion);
        }

        private TimeDateZoneInfo InstallInfo(string[] splitTz, bool installType, int altId, Version cardVersion) =>
            InstallInfo(splitTz, installType, [], altId, cardVersion);

        private TimeDateZoneInfo InstallInfo(string[] splitTz, bool installType, string[] finalArgs, int altId, Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;
            bool typesSupported = cardVersion.Major >= 3;

            // Get the types and the number
            string[] _geoTypes = typesSupported ? installType ? VcardParserTools.GetValues(splitTz, "", VcardConstants._valueArgumentSpecifier) : ["uri-offset"] : [];
            string _geoStr = Regex.Unescape(typesSupported ? installType ? splitTz[1] : splitTz[0] : splitTz[0]);

            // Add the fetched information
            TimeDateZoneInfo _timeZone = new(altIdSupported ? altId : 0, finalArgs, elementTypes, valueType, _geoTypes, _geoStr);
            return _timeZone;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            base.Equals(obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="TimeDateZoneInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(TimeDateZoneInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="TimeDateZoneInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="TimeDateZoneInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(TimeDateZoneInfo source, TimeDateZoneInfo target)
        {
            // We can't perform this operation on null.
            if (source is null)
                return false;

            // Check all the properties
            return
                source.AltArguments.SequenceEqual(target.AltArguments) &&
                source.TimeZoneTypes.SequenceEqual(target.TimeZoneTypes) &&
                base.Equals(source, target) &&
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

        /// <inheritdoc/>
        public static bool operator ==(TimeDateZoneInfo left, TimeDateZoneInfo right) =>
            EqualityComparer<TimeDateZoneInfo>.Default.Equals(left, right);

        /// <inheritdoc/>
        public static bool operator !=(TimeDateZoneInfo left, TimeDateZoneInfo right) =>
            !(left == right);

        internal TimeDateZoneInfo() { }

        internal TimeDateZoneInfo(int altId, string[] arguments, string[] elementTypes, string valueType, string[] timeZoneTypes, string timeZone)
        {
            AltId = altId;
            Arguments = arguments;
            TimeZoneTypes = timeZoneTypes;
            TimeZone = timeZone;
        }
    }
}
