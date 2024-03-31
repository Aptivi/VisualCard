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
    /// Contact nickname info
    /// </summary>
    [DebuggerDisplay("Nickname = {ContactNickname}")]
    public class NicknameInfo : BaseCardPartInfo, IEquatable<NicknameInfo>
    {
        /// <summary>
        /// The contact's nickname
        /// </summary>
        public string ContactNickname { get; }
        /// <summary>
        /// The contact's nickname types
        /// </summary>
        public string[] NicknameTypes { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion) =>
            new NicknameInfo().FromStringVcardInternal(value, finalArgs, altId, elementTypes, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;
            if (altIdSupported)
            {
                bool installAltId = AltId >= 0 && Arguments.Length > 0;
                return
                    $"{VcardConstants._nicknameSpecifier};" +
                    $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                    $"{(installAltId ? string.Join(VcardConstants._fieldDelimiter.ToString(), AltArguments) + VcardConstants._fieldDelimiter : "")}" +
                    $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", NicknameTypes)}{VcardConstants._argumentDelimiter}" +
                    $"{ContactNickname}";
            }
            else
            {
                return
                    $"{VcardConstants._nicknameSpecifier};" +
                    $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", NicknameTypes)}{VcardConstants._argumentDelimiter}" +
                    $"{ContactNickname}";
            }
        }

        internal override BaseCardPartInfo FromStringVcardInternal(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion)
        {
            // Get the value
            string nickValue = value.Substring(VcardConstants._nicknameSpecifier.Length + 1);

            // Populate the fields
            return InstallInfo([nickValue], altId, cardVersion);
        }

        internal override BaseCardPartInfo FromStringVcardWithTypeInternal(string value, string[] finalArgs, int altId, Version cardVersion)
        {
            // Get the value
            string nickValue = value.Substring(VcardConstants._nicknameSpecifier.Length + 1);
            string[] splitNick = nickValue.Split(VcardConstants._argumentDelimiter);
            if (splitNick.Length < 2)
                throw new InvalidDataException("Nickname field must specify exactly two values (Type (must be prepended with TYPE=), and nickname)");

            // Populate the fields
            return InstallInfo(splitNick, finalArgs, altId, cardVersion);
        }

        private NicknameInfo InstallInfo(string[] splitNick, int altId, Version cardVersion) =>
            InstallInfo(splitNick, [], altId, cardVersion);

        private NicknameInfo InstallInfo(string[] splitNick, string[] finalArgs, int altId, Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;
            bool installType = splitNick.Length > 1;
            bool specifierRequired = cardVersion.Major >= 3;

            // Populate the fields
            string[] _nicknameTypes = installType ? VcardParserTools.GetTypes(splitNick, "WORK", specifierRequired) : ["HOME"];
            string _nick = Regex.Unescape(installType ? splitNick[1] : splitNick[0]);
            NicknameInfo _nickInstance = new(altIdSupported ? altId : 0, finalArgs, elementTypes, valueType, _nick, _nicknameTypes);
            return _nickInstance;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            base.Equals(obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="NicknameInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(NicknameInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="NicknameInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="NicknameInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(NicknameInfo source, NicknameInfo target)
        {
            // We can't perform this operation on null.
            if (source is null)
                return false;

            // Check all the properties
            return
                source.AltArguments.SequenceEqual(target.AltArguments) &&
                source.NicknameTypes.SequenceEqual(target.NicknameTypes) &&
                base.Equals(source, target) &&
                source.ContactNickname == target.ContactNickname
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -1183179154;
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AltArguments);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContactNickname);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(NicknameTypes);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(NicknameInfo left, NicknameInfo right) =>
            EqualityComparer<NicknameInfo>.Default.Equals(left, right);

        /// <inheritdoc/>
        public static bool operator !=(NicknameInfo left, NicknameInfo right) =>
            !(left == right);

        internal NicknameInfo() { }

        internal NicknameInfo(int altId, string[] arguments, string[] elementTypes, string valueType, string contactNickname, string[] nicknameTypes)
        {
            AltId = altId;
            Arguments = arguments;
            ContactNickname = contactNickname;
            NicknameTypes = nicknameTypes;
        }
    }
}
