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
using Textify.General;
using VisualCard.Calendar.Languages;
using VisualCard.Common.Parsers;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts;
using VisualCard.Parsers;

namespace VisualCard.Calendar.Parts.Implementations
{
    /// <summary>
    /// Calendar attach info
    /// </summary>
    [DebuggerDisplay("Attach, {Encoding}, {ValueType}")]
    public class AttachInfo : BaseCalendarPartInfo, IEquatable<AttachInfo>
    {
        /// <summary>
        /// Encoded attach
        /// </summary>
        public string? AttachEncoded { get; set; }
        /// <summary>
        /// Whether this attach is a blob or not
        /// </summary>
        public bool IsBlob =>
            CommonTools.IsEncodingBlob(Arguments ?? [], AttachEncoded);

        internal static BaseCalendarPartInfo FromStringStatic(string value, PropertyInfo property, int altId, string[] elementTypes, Version calendarVersion) =>
            (BaseCalendarPartInfo)new AttachInfo().FromStringInternal(value, property, altId, elementTypes, calendarVersion);

        internal override string ToStringInternal(Version calendarVersion) =>
            AttachEncoded ?? "";

        internal override BasePartInfo FromStringInternal(string value, PropertyInfo property, int altId, string[] elementTypes, Version calendarVersion)
        {
            var arguments = property?.Arguments ?? [];

            // Check to see if the value is prepended by the ENCODING= argument
            if (!CommonTools.IsEncodingBlob(arguments, value))
            {
                // Since we don't need embedded attachs, we need to check a URL.
                if (!Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
                    throw new InvalidDataException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_ATTACH_INVALIDURL").FormatString(value));
                value = uri.ToString();
            }

            // Populate the fields
            AttachInfo _attach = new(property, elementTypes, value);
            return _attach;
        }

        /// <summary>
        /// Gets a stream representing the image data
        /// </summary>
        /// <returns>A stream that contains image data</returns>
        public Stream GetStream() =>
            CommonTools.GetBlobData(Arguments ?? [], AttachEncoded);

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((AttachInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="AttachInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(AttachInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="AttachInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="AttachInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(AttachInfo source, AttachInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.Encoding == target.Encoding &&
                source.AttachEncoded == target.AttachEncoded
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -365738507;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(Encoding);
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(AttachEncoded);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(AttachInfo left, AttachInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(AttachInfo left, AttachInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BasePartInfo source, BasePartInfo target) =>
            ((AttachInfo)source) == ((AttachInfo)target);

        internal AttachInfo() { }

        internal AttachInfo(PropertyInfo? property, string[] elementTypes, string attachEncoded) :
            base(property, elementTypes)
        {
            AttachEncoded = attachEncoded;
        }
    }
}
