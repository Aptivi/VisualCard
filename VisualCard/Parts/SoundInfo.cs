/*
 * MIT License
 *
 * Copyright (c) 2021-2022 Aptivi
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using VisualCard.Parsers;

namespace VisualCard.Parts
{
    /// <summary>
    /// Contact sound info
    /// </summary>
    [DebuggerDisplay("Sound, {Encoding}, {SoundType}, {ValueType}")]
    public class SoundInfo : IEquatable<SoundInfo>
    {
        /// <summary>
        /// Alternative ID. Zero if unspecified.
        /// </summary>
        public int AltId { get; }
        /// <summary>
        /// Arguments that follow the AltId
        /// </summary>
        public string[] AltArguments { get; }
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

        internal string ToStringVcardTwo()
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
                return soundArgsLine + BaseVcardParser.MakeStringBlock(SoundEncoded, soundArgsLine.Length);
            }
        }

        internal string ToStringVcardThree()
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
                return soundArgsLine + BaseVcardParser.MakeStringBlock(SoundEncoded, soundArgsLine.Length);
            }
        }

        internal string ToStringVcardFour()
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
                return soundArgsLine + BaseVcardParser.MakeStringBlock(SoundEncoded, soundArgsLine.Length);
            }
        }

        internal static SoundInfo FromStringVcardTwoWithType(string value, StreamReader cardContentReader)
        {
            // Get the value
            string soundValue = value.Substring(VcardConstants._soundSpecifier.Length + 1);
            string[] splitSound = soundValue.Split(VcardConstants._argumentDelimiter);
            if (splitSound.Length < 2)
                throw new InvalidDataException("Sound field must specify exactly two values (Type and arguments, and sound information)");

            // Check to see if the value is prepended by the VALUE= argument
            string valueType = VcardParserTools.GetValuesString(splitSound, "", VcardConstants._valueArgumentSpecifier).ToLower();
            bool isUrl = valueType == "url" || valueType == "uri";

            // Check to see if the value is prepended by the ENCODING= argument
            string soundEncoding = VcardParserTools.GetValuesString(splitSound, "BASE64", VcardConstants._encodingArgumentSpecifier);

            // Check to see if the value is prepended with the TYPE= argument
            string soundType = VcardParserTools.GetTypesString(splitSound, "WAVE", false);

            // Now, get the encoded sound
            StringBuilder encodedSound = new();
            if (splitSound.Length == 2)
                encodedSound.Append(splitSound[1]);

            // Make sure to get all the blocks until we reach an empty line
            if (!isUrl)
            {
                string lineToBeAppended = cardContentReader.ReadLine();
                while (!string.IsNullOrWhiteSpace(lineToBeAppended))
                {
                    encodedSound.Append(lineToBeAppended);
                    lineToBeAppended = cardContentReader.ReadLine()?.Trim();
                }
            }

            // Populate the fields
            SoundInfo _sound = new(0, [], valueType, soundEncoding, soundType, encodedSound.ToString());
            return _sound;
        }

        internal static SoundInfo FromStringVcardThreeWithType(string value, StreamReader cardContentReader)
        {
            // Get the value
            string soundValue = value.Substring(VcardConstants._soundSpecifier.Length + 1);
            string[] splitSound = soundValue.Split(VcardConstants._argumentDelimiter);
            if (splitSound.Length < 2)
                throw new InvalidDataException("Sound field must specify exactly two values (Type and arguments, and sound information)");

            // Check to see if the value is prepended by the VALUE= argument
            string valueType = VcardParserTools.GetValuesString(splitSound, "", VcardConstants._valueArgumentSpecifier).ToLower();
            bool isUrl = valueType == "url" || valueType == "uri";

            // Check to see if the value is prepended by the ENCODING= argument
            string soundEncoding = VcardParserTools.GetValuesString(splitSound, "BASE64", VcardConstants._encodingArgumentSpecifier);

            // Check to see if the value is prepended with the TYPE= argument
            string soundType = VcardParserTools.GetTypesString(splitSound, "WAVE", true);

            // Now, get the encoded sound
            StringBuilder encodedSound = new();
            if (splitSound.Length == 2)
                encodedSound.Append(splitSound[1]);

            // Make sure to get all the blocks until we reach an empty line
            if (!isUrl)
            {
                string lineToBeAppended = cardContentReader.ReadLine();
                while (!string.IsNullOrWhiteSpace(lineToBeAppended))
                {
                    encodedSound.Append(lineToBeAppended);
                    lineToBeAppended = cardContentReader.ReadLine()?.Trim();
                }
            }

            // Populate the fields
            SoundInfo _sound = new(0, [], valueType, soundEncoding, soundType, encodedSound.ToString());
            return _sound;
        }

        internal static SoundInfo FromStringVcardFourWithType(string value, List<string> finalArgs, int altId, StreamReader cardContentReader)
        {
            // Get the value
            string soundValue = value.Substring(VcardConstants._soundSpecifier.Length + 1);
            string[] splitSound = soundValue.Split(VcardConstants._argumentDelimiter);
            if (splitSound.Length < 2)
                throw new InvalidDataException("Sound field must specify exactly two values (Type and arguments, and sound information)");

            // Check to see if the value is prepended by the VALUE= argument
            string valueType = VcardParserTools.GetValuesString(splitSound, "", VcardConstants._valueArgumentSpecifier).ToLower();
            bool isUrl = valueType == "url" || valueType == "uri";

            // Check to see if the value is prepended by the ENCODING= argument
            string soundEncoding = VcardParserTools.GetValuesString(splitSound, "BASE64", VcardConstants._encodingArgumentSpecifier);

            // Check to see if the value is prepended with the TYPE= argument
            string soundType = VcardParserTools.GetTypesString(splitSound, "WAVE", true);

            // Now, get the encoded sound
            StringBuilder encodedSound = new();
            if (splitSound.Length == 2)
                encodedSound.Append(splitSound[1]);

            // Make sure to get all the blocks until we reach an empty line
            if (!isUrl)
            {
                string lineToBeAppended = cardContentReader.ReadLine();
                while (!string.IsNullOrWhiteSpace(lineToBeAppended))
                {
                    encodedSound.Append(lineToBeAppended);
                    lineToBeAppended = cardContentReader.ReadLine()?.Trim();
                }
            }

            // Populate the fields
            SoundInfo _sound = new(altId, [.. finalArgs], valueType, soundEncoding, soundType, encodedSound.ToString());
            return _sound;
        }

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
