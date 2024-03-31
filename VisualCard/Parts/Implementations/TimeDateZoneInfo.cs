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
        /// The contact's time zone
        /// </summary>
        public string TimeZone { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion) =>
            new TimeDateZoneInfo().FromStringVcardInternal(value, finalArgs, altId, elementTypes, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion) =>
            TimeZone;

        internal override BaseCardPartInfo FromStringVcardInternal(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;

            // Get the types and the number
            string _tzStr = Regex.Unescape(value);

            // Add the fetched information
            TimeDateZoneInfo _timeZone = new(altIdSupported ? altId : 0, finalArgs, elementTypes, valueType, _tzStr);
            return _timeZone;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((TimeDateZoneInfo)obj);

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
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.TimeZone == target.TimeZone
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 1988546296;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TimeZone);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(TimeDateZoneInfo left, TimeDateZoneInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(TimeDateZoneInfo left, TimeDateZoneInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCardPartInfo source, BaseCardPartInfo target) =>
            ((TimeDateZoneInfo)source) == ((TimeDateZoneInfo)target);

        internal TimeDateZoneInfo() { }

        internal TimeDateZoneInfo(int altId, string[] arguments, string[] elementTypes, string valueType, string timeZone) :
            base(arguments, altId, elementTypes, valueType)
        {
            TimeZone = timeZone;
        }
    }
}
