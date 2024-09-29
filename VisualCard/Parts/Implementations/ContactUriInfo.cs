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

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact URI info
    /// </summary>
    [DebuggerDisplay("Contact URI = {ContactUri}")]
    public class ContactUriInfo : BaseCardPartInfo, IEquatable<ContactUriInfo>
    {
        /// <summary>
        /// Encoded contact URI
        /// </summary>
        public string? ContactUri { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion) =>
            new ContactUriInfo().FromStringVcardInternal(value, finalArgs, altId, elementTypes, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion) =>
            ContactUri ?? "";

        internal override BaseCardPartInfo FromStringVcardInternal(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion)
        {
            // Try to parse the source to ensure that it conforms the IETF RFC 1738: Uniform Resource Locators
            if (!Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
                throw new InvalidDataException($"source {value} is invalid");
            value = uri.ToString();

            // Populate the fields
            ContactUriInfo _source = new(altId, finalArgs, elementTypes, valueType, value);
            return _source;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((ContactUriInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="ContactUriInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(ContactUriInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="ContactUriInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="ContactUriInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(ContactUriInfo source, ContactUriInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.ContactUri == target.ContactUri
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 1694405134;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(ContactUri);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(ContactUriInfo left, ContactUriInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(ContactUriInfo left, ContactUriInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCardPartInfo source, BaseCardPartInfo target) =>
            ((ContactUriInfo)source) == ((ContactUriInfo)target);

        internal ContactUriInfo() { }

        internal ContactUriInfo(int altId, string[] arguments, string[] elementTypes, string valueType, string source) :
            base(arguments, altId, elementTypes, valueType)
        {
            ContactUri = source;
        }
    }
}
