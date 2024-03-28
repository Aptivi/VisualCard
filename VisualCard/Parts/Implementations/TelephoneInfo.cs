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

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact telephone number information
    /// </summary>
    [DebuggerDisplay("Telephone = {ContactPhoneNumber}")]
    public class TelephoneInfo : BaseCardPartInfo, IEquatable<TelephoneInfo>
    {
        /// <summary>
        /// The contact's phone types
        /// </summary>
        public string[] ContactPhoneTypes { get; }
        /// <summary>
        /// The contact's phone number
        /// </summary>
        public string ContactPhoneNumber { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, int altId, Version cardVersion, StreamReader cardContentReader) =>
            new TelephoneInfo().FromStringVcardInternal(value, altId, cardVersion, cardContentReader);

        internal static BaseCardPartInfo FromStringVcardWithTypeStatic(string value, string[] finalArgs, int altId, Version cardVersion, StreamReader cardContentReader) =>
            new TelephoneInfo().FromStringVcardWithTypeInternal(value, finalArgs, altId, cardVersion, cardContentReader);

        internal override string ToStringVcardInternal(Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;
            if (altIdSupported)
            {
                bool installAltId = AltId >= 0 && AltArguments.Length > 0;
                return
                    $"{VcardConstants._telephoneSpecifier};" +
                    $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                    $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", ContactPhoneTypes)}{VcardConstants._argumentDelimiter}" +
                    $"{ContactPhoneNumber}";
            }
            else
            {
                return
                    $"{VcardConstants._telephoneSpecifier};" +
                    $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", ContactPhoneTypes)}{VcardConstants._argumentDelimiter}" +
                    $"{ContactPhoneNumber}";
            }
        }

        internal override BaseCardPartInfo FromStringVcardInternal(string value, int altId, Version cardVersion, StreamReader cardContentReader)
        {
            // Get the value
            string telValue = value.Substring(VcardConstants._telephoneSpecifier.Length + 1);

            // Populate the fields
            return InstallInfo([telValue], altId, cardVersion);
        }

        internal override BaseCardPartInfo FromStringVcardWithTypeInternal(string value, string[] finalArgs, int altId, Version cardVersion, StreamReader cardContentReader)
        {
            // Get the value
            string telValue = value.Substring(VcardConstants._telephoneSpecifier.Length + 1);
            string[] splitTel = telValue.Split(VcardConstants._argumentDelimiter);
            if (splitTel.Length < 2)
                throw new InvalidDataException("Telephone field must specify exactly two values (Type (optionally prepended with TYPE=), and phone number)");

            // Populate the fields
            return InstallInfo(splitTel, finalArgs, altId, cardVersion);
        }

        private TelephoneInfo InstallInfo(string[] splitTel, int altId, Version cardVersion) =>
            InstallInfo(splitTel, [], altId, cardVersion);

        private TelephoneInfo InstallInfo(string[] splitTel, string[] finalArgs, int altId, Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;
            bool installType = splitTel.Length > 1;
            bool specifierRequired = cardVersion.Major >= 3;

            // Populate the fields
            string[] _telephoneTypes = installType ? VcardParserTools.GetTypes(splitTel, "CELL", specifierRequired) : ["CELL"];
            string _telephoneNumber = Regex.Unescape(installType ? splitTel[1] : splitTel[0]);
            TelephoneInfo _telephone = new(altIdSupported ? altId : 0, altIdSupported ? finalArgs : [], _telephoneTypes, _telephoneNumber);
            return _telephone;
        }

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

        /// <inheritdoc/>
        public static bool operator ==(TelephoneInfo left, TelephoneInfo right) =>
            EqualityComparer<TelephoneInfo>.Default.Equals(left, right);

        /// <inheritdoc/>
        public static bool operator !=(TelephoneInfo left, TelephoneInfo right) =>
            !(left == right);

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
