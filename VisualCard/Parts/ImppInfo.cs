/*
 * MIT License
 *
 * Copyright (c) 2021-2022 Aptivi
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using VisualCard.Parsers;

namespace VisualCard.Parts
{
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
            return
                $"{VcardConstants._imppSpecifierWithType}" +
                $"TYPE={string.Join(",", ImppTypes)}{VcardConstants._argumentDelimiter}" +
                $"{ContactIMPP}";
        }

        internal string ToStringVcardThree()
        {
            return
                $"{VcardConstants._imppSpecifierWithType}" +
                $"TYPE={string.Join(",", ImppTypes)}{VcardConstants._argumentDelimiter}" +
                $"{ContactIMPP}";
        }

        internal string ToStringVcardFour()
        {
            bool installAltId = AltId >= 0 && AltArguments.Length > 0;
            return
                $"{(installAltId ? VcardConstants._imppSpecifierWithType : VcardConstants._imppSpecifier)}" +
                $"{(installAltId ? "ALTID=" + AltId + VcardConstants._fieldDelimiter : "")}" +
                $"{(installAltId ? string.Join(VcardConstants._fieldDelimiter.ToString(), AltArguments) + VcardConstants._argumentDelimiter : "")}" +
                $"{ContactIMPP}";
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
