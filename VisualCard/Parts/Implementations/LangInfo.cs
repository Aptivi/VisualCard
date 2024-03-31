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
using VisualCard.Parsers;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact language information
    /// </summary>
    [DebuggerDisplay("Language = {ContactLang}")]
    public class LangInfo : BaseCardPartInfo, IEquatable<LangInfo>
    {
        /// <summary>
        /// The contact's preference order
        /// </summary>
        public int ContactLangPreference { get; }
        /// <summary>
        /// The contact's language code
        /// </summary>
        public string ContactLang { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion) =>
            new LangInfo().FromStringVcardInternal(value, finalArgs, altId, elementTypes, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;
            if (altIdSupported)
            {
                bool installAltId = AltId >= 0 && Arguments.Length > 0;
                return
                    $"{VcardConstants._langSpecifier};" +
                    $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                    $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", ElementTypes)}{VcardConstants._fieldDelimiter}" +
                    $"{VcardConstants._prefArgumentSpecifier}{ContactLangPreference}{VcardConstants._argumentDelimiter}" +
                    $"{ContactLang}";
            }
            else
            {
                return
                    $"{VcardConstants._langSpecifier};" +
                    $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", ElementTypes)}{VcardConstants._fieldDelimiter}" +
                    $"{VcardConstants._prefArgumentSpecifier}{ContactLangPreference}{VcardConstants._argumentDelimiter}" +
                    $"{ContactLang}";
            }
        }

        internal override BaseCardPartInfo FromStringVcardInternal(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion)
        {
            // Populate the fields
            LangInfo _lang = new(altId, finalArgs, elementTypes, valueType, value);
            return _lang;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            base.Equals(obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="LangInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(LangInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="LangInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="LangInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(LangInfo source, LangInfo target)
        {
            // We can't perform this operation on null.
            if (source is null)
                return false;

            // Check all the properties
            return
                base.Equals(source, target) &&
                source.ContactLang == target.ContactLang
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -2101786561;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + ContactLangPreference.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContactLang);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(LangInfo left, LangInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(LangInfo left, LangInfo right) =>
            !(left == right);

        internal LangInfo() { }

        internal LangInfo(int altId, string[] arguments, string[] elementTypes, string valueType, string contactLangCode) :
            base(arguments, altId, elementTypes, valueType)
        {
            ContactLang = contactLangCode;
        }
    }
}
