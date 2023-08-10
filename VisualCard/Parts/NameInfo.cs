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
    public class NameInfo : IEquatable<NameInfo>
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
        /// The contact's first name
        /// </summary>
        public string ContactFirstName { get; }
        /// <summary>
        /// The contact's last name
        /// </summary>
        public string ContactLastName { get; }
        /// <summary>
        /// The contact's alternative names
        /// </summary>
        public string[] AltNames { get; }
        /// <summary>
        /// The contact's prefixes
        /// </summary>
        public string[] Prefixes { get; }
        /// <summary>
        /// The contact's suffixes
        /// </summary>
        public string[] Suffixes { get; }

        public override bool Equals(object obj) =>
            base.Equals(obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="NameInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(NameInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="NameInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="NameInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(NameInfo source, NameInfo target)
        {
            // We can't perform this operation on null.
            if (source is null)
                return false;

            // Check all the properties
            return
                source.AltArguments.SequenceEqual(target.AltArguments) &&
                source.AltNames.SequenceEqual(target.AltNames) &&
                source.Prefixes.SequenceEqual(target.Prefixes) &&
                source.Suffixes.SequenceEqual(target.Suffixes) &&
                source.AltId == target.AltId &&
                source.ContactFirstName == target.ContactFirstName &&
                source.ContactLastName == target.ContactLastName
            ;
        }

        public override int GetHashCode()
        {
            int hashCode = 357851718;
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AltArguments);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContactFirstName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContactLastName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AltNames);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(Prefixes);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(Suffixes);
            return hashCode;
        }

        internal string ToStringVcardTwo()
        {
            string altNamesStr = string.Join(VcardConstants._valueDelimiter.ToString(), AltNames);
            string prefixesStr = string.Join(VcardConstants._valueDelimiter.ToString(), Prefixes);
            string suffixesStr = string.Join(VcardConstants._valueDelimiter.ToString(), Suffixes);
            return
                $"{VcardConstants._nameSpecifier}" +
                $"{ContactLastName}{VcardConstants._fieldDelimiter}" +
                $"{ContactFirstName}{VcardConstants._fieldDelimiter}" +
                $"{altNamesStr}{VcardConstants._fieldDelimiter}" +
                $"{prefixesStr}{VcardConstants._fieldDelimiter}" +
                $"{suffixesStr}{VcardConstants._fieldDelimiter}";
        }

        internal string ToStringVcardThree()
        {
            string altNamesStr = string.Join(VcardConstants._valueDelimiter.ToString(), AltNames);
            string prefixesStr = string.Join(VcardConstants._valueDelimiter.ToString(), Prefixes);
            string suffixesStr = string.Join(VcardConstants._valueDelimiter.ToString(), Suffixes);
            return
                $"{VcardConstants._nameSpecifier}" +
                $"{ContactLastName}{VcardConstants._fieldDelimiter}" +
                $"{ContactFirstName}{VcardConstants._fieldDelimiter}" +
                $"{altNamesStr}{VcardConstants._fieldDelimiter}" +
                $"{prefixesStr}{VcardConstants._fieldDelimiter}" +
                $"{suffixesStr}{VcardConstants._fieldDelimiter}";
        }

        internal string ToStringVcardFour()
        {
            bool installAltId = AltId >= 0 && AltArguments.Length > 0;
            string altNamesStr = string.Join(VcardConstants._valueDelimiter.ToString(), AltNames);
            string prefixesStr = string.Join(VcardConstants._valueDelimiter.ToString(), Prefixes);
            string suffixesStr = string.Join(VcardConstants._valueDelimiter.ToString(), Suffixes);
            return
                $"{(installAltId ? VcardConstants._nameSpecifierWithType : VcardConstants._nameSpecifier)}" +
                $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                $"{(installAltId ? string.Join(VcardConstants._fieldDelimiter.ToString(), AltArguments) + VcardConstants._argumentDelimiter : "")}" +
                $"{ContactLastName}{VcardConstants._fieldDelimiter}" +
                $"{ContactFirstName}{VcardConstants._fieldDelimiter}" +
                $"{altNamesStr}{VcardConstants._fieldDelimiter}" +
                $"{prefixesStr}{VcardConstants._fieldDelimiter}" +
                $"{suffixesStr}{VcardConstants._fieldDelimiter}";
        }

        internal NameInfo() { }

        internal NameInfo(int altId, string[] altArguments, string contactFirstName, string contactLastName, string[] altNames, string[] prefixes, string[] suffixes)
        {
            AltId = altId;
            AltArguments = altArguments;
            ContactFirstName = contactFirstName;
            ContactLastName = contactLastName;
            AltNames = altNames;
            Prefixes = prefixes;
            Suffixes = suffixes;
        }
    }
}
