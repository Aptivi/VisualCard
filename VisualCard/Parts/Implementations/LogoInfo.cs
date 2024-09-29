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

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact logo info
    /// </summary>
    [DebuggerDisplay("Logo, {Encoding}, {LogoType}, {ValueType}")]
    public class LogoInfo : BaseCardPartInfo, IEquatable<LogoInfo>
    {
        /// <summary>
        /// Logo encoding type
        /// </summary>
        public string? Encoding { get; }
        /// <summary>
        /// Encoded logo
        /// </summary>
        public string? LogoEncoded { get; }
        /// <summary>
        /// Whether this logo is a blob or not
        /// </summary>
        public bool IsBlob =>
            VcardCommonTools.IsEncodingBlob(Arguments, LogoEncoded);

        internal static BaseCardPartInfo FromStringVcardStatic(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion) =>
            new LogoInfo().FromStringVcardInternal(value, finalArgs, altId, elementTypes, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion) =>
            LogoEncoded ?? "";

        internal override BaseCardPartInfo FromStringVcardInternal(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion)
        {
            bool vCard4 = cardVersion.Major >= 4;

            // Check to see if the value is prepended by the ENCODING= argument
            string logoEncoding = "";
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
                logoEncoding = VcardCommonTools.GetValuesString(finalArgs, "b", VcardConstants._encodingArgumentSpecifier);
                if (!VcardCommonTools.IsEncodingBlob(finalArgs, value))
                {
                    // Since we don't need embedded logos, we need to check a URL.
                    if (!Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
                        throw new InvalidDataException($"URL {value} is invalid");
                    value = uri.ToString();
                }
            }

            // Populate the fields
            LogoInfo _logo = new(altId, finalArgs, elementTypes, valueType, logoEncoding, value);
            return _logo;
        }

        /// <summary>
        /// Gets a stream representing the logo data
        /// </summary>
        /// <returns>A stream that contains logo data</returns>
        public Stream GetStream() =>
            VcardCommonTools.GetBlobData(Arguments, LogoEncoded);

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((LogoInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="LogoInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(LogoInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="LogoInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="LogoInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(LogoInfo source, LogoInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.Encoding == target.Encoding &&
                source.LogoEncoded == target.LogoEncoded
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 2051368178;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(Encoding);
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(LogoEncoded);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(LogoInfo left, LogoInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(LogoInfo left, LogoInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCardPartInfo source, BaseCardPartInfo target) =>
            ((LogoInfo)source) == ((LogoInfo)target);

        internal LogoInfo() { }

        internal LogoInfo(int altId, string[] arguments, string[] elementTypes, string valueType, string encoding, string logoEncoded) :
            base(arguments, altId, elementTypes, valueType)
        {
            Encoding = encoding;
            LogoEncoded = logoEncoded;
        }
    }
}
