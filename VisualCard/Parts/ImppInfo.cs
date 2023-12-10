//
// MIT License
//
// Copyright (c) 2021-2024 Aptivi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
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
    /// Contact IMPP info
    /// </summary>
    [DebuggerDisplay("IMPP info = {ContactIMPP}")]
    public class ImppInfo : IEquatable<ImppInfo>
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
        /// The contact's IMPP information, such as SIP and XMPP
        /// </summary>
        public string ContactIMPP { get; }
        /// <summary>
        /// The contact's IMPP info types
        /// </summary>
        public string[] ImppTypes { get; }

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

        internal string ToStringVcardTwo()
        {
            bool installType = ImppTypes.Length > 0 && ImppTypes[0].ToUpper() != "HOME";
            return
                $"{VcardConstants._imppSpecifier}{(installType ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter)}" +
                $"{(installType ? $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", ImppTypes)}{VcardConstants._argumentDelimiter}" : "")}" +
                $"{ContactIMPP}";
        }

        internal string ToStringVcardThree()
        {
            bool installType = ImppTypes.Length > 0 && ImppTypes[0].ToUpper() != "HOME";
            return
                $"{VcardConstants._imppSpecifier}{(installType ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter)}" +
                $"{(installType ? $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", ImppTypes)}{VcardConstants._argumentDelimiter}" : "")}" +
                $"{ContactIMPP}";
        }

        internal string ToStringVcardFour()
        {
            bool installAltId = AltId >= 0 && AltArguments.Length > 0;
            bool installType = ImppTypes.Length > 0 && ImppTypes[0].ToUpper() != "HOME";
            return
                $"{VcardConstants._imppSpecifier}{(installType || installAltId ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter)}" +
                $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + (installType ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter) : "")}" +
                $"{(installType ? $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", ImppTypes)}{VcardConstants._argumentDelimiter}" : "")}" +
                $"{ContactIMPP}";
        }

        internal static ImppInfo FromStringVcardTwo(string value)
        {
            string imppValue = value.Substring(VcardConstants._imppSpecifier.Length + 1);
            string[] _imppTypes = ["HOME"];
            string _impp = Regex.Unescape(imppValue);
            ImppInfo _imppInstance = new(0, [], _impp, _imppTypes);
            return _imppInstance;
        }

        internal static ImppInfo FromStringVcardTwoWithType(string value)
        {
            string imppValue = value.Substring(VcardConstants._imppSpecifier.Length + 1);
            string[] splitImpp = imppValue.Split(VcardConstants._argumentDelimiter);
            if (splitImpp.Length < 2)
                throw new InvalidDataException("IMPP information field must specify exactly two values (Type (must be prepended with TYPE=), and impp)");

            // Install the values
            string[] _imppTypes = VcardParserTools.GetTypes(splitImpp, "SIP", false);
            string _impp = Regex.Unescape(imppValue.Substring(imppValue.IndexOf(":") + 1));
            ImppInfo _imppInstance = new(0, [], _impp, _imppTypes);
            return _imppInstance;
        }

        internal static ImppInfo FromStringVcardThree(string value)
        {
            // Get the value
            string imppValue = value.Substring(VcardConstants._imppSpecifier.Length + 1);

            // Populate the fields
            string[] _imppTypes = ["HOME"];
            string _impp = Regex.Unescape(imppValue);
            ImppInfo _imppInstance = new(0, [], _impp, _imppTypes);
            return _imppInstance;
        }

        internal static ImppInfo FromStringVcardThreeWithType(string value)
        {
            // Get the value
            string imppValue = value.Substring(VcardConstants._imppSpecifier.Length + 1);
            string[] splitImpp = imppValue.Split(VcardConstants._argumentDelimiter);
            if (splitImpp.Length < 2)
                throw new InvalidDataException("IMPP information field must specify exactly two values (Type (must be prepended with TYPE=), and impp)");

            // Install the values
            string[] _imppTypes = VcardParserTools.GetTypes(splitImpp, "SIP", true);
            string _impp = Regex.Unescape(imppValue.Substring(imppValue.IndexOf(":") + 1));
            ImppInfo _imppInstance = new(0, [], _impp, _imppTypes);
            return _imppInstance;
        }

        internal static ImppInfo FromStringVcardFour(string value, int altId)
        {
            // Get the value
            string imppValue = value.Substring(VcardConstants._imppSpecifier.Length + 1);

            // Populate the fields
            string[] _imppTypes = ["HOME"];
            string _impp = Regex.Unescape(imppValue);
            ImppInfo _imppInstance = new(altId, [], _impp, _imppTypes);
            return _imppInstance;
        }

        internal static ImppInfo FromStringVcardFourWithType(string value, List<string> finalArgs, int altId)
        {
            // Get the value
            string imppValue = value.Substring(VcardConstants._imppSpecifier.Length + 1);
            string[] splitImpp = imppValue.Split(VcardConstants._argumentDelimiter);
            if (splitImpp.Length < 2)
                throw new InvalidDataException("IMPP information field must specify exactly two values (Type (must be prepended with TYPE=), and impp)");

            // Install the values
            string[] _imppTypes = VcardParserTools.GetTypes(splitImpp, "SIP", true);
            string _impp = Regex.Unescape(imppValue.Substring(imppValue.IndexOf(":") + 1));
            ImppInfo _imppInstance = new(altId, [.. finalArgs], _impp, _imppTypes);
            return _imppInstance;
        }

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
