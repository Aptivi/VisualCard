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
using System.Linq;
using System.Xml.Linq;
using VisualCard.Parsers;

namespace VisualCard.Parts
{
    public class EmailInfo : IEquatable<EmailInfo>
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
        /// The contact's email types
        /// </summary>
        public string[] ContactEmailTypes { get; }
        /// <summary>
        /// The contact's email address
        /// </summary>
        public string ContactEmailAddress { get; }

        public override bool Equals(object obj) =>
            base.Equals(obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="EmailInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(EmailInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="EmailInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="EmailInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(EmailInfo source, EmailInfo target)
        {
            // We can't perform this operation on null.
            if (source is null)
                return false;

            // Check all the properties
            return
                source.ContactEmailTypes.SequenceEqual(target.ContactEmailTypes) &&
                source.AltId == target.AltId &&
                source.ContactEmailAddress == target.ContactEmailAddress
            ;
        }

        public override int GetHashCode()
        {
            int hashCode = 2091849342;
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AltArguments);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(ContactEmailTypes);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContactEmailAddress);
            return hashCode;
        }

        internal string ToStringVcardTwo()
        {
            return
                $"{VcardConstants._emailSpecifier}" +
                $"TYPE={string.Join(",", ContactEmailTypes)}{VcardConstants._argumentDelimiter}" +
                $"{ContactEmailAddress}";
        }

        internal string ToStringVcardThree()
        {
            return
                $"{VcardConstants._emailSpecifier}" +
                $"TYPE={string.Join(",", ContactEmailTypes)}{VcardConstants._argumentDelimiter}" +
                $"{ContactEmailAddress}";
        }

        internal string ToStringVcardFour()
        {
            bool installAltId = AltId >= 0 && AltArguments.Length > 0;
            return
                $"{VcardConstants._emailSpecifier}" +
                $"{(installAltId ? "ALTID=" + AltId + VcardConstants._fieldDelimiter : "")}" +
                $"TYPE={string.Join(",", ContactEmailTypes)}{VcardConstants._argumentDelimiter}" +
                $"{ContactEmailAddress}";
        }

        internal EmailInfo() { }

        internal EmailInfo(int altId, string[] altArguments, string[] contactEmailTypes, string contactEmailAddress)
        {
            AltId = altId;
            AltArguments = altArguments;
            ContactEmailTypes = contactEmailTypes;
            ContactEmailAddress = contactEmailAddress;
        }
    }
}
