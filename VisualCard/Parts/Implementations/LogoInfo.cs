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
        public string Encoding { get; }
        /// <summary>
        /// Encoded logo
        /// </summary>
        public string LogoEncoded { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion) =>
            new LogoInfo().FromStringVcardInternal(value, finalArgs, altId, elementTypes, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;
            if (altIdSupported)
            {
                bool installAltId = AltId >= 0 && Arguments.Length > 0;
                if (ValueType == "uri" || ValueType == "url")
                {
                    return
                        $"{VcardConstants._logoSpecifier};" +
                        $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                        $"{(installAltId ? string.Join(VcardConstants._fieldDelimiter.ToString(), Arguments) + VcardConstants._fieldDelimiter : "")}" +
                        $"{VcardConstants._valueArgumentSpecifier}{ValueType}{VcardConstants._argumentDelimiter}" +
                        $"{LogoEncoded}";
                }
                else
                {
                    string logoArgsLine =
                        $"{VcardConstants._logoSpecifier};" +
                        $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                        $"{(installAltId ? string.Join(VcardConstants._fieldDelimiter.ToString(), Arguments) + VcardConstants._fieldDelimiter : "")}" +
                        $"{VcardConstants._valueArgumentSpecifier}{ValueType}{VcardConstants._fieldDelimiter}" +
                        $"{VcardConstants._encodingArgumentSpecifier}{Encoding}{VcardConstants._fieldDelimiter}" +
                        $"{VcardConstants._typeArgumentSpecifier}{string.Join(VcardConstants._valueDelimiter.ToString(), ElementTypes)}{VcardConstants._argumentDelimiter}";
                    return logoArgsLine + VcardParserTools.MakeStringBlock(LogoEncoded, logoArgsLine.Length);
                }
            }
            else
            {
                if (ValueType == "uri" || ValueType == "url")
                {
                    return
                        $"{VcardConstants._logoSpecifier};" +
                        $"{VcardConstants._valueArgumentSpecifier}{ValueType}{VcardConstants._argumentDelimiter}" +
                        $"{LogoEncoded}";
                }
                else
                {
                    string logoArgsLine =
                        $"{VcardConstants._logoSpecifier};" +
                        $"{VcardConstants._valueArgumentSpecifier}{ValueType}{VcardConstants._fieldDelimiter}" +
                        $"{VcardConstants._encodingArgumentSpecifier}{Encoding}{VcardConstants._fieldDelimiter}" +
                        $"{VcardConstants._typeArgumentSpecifier}{string.Join(VcardConstants._valueDelimiter.ToString(), ElementTypes)}{VcardConstants._argumentDelimiter}";
                    return logoArgsLine + VcardParserTools.MakeStringBlock(LogoEncoded, logoArgsLine.Length);
                }
            }
        }

        internal override BaseCardPartInfo FromStringVcardInternal(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;

            // Check to see if the value is prepended by the ENCODING= argument
            string logoEncoding = VcardParserTools.GetValuesString(finalArgs, "BASE64", VcardConstants._encodingArgumentSpecifier);

            // Populate the fields
            LogoInfo _logo = new(altIdSupported ? altId : 0, finalArgs, elementTypes, valueType, logoEncoding, value);
            return _logo;
        }

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
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Encoding);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LogoEncoded);
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
