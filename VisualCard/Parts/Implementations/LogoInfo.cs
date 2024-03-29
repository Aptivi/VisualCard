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
using System.Linq;
using System.Text;
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
        /// Value type
        /// </summary>
        public string ValueType { get; }
        /// <summary>
        /// Logo encoding type
        /// </summary>
        public string Encoding { get; }
        /// <summary>
        /// Logo type (JPEG, ...)
        /// </summary>
        public string LogoType { get; }
        /// <summary>
        /// Encoded logo
        /// </summary>
        public string LogoEncoded { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, int altId, Version cardVersion, StreamReader cardContentReader) =>
            new LogoInfo().FromStringVcardInternal(value, altId, cardVersion, cardContentReader);

        internal static BaseCardPartInfo FromStringVcardWithTypeStatic(string value, string[] finalArgs, int altId, Version cardVersion, StreamReader cardContentReader) =>
            new LogoInfo().FromStringVcardWithTypeInternal(value, finalArgs, altId, cardVersion, cardContentReader);

        internal override string ToStringVcardInternal(Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;
            if (altIdSupported)
            {
                bool installAltId = AltId >= 0 && AltArguments.Length > 0;
                if (ValueType == "uri" || ValueType == "url")
                {
                    return
                        $"{VcardConstants._logoSpecifier};" +
                        $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                        $"{(installAltId ? string.Join(VcardConstants._fieldDelimiter.ToString(), AltArguments) + VcardConstants._fieldDelimiter : "")}" +
                        $"{VcardConstants._valueArgumentSpecifier}{ValueType}{VcardConstants._argumentDelimiter}" +
                        $"{LogoEncoded}";
                }
                else
                {
                    string logoArgsLine =
                        $"{VcardConstants._logoSpecifier};" +
                        $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                        $"{(installAltId ? string.Join(VcardConstants._fieldDelimiter.ToString(), AltArguments) + VcardConstants._fieldDelimiter : "")}" +
                        $"{VcardConstants._valueArgumentSpecifier}{ValueType}{VcardConstants._fieldDelimiter}" +
                        $"{VcardConstants._encodingArgumentSpecifier}{Encoding}{VcardConstants._fieldDelimiter}" +
                        $"{VcardConstants._typeArgumentSpecifier}{LogoType}{VcardConstants._argumentDelimiter}";
                    return logoArgsLine + BaseVcardParser.MakeStringBlock(LogoEncoded, logoArgsLine.Length);
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
                        $"{VcardConstants._typeArgumentSpecifier}{LogoType}{VcardConstants._argumentDelimiter}";
                    return logoArgsLine + BaseVcardParser.MakeStringBlock(LogoEncoded, logoArgsLine.Length);
                }
            }
        }

        internal override BaseCardPartInfo FromStringVcardInternal(string value, int altId, Version cardVersion, StreamReader cardContentReader) =>
            throw new InvalidDataException("Logo field must not have empty type.");

        internal override BaseCardPartInfo FromStringVcardWithTypeInternal(string value, string[] finalArgs, int altId, Version cardVersion, StreamReader cardContentReader)
        {
            // Get the value
            string logoValue = value.Substring(VcardConstants._logoSpecifier.Length + 1);
            string[] splitLogo = logoValue.Split(VcardConstants._argumentDelimiter);
            if (splitLogo.Length < 2)
                throw new InvalidDataException("Logo field must specify exactly two values (Type and arguments, and logo information)");

            // Populate the fields
            return InstallInfo(splitLogo, finalArgs, altId, cardVersion, cardContentReader);
        }

        private LogoInfo InstallInfo(string[] splitLogo, string[] finalArgs, int altId, Version cardVersion, StreamReader cardContentReader)
        {
            bool altIdSupported = cardVersion.Major >= 4;

            // Check to see if the value is prepended by the VALUE= argument
            string valueType = VcardParserTools.GetValuesString(splitLogo, "", VcardConstants._valueArgumentSpecifier).ToLower();
            bool isUrl = valueType == "url" || valueType == "uri";

            // Check to see if the value is prepended by the ENCODING= argument
            string logoEncoding = VcardParserTools.GetValuesString(splitLogo, "BASE64", VcardConstants._encodingArgumentSpecifier);

            // Check to see if the value is prepended with the TYPE= argument
            string logoType = VcardParserTools.GetTypesString(splitLogo, "JPEG", false);

            // Now, get the encoded logo
            StringBuilder encodedLogo = new();
            if (splitLogo.Length == 2)
                encodedLogo.Append(splitLogo[1]);

            // Make sure to get all the blocks until we reach an empty line
            if (!isUrl)
            {
                string lineToBeAppended = cardContentReader.ReadLine();
                while (!string.IsNullOrWhiteSpace(lineToBeAppended) && lineToBeAppended.StartsWith(" "))
                {
                    encodedLogo.Append(lineToBeAppended.Trim());
                    lineToBeAppended = cardContentReader.ReadLine();
                }
            }

            // Populate the fields
            LogoInfo _logo = new(altIdSupported ? altId : 0, altIdSupported ? finalArgs : [], valueType, logoEncoding, logoType, encodedLogo.ToString());
            return _logo;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            base.Equals(obj);

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
            if (source is null)
                return false;

            // Check all the properties
            return
                source.AltArguments.SequenceEqual(target.AltArguments) &&
                source.AltId == target.AltId &&
                source.ValueType == target.ValueType &&
                source.Encoding == target.Encoding &&
                source.LogoType == target.LogoType &&
                source.LogoEncoded == target.LogoEncoded
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -1881924127;
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AltArguments);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ValueType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Encoding);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LogoType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LogoEncoded);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(LogoInfo left, LogoInfo right) =>
            EqualityComparer<LogoInfo>.Default.Equals(left, right);

        /// <inheritdoc/>
        public static bool operator !=(LogoInfo left, LogoInfo right) =>
            !(left == right);

        internal LogoInfo() { }

        internal LogoInfo(int altId, string[] altArguments, string valueType, string encoding, string logoType, string logoEncoded)
        {
            AltId = altId;
            AltArguments = altArguments;
            ValueType = valueType;
            Encoding = encoding;
            LogoType = logoType;
            LogoEncoded = logoEncoded;
        }
    }
}
