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
using System.Text.RegularExpressions;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Related URI info
    /// </summary>
    [DebuggerDisplay("Related URI = {Related}")]
    public class RelatedInfo : BaseCardPartInfo, IEquatable<RelatedInfo>
    {
        /// <summary>
        /// Encoded related URI
        /// </summary>
        public string? RelatedUri { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, string[] finalArgs, int altId, string[] elementTypes, string group, string valueType, Version cardVersion) =>
            new RelatedInfo().FromStringVcardInternal(value, finalArgs, altId, elementTypes, group, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion) =>
            RelatedUri ?? "";

        internal override BaseCardPartInfo FromStringVcardInternal(string value, string[] finalArgs, int altId, string[] elementTypes, string group, string valueType, Version cardVersion)
        {
            // Populate the fields
            string _relationship = Regex.Unescape(value);
            if (valueType.Equals("uri", StringComparison.OrdinalIgnoreCase))
            {
                // Try to parse the source to ensure that it conforms the IETF RFC 1738: Uniform Resource Locators
                if (!Uri.TryCreate(_relationship, UriKind.Absolute, out Uri uri))
                    throw new InvalidDataException($"source {_relationship} is invalid");
                _relationship = uri.ToString();
            }
            RelatedInfo _source = new(altId, finalArgs, elementTypes, valueType, group, _relationship);
            return _source;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((RelatedInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="RelatedInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(RelatedInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="RelatedInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="RelatedInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(RelatedInfo source, RelatedInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.RelatedUri == target.RelatedUri
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 881496785;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(RelatedUri);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(RelatedInfo left, RelatedInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(RelatedInfo left, RelatedInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCardPartInfo source, BaseCardPartInfo target) =>
            ((RelatedInfo)source) == ((RelatedInfo)target);

        internal RelatedInfo() { }

        internal RelatedInfo(int altId, string[] arguments, string[] elementTypes, string valueType, string group, string source) :
            base(arguments, altId, elementTypes, valueType, group)
        {
            RelatedUri = source;
        }
    }
}
