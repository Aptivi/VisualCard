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
using Textify.General;
using VisualCard.Parsers;
using VisualCard.Parts.Comparers;
using VisualCard.Parts.Enums;

namespace VisualCard.Parts
{
    /// <summary>
    /// A vCard card instance
    /// </summary>
    [DebuggerDisplay("vCard version {CardVersion.ToString()}, parts: (P [{parts.Count}] | A [{partsArray.Count}] | S [{strings.Count}]), explicit kind: {kindExplicitlySpecified}")]
    public class Card : IEquatable<Card>
    {
        internal bool kindExplicitlySpecified = false;
        private readonly Version version;
        private readonly Dictionary<PartsArrayEnum, List<BaseCardPartInfo>> partsArray = [];
        private readonly Dictionary<StringsEnum, string> strings = [];

        /// <summary>
        /// The VCard version
        /// </summary>
        public Version CardVersion =>
            version;

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <returns>An array of values or an empty part array []</returns>
        public TPart[] GetPartsArray<TPart>() where TPart : BaseCardPartInfo
        {
            // Get the parts enumeration according to the type
            var key = VcardParserTools.GetPartsArrayEnumFromType(typeof(TPart), CardVersion);

            // Now, return the value
            return GetPartsArray<TPart>(key.Item1);
        }

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>An array of values or an empty part array []</returns>
        public TPart[] GetPartsArray<TPart>(PartsArrayEnum key) where TPart : BaseCardPartInfo
        {
            // Check for version support
            if (!VcardParserTools.EnumArrayTypeSupported(key, CardVersion))
                return [];

            // Get the parts enumeration according to the type
            var type = typeof(TPart);
            if (type != typeof(BaseCardPartInfo))
            {
                // We don't need the base, but a derivative of it. Check it.
                var partsArrayEnum = VcardParserTools.GetPartsArrayEnumFromType(typeof(TPart), CardVersion).Item1;
                if (key != partsArrayEnum)
                    throw new InvalidOperationException($"Parts array enumeration [{key}] is different from the expected one [{partsArrayEnum}] according to type {typeof(TPart).Name}.");
            }

            // Get the fallback value
            TPart[] fallback = [];

            // Check to see if the partarray has a value or not
            bool hasValue = partsArray.ContainsKey(key);
            if (!hasValue)
                return fallback;

            // Cast the values
            var value = partsArray[key];
            TPart[] parts = value.Cast<TPart>().ToArray();

            // Now, return the value
            return parts;
        }

        /// <summary>
        /// Gets a string from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>A value, or "individual" if the kind doesn't exist, or an empty string ("") if any other type either doesn't exist or the type is not supported by the card version</returns>
        public string GetString(StringsEnum key)
        {
            // Check for version support
            if (!VcardParserTools.StringSupported(key, CardVersion))
                return "";

            // Get the fallback value
            string fallback = key == StringsEnum.Kind ? "individual" : "";

            // Check to see if the string has a value or not
            bool hasValue = strings.TryGetValue(key, out string value);
            if (!hasValue)
                return fallback;

            // Now, verify that the string is not empty
            hasValue = !string.IsNullOrEmpty(value);
            return hasValue ? value : fallback;
        }

