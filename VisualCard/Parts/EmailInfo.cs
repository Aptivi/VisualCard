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
using System.Net.Mail;
using VisualCard.Parsers;

namespace VisualCard.Parts
{
    /// <summary>
    /// Contact e-mail information
    /// </summary>
    [DebuggerDisplay("E-mail = {ContactEmailAddress}")]
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
                $"{VcardConstants._emailSpecifier};" +
                $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", ContactEmailTypes)}{VcardConstants._argumentDelimiter}" +
                $"{ContactEmailAddress}";
        }

        internal string ToStringVcardThree()
        {
            return
                $"{VcardConstants._emailSpecifier};" +
                $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", ContactEmailTypes)}{VcardConstants._argumentDelimiter}" +
                $"{ContactEmailAddress}";
        }

        internal string ToStringVcardFour()
        {
            bool installAltId = AltId >= 0 && AltArguments.Length > 0;
            return
                $"{VcardConstants._emailSpecifier};" +
                $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", ContactEmailTypes)}{VcardConstants._argumentDelimiter}" +
                $"{ContactEmailAddress}";
        }

        internal static EmailInfo FromStringVcardTwo(string value)
        {
            // Get the value
            string mailValue = value.Substring(VcardConstants._emailSpecifier.Length + 1);
            string[] splitMail = mailValue.Split(VcardConstants._argumentDelimiter);
            MailAddress mail;

            // Try to create mail address
            try
            {
                mail = new MailAddress(splitMail[0]);
            }
            catch (ArgumentException aex)
            {
                throw new InvalidDataException("E-mail address is invalid", aex);
            }

            // Populate the fields
            string[] _emailTypes = ["HOME"];
            string _emailAddress = mail.Address;
            EmailInfo _email = new(0, [], _emailTypes, _emailAddress);
            return _email;
        }

        internal static EmailInfo FromStringVcardTwoWithType(string value)
        {
            // Get the value
            string mailValue = value.Substring(VcardConstants._emailSpecifier.Length + 1);
            string[] splitMail = mailValue.Split(VcardConstants._argumentDelimiter);
            MailAddress mail;
            if (splitMail.Length < 2)
                throw new InvalidDataException("E-mail field must specify exactly two values (Type (optionally prepended with TYPE=), and a valid e-mail address)");

            // Try to create mail address
            try
            {
                mail = new MailAddress(splitMail[1]);
            }
            catch (ArgumentException aex)
            {
                throw new InvalidDataException("E-mail address is invalid", aex);
            }

            // Populate the fields
            string[] _emailTypes = VcardParserTools.GetTypes(splitMail, "HOME", false);
            string _emailAddress = mail.Address;
            EmailInfo _email = new(0, [], _emailTypes, _emailAddress);
            return _email;
        }

        internal static EmailInfo FromStringVcardThree(string value)
        {
            // Get the value
            string mailValue = value.Substring(VcardConstants._emailSpecifier.Length + 1);
            string[] splitMail = mailValue.Split(VcardConstants._argumentDelimiter);
            MailAddress mail;

            // Try to create mail address
            try
            {
                mail = new MailAddress(splitMail[0]);
            }
            catch (ArgumentException aex)
            {
                throw new InvalidDataException("E-mail address is invalid", aex);
            }

            // Populate the fields
            string[] _emailTypes = ["HOME"];
            string _emailAddress = mail.Address;
            EmailInfo _email = new(0, [], _emailTypes, _emailAddress);
            return _email;
        }

        internal static EmailInfo FromStringVcardThreeWithType(string value)
        {
            // Get the value
            string mailValue = value.Substring(VcardConstants._emailSpecifier.Length + 1);
            string[] splitMail = mailValue.Split(VcardConstants._argumentDelimiter);
            MailAddress mail;
            if (splitMail.Length < 2)
                throw new InvalidDataException("E-mail field must specify exactly two values (Type (must be prepended with TYPE=), and a valid e-mail address)");

            // Try to create mail address
            try
            {
                mail = new MailAddress(splitMail[1]);
            }
            catch (ArgumentException aex)
            {
                throw new InvalidDataException("E-mail address is invalid", aex);
            }

            // Populate the fields
            string[] _emailTypes = VcardParserTools.GetTypes(splitMail, "HOME", true);
            string _emailAddress = mail.Address;
            EmailInfo _email = new(0, [], _emailTypes, _emailAddress);
            return _email;
        }

        internal static EmailInfo FromStringVcardFour(string value, int altId)
        {
            // Get the value
            string mailValue = value.Substring(VcardConstants._emailSpecifier.Length + 1);
            string[] splitMail = mailValue.Split(VcardConstants._argumentDelimiter);
            MailAddress mail;

            // Try to create mail address
            try
            {
                mail = new MailAddress(splitMail[0]);
            }
            catch (ArgumentException aex)
            {
                throw new InvalidDataException("E-mail address is invalid", aex);
            }

            // Populate the fields
            string[] _emailTypes = ["HOME"];
            string _emailAddress = mail.Address;
            EmailInfo _email = new(altId, [], _emailTypes, _emailAddress);
            return _email;
        }

        internal static EmailInfo FromStringVcardFourWithType(string value, List<string> finalArgs, int altId)
        {
            // Get the value
            string mailValue = value.Substring(VcardConstants._emailSpecifier.Length + 1);
            string[] splitMail = mailValue.Split(VcardConstants._argumentDelimiter);
            MailAddress mail;
            if (splitMail.Length < 2)
                throw new InvalidDataException("E-mail field must specify exactly two values (Type (must be prepended with TYPE=), and a valid e-mail address)");

            // Try to create mail address
            try
            {
                mail = new MailAddress(splitMail[1]);
            }
            catch (ArgumentException aex)
            {
                throw new InvalidDataException("E-mail address is invalid", aex);
            }

            // Populate the fields
            string[] _emailTypes = VcardParserTools.GetTypes(splitMail, "HOME", true);
            string _emailAddress = mail.Address;
            EmailInfo _email = new(altId, [.. finalArgs], _emailTypes, _emailAddress);
            return _email;
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
