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
using System.Text.RegularExpressions;
using VisualCard.Parsers.Arguments;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact telephone number information
    /// </summary>
    [DebuggerDisplay("Telephone = {ContactPhoneNumber}")]
    public class TelephoneInfo : BaseCardPartInfo, IEquatable<TelephoneInfo>
    {
        /// <summary>
        /// The contact's phone number
        /// </summary>
        public string? ContactPhoneNumber { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, ArgumentInfo[] finalArgs, int altId, string[] elementTypes, string group, string valueType, Version cardVersion) =>
            new TelephoneInfo().FromStringVcardInternal(value, finalArgs, altId, elementTypes, group, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion) =>
            ContactPhoneNumber ?? "";

        internal override BaseCardPartInfo FromStringVcardInternal(string value, ArgumentInfo[] finalArgs, int altId, string[] elementTypes, string group, string valueType, Version cardVersion)
        {
            // Populate the fields
            string _telephoneNumber = Regex.Unescape(value);
            if (valueType.Equals("uri", StringComparison.OrdinalIgnoreCase))
            {
                // Try to parse the source to ensure that it conforms the IETF RFC 1738: Uniform Resource Locators
                if (!Uri.TryCreate(_telephoneNumber, UriKind.Absolute, out Uri uri))
                    throw new InvalidDataException($"source {_telephoneNumber} is invalid");
                _telephoneNumber = uri.ToString();
            }
            TelephoneInfo _telephone = new(altId, finalArgs, elementTypes, valueType, group, _telephoneNumber);
            return _telephone;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((TelephoneInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="TelephoneInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(TelephoneInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="TelephoneInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="TelephoneInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(TelephoneInfo source, TelephoneInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.ContactPhoneNumber == target.ContactPhoneNumber
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 292984562;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(ContactPhoneNumber);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(TelephoneInfo left, TelephoneInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(TelephoneInfo left, TelephoneInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCardPartInfo source, BaseCardPartInfo target) =>
            ((TelephoneInfo)source) == ((TelephoneInfo)target);

        internal TelephoneInfo() { }

        internal TelephoneInfo(int altId, ArgumentInfo[] arguments, string[] elementTypes, string valueType, string group, string contactPhoneNumber) :
            base(arguments, altId, elementTypes, valueType, group)
        {
            ContactPhoneNumber = contactPhoneNumber;
        }
    }
}
