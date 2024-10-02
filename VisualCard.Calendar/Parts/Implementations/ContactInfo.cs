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
using VisualCard.Parsers.Arguments;

namespace VisualCard.Calendar.Parts.Implementations
{
    /// <summary>
    /// Calendar contact info
    /// </summary>
    [DebuggerDisplay("Contact = {Contact}")]
    public class ContactInfo : BaseCalendarPartInfo, IEquatable<ContactInfo>
    {
        /// <summary>
        /// The calendar's contact info
        /// </summary>
        public string? Contact { get; }

        internal static BaseCalendarPartInfo FromStringVcalendarStatic(string value, ArgumentInfo[] finalArgs, string[] elementTypes, string valueType, Version cardVersion) =>
            new ContactInfo().FromStringVcalendarInternal(value, finalArgs, elementTypes, valueType, cardVersion);

        internal override string ToStringVcalendarInternal(Version cardVersion) =>
            Contact ?? "";

        internal override BaseCalendarPartInfo FromStringVcalendarInternal(string value, ArgumentInfo[] finalArgs, string[] elementTypes, string valueType, Version cardVersion)
        {
            // Populate the fields
            var contact = Regex.Unescape(value);

            // Add the fetched information
            ContactInfo _time = new([], elementTypes, valueType, contact);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((ContactInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="ContactInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(ContactInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="ContactInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="ContactInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(ContactInfo source, ContactInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.Contact == target.Contact
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -1115589996;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(Contact);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(ContactInfo left, ContactInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(ContactInfo left, ContactInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCalendarPartInfo source, BaseCalendarPartInfo target) =>
            (ContactInfo)source == (ContactInfo)target;

        internal ContactInfo() { }

        internal ContactInfo(ArgumentInfo[] arguments, string[] elementTypes, string valueType, string contact) :
            base(arguments, elementTypes, valueType)
        {
            Contact = contact;
        }
    }
}
