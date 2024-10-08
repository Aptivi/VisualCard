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
using VisualCard.Parsers;
using VisualCard.Parsers.Arguments;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact photo info
    /// </summary>
    [DebuggerDisplay("Photo, {Encoding}, {PhotoType}, {ValueType}")]
    public class PhotoInfo : BaseCardPartInfo, IEquatable<PhotoInfo>
    {
        /// <summary>
        /// Photo encoding type
        /// </summary>
        public string? Encoding { get; }
        /// <summary>
        /// Encoded photo
        /// </summary>
        public string? PhotoEncoded { get; }
        /// <summary>
        /// Whether this photo is a blob or not
        /// </summary>
        public bool IsBlob =>
            VcardCommonTools.IsEncodingBlob(Property?.Arguments ?? [], PhotoEncoded);

        internal static BaseCardPartInfo FromStringVcardStatic(string value, PropertyInfo property, int altId, string[] elementTypes, string group, string valueType, Version cardVersion) =>
            new PhotoInfo().FromStringVcardInternal(value, property, altId, elementTypes, group, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion) =>
            PhotoEncoded ?? "";

        internal override BaseCardPartInfo FromStringVcardInternal(string value, PropertyInfo property, int altId, string[] elementTypes, string group, string valueType, Version cardVersion)
        {
            bool vCard4 = cardVersion.Major >= 4;
            var arguments = property?.Arguments ?? [];

            // Check to see if the value is prepended by the ENCODING= argument
            string photoEncoding = "";
            if (vCard4)
            {
                // We're on a vCard 4.0 contact that contains this information
                if (!Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
                    throw new InvalidDataException($"URL {value} is invalid");
                value = uri.ToString();
            }
            else
            {
                // vCard 3.0 handles this in a different way
                photoEncoding = VcardCommonTools.GetValuesString(arguments, "b", VcardConstants._encodingArgumentSpecifier);
                if (!VcardCommonTools.IsEncodingBlob(arguments, value))
                {
                    // Since we don't need embedded photos, we need to check a URL.
                    if (!Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
                        throw new InvalidDataException($"URL {value} is invalid");
                    value = uri.ToString();
                }
            }

            // Populate the fields
            PhotoInfo _photo = new(altId, property, elementTypes, valueType, group, photoEncoding, value);
            return _photo;
        }

        /// <summary>
        /// Gets a stream representing the image data
        /// </summary>
        /// <returns>A stream that contains image data</returns>
        public Stream GetStream() =>
            VcardCommonTools.GetBlobData(Property?.Arguments ?? [], PhotoEncoded);

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((PhotoInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="PhotoInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(PhotoInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="PhotoInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="PhotoInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(PhotoInfo source, PhotoInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.Encoding == target.Encoding &&
                source.PhotoEncoded == target.PhotoEncoded
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -365738507;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(Encoding);
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(PhotoEncoded);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(PhotoInfo left, PhotoInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(PhotoInfo left, PhotoInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCardPartInfo source, BaseCardPartInfo target) =>
            ((PhotoInfo)source) == ((PhotoInfo)target);

        internal PhotoInfo() { }

        internal PhotoInfo(int altId, PropertyInfo? property, string[] elementTypes, string valueType, string group, string encoding, string photoEncoded) :
            base(property, altId, elementTypes, valueType, group)
        {
            Encoding = encoding;
            PhotoEncoded = photoEncoded;
        }
    }
}
