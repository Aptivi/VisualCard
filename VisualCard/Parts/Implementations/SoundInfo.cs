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
using VisualCard.Parsers;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact sound info
    /// </summary>
    [DebuggerDisplay("Sound, {Encoding}, {SoundType}, {ValueType}")]
    public class SoundInfo : BaseCardPartInfo, IEquatable<SoundInfo>
    {
        /// <summary>
        /// Value type
        /// </summary>
        public string ValueType { get; }
        /// <summary>
        /// Sound encoding type
        /// </summary>
        public string Encoding { get; }
        /// <summary>
        /// Sound type (MP3, ...)
        /// </summary>
        public string SoundType { get; }
        /// <summary>
        /// Encoded sound
        /// </summary>
        public string SoundEncoded { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, int altId, Version cardVersion) =>
            new SoundInfo().FromStringVcardInternal(value, altId, cardVersion);

        internal static BaseCardPartInfo FromStringVcardWithTypeStatic(string value, string[] finalArgs, int altId, Version cardVersion) =>
            new SoundInfo().FromStringVcardWithTypeInternal(value, finalArgs, altId, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;
            if (altIdSupported)
            {
                bool installAltId = AltId >= 0 && AltArguments.Length > 0;
                if (ValueType == "uri" || ValueType == "url")
                {
                    return
                        $"{VcardConstants._soundSpecifier};" +
                        $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                        $"{(installAltId ? string.Join(VcardConstants._fieldDelimiter.ToString(), AltArguments) + VcardConstants._fieldDelimiter : "")}" +
                        $"{VcardConstants._valueArgumentSpecifier}{ValueType}{VcardConstants._argumentDelimiter}" +
                        $"{SoundEncoded}";
                }
                else
                {
                    string soundArgsLine =
                        $"{VcardConstants._soundSpecifier};" +
                        $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                        $"{(installAltId ? string.Join(VcardConstants._fieldDelimiter.ToString(), AltArguments) + VcardConstants._fieldDelimiter : "")}" +
                        $"{VcardConstants._valueArgumentSpecifier}{ValueType}{VcardConstants._fieldDelimiter}" +
                        $"{VcardConstants._encodingArgumentSpecifier}{Encoding}{VcardConstants._fieldDelimiter}" +
                        $"{VcardConstants._typeArgumentSpecifier}{SoundType}{VcardConstants._argumentDelimiter}";
                    return soundArgsLine + VcardParserTools.MakeStringBlock(SoundEncoded, soundArgsLine.Length);
                }
            }
            else
            {
                if (ValueType == "uri" || ValueType == "url")
                {
                    return
                        $"{VcardConstants._soundSpecifier};" +
                        $"{VcardConstants._valueArgumentSpecifier}{ValueType}{VcardConstants._argumentDelimiter}" +
                        $"{SoundEncoded}";
                }
                else
                {
                    string soundArgsLine =
                        $"{VcardConstants._soundSpecifier};" +
                        $"{VcardConstants._valueArgumentSpecifier}{ValueType}{VcardConstants._fieldDelimiter}" +
                        $"{VcardConstants._encodingArgumentSpecifier}{Encoding}{VcardConstants._fieldDelimiter}" +
                        $"{VcardConstants._typeArgumentSpecifier}{SoundType}{VcardConstants._argumentDelimiter}";
                    return soundArgsLine + VcardParserTools.MakeStringBlock(SoundEncoded, soundArgsLine.Length);
                }
            }
        }

        internal override BaseCardPartInfo FromStringVcardInternal(string value, int altId, Version cardVersion) =>
            throw new InvalidDataException("Sound field must not have empty type.");

        internal override BaseCardPartInfo FromStringVcardWithTypeInternal(string value, string[] finalArgs, int altId, Version cardVersion)
        {
            // Get the value
            string soundValue = value.Substring(VcardConstants._soundSpecifier.Length + 1);
            string[] splitSound = soundValue.Split(VcardConstants._argumentDelimiter);
            if (splitSound.Length < 2)
                throw new InvalidDataException("Sound field must specify exactly two values (Type and arguments, and sound information)");

            // Populate the fields
            return InstallInfo(splitSound, finalArgs, altId, cardVersion);
        }

        private SoundInfo InstallInfo(string[] splitSound, string[] finalArgs, int altId, Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;

            // Check to see if the value is prepended by the VALUE= argument
            string valueType = VcardParserTools.GetValuesString(splitSound, "", VcardConstants._valueArgumentSpecifier).ToLower();

            // Check to see if the value is prepended by the ENCODING= argument
            string soundEncoding = VcardParserTools.GetValuesString(splitSound, "BASE64", VcardConstants._encodingArgumentSpecifier);

            // Check to see if the value is prepended with the TYPE= argument
            string soundType = VcardParserTools.GetTypesString(splitSound, "WAVE", false);

            // Populate the fields
            SoundInfo _sound = new(altIdSupported ? altId : 0, altIdSupported ? finalArgs : [], valueType, soundEncoding, soundType, splitSound[1]);
            return _sound;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            base.Equals(obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="SoundInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(SoundInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="SoundInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="SoundInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(SoundInfo source, SoundInfo target)
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
                source.SoundType == target.SoundType &&
                source.SoundEncoded == target.SoundEncoded
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 21154477;
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AltArguments);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ValueType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Encoding);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SoundType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SoundEncoded);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(SoundInfo left, SoundInfo right) =>
            EqualityComparer<SoundInfo>.Default.Equals(left, right);

        /// <inheritdoc/>
        public static bool operator !=(SoundInfo left, SoundInfo right) =>
            !(left == right);

        internal SoundInfo() { }

        internal SoundInfo(int altId, string[] altArguments, string valueType, string encoding, string soundType, string soundEncoded)
        {
            AltId = altId;
            AltArguments = altArguments;
            ValueType = valueType;
            Encoding = encoding;
            SoundType = soundType;
            SoundEncoded = soundEncoded;
        }
    }
}
