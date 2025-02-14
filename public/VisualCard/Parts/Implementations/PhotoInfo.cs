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
using VisualCard.Parsers;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts;
using VisualCard.Common.Parsers;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact photo info
    /// </summary>
    [DebuggerDisplay("Photo, {Encoding}, {PhotoType}, {ValueType}")]
    public class PhotoInfo : BaseCardPartInfo, IEquatable<PhotoInfo>
    {
        /// <summary>
        /// Encoded photo
        /// </summary>
        public string? PhotoEncoded { get; set; }
        /// <summary>
        /// Whether this photo is a blob or not
        /// </summary>
        public bool IsBlob =>
            CommonTools.IsEncodingBlob(Arguments ?? [], PhotoEncoded);

        internal static BaseCardPartInfo FromStringStatic(string value, PropertyInfo property, int altId, string[] elementTypes, Version cardVersion) =>
            (BaseCardPartInfo)new PhotoInfo().FromStringInternal(value, property, altId, elementTypes, cardVersion);

        internal override string ToStringInternal(Version cardVersion) =>
            PhotoEncoded ?? "";

        internal override BasePartInfo FromStringInternal(string value, PropertyInfo property, int altId, string[] elementTypes, Version cardVersion)
        {
            bool vCard4 = cardVersion.Major >= 4;
            var arguments = property?.Arguments ?? [];

            // Check to see if the value is prepended by the ENCODING= argument
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
                if (!CommonTools.IsEncodingBlob(arguments, value))
                {
                    // Since we don't need embedded photos, we need to check a URL.
                    if (!Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
                        throw new InvalidDataException($"URL {value} is invalid");
                    value = uri.ToString();
                }
            }

            // Populate the fields
            PhotoInfo _photo = new(altId, property, elementTypes, value);
            return _photo;
        }

        /// <summary>
        /// Gets a stream representing the image data
        /// </summary>
        /// <returns>A stream that contains image data</returns>
        public Stream GetStream() =>
            CommonTools.GetBlobData(Arguments ?? [], PhotoEncoded);

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

        internal override bool EqualsInternal(BasePartInfo source, BasePartInfo target) =>
            ((PhotoInfo)source) == ((PhotoInfo)target);

        internal PhotoInfo() { }

        internal PhotoInfo(int altId, PropertyInfo? property, string[] elementTypes, string photoEncoded) :
            base(property, altId, elementTypes)
        {
            PhotoEncoded = photoEncoded;
        }
    }
}
