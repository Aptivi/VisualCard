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

        internal string ToStringVcardFive() =>
            ToStringVcardFour();

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

        internal static EmailInfo FromStringVcardFive(string value, int altId) =>
            FromStringVcardFour(value, altId);

        internal static EmailInfo FromStringVcardFiveWithType(string value, List<string> finalArgs, int altId) =>
            FromStringVcardFourWithType(value, finalArgs, altId);

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
