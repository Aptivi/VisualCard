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
using VisualCard.Parsers.Arguments;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact IMPP info
    /// </summary>
    [DebuggerDisplay("IMPP info = {ContactIMPP}")]
    public class ImppInfo : BaseCardPartInfo, IEquatable<ImppInfo>
    {
        /// <summary>
        /// The contact's IMPP information, such as SIP and XMPP
        /// </summary>
        public string? ContactIMPP { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, ArgumentInfo[] finalArgs, int altId, string[] elementTypes, string group, string valueType, Version cardVersion) =>
            new ImppInfo().FromStringVcardInternal(value, finalArgs, altId, elementTypes, group, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion) =>
            ContactIMPP ?? "";

        internal override BaseCardPartInfo FromStringVcardInternal(string value, ArgumentInfo[] finalArgs, int altId, string[] elementTypes, string group, string valueType, Version cardVersion)
        {
            // Populate the fields
            string _impp = Regex.Unescape(value);
            ImppInfo _imppInstance = new(altId, finalArgs, elementTypes, valueType, group, _impp);
            return _imppInstance;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((ImppInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="ImppInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(ImppInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="ImppInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="ImppInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(ImppInfo source, ImppInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.ContactIMPP == target.ContactIMPP
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 175591591;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(ContactIMPP);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(ImppInfo left, ImppInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(ImppInfo left, ImppInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCardPartInfo source, BaseCardPartInfo target) =>
            ((ImppInfo)source) == ((ImppInfo)target);

        internal ImppInfo() { }

        internal ImppInfo(int altId, ArgumentInfo[] arguments, string[] elementTypes, string valueType, string group, string contactImpp) :
            base(arguments, altId, elementTypes, valueType, group)
        {
            ContactIMPP = contactImpp;
        }
    }
}
