//
// VisualCard  Copyright (C) 2021-2025  Aptivi
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
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Name information
    /// </summary>
    [DebuggerDisplay("First name = {ContactFirstName}, Last name = {ContactLastName}")]
    public class NameInfo : BaseCardPartInfo, IEquatable<NameInfo>
    {
        /// <summary>
        /// The contact's first name
        /// </summary>
        public string? ContactFirstName { get; set; }
        /// <summary>
        /// The contact's last name
        /// </summary>
        public string? ContactLastName { get; set; }
        /// <summary>
        /// The contact's alternative names
        /// </summary>
        public string[]? AltNames { get; set; }
        /// <summary>
        /// The contact's prefixes
        /// </summary>
        public string[]? Prefixes { get; set; }
        /// <summary>
        /// The contact's suffixes
        /// </summary>
        public string[]? Suffixes { get; set; }

        internal static BaseCardPartInfo FromStringStatic(string value, PropertyInfo property, int altId, string[] elementTypes, string group, string valueType, Version cardVersion) =>
            (BaseCardPartInfo)new NameInfo().FromStringInternal(value, property, altId, elementTypes, group, valueType, cardVersion);

        internal override string ToStringInternal(Version cardVersion)
        {
            string altNamesStr = string.Join(VcardConstants._valueDelimiter.ToString(), AltNames);
            string prefixesStr = string.Join(VcardConstants._valueDelimiter.ToString(), Prefixes);
            string suffixesStr = string.Join(VcardConstants._valueDelimiter.ToString(), Suffixes);
            return
                $"{ContactLastName}{VcardConstants._fieldDelimiter}" +
                $"{ContactFirstName}{VcardConstants._fieldDelimiter}" +
                $"{altNamesStr}{VcardConstants._fieldDelimiter}" +
                $"{prefixesStr}{VcardConstants._fieldDelimiter}" +
                $"{suffixesStr}";
        }

        internal override BasePartInfo FromStringInternal(string value, PropertyInfo property, int altId, string[] elementTypes, string group, string valueType, Version cardVersion)
        {
            string[] splitName = value.Split(VcardConstants._fieldDelimiter);
            if (splitName.Length < 2)
                throw new InvalidDataException("Name field must specify the first two or more of the five values (Last name, first name, alt names, prefixes, and suffixes)");

            // Populate fields
            string _lastName = Regex.Unescape(splitName[0]);
            string _firstName = Regex.Unescape(splitName[1]);
            string[] _altNames = splitName.Length >= 3 ? Regex.Unescape(splitName[2]).Split(new char[] { VcardConstants._valueDelimiter }, StringSplitOptions.RemoveEmptyEntries) : [];
            string[] _prefixes = splitName.Length >= 4 ? Regex.Unescape(splitName[3]).Split(new char[] { VcardConstants._valueDelimiter }, StringSplitOptions.RemoveEmptyEntries) : [];
            string[] _suffixes = splitName.Length >= 5 ? Regex.Unescape(splitName[4]).Split(new char[] { VcardConstants._valueDelimiter }, StringSplitOptions.RemoveEmptyEntries) : [];
            NameInfo _name = new(altId, property, elementTypes, group, valueType, _firstName, _lastName, _altNames, _prefixes, _suffixes);
            return _name;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((NameInfo)obj);

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
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.AltNames.SequenceEqual(target.AltNames) &&
                source.Prefixes.SequenceEqual(target.Prefixes) &&
                source.Suffixes.SequenceEqual(target.Suffixes) &&
                source.ContactFirstName == target.ContactFirstName &&
                source.ContactLastName == target.ContactLastName
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -465884477;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(ContactFirstName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(ContactLastName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]?>.Default.GetHashCode(AltNames);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]?>.Default.GetHashCode(Prefixes);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]?>.Default.GetHashCode(Suffixes);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(NameInfo left, NameInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(NameInfo left, NameInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BasePartInfo source, BasePartInfo target) =>
            ((NameInfo)source) == ((NameInfo)target);

        internal NameInfo() { }

        internal NameInfo(int altId, PropertyInfo? property, string[] elementTypes, string group, string valueType, string contactFirstName, string contactLastName, string[] altNames, string[] prefixes, string[] suffixes) :
            base(property, altId, elementTypes, group, valueType)
        {
            ContactFirstName = contactFirstName;
            ContactLastName = contactLastName;
            AltNames = altNames;
            Prefixes = prefixes;
            Suffixes = suffixes;
        }
    }
}
