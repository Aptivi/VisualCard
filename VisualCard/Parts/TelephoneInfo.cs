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

        internal static TelephoneInfo FromStringVcardTwo(string value)
        {
            // Get the value
            string telValue = value.Substring(VcardConstants._telephoneSpecifier.Length + 1);

            // Populate the fields
            string[] _telephoneTypes = new string[] { "CELL" };
            string _telephoneNumber = Regex.Unescape(telValue);
            TelephoneInfo _telephone = new(0, Array.Empty<string>(), _telephoneTypes, _telephoneNumber);
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
            TelephoneInfo _telephone = new(0, Array.Empty<string>(), _telephoneTypes, _telephoneNumber);
            return _telephone;
        }

        internal static TelephoneInfo FromStringVcardThree(string value)
        {
            // Get the value
            string telValue = value.Substring(VcardConstants._telephoneSpecifier.Length + 1);

            // Populate the fields
            string[] _telephoneTypes = new string[] { "CELL" };
            string _telephoneNumber = Regex.Unescape(telValue);
            TelephoneInfo _telephone = new(0, Array.Empty<string>(), _telephoneTypes, _telephoneNumber);
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
            TelephoneInfo _telephone = new(0, Array.Empty<string>(), _telephoneTypes, _telephoneNumber);
            return _telephone;
        }

        internal static TelephoneInfo FromStringVcardFour(string value, int altId)
        {
            // Get the value
            string telValue = value.Substring(VcardConstants._telephoneSpecifier.Length + 1);

            // Populate the fields
            string[] _telephoneTypes = new string[] { "CELL" };
            string _telephoneNumber = Regex.Unescape(telValue);
            TelephoneInfo _telephone = new(altId, Array.Empty<string>(), _telephoneTypes, _telephoneNumber);
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
            TelephoneInfo _telephone = new(altId, finalArgs.ToArray(), _telephoneTypes, _telephoneNumber);
            return _telephone;
        }

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
