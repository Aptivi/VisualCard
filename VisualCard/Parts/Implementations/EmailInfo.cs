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

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact e-mail information
    /// </summary>
    [DebuggerDisplay("E-mail = {ContactEmailAddress}")]
    public class EmailInfo : BaseCardPartInfo, IEquatable<EmailInfo>
    {
        /// <summary>
        /// The contact's email types
        /// </summary>
        public string[] ContactEmailTypes { get; }
        /// <summary>
        /// The contact's email address
        /// </summary>
        public string ContactEmailAddress { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, int altId, Version cardVersion) =>
            new EmailInfo().FromStringVcardInternal(value, altId, cardVersion);

        internal static BaseCardPartInfo FromStringVcardWithTypeStatic(string value, string[] finalArgs, int altId, Version cardVersion) =>
            new EmailInfo().FromStringVcardWithTypeInternal(value, finalArgs, altId, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;
            if (altIdSupported)
            {
                bool installAltId = AltId >= 0 && AltArguments.Length > 0;
                return
                    $"{VcardConstants._emailSpecifier};" +
                    $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                    $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", ContactEmailTypes)}{VcardConstants._argumentDelimiter}" +
                    $"{ContactEmailAddress}";
            }
            else
            {
                return
                    $"{VcardConstants._emailSpecifier};" +
                    $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", ContactEmailTypes)}{VcardConstants._argumentDelimiter}" +
                    $"{ContactEmailAddress}";
            }
        }

        internal override BaseCardPartInfo FromStringVcardInternal(string value, int altId, Version cardVersion)
        {
            // Get the value
            string mailValue = value.Substring(VcardConstants._emailSpecifier.Length + 1);
            string[] splitMail = mailValue.Split(VcardConstants._argumentDelimiter);

            // Populate the fields
            return InstallInfo(splitMail, altId, cardVersion);
        }

        internal override BaseCardPartInfo FromStringVcardWithTypeInternal(string value, string[] finalArgs, int altId, Version cardVersion)
        {
            // Get the value
            string mailValue = value.Substring(VcardConstants._emailSpecifier.Length + 1);
            string[] splitMail = mailValue.Split(VcardConstants._argumentDelimiter);
            if (splitMail.Length < 2)
                throw new InvalidDataException("E-mail field must specify exactly two values (Type (must be prepended with TYPE=), and a valid e-mail address)");

            // Populate the fields
            return InstallInfo(splitMail, finalArgs, altId, cardVersion);
        }

        private EmailInfo InstallInfo(string[] splitMail, int altId, Version cardVersion) =>
            InstallInfo(splitMail, [], altId, cardVersion);

        private EmailInfo InstallInfo(string[] splitMail, string[] finalArgs, int altId, Version cardVersion)
        {
            MailAddress mail;
            bool altIdSupported = cardVersion.Major >= 4;
            bool installType = splitMail.Length > 1;
            bool specifierRequired = cardVersion.Major >= 3;

            // Try to create mail address
            try
            {
                mail = new MailAddress(installType ? splitMail[1] : splitMail[0]);
            }
            catch (ArgumentException aex)
            {
                throw new InvalidDataException("E-mail address is invalid", aex);
            }

            // Populate the fields
            string[] _emailTypes = installType ? VcardParserTools.GetTypes(splitMail, "HOME", specifierRequired) : ["HOME"];
            string _emailAddress = mail.Address;
            EmailInfo _address = new(altIdSupported ? altId : 0, altIdSupported ? finalArgs : [], _emailTypes, _emailAddress);
            return _address;
        }

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

        /// <inheritdoc/>
        public static bool operator ==(EmailInfo left, EmailInfo right) =>
            EqualityComparer<EmailInfo>.Default.Equals(left, right);

        /// <inheritdoc/>
        public static bool operator !=(EmailInfo left, EmailInfo right) =>
            !(left == right);

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
