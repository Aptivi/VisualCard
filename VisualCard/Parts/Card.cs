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
using System.Collections.ObjectModel;
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
    [DebuggerDisplay("vCard version {CardVersion.ToString()}, parts: (A [{partsArray.Count}] | S [{strings.Count}]), explicit kind: {kindExplicitlySpecified}")]
    public class Card : IEquatable<Card>
    {
        internal bool kindExplicitlySpecified = false;
        internal Card[] nestedCards = [];
        private readonly Version version;
        private readonly Dictionary<PartsArrayEnum, List<BaseCardPartInfo>> partsArray = [];
        private readonly Dictionary<StringsEnum, List<CardValueInfo<string>>> strings = [];

        /// <summary>
        /// The VCard version
        /// </summary>
        public Version CardVersion =>
            version;

        /// <summary>
        /// List of parsed nested cards
        /// </summary>
        public Card[] NestedCards =>
            nestedCards;

        /// <summary>
        /// Unique ID for this card
        /// </summary>
        public string UniqueId =>
            GetString(StringsEnum.Uid).Length > 0 ? GetString(StringsEnum.Uid)[0].Value : "";

        /// <summary>
        /// Card kind string
        /// </summary>
        public string CardKindStr =>
            strings.ContainsKey(StringsEnum.Kind) && strings[StringsEnum.Kind].Count > 0 ? strings[StringsEnum.Kind][0].Value : "individual";

        /// <summary>
        /// Card kind
        /// </summary>
        public CardKind CardKind =>
            VcardCommonTools.GetKindEnum(CardKindStr);

        /// <summary>
        /// Part array list in a dictionary (for enumeration operations)
        /// </summary>
        public ReadOnlyDictionary<PartsArrayEnum, ReadOnlyCollection<BaseCardPartInfo>> PartsArray =>
            new(partsArray.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.AsReadOnly()));

        /// <summary>
        /// String list in a dictionary (for enumeration operations)
        /// </summary>
        public ReadOnlyDictionary<StringsEnum, ReadOnlyCollection<CardValueInfo<string>>> Strings =>
            new(strings.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.AsReadOnly()));

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <returns>An array of values or an empty part array []</returns>
        public TPart[] GetPartsArray<TPart>()
            where TPart : BaseCardPartInfo
        {
            // Get the parts enumeration according to the type
            var key = VcardParserTools.GetPartsArrayEnumFromType(typeof(TPart), CardVersion, CardKindStr);

            // Now, return the value
            return GetPartsArray<TPart>(key);
        }

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>An array of values or an empty part array []</returns>
        public TPart[] GetPartsArray<TPart>(PartsArrayEnum key)
            where TPart : BaseCardPartInfo =>
            GetPartsArray(typeof(TPart), key).Cast<TPart>().ToArray();

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="partType">Target part type that derives from <see cref="BaseCardPartInfo"/></param>
        /// <returns>An array of values or an empty part array []</returns>
        public BaseCardPartInfo[] GetPartsArray(Type partType)
        {
            // Check the base type
            if (partType.BaseType != typeof(BaseCardPartInfo))
                throw new InvalidOperationException($"Base type is not BaseCardPartInfo [{partType.BaseType.Name}] and the part type is [{partType.Name}] that doesn't represent card part.");

            // Get the parts enumeration according to the type
            var key = VcardParserTools.GetPartsArrayEnumFromType(partType, CardVersion, CardKindStr);

            // Now, return the value
            return GetPartsArray(partType, key);
        }

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="partType">Target part type that derives from <see cref="BaseCardPartInfo"/></param>
        /// <param name="key">A key to use</param>
        /// <returns>An array of values or an empty part array []</returns>
        public BaseCardPartInfo[] GetPartsArray(Type partType, PartsArrayEnum key)
        {
            // Check the base type
            if (partType.BaseType != typeof(BaseCardPartInfo) && partType != typeof(BaseCardPartInfo))
                throw new InvalidOperationException($"Base type is not BaseCardPartInfo [{partType.BaseType.Name}] and the part type is [{partType.Name}] that doesn't represent card part.");

            // Check for version support
            string prefix = VcardParserTools.GetPrefixFromPartsArrayEnum(key);
            var type = VcardParserTools.GetPartType(prefix, CardVersion, CardKindStr);
            if (!type.minimumVersionCondition(CardVersion))
                return [];

            // Get the parts enumeration according to the type
            if (partType != typeof(BaseCardPartInfo))
            {
                // We don't need the base, but a derivative of it. Check it.
                var partsArrayEnum = (PartsArrayEnum)type.enumeration;
                if (key != partsArrayEnum)
                    throw new InvalidOperationException($"Parts array enumeration [{key}] is different from the expected one [{partsArrayEnum}] according to type {typeof(BaseCardPartInfo).Name}.");
            }

            // Get the fallback value
            BaseCardPartInfo[] fallback = [];

            // Check to see if the partarray has a value or not
            bool hasValue = partsArray.ContainsKey(key);
            if (!hasValue)
                return fallback;

            // Cast the values
            var value = partsArray[key];
            BaseCardPartInfo[] parts = [.. value];

            // Now, return the value
            return parts;
        }

        /// <summary>
        /// Gets a string from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>A list of strings that stores a group and a value, or "individual" if the kind doesn't exist, or an empty list if any other type either doesn't exist or the type is not supported by the card version</returns>
        public CardValueInfo<string>[] GetString(StringsEnum key)
        {
            // Check for version support
            string prefix = VcardParserTools.GetPrefixFromStringsEnum(key);
            var partType = VcardParserTools.GetPartType(prefix, CardVersion, CardKindStr);
            if (!partType.minimumVersionCondition(CardVersion))
                return [];

            // Get the fallback value
            string fallback = key == StringsEnum.Kind ? "individual" : "";
            CardValueInfo<string>[] fallbacks =
                !string.IsNullOrEmpty(fallback) ?
                [new CardValueInfo<string>(null, -1, [], "", "", fallback)] :
                [];

            // Check to see if the string has a value or not
            bool hasValue = strings.TryGetValue(key, out var value);
            if (!hasValue)
                return fallbacks;

            // Now, verify that the string is not empty
            hasValue = value.Count > 0;
            return hasValue ? [.. value] : fallbacks;
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
            foreach (StringsEnum stringEnum in strings.Keys)
            {
                // Get the string values
                var array = GetString(stringEnum);
                if (array is null || array.Length == 0)
                    continue;

                // Check to see if kind is specified
                if (!kindExplicitlySpecified && stringEnum == StringsEnum.Kind)
                    continue;

                // Get the prefix
                string prefix = VcardParserTools.GetPrefixFromStringsEnum(stringEnum);
                var type = VcardParserTools.GetPartType(prefix, version, CardKindStr);
                string defaultType = type.defaultType;
                string defaultValueType = type.defaultValueType;

                // Now, assemble the line
                foreach (var part in array)
                {
                    var partBuilder = new StringBuilder();
                    string partArguments = CardBuilderTools.BuildArguments(part, defaultType, defaultValueType);
                    string[] partArgumentsLines = partArguments.SplitNewLines();
                    string group = part.Group;
                    if (!string.IsNullOrEmpty(group))
                        cardBuilder.Append($"{group}.");
                    partBuilder.Append($"{prefix}");
                    partBuilder.Append($"{partArguments}");
                    partBuilder.Append($"{VcardCommonTools.MakeStringBlock(part.Value, partArgumentsLines[partArgumentsLines.Length - 1].Length + prefix.Length)}");
                    cardBuilder.AppendLine($"{partBuilder}");
                }
            }

            // Then, enumerate all the arrays
            foreach (PartsArrayEnum partsArrayEnum in partsArray.Keys)
            {
                // Get the array value
                var array = GetPartsArray<BaseCardPartInfo>(partsArrayEnum);
                if (array is null || array.Length == 0)
                    continue;

                // Get the prefix
                string prefix = VcardParserTools.GetPrefixFromPartsArrayEnum(partsArrayEnum);
                var type = VcardParserTools.GetPartType(prefix, version, CardKindStr);
                string defaultType = type.defaultType;
                string defaultValueType = type.defaultValueType;

                // Now, assemble the line
                foreach (var part in array)
                {
                    var partBuilder = new StringBuilder();
                    string partRepresentation = part.ToStringVcardInternal(version);
                    string partArguments = CardBuilderTools.BuildArguments(part, defaultType, defaultValueType);
                    string[] partArgumentsLines = partArguments.SplitNewLines();
                    string group = part.Group;

                    // Special treatment for vCard 2.1's AGENT property: add the AGENT vcard line by line
                    if (partsArrayEnum == PartsArrayEnum.Agents && version.Major == 2)
                        partRepresentation = "\n" + string.Join("\n", partRepresentation.Split(["\\n", "\\N"], StringSplitOptions.None));
                    if (!string.IsNullOrEmpty(group))
                        cardBuilder.Append($"{group}.");
                    partBuilder.Append($"{prefix}");
                    partBuilder.Append($"{partArguments}");
                    partBuilder.Append($"{VcardCommonTools.MakeStringBlock(partRepresentation, partArgumentsLines[partArgumentsLines.Length - 1].Length + prefix.Length, !(partsArrayEnum == PartsArrayEnum.Agents && version.Major == 2))}");
                    cardBuilder.AppendLine($"{partBuilder}");
                }
            }

            // Finally, save the nested cards
            foreach (var nestedCard in nestedCards)
                cardBuilder.Append(nestedCard.ToString());

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
            int hashCode = 1365540608;
            hashCode = hashCode * -1521134295 + EqualityComparer<Card[]>.Default.GetHashCode(nestedCards);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<PartsArrayEnum, List<BaseCardPartInfo>>>.Default.GetHashCode(partsArray);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<StringsEnum, List<CardValueInfo<string>>>>.Default.GetHashCode(strings);
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
            var partType = VcardParserTools.GetPartType(prefix, CardVersion, CardKindStr);
            var enumType = partType.enumType;
            if (value.GetType() != enumType)
                return;

            // If we don't have this key yet, add it.
            if (!partsArray.ContainsKey(key))
                partsArray.Add(key, [value]);
            else
            {
                // We need to check the cardinality.
                var cardinality = partType.cardinality;
                int actualAltId = partsArray[key][0].AltId;
                bool onlyOne =
                    cardinality == PartCardinality.ShouldBeOne ||
                    cardinality == PartCardinality.MayBeOne;
                bool onlyOneNoAltId =
                    cardinality == PartCardinality.ShouldBeOneNoAltId ||
                    cardinality == PartCardinality.MayBeOneNoAltId;
                if (onlyOne && actualAltId != value.AltId)
                    throw new InvalidOperationException($"Can't overwrite part array {key} with AltID {value.AltId}, because cardinality is {cardinality} and expected AltID is {actualAltId}.");
                if (onlyOneNoAltId)
                    throw new InvalidOperationException($"Can never overwrite part array {key} with AltID {value.AltId}, because cardinality is {cardinality}, even though the expected AltID is {actualAltId}.");
                partsArray[key].Add(value);
            }
        }

        internal void AddString(StringsEnum key, CardValueInfo<string> value)
        {
            if (value is null || string.IsNullOrEmpty(value.Value))
                return;

            // Get the appropriate type
            string prefix = VcardParserTools.GetPrefixFromStringsEnum(key);
            var partType = VcardParserTools.GetPartType(prefix, CardVersion, CardKindStr);

            // If we don't have this key yet, add it.
            if (!strings.ContainsKey(key))
                strings.Add(key, [value]);
            else
            {
                // We need to check the cardinality.
                var cardinality = partType.cardinality;
                int actualAltId = strings[key][0].AltId;
                bool onlyOne =
                    cardinality == PartCardinality.ShouldBeOne ||
                    cardinality == PartCardinality.MayBeOne;
                bool onlyOneNoAltId =
                    cardinality == PartCardinality.ShouldBeOneNoAltId ||
                    cardinality == PartCardinality.MayBeOneNoAltId;
                if (onlyOne && actualAltId != value.AltId)
                    throw new InvalidOperationException($"Can't overwrite string {key} with AltID {value.AltId}, because cardinality is {cardinality} and expected AltID is {actualAltId}.");
                if (onlyOneNoAltId)
                    throw new InvalidOperationException($"Can never overwrite string {key} with AltID {value.AltId}, because cardinality is {cardinality}, even though the expected AltID is {actualAltId}.");
                strings[key].Add(value);
            }
        }

        internal Card(Version version) =>
            this.version = version;
    }
}
