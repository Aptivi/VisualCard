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
    /// Contact nickname info
    /// </summary>
    [DebuggerDisplay("Nickname = {ContactNickname}")]
    public class NicknameInfo : IEquatable<NicknameInfo>
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
        /// The contact's nickname
        /// </summary>
        public string ContactNickname { get; }
        /// <summary>
        /// The contact's nickname types
        /// </summary>
        public string[] NicknameTypes { get; }

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
                source.AltId == target.AltId &&
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

        internal string ToStringVcardTwo()
        {
            throw new NotImplementedException();
        }

        internal string ToStringVcardThree()
        {
            return
                $"{VcardConstants._nicknameSpecifier};" +
                $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", NicknameTypes)}{VcardConstants._argumentDelimiter}" +
                $"{ContactNickname}";
        }

        internal string ToStringVcardFour()
        {
            bool installAltId = AltId >= 0 && AltArguments.Length > 0;
            return
                $"{VcardConstants._nicknameSpecifier};" +
                $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                $"{(installAltId ? string.Join(VcardConstants._fieldDelimiter.ToString(), AltArguments) + VcardConstants._fieldDelimiter : "")}" +
                $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", NicknameTypes)}{VcardConstants._argumentDelimiter}" +
                $"{ContactNickname}";
        }

        internal string ToStringVcardFive() =>
            ToStringVcardFour();

        internal static NicknameInfo FromStringVcardThree(string value)
        {
            // Get the value
            string nickValue = value.Substring(VcardConstants._nicknameSpecifier.Length + 1);

            // Populate the fields
            string[] _nicknameTypes = ["HOME"];
            string _nick = Regex.Unescape(nickValue);
            NicknameInfo _nickInstance = new(0, [], _nick, _nicknameTypes);
            return _nickInstance;
        }

        internal static NicknameInfo FromStringVcardThreeWithType(string value)
        {
            // Get the value
            string nickValue = value.Substring(VcardConstants._nicknameSpecifier.Length + 1);
            string[] splitNick = nickValue.Split(VcardConstants._argumentDelimiter);
            if (splitNick.Length < 2)
                throw new InvalidDataException("Nickname field must specify exactly two values (Type (must be prepended with TYPE=), and nickname)");

            // Populate the fields
            string[] _nicknameTypes = VcardParserTools.GetTypes(splitNick, "WORK", true);
            string _nick = Regex.Unescape(splitNick[1]);
            NicknameInfo _nickInstance = new(0, [], _nick, _nicknameTypes);
            return _nickInstance;
        }

        internal static NicknameInfo FromStringVcardFour(string value, int altId)
        {
            // Get the value
            string nickValue = value.Substring(VcardConstants._nicknameSpecifier.Length + 1);

            // Populate the fields
            string[] _nicknameTypes = ["HOME"];
            string _nick = Regex.Unescape(nickValue);
            NicknameInfo _nickInstance = new(altId, [], _nick, _nicknameTypes);
            return _nickInstance;
        }

        internal static NicknameInfo FromStringVcardFourWithType(string value, List<string> finalArgs, int altId)
        {
            // Get the value
            string nickValue = value.Substring(VcardConstants._nicknameSpecifier.Length + 1);
            string[] splitNick = nickValue.Split(VcardConstants._argumentDelimiter);
            if (splitNick.Length < 2)
                throw new InvalidDataException("Nickname field must specify exactly two values (Type (must be prepended with TYPE=), and nickname)");

            // Populate the fields
            string[] _nicknameTypes = VcardParserTools.GetTypes(splitNick, "WORK", true);
            string _nick = Regex.Unescape(splitNick[1]);
            NicknameInfo _nickInstance = new(altId, [.. finalArgs], _nick, _nicknameTypes);
            return _nickInstance;
        }

        internal static NicknameInfo FromStringVcardFive(string value, int altId) =>
            FromStringVcardFour(value, altId);

        internal static NicknameInfo FromStringVcardFiveWithType(string value, List<string> finalArgs, int altId) =>
            FromStringVcardFourWithType(value, finalArgs, altId);

        internal NicknameInfo() { }

        internal NicknameInfo(int altId, string[] altArguments, string contactNickname, string[] nicknameTypes)
        {
            AltId = altId;
            AltArguments = altArguments;
            ContactNickname = contactNickname;
            NicknameTypes = nicknameTypes;
        }
    }
}