        /// <summary>
        /// Saves this parsed card to the string
        /// </summary>
        public string SaveToString()
        {
            // Initialize the card builder
            var cardBuilder = new StringBuilder();
            var version = CardVersion;

            // First, write the header
            cardBuilder.AppendLine(VcardConstants._beginText);
            cardBuilder.AppendLine($"{VcardConstants._versionSpecifier}:{version}");

            // Then, enumerate all the strings
            StringsEnum[] stringEnums = (StringsEnum[])Enum.GetValues(typeof(StringsEnum));
            foreach (StringsEnum stringEnum in stringEnums)
            {
                // Get the string value
                string stringValue = GetString(stringEnum);
                if (string.IsNullOrEmpty(stringValue))
                    continue;

                // Check to see if kind is specified
                if (!kindExplicitlySpecified && stringEnum == StringsEnum.Kind)
                    continue;

                // Now, locate the prefix and assemble the line
                string prefix = VcardParserTools.GetPrefixFromStringsEnum(stringEnum);
                cardBuilder.AppendLine($"{prefix}{VcardConstants._argumentDelimiter}{stringValue}");
            }

            // Finally, enumerate all the arrays
            PartsArrayEnum[] partsArrayEnums = (PartsArrayEnum[])Enum.GetValues(typeof(PartsArrayEnum));
            foreach (PartsArrayEnum partsArrayEnum in partsArrayEnums)
            {
                // Get the array value
                var array = GetPartsArray<BaseCardPartInfo>(partsArrayEnum);
                if (array is null || array.Length == 0)
                    continue;

                // Get the prefix
                string prefix = VcardParserTools.GetPrefixFromPartsArrayEnum(partsArrayEnum);
                var type = VcardParserTools.GetPartType(prefix);
                string defaultType = type.defaultType;
                string defaultValue = type.defaultValue;

                // Now, assemble the line
                foreach (var part in array)
                {
                    var partBuilder = new StringBuilder();
                    string partRepresentation = part.ToStringVcardInternal(version);
                    string partArguments = CardBuilderTools.BuildArguments(part, version, defaultType, defaultValue);
                    string[] partArgumentsLines = partArguments.SplitNewLines();
                    partBuilder.Append($"{prefix}");
                    partBuilder.Append($"{partArguments}");
                    partBuilder.Append($"{VcardParserTools.MakeStringBlock(partRepresentation, partArgumentsLines[partArgumentsLines.Length - 1].Length + prefix.Length)}");
                    cardBuilder.AppendLine($"{partBuilder}");
                }
            }

            // End the card and return it
            cardBuilder.AppendLine(VcardConstants._endText);
            return cardBuilder.ToString();
        }

        /// <summary>
        /// Saves this parsed card to a file path
        /// </summary>
        /// <param name="path">File path to save this card to</param>
        public void SaveTo(string path)
        {
            // Save all the changes to the file
            var cardString = SaveToString();
            File.WriteAllText(path, cardString);
        }

        /// <summary>
        /// Saves the contact to the returned string
        /// </summary>
        public override string ToString() =>
            SaveToString();

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((Card)obj);

        /// <summary>
        /// Checks to see if both the cards are equal
        /// </summary>
        /// <param name="other">The target <see cref="Card"/> instance to check to see if they equal</param>
        /// <returns>True if all the card elements are equal. Otherwise, false.</returns>
        public bool Equals(Card other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the cards are equal
        /// </summary>
        /// <param name="source">The source <see cref="Card"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="Card"/> instance to check to see if they equal</param>
        /// <returns>True if all the card elements are equal. Otherwise, false.</returns>
        public bool Equals(Card source, Card target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                PartComparison.PartsArrayEnumEqual(source.partsArray, target.partsArray) &&
                PartComparison.StringsEqual(source.strings, target.strings)
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 1047895655;
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<PartsArrayEnum, List<BaseCardPartInfo>>>.Default.GetHashCode(partsArray);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<StringsEnum, string>>.Default.GetHashCode(strings);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(Card a, Card b)
            => a.Equals(b);

        /// <inheritdoc/>
        public static bool operator !=(Card a, Card b)
            => !a.Equals(b);

        internal void AddPartToArray(PartsArrayEnum key, BaseCardPartInfo value)
        {
            if (value is null)
                return;

            // Get the appropriate type and check it
            string prefix = VcardParserTools.GetPrefixFromPartsArrayEnum(key);
            var enumType = VcardParserTools.GetPartType(prefix).enumType;
            if (value.GetType() != enumType)
                return;

            // If we don't have this key yet, add it.
            if (!partsArray.ContainsKey(key))
                partsArray.Add(key, [value]);
            else
            {
                // We need to check the cardinality.
                var cardinality = VcardParserTools.GetPartsArrayEnumFromType(enumType, CardVersion).Item2;
                int actualAltId = partsArray[key][0].AltId;
                if ((cardinality == PartCardinality.ShouldBeOne || cardinality == PartCardinality.MayBeOne) && actualAltId != value.AltId)
                    throw new InvalidOperationException($"Can't overwrite part array {key} with AltID {actualAltId}, because cardinality is {cardinality} and expected AltID is {value.AltId}.");
                partsArray[key].Add(value);
            }
        }

        internal void SetString(StringsEnum key, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            // If we don't have this key yet, add it.
            if (!strings.ContainsKey(key))
                strings.Add(key, value);
            else
                throw new InvalidOperationException($"Can't overwrite string {key}.");
        }

        internal Card(Version version) =>
            this.version = version;
    }
}
