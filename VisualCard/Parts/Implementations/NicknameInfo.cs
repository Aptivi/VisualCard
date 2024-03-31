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
                    $"{(installAltId ? string.Join(VcardConstants._fieldDelimiter.ToString(), Arguments) + VcardConstants._fieldDelimiter : "")}" +
                    $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", ElementTypes)}{VcardConstants._argumentDelimiter}" +
                    $"{ContactNickname}";
            }
            else
            {
                return
                    $"{VcardConstants._nicknameSpecifier};" +
                    $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", ElementTypes)}{VcardConstants._argumentDelimiter}" +
                    $"{ContactNickname}";
            }
        }

        internal override BaseCardPartInfo FromStringVcardInternal(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;

            // Populate the fields
            string _nick = Regex.Unescape(value);
            NicknameInfo _nickInstance = new(altIdSupported ? altId : 0, finalArgs, elementTypes, valueType, _nick);
            return _nickInstance;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((NicknameInfo)obj);

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
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.ContactNickname == target.ContactNickname
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 536678633;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContactNickname);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(NicknameInfo left, NicknameInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(NicknameInfo left, NicknameInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCardPartInfo source, BaseCardPartInfo target) =>
            ((NicknameInfo)source) == ((NicknameInfo)target);

        internal NicknameInfo() { }

        internal NicknameInfo(int altId, string[] arguments, string[] elementTypes, string valueType, string contactNickname) :
            base(arguments, altId, elementTypes, valueType)
        {
            ContactNickname = contactNickname;
        }
    }
}
