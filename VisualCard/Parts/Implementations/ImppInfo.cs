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
    /// Contact IMPP info
    /// </summary>
    [DebuggerDisplay("IMPP info = {ContactIMPP}")]
    public class ImppInfo : BaseCardPartInfo, IEquatable<ImppInfo>
    {
        /// <summary>
        /// The contact's IMPP information, such as SIP and XMPP
        /// </summary>
        public string ContactIMPP { get; }
        /// <summary>
        /// The contact's IMPP info types
        /// </summary>
        public string[] ImppTypes { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, int altId, Version cardVersion, StreamReader cardContentReader) =>
            new ImppInfo().FromStringVcardInternal(value, altId, cardVersion, cardContentReader);

        internal static BaseCardPartInfo FromStringVcardWithTypeStatic(string value, string[] finalArgs, int altId, Version cardVersion, StreamReader cardContentReader) =>
            new ImppInfo().FromStringVcardWithTypeInternal(value, finalArgs, altId, cardVersion, cardContentReader);

        internal override string ToStringVcardInternal(Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;
            if (altIdSupported)
            {
                bool installAltId = AltId >= 0 && AltArguments.Length > 0;
                bool installType = ImppTypes.Length > 0 && ImppTypes[0].ToUpper() != "HOME";
                return
                    $"{VcardConstants._imppSpecifier}{(installType || installAltId ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter)}" +
                    $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + (installType ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter) : "")}" +
                    $"{(installType ? $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", ImppTypes)}{VcardConstants._argumentDelimiter}" : "")}" +
                    $"{ContactIMPP}";
            }
            else
            {
                bool installType = ImppTypes.Length > 0 && ImppTypes[0].ToUpper() != "HOME";
                return
                    $"{VcardConstants._imppSpecifier}{(installType ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter)}" +
                    $"{(installType ? $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", ImppTypes)}{VcardConstants._argumentDelimiter}" : "")}" +
                    $"{ContactIMPP}";
            }
        }

        internal override BaseCardPartInfo FromStringVcardInternal(string value, int altId, Version cardVersion, StreamReader cardContentReader)
        {
            // Get the value
            string imppValue = value.Substring(VcardConstants._imppSpecifier.Length + 1);
            string[] _imppTypes = ["HOME"];

            // Populate the fields
            return InstallInfo(_imppTypes, imppValue, altId, cardVersion);
        }

        internal override BaseCardPartInfo FromStringVcardWithTypeInternal(string value, string[] finalArgs, int altId, Version cardVersion, StreamReader cardContentReader)
        {
            bool specifierRequired = cardVersion.Major >= 3;

            // Get the value
            string imppValue = value.Substring(VcardConstants._imppSpecifier.Length + 1);
            string[] splitImpp = imppValue.Split(VcardConstants._argumentDelimiter);
            if (splitImpp.Length < 2)
                throw new InvalidDataException("IMPP information field must specify exactly two values (Type (must be prepended with TYPE=), and impp)");

            // Install the values
            string[] _imppTypes = VcardParserTools.GetTypes(splitImpp, "SIP", specifierRequired);

            // Populate the fields
            return InstallInfo(_imppTypes, imppValue, finalArgs, altId, cardVersion);
        }

        private ImppInfo InstallInfo(string[] types, string imppValue, int altId, Version cardVersion) =>
            InstallInfo(types, imppValue, [], altId, cardVersion);

        private ImppInfo InstallInfo(string[] types, string imppValue, string[] finalArgs, int altId, Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;
            string _impp =
                imppValue.Contains(':') ?
                Regex.Unescape(imppValue.Substring(imppValue.IndexOf(":") + 1)) :
                Regex.Unescape(imppValue);

            ImppInfo _imppInstance = new(altIdSupported ? altId : 0, altIdSupported ? finalArgs : [], _impp, types);
            return _imppInstance;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            base.Equals(obj);

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
            if (source is null)
                return false;

            // Check all the properties
            return
                source.AltArguments.SequenceEqual(target.AltArguments) &&
                source.ImppTypes.SequenceEqual(target.ImppTypes) &&
                source.AltId == target.AltId &&
                source.ContactIMPP == target.ContactIMPP
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -700274766;
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AltArguments);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContactIMPP);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(ImppTypes);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(ImppInfo left, ImppInfo right) =>
            EqualityComparer<ImppInfo>.Default.Equals(left, right);

        /// <inheritdoc/>
        public static bool operator !=(ImppInfo left, ImppInfo right) =>
            !(left == right);

        internal ImppInfo() { }

        internal ImppInfo(int altId, string[] altArguments, string contactImpp, string[] imppTypes)
        {
            AltId = altId;
            AltArguments = altArguments;
            ContactIMPP = contactImpp;
            ImppTypes = imppTypes;
        }
    }
}
