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
using System.Net.Mail;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact e-mail information
    /// </summary>
    [DebuggerDisplay("E-mail = {ContactEmailAddress}")]
    public class EmailInfo : BaseCardPartInfo, IEquatable<EmailInfo>
    {
        /// <summary>
        /// The contact's email address
        /// </summary>
        public string? ContactEmailAddress { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion) =>
            new EmailInfo().FromStringVcardInternal(value, finalArgs, altId, elementTypes, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion) =>
            ContactEmailAddress ?? "";

        internal override BaseCardPartInfo FromStringVcardInternal(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion)
        {
            MailAddress mail;

            // Try to create mail address
            try
            {
                mail = new MailAddress(value);
            }
            catch (ArgumentException aex)
            {
                throw new InvalidDataException("E-mail address is invalid", aex);
            }

            // Populate the fields
            string _emailAddress = mail.Address;
            EmailInfo _address = new(altId, finalArgs, elementTypes, valueType, _emailAddress);
            return _address;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((EmailInfo)obj);

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
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.ContactEmailAddress == target.ContactEmailAddress
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -1504605771;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(ContactEmailAddress);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(EmailInfo left, EmailInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(EmailInfo left, EmailInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCardPartInfo source, BaseCardPartInfo target) =>
            ((EmailInfo)source) == ((EmailInfo)target);

        internal EmailInfo() { }

        internal EmailInfo(int altId, string[] arguments, string[] elementTypes, string valueType, string contactEmailAddress) :
            base(arguments, altId, elementTypes, valueType)
        {
            ContactEmailAddress = contactEmailAddress;
        }
    }
}
