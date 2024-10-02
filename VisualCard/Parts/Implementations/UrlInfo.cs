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
using VisualCard.Parsers.Arguments;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact URL info
    /// </summary>
    [DebuggerDisplay("Url = {Url}")]
    public class UrlInfo : BaseCardPartInfo, IEquatable<UrlInfo>
    {
        /// <summary>
        /// Encoded URL
        /// </summary>
        public string? Url { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, ArgumentInfo[] finalArgs, int altId, string[] elementTypes, string group, string valueType, Version cardVersion) =>
            new UrlInfo().FromStringVcardInternal(value, finalArgs, altId, elementTypes, group, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion) =>
            Url ?? "";

        internal override BaseCardPartInfo FromStringVcardInternal(string value, ArgumentInfo[] finalArgs, int altId, string[] elementTypes, string group, string valueType, Version cardVersion)
        {
            // Try to parse the URL to ensure that it conforms the IETF RFC 1738: Uniform Resource Locators
            if (!Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
                throw new InvalidDataException($"URL {value} is invalid");
            value = uri.ToString();

            // Populate the fields
            UrlInfo _url = new(altId, finalArgs, elementTypes, valueType, group, value);
            return _url;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((UrlInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="UrlInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(UrlInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="UrlInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="UrlInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(UrlInfo source, UrlInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.Url == target.Url
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -1169443244;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(Url);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(UrlInfo left, UrlInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(UrlInfo left, UrlInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCardPartInfo source, BaseCardPartInfo target) =>
            ((UrlInfo)source) == ((UrlInfo)target);

        internal UrlInfo() { }

        internal UrlInfo(int altId, ArgumentInfo[] arguments, string[] elementTypes, string valueType, string group, string url) :
            base(arguments, altId, elementTypes, valueType, group)
        {
            Url = url;
        }
    }
}
