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
    /// Contact telephone number information
    /// </summary>
    [DebuggerDisplay("Telephone = {ContactPhoneNumber}")]
    public class TelephoneInfo : IEquatable<TelephoneInfo>
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
        /// The contact's phone types
        /// </summary>
        public string[] ContactPhoneTypes { get; }
        /// <summary>
        /// The contact's phone number
        /// </summary>
        public string ContactPhoneNumber { get; }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            base.Equals(obj);

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
            if (source is null)
                return false;

            // Check all the properties
            return
                source.ContactPhoneTypes.SequenceEqual(target.ContactPhoneTypes) &&
                source.AltArguments.SequenceEqual(target.AltArguments) &&
                source.AltId == target.AltId &&
                source.ContactPhoneNumber == target.ContactPhoneNumber
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -986063477;
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AltArguments);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(ContactPhoneTypes);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContactPhoneNumber);
            return hashCode;
        }

        internal string ToStringVcardTwo()
        {
            return
                $"{VcardConstants._telephoneSpecifier};" +
                $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", ContactPhoneTypes)}{VcardConstants._argumentDelimiter}" +
                $"{ContactPhoneNumber}";
        }

        internal string ToStringVcardThree()
        {
            return
                $"{VcardConstants._telephoneSpecifier};" +
                $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", ContactPhoneTypes)}{VcardConstants._argumentDelimiter}" +
                $"{ContactPhoneNumber}";
        }

        internal string ToStringVcardFour()
        {
            bool installAltId = AltId >= 0 && AltArguments.Length > 0;
            return
                $"{VcardConstants._telephoneSpecifier};" +
                $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", ContactPhoneTypes)}{VcardConstants._argumentDelimiter}" +
                $"{ContactPhoneNumber}";
        }

        internal string ToStringVcardFive() =>
            ToStringVcardFour();

        internal static TelephoneInfo FromStringVcardTwo(string value)
        {
            // Get the value
            string telValue = value.Substring(VcardConstants._telephoneSpecifier.Length + 1);

            // Populate the fields
            string[] _telephoneTypes = ["CELL"];
            string _telephoneNumber = Regex.Unescape(telValue);
            TelephoneInfo _telephone = new(0, [], _telephoneTypes, _telephoneNumber);
            return _telephone;
        }

        internal static TelephoneInfo FromStringVcardTwoWithType(string value)
        {
            // Get the value
            string telValue = value.Substring(VcardConstants._telephoneSpecifier.Length + 1);
            string[] splitTel = telValue.Split(VcardConstants._argumentDelimiter);
            if (splitTel.Length < 2)
                throw new InvalidDataException("Telephone field must specify exactly two values (Type (optionally prepended with TYPE=), and phone number)");

            // Populate the fields
            string[] _telephoneTypes = VcardParserTools.GetTypes(splitTel, "CELL", false);
            string _telephoneNumber = Regex.Unescape(splitTel[1]);
            TelephoneInfo _telephone = new(0, [], _telephoneTypes, _telephoneNumber);
            return _telephone;
        }

        internal static TelephoneInfo FromStringVcardThree(string value)
        {
            // Get the value
            string telValue = value.Substring(VcardConstants._telephoneSpecifier.Length + 1);

            // Populate the fields
            string[] _telephoneTypes = ["CELL"];
            string _telephoneNumber = Regex.Unescape(telValue);
            TelephoneInfo _telephone = new(0, [], _telephoneTypes, _telephoneNumber);
            return _telephone;
        }

        internal static TelephoneInfo FromStringVcardThreeWithType(string value)
        {
            // Get the value
            string telValue = value.Substring(VcardConstants._telephoneSpecifier.Length + 1);
            string[] splitTel = telValue.Split(VcardConstants._argumentDelimiter);
            if (splitTel.Length < 2)
                throw new InvalidDataException("Telephone field must specify exactly two values (Type (must be prepended with TYPE=), and phone number)");

            // Populate the fields
            string[] _telephoneTypes = VcardParserTools.GetTypes(splitTel, "CELL", true);
            string _telephoneNumber = Regex.Unescape(splitTel[1]);
            TelephoneInfo _telephone = new(0, [], _telephoneTypes, _telephoneNumber);
            return _telephone;
        }

        internal static TelephoneInfo FromStringVcardFour(string value, int altId)
        {
            // Get the value
            string telValue = value.Substring(VcardConstants._telephoneSpecifier.Length + 1);

            // Populate the fields
            string[] _telephoneTypes = ["CELL"];
            string _telephoneNumber = Regex.Unescape(telValue);
            TelephoneInfo _telephone = new(altId, [], _telephoneTypes, _telephoneNumber);
            return _telephone;
        }

        internal static TelephoneInfo FromStringVcardFourWithType(string value, List<string> finalArgs, int altId)
        {
            // Get the value
            string telValue = value.Substring(VcardConstants._telephoneSpecifier.Length + 1);
            string[] splitTel = telValue.Split(VcardConstants._argumentDelimiter);
            if (splitTel.Length < 2)
                throw new InvalidDataException("Telephone field must specify exactly two values (Type (must be prepended with TYPE=), and phone number)");

            // Populate the fields
            string[] _telephoneTypes = VcardParserTools.GetTypes(splitTel, "CELL", true);
            string _telephoneNumber = Regex.Unescape(splitTel[1]);
            TelephoneInfo _telephone = new(altId, [.. finalArgs], _telephoneTypes, _telephoneNumber);
            return _telephone;
        }

        internal static TelephoneInfo FromStringVcardFive(string value, int altId) =>
            FromStringVcardFour(value, altId);

        internal static TelephoneInfo FromStringVcardFiveWithType(string value, List<string> finalArgs, int altId) =>
            FromStringVcardFourWithType(value, finalArgs, altId);

        internal TelephoneInfo() { }

        internal TelephoneInfo(int altId, string[] altArguments, string[] contactPhoneTypes, string contactPhoneNumber)
        {
            AltId = altId;
            AltArguments = altArguments;
            ContactPhoneTypes = contactPhoneTypes;
            ContactPhoneNumber = contactPhoneNumber;
        }
    }
}
