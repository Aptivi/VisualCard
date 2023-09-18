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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VisualCard.Parsers;

namespace VisualCard.Parts
{
    /// <summary>
    /// Contact role info
    /// </summary>
    public class RoleInfo : IEquatable<RoleInfo>
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
        /// The contact's role
        /// </summary>
        public string ContactRole { get; }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            base.Equals(obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="RoleInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(RoleInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="RoleInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="RoleInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(RoleInfo source, RoleInfo target)
        {
            // We can't perform this operation on null.
            if (source is null)
                return false;

            // Check all the properties
            return
                source.AltArguments.SequenceEqual(target.AltArguments) &&
                source.AltId == target.AltId &&
                source.ContactRole == target.ContactRole
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -1215418912;
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AltArguments);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContactRole);
            return hashCode;
        }

        internal string ToStringVcardTwo()
        {
            return
                $"{VcardConstants._roleSpecifier};" +
                $"{ContactRole}";
        }

        internal string ToStringVcardThree()
        {
            return
                $"{VcardConstants._roleSpecifier};" +
                $"{ContactRole}";
        }

        internal string ToStringVcardFour()
        {
            bool installAltId = AltId >= 0 && AltArguments.Length > 0;
            return
                $"{VcardConstants._roleSpecifier}{(installAltId ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter)}" +
                $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                $"{(installAltId ? string.Join(VcardConstants._fieldDelimiter.ToString(), AltArguments) + VcardConstants._argumentDelimiter : "")}" +
                $"{ContactRole}";
        }

        internal static RoleInfo FromStringVcardTwo(string value)
        {
            // Get the value
            string roleValue = value.Substring(VcardConstants._roleSpecifier.Length + 1);

            // Populate the fields
            RoleInfo _role = new(0, Array.Empty<string>(), roleValue);
            return _role;
        }

        internal static RoleInfo FromStringVcardThree(string value)
        {
            // Get the value
            string roleValue = value.Substring(VcardConstants._roleSpecifier.Length + 1);

            // Populate the fields
            RoleInfo _role = new(0, Array.Empty<string>(), roleValue);
            return _role;
        }

        internal static RoleInfo FromStringVcardFour(string value, int altId)
        {
            // Get the value
            string roleValue = value.Substring(VcardConstants._roleSpecifier.Length + 1);

            // Populate the fields
            RoleInfo _role = new(altId, Array.Empty<string>(), roleValue);
            return _role;
        }

        internal static RoleInfo FromStringVcardFourWithType(string value, List<string> finalArgs, int altId)
        {
            // Get the value
            string roleValue = value.Substring(VcardConstants._roleSpecifier.Length + 1);

            // Populate the fields
            RoleInfo _role = new(altId, finalArgs.ToArray(), roleValue);
            return _role;
        }

        internal RoleInfo() { }

        internal RoleInfo(int altId, string[] altArguments, string contactRole)
        {
            AltId = altId;
            AltArguments = altArguments;
            ContactRole = contactRole;
        }
    }
}
