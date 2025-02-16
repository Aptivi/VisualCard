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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Textify.General;
using VisualCard.Common.Diagnostics;
using VisualCard.Common.Parsers;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts;
using VisualCard.Common.Parts.Comparers;
using VisualCard.Common.Parts.Enums;
using VisualCard.Common.Parts.Implementations;
using VisualCard.Parsers;
using VisualCard.Parts.Comparers;
using VisualCard.Parts.Enums;

namespace VisualCard.Parts
{
    /// <summary>
    /// A vCard card instance
    /// </summary>
    [DebuggerDisplay("vCard version {CardVersion.ToString()}, parts: (A [{partsArray.Count}] | S [{strings.Count}] | E [{extraParts.Count}])")]
    public class Card : IEquatable<Card>
    {
        internal List<Card> nestedCards = [];
        private readonly Version version;
        private readonly Dictionary<PartsArrayEnum, List<BasePartInfo>> extraParts = [];
        private readonly Dictionary<CardPartsArrayEnum, List<BaseCardPartInfo>> partsArray = [];
        private readonly Dictionary<CardStringsEnum, List<ValueInfo<string>>> strings = [];

        /// <summary>
        /// The VCard version
        /// </summary>
        public Version CardVersion =>
            version;

        /// <summary>
        /// List of parsed nested cards
        /// </summary>
        public List<Card> NestedCards =>
            nestedCards;

        /// <summary>
        /// Unique ID for this card
        /// </summary>
        public string UniqueId =>
            GetString(CardStringsEnum.Uid).Length > 0 ? GetString(CardStringsEnum.Uid)[0].Value : "";

        /// <summary>
        /// Card kind string
        /// </summary>
        public string CardKindStr =>
            strings.ContainsKey(CardStringsEnum.Kind) && strings[CardStringsEnum.Kind].Count > 0 ? strings[CardStringsEnum.Kind][0].Value : "individual";

        /// <summary>
        /// Card kind
        /// </summary>
        public CardKind CardKind =>
            VcardParserTools.GetKindEnum(CardKindStr);

        /// <summary>
        /// Extra part array list in a dictionary (for enumeration operations)
        /// </summary>
        public ReadOnlyDictionary<PartsArrayEnum, ReadOnlyCollection<BasePartInfo>> ExtraParts =>
            new(extraParts.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.AsReadOnly()));

        /// <summary>
        /// Part array list in a dictionary (for enumeration operations)
        /// </summary>
        public ReadOnlyDictionary<CardPartsArrayEnum, ReadOnlyCollection<BaseCardPartInfo>> PartsArray =>
            new(partsArray.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.AsReadOnly()));

        /// <summary>
        /// String list in a dictionary (for enumeration operations)
        /// </summary>
        public ReadOnlyDictionary<CardStringsEnum, ReadOnlyCollection<ValueInfo<string>>> Strings =>
            new(strings.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.AsReadOnly()));

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <returns>An array of values or an empty part array []</returns>
        public TPart[] GetExtraPartsArray<TPart>()
            where TPart : BasePartInfo
        {
            // Get the extra part enumeration according to the type
            var key = (PartsArrayEnum)VcardParserTools.GetPartsArrayEnumFromType(typeof(TPart), CardVersion, CardKindStr);

            // Now, return the value
            return GetExtraPartsArray<TPart>(key);
        }

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>An array of values or an empty part array []</returns>
        public TPart[] GetExtraPartsArray<TPart>(PartsArrayEnum key)
            where TPart : BasePartInfo =>
            GetExtraPartsArray(typeof(TPart), key).Cast<TPart>().ToArray();

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="partType">Target part type that derives from <see cref="BasePartInfo"/></param>
        /// <returns>An array of values or an empty part array []</returns>
        public BasePartInfo[] GetExtraPartsArray(Type partType)
        {
            // Check the base type
            VerifyBasePartType(partType);

            // Get the extra part enumeration according to the type
            var key = (PartsArrayEnum)VcardParserTools.GetPartsArrayEnumFromType(partType, CardVersion, CardKindStr);

            // Now, return the value
            return GetExtraPartsArray(partType, key);
        }

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="partType">Target part type that derives from <see cref="BasePartInfo"/></param>
        /// <param name="key">A key to use</param>
        /// <returns>An array of values or an empty part array []</returns>
        public BasePartInfo[] GetExtraPartsArray(Type partType, PartsArrayEnum key)
        {
            // Check the type
            VerifyPartsArrayType((CardPartsArrayEnum)key, partType);

            // Check for version support
            string prefix = VcardParserTools.GetPrefixFromPartsArrayEnum((CardPartsArrayEnum)key);
            var type = VcardParserTools.GetPartType(prefix, CardVersion, CardKindStr);
            if (!type.minimumVersionCondition(CardVersion))
                return [];

            // Get the fallback value
            BasePartInfo[] fallback = [];

            // Check to see if the partarray has a value or not
            bool hasValue = extraParts.ContainsKey(key);
            if (!hasValue)
                return fallback;

            // Cast the values
            var value = extraParts[key];
            BasePartInfo[] extraPartsArray = [.. value];

            // Now, return the value
            return extraPartsArray;
        }

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
        public TPart[] GetPartsArray<TPart>(CardPartsArrayEnum key)
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
            VerifyPartType(partType);

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
        public BaseCardPartInfo[] GetPartsArray(Type partType, CardPartsArrayEnum key)
        {
            // Check the type
            VerifyPartsArrayType(key, partType);

            // Check for version support
            string prefix = VcardParserTools.GetPrefixFromPartsArrayEnum(key);
            var type = VcardParserTools.GetPartType(prefix, CardVersion, CardKindStr);
            if (!type.minimumVersionCondition(CardVersion))
                return [];

            // Check to see if the partarray has a value or not
            bool hasValue = partsArray.ContainsKey(key);
            if (!hasValue)
                return [];

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
        public ValueInfo<string>[] GetString(CardStringsEnum key)
        {
            // Check for version support
            string prefix = VcardParserTools.GetPrefixFromStringsEnum(key);
            var partType = VcardParserTools.GetPartType(prefix, CardVersion, CardKindStr);
            if (!partType.minimumVersionCondition(CardVersion))
                return [];

            // Get the fallback value
            string fallback = key == CardStringsEnum.Kind ? "individual" : "";
            ValueInfo<string>[] fallbacks =
                !string.IsNullOrEmpty(fallback) ?
                [new ValueInfo<string>(null, -1, [], fallback)] :
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
        /// Finds a part array from a specified key
        /// </summary>
        /// <param name="prefixToFind">Part of prefix to find (case-insensitive)</param>
        /// <returns>An array of values or an empty part array []</returns>
        public TPart[] FindExtraPartsArray<TPart>(string prefixToFind)
            where TPart : BasePartInfo
        {
            // Get the extra part enumeration according to the type
            var key = (PartsArrayEnum)VcardParserTools.GetPartsArrayEnumFromType(typeof(TPart), CardVersion, CardKindStr);

            // Now, return the value
            return FindExtraPartsArray<TPart>(key, prefixToFind);
        }

        /// <summary>
        /// Finds a part array from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <param name="prefixToFind">Part of prefix to find (case-insensitive)</param>
        /// <returns>An array of values or an empty part array []</returns>
        public TPart[] FindExtraPartsArray<TPart>(PartsArrayEnum key, string prefixToFind)
            where TPart : BasePartInfo =>
            FindExtraPartsArray(typeof(TPart), key, prefixToFind).Cast<TPart>().ToArray();

        /// <summary>
        /// Finds a part array from a specified key
        /// </summary>
        /// <param name="partType">Target part type that derives from <see cref="BasePartInfo"/></param>
        /// <param name="prefixToFind">Part of prefix to find (case-insensitive)</param>
        /// <returns>An array of values or an empty part array []</returns>
        public BasePartInfo[] FindExtraPartsArray(Type partType, string prefixToFind)
        {
            // Check the base type
            VerifyPartType(partType);

            // Get the extra part enumeration according to the type
            var key = (PartsArrayEnum)VcardParserTools.GetPartsArrayEnumFromType(partType, CardVersion, CardKindStr);

            // Now, return the value
            return FindExtraPartsArray(partType, key, prefixToFind);
        }

        /// <summary>
        /// Finds a part array from a specified key
        /// </summary>
        /// <param name="partType">Target part type that derives from <see cref="BasePartInfo"/></param>
        /// <param name="key">A key to use</param>
        /// <param name="prefixToFind">Part of prefix to find (case-insensitive)</param>
        /// <returns>An array of values or an empty part array []</returns>
        public BasePartInfo[] FindExtraPartsArray(Type partType, PartsArrayEnum key, string prefixToFind) =>
            GetExtraPartsArray(partType, key).Where((bpi) =>
            {
                if (bpi is XNameInfo xNameInfo)
                    return xNameInfo.XKeyName?.ContainsWithNoCase(prefixToFind) ?? false;
                else if (bpi is ExtraInfo extraInfo)
                    return extraInfo.KeyName?.ContainsWithNoCase(prefixToFind) ?? false;
                return false;
            }).ToArray();

        /// <summary>
        /// Saves this parsed card to the string
        /// </summary>
        /// <param name="validate">Whether to validate all the fields or not</param>
        public string SaveToString(bool validate = false)
        {
            // Check to see if we need to validate
            if (validate)
            {
                LoggingTools.Info("Validation requested before saving");
                Validate();
            }

            // Initialize the card builder
            var cardBuilder = new StringBuilder();
            var version = CardVersion;

            // First, write the header
            LoggingTools.Debug("Writing header");
            cardBuilder.AppendLine(VcardConstants._beginText);
            cardBuilder.AppendLine($"{CommonConstants._versionSpecifier}:{version}");

            // Then, enumerate all the strings
            foreach (CardStringsEnum stringEnum in strings.Keys)
            {
                // Get the string values
                var array = GetString(stringEnum);
                if (array is null || array.Length == 0)
                    continue;
                LoggingTools.Debug("Installing {0} strings to card text...", array.Length);

                // Get the prefix
                string prefix = VcardParserTools.GetPrefixFromStringsEnum(stringEnum);
                var type = VcardParserTools.GetPartType(prefix, version, CardKindStr);
                string defaultType = type.defaultType;
                string defaultValueType = type.defaultValueType;

                // Now, assemble the line
                foreach (var part in array)
                {
                    var partBuilder = new StringBuilder();
                    string partArguments = BuilderTools.BuildArguments(part, defaultType, defaultValueType);
                    string[] partArgumentsLines = partArguments.SplitNewLines();
                    string group = part.Group;
                    if (!string.IsNullOrEmpty(group))
                        partBuilder.Append($"{group}.");
                    partBuilder.Append($"{prefix}");
                    partBuilder.Append($"{partArguments}");
                    partBuilder.Append($"{CommonTools.MakeStringBlock(part.Value, partArgumentsLines[partArgumentsLines.Length - 1].Length + prefix.Length, encoding: part.Encoding ?? "")}");
                    LoggingTools.Debug("Adding part to line with length {0} [prefix: {1}, {2}]", partBuilder.Length, prefix, partArguments);
                    cardBuilder.AppendLine($"{partBuilder}");
                }
            }

            // Then, enumerate all the arrays
            foreach (CardPartsArrayEnum partsArrayEnum in partsArray.Keys)
            {
                // Get the array value
                var array = GetPartsArray<BaseCardPartInfo>(partsArrayEnum);
                if (array is null || array.Length == 0)
                    continue;
                LoggingTools.Debug("Installing {0} parts to card text...", array.Length);

                // Get the prefix
                string prefix = VcardParserTools.GetPrefixFromPartsArrayEnum(partsArrayEnum);
                var type = VcardParserTools.GetPartType(prefix, version, CardKindStr);
                string defaultType = type.defaultType;
                string defaultValueType = type.defaultValueType;

                // Now, assemble the line
                foreach (var part in array)
                {
                    var partBuilder = new StringBuilder();
                    string partRepresentation = part.ToStringInternal(version);
                    string partArguments = BuilderTools.BuildArguments(part, defaultType, defaultValueType);
                    string[] partArgumentsLines = partArguments.SplitNewLines();
                    string group = part.Group;

                    // Special treatment for vCard 2.1's AGENT property: add the AGENT vcard line by line
                    if (partsArrayEnum == CardPartsArrayEnum.Agents && version.Major == 2)
                        partRepresentation = "\n" + string.Join("\n", partRepresentation.Split(["\\n", "\\N"], StringSplitOptions.None));
                    if (!string.IsNullOrEmpty(group))
                        partBuilder.Append($"{group}.");
                    partBuilder.Append($"{prefix}");
                    partBuilder.Append($"{partArguments}");
                    partBuilder.Append($"{CommonTools.MakeStringBlock(partRepresentation, partArgumentsLines[partArgumentsLines.Length - 1].Length + prefix.Length, !(partsArrayEnum == CardPartsArrayEnum.Agents && version.Major == 2), part.Encoding ?? "")}");
                    LoggingTools.Debug("Adding part to line with length {0} [prefix: {1}, {2}]", partBuilder.Length, prefix, partArguments);
                    cardBuilder.AppendLine($"{partBuilder}");
                }
            }

            // Then, enumerate all the extra arrays
            foreach (PartsArrayEnum partsArrayEnum in extraParts.Keys)
            {
                // Get the array value
                var array = GetExtraPartsArray<BasePartInfo>(partsArrayEnum);
                if (array is null || array.Length == 0)
                    continue;
                LoggingTools.Debug("Installing {0} extra parts to card text...", array.Length);

                // Get the prefix
                string prefix = VcardParserTools.GetPrefixFromPartsArrayEnum((CardPartsArrayEnum)partsArrayEnum);
                var type = VcardParserTools.GetPartType(prefix, version, CardKindStr);
                string defaultType = type.defaultType;
                string defaultValueType = type.defaultValueType;

                // Now, assemble the line
                foreach (var part in array)
                {
                    var partBuilder = new StringBuilder();
                    string partRepresentation = part.ToStringInternal(version);
                    string partArguments = BuilderTools.BuildArguments(part, defaultType, defaultValueType);
                    string[] partArgumentsLines = partArguments.SplitNewLines();
                    string group = part.Group;

                    // Special treatment for vCard 2.1's AGENT property: add the AGENT vcard line by line
                    if ((CardPartsArrayEnum)partsArrayEnum == CardPartsArrayEnum.Agents && version.Major == 2)
                        partRepresentation = "\n" + string.Join("\n", partRepresentation.Split(["\\n", "\\N"], StringSplitOptions.None));
                    if (!string.IsNullOrEmpty(group))
                        partBuilder.Append($"{group}.");
                    partBuilder.Append($"{prefix}");
                    partBuilder.Append($"{partArguments}");
                    partBuilder.Append($"{CommonTools.MakeStringBlock(partRepresentation, partArgumentsLines[partArgumentsLines.Length - 1].Length + prefix.Length, !((CardPartsArrayEnum)partsArrayEnum == CardPartsArrayEnum.Agents && version.Major == 2), part.Encoding ?? "")}");
                    LoggingTools.Debug("Adding extra part to line with length {0} [prefix: {1}, {2}]", partBuilder.Length, prefix, partArguments);
                    cardBuilder.AppendLine($"{partBuilder}");
                }
            }

            // Finally, save the nested cards
            LoggingTools.Debug("Installing {0} nested cards to card text...", nestedCards.Count);
            foreach (var nestedCard in nestedCards)
                cardBuilder.Append(nestedCard.ToString());

            // End the card and return it
            LoggingTools.Debug("Writing footer...");
            cardBuilder.AppendLine(VcardConstants._endText);
            LoggingTools.Info("Returning card text with length {0}...", cardBuilder.Length);
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
        /// Deletes a string from the list of string values
        /// </summary>
        /// <param name="stringsEnum">String type</param>
        /// <param name="idx">Index of a string value</param>
        /// <returns>True if successful; false otherwise</returns>
        public bool DeleteString(CardStringsEnum stringsEnum, int idx)
        {
            // Get the string values
            var stringValues = GetString(stringsEnum);

            // Check the index
            if (idx >= stringValues.Length)
                return false;

            // Remove the string value
            var stringValue = strings[stringsEnum][idx];
            bool result = strings[stringsEnum].Remove(stringValue);
            LoggingTools.Debug("Removal of {0} from {1} result: {2}", idx, stringsEnum, result);

            // Delete section if needed
            if (strings[stringsEnum].Count == 0)
            {
                LoggingTools.Warning("Deleting dangling section {0}...", stringsEnum);
                strings.Remove(stringsEnum);
            }
            return result;
        }

        /// <summary>
        /// Deletes a part from the list of parts
        /// </summary>
        /// <param name="partsArrayEnum">Part array type</param>
        /// <param name="idx">Index of a part</param>
        /// <returns>True if successful; false otherwise</returns>
        public bool DeletePartsArray(CardPartsArrayEnum partsArrayEnum, int idx)
        {
            // Get the part type
            string prefix = VcardParserTools.GetPrefixFromPartsArrayEnum(partsArrayEnum);
            var type = VcardParserTools.GetPartType(prefix, CardVersion, CardKindStr);
            var partType = type.enumType ??
                throw new ArgumentException("Can't determine enumeration type to delete part.");

            // Remove the string value
            return DeletePartsArray(partsArrayEnum, partType, idx);
        }

        /// <summary>
        /// Deletes a part from the list of parts
        /// </summary>
        /// <param name="partsArrayEnum">Part array type</param>
        /// <param name="enumType">Enumeration type</param>
        /// <param name="idx">Index of a part</param>
        /// <returns>True if successful; false otherwise</returns>
        public bool DeletePartsArray(CardPartsArrayEnum partsArrayEnum, Type enumType, int idx)
        {
            // Get the string values
            var parts = GetPartsArray(enumType, partsArrayEnum);

            // Check the index
            if (idx >= parts.Length)
                return false;

            // Remove the string value
            return DeletePartsArrayInternal(partsArrayEnum, idx);
        }

        /// <summary>
        /// Deletes a part from the list of parts
        /// </summary>
        /// <param name="idx">Index of a part</param>
        /// <returns>True if successful; false otherwise</returns>
        public bool DeletePartsArray<TPart>(int idx)
            where TPart : BaseCardPartInfo
        {
            // Get the parts enumeration according to the type
            var key = VcardParserTools.GetPartsArrayEnumFromType(typeof(TPart), CardVersion, CardKindStr);

            // Remove the part
            return DeletePartsArray<TPart>(key, idx);
        }

        /// <summary>
        /// Deletes a part from the list of parts
        /// </summary>
        /// <param name="partsArrayEnum">Part array type</param>
        /// <param name="idx">Index of a part</param>
        /// <returns>True if successful; false otherwise</returns>
        public bool DeletePartsArray<TPart>(CardPartsArrayEnum partsArrayEnum, int idx)
            where TPart : BaseCardPartInfo
        {
            // Get the parts
            var parts = GetPartsArray(typeof(TPart), partsArrayEnum);

            // Check the index
            if (idx >= parts.Length)
                return false;

            // Remove the part
            return DeletePartsArrayInternal(partsArrayEnum, idx);
        }

        internal bool DeletePartsArrayInternal(CardPartsArrayEnum partsArrayEnum, int idx)
        {
            if (partsArrayEnum is CardPartsArrayEnum.IanaNames or CardPartsArrayEnum.NonstandardNames)
            {
                // Remove the extra part
                var extraPartEnum = (PartsArrayEnum)partsArrayEnum;
                var part = extraParts[extraPartEnum][idx];
                bool result = extraParts[extraPartEnum].Remove(part);
                LoggingTools.Debug("Removal of {0} from {1} result: {2}", idx, partsArrayEnum, result);
                if (extraParts[extraPartEnum].Count == 0)
                {
                    LoggingTools.Warning("Deleting dangling section {0}...", extraPartEnum);
                    extraParts.Remove(extraPartEnum);
                }
                return result;
            }
            else
            {
                // Remove the part
                var part = partsArray[partsArrayEnum][idx];
                bool result = partsArray[partsArrayEnum].Remove(part);
                LoggingTools.Debug("Removal of {0} from {1} result: {2}", idx, partsArrayEnum, result);
                if (partsArray[partsArrayEnum].Count == 0)
                {
                    LoggingTools.Warning("Deleting dangling section {0}...", partsArrayEnum);
                    partsArray.Remove(partsArrayEnum);
                }
                return result;
            }
        }

        /// <summary>
        /// Adds a part to the array
        /// </summary>
        /// <typeparam name="TPart">Part type to add</typeparam>
        /// <param name="rawValue">Raw value representing a group of values delimited by the semicolon</param>
        /// <param name="group">Property group (can be nested with a dot)</param>
        /// <param name="extraPrefix">Extra prefix to append (only for <see cref="PartsArrayEnum.NonstandardNames"/> and <see cref="PartsArrayEnum.IanaNames"/>)</param>
        /// <param name="args">Argument list to be added</param>
        public void AddPartToArray<TPart>(string rawValue, string group = "", string extraPrefix = "", params ArgumentInfo[] args)
            where TPart : BaseCardPartInfo
        {
            // Get the parts enumeration according to the type
            var key = VcardParserTools.GetPartsArrayEnumFromType(typeof(TPart), CardVersion, CardKindStr);

            // Now, add the part
            AddPartToArray<TPart>(key, rawValue, group, extraPrefix, args);
        }

        /// <summary>
        /// Adds a part to the array
        /// </summary>
        /// <typeparam name="TPart">Part type to add</typeparam>
        /// <param name="key">Part array type</param>
        /// <param name="rawValue">Raw value representing a group of values delimited by the semicolon</param>
        /// <param name="group">Property group (can be nested with a dot)</param>
        /// <param name="extraPrefix">Extra prefix to append (only for <see cref="PartsArrayEnum.NonstandardNames"/> and <see cref="PartsArrayEnum.IanaNames"/>)</param>
        /// <param name="args">Argument list to be added</param>
        public void AddPartToArray<TPart>(CardPartsArrayEnum key, string rawValue, string group = "", string extraPrefix = "", params ArgumentInfo[] args)
            where TPart : BaseCardPartInfo =>
            AddPartToArray(key, typeof(TPart), rawValue, group, extraPrefix, args);

        /// <summary>
        /// Adds a part to the array
        /// </summary>
        /// <param name="key">Part array type</param>
        /// <param name="rawValue">Raw value representing a group of values delimited by the semicolon</param>
        /// <param name="group">Property group (can be nested with a dot)</param>
        /// <param name="extraPrefix">Extra prefix to append (only for <see cref="PartsArrayEnum.NonstandardNames"/> and <see cref="PartsArrayEnum.IanaNames"/>)</param>
        /// <param name="args">Argument list to be added</param>
        public void AddPartToArray(CardPartsArrayEnum key, string rawValue, string group = "", string extraPrefix = "", params ArgumentInfo[] args)
        {
            // Get the part type
            string prefix = VcardParserTools.GetPrefixFromPartsArrayEnum(key);
            var type = VcardParserTools.GetPartType(prefix, CardVersion, CardKindStr);
            var partType = type.enumType ??
                throw new ArgumentException("Can't determine enumeration type to add part.");

            // Now, add the part
            AddPartToArray(key, partType, rawValue, group, extraPrefix, args);
        }

        /// <summary>
        /// Adds a part to the array
        /// </summary>
        /// <param name="key">Part array type</param>
        /// <param name="partType">Enumeration type</param>
        /// <param name="rawValue">Raw value representing a group of values delimited by the semicolon</param>
        /// <param name="group">Property group (can be nested with a dot)</param>
        /// <param name="extraPrefix">Extra prefix to append (only for <see cref="PartsArrayEnum.NonstandardNames"/> and <see cref="PartsArrayEnum.IanaNames"/>)</param>
        /// <param name="args">Argument list to be added</param>
        public void AddPartToArray(CardPartsArrayEnum key, Type partType, string rawValue, string group = "", string extraPrefix = "", params ArgumentInfo[] args)
        {
            VerifyPartsArrayType(key, partType);

            // Get the prefix and build the resultant line
            string prefix = VcardParserTools.GetPrefixFromPartsArrayEnum(key);
            if (key == CardPartsArrayEnum.IanaNames || key == CardPartsArrayEnum.NonstandardNames)
                prefix += extraPrefix.ToUpper();
            string line = CommonTools.BuildRawValue(prefix, rawValue, group, args);

            // Process the value
            VcardParser.Process(line, this, version);
        }

        internal void AddPartToArray(CardPartsArrayEnum key, BaseCardPartInfo value)
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
            {
                LoggingTools.Debug("Adding part storage: {0}", key);
                partsArray.Add(key, []);
            }

            // Maybe somehow we no longer have any value info but we still have the key entry?
            if (partsArray[key].Count > 0)
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
                LoggingTools.Debug("Checking altid [actual: {0}, value: {1}] with {2} [{3}, {4}]", actualAltId, value.AltId, cardinality, onlyOne, onlyOneNoAltId);
                if (onlyOne && actualAltId != value.AltId)
                    throw new InvalidOperationException($"Can't overwrite part array {key} with AltID {value.AltId}, because cardinality is {cardinality} and expected AltID is {actualAltId}.");
                if (onlyOneNoAltId)
                    throw new InvalidOperationException($"Can never overwrite part array {key} with AltID {value.AltId}, because cardinality is {cardinality}, even though the expected AltID is {actualAltId}.");
            }

            // Add this value info!
            LoggingTools.Debug("Adding value to storage: {0}", key);
            partsArray[key].Add(value);
        }

        internal void AddExtraPartToArray(PartsArrayEnum key, BasePartInfo value)
        {
            if (value is null)
                return;

            // Get the appropriate type and check it
            string prefix = VcardParserTools.GetPrefixFromPartsArrayEnum((CardPartsArrayEnum)key);
            var partType = VcardParserTools.GetPartType(prefix, CardVersion, CardKindStr);
            var enumType = partType.enumType;
            if (value.GetType() != enumType)
                return;

            // If we don't have this key yet, add it.
            if (!extraParts.ContainsKey(key))
            {
                LoggingTools.Debug("Adding part storage: {0}", key);
                extraParts.Add(key, []);
            }

            // Maybe somehow we no longer have any value info but we still have the key entry?
            if (extraParts[key].Count > 0)
            {
                // We need to check the cardinality.
                var cardinality = partType.cardinality;
                int actualAltId = extraParts[key][0].AltId;
                bool onlyOne =
                    cardinality == PartCardinality.ShouldBeOne ||
                    cardinality == PartCardinality.MayBeOne;
                bool onlyOneNoAltId =
                    cardinality == PartCardinality.ShouldBeOneNoAltId ||
                    cardinality == PartCardinality.MayBeOneNoAltId;
                LoggingTools.Debug("Checking altid [actual: {0}, value: {1}] with {2} [{3}, {4}]", actualAltId, value.AltId, cardinality, onlyOne, onlyOneNoAltId);
                if (onlyOne && actualAltId != value.AltId)
                    throw new InvalidOperationException($"Can't overwrite part array {key} with AltID {value.AltId}, because cardinality is {cardinality} and expected AltID is {actualAltId}.");
                if (onlyOneNoAltId)
                    throw new InvalidOperationException($"Can never overwrite part array {key} with AltID {value.AltId}, because cardinality is {cardinality}, even though the expected AltID is {actualAltId}.");
            }

            // Add this value info!
            LoggingTools.Debug("Adding value to storage: {0}", key);
            extraParts[key].Add(value);
        }

        /// <summary>
        /// Adds a string to the array
        /// </summary>
        /// <param name="key">String type</param>
        /// <param name="rawValue">Raw value representing a group of values delimited by the semicolon</param>
        /// <param name="group">Property group (can be nested with a dot)</param>
        /// <param name="args">Argument list to be added</param>
        public void AddString(CardStringsEnum key, string rawValue, string group = "", params ArgumentInfo[] args)
        {
            // Get the part type and build the line
            string prefix = VcardParserTools.GetPrefixFromStringsEnum(key);
            string line = CommonTools.BuildRawValue(prefix, rawValue, group, args);

            // Process the value
            VcardParser.Process(line, this, version);
        }

        internal void AddString(CardStringsEnum key, ValueInfo<string> value)
        {
            if (value is null || string.IsNullOrEmpty(value.Value))
                return;

            // Get the appropriate type
            string prefix = VcardParserTools.GetPrefixFromStringsEnum(key);
            var partType = VcardParserTools.GetPartType(prefix, CardVersion, CardKindStr);

            // If we don't have this key yet, add it.
            if (!strings.ContainsKey(key))
            {
                LoggingTools.Debug("Adding string storage: {0}", key);
                strings.Add(key, []);
            }

            // Maybe somehow we no longer have any value info but we still have the key entry?
            if (strings[key].Count > 0)
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
                LoggingTools.Debug("Checking altid [actual: {0}, value: {1}] with {2} [{3}, {4}]", actualAltId, value.AltId, cardinality, onlyOne, onlyOneNoAltId);
                if (onlyOne && actualAltId != value.AltId)
                    throw new InvalidOperationException($"Can't overwrite string {key} with AltID {value.AltId}, because cardinality is {cardinality} and expected AltID is {actualAltId}.");
                if (onlyOneNoAltId)
                    throw new InvalidOperationException($"Can never overwrite string {key} with AltID {value.AltId}, because cardinality is {cardinality}, even though the expected AltID is {actualAltId}.");
            }

            // Add this value info!
            LoggingTools.Debug("Adding value to storage: {0}", key);
            strings[key].Add(value);
        }

        /// <summary>
        /// Validates the card
        /// </summary>
        /// <exception cref="InvalidDataException"></exception>
        public void Validate()
        {
            // Track the required fields
            List<string> expectedFieldList = [];
            var partType = VcardParserTools.GetPartType(VcardConstants._nameSpecifier, CardVersion, CardKindStr);
            var nameCardinality = partType.cardinality;
            if (nameCardinality == PartCardinality.ShouldBeOne)
                expectedFieldList.Add(VcardConstants._nameSpecifier);
            if (CardVersion.Major >= 3)
                expectedFieldList.Add(VcardConstants._fullNameSpecifier);
            LoggingTools.Debug("Expected fields: {0} [{1}]", expectedFieldList.Count, string.Join(", ", expectedFieldList));

            // Now, check for requirements
            string[] expectedFields = [.. expectedFieldList];
            if (!ValidateFields(ref expectedFields, out string[] actualFields))
                throw new InvalidDataException($"The following keys [{string.Join(", ", expectedFields)}] are required. Got [{string.Join(", ", actualFields)}].");

            // Check for organization vCards that may not have MEMBER properties
            string[] forbiddenOrgFields = [VcardConstants._memberSpecifier];
            if (CardKind != CardKind.Group && ValidateFields(ref forbiddenOrgFields, out _))
            {
                LoggingTools.Error("Unexpected field in card kind {0}: MEMBER", CardKind);
                throw new InvalidDataException($"{CardKind} vCards are forbidden from having MEMBER properties.");
            }
        }

        private bool ValidateFields(ref string[] expectedFields, out string[] actualFields)
        {
            // Track the required fields
            List<string> actualFieldList = [];

            // Requirement checks
            foreach (string expectedFieldName in expectedFields)
            {
                var partType = VcardParserTools.GetPartType(expectedFieldName, CardVersion, CardKindStr);
                switch (partType.type)
                {
                    case PartType.Strings:
                        {
                            var values = GetString((CardStringsEnum)partType.enumeration);
                            bool exists = values.Length > 0;
                            if (exists)
                            {
                                LoggingTools.Debug("Added {0} to actual field list", expectedFieldName);
                                actualFieldList.Add(expectedFieldName);
                            }
                        }
                        break;
                    case PartType.PartsArray:
                        {
                            if (partType.enumType is null)
                                continue;
                            var values = GetPartsArray(partType.enumType, (CardPartsArrayEnum)partType.enumeration);
                            bool exists = values.Length > 0;
                            if (exists)
                            {
                                LoggingTools.Debug("Added {0} to actual field list", expectedFieldName);
                                actualFieldList.Add(expectedFieldName);
                            }
                        }
                        break;
                }
            }
            Array.Sort(expectedFields);
            actualFieldList.Sort();
            actualFields = [.. actualFieldList];
            LoggingTools.Debug("Field count: {0}", actualFields.Length);
            return actualFields.SequenceEqual(expectedFields);
        }

        internal void VerifyBasePartType(Type partType)
        {
            // Check the base type
            if (partType.BaseType != typeof(BasePartInfo) && partType != typeof(BasePartInfo))
                throw new InvalidOperationException($"Base type is not BasePartInfo [{partType.BaseType.Name}] and the part type is [{partType.Name}] that doesn't represent the part.");
        }

        internal void VerifyPartType(Type partType)
        {
            // Check the base type
            if (partType.BaseType != typeof(BaseCardPartInfo) && partType != typeof(BaseCardPartInfo))
                throw new InvalidOperationException($"Base type is not BaseCardPartInfo [{partType.BaseType.Name}] and the part type is [{partType.Name}] that doesn't represent card part.");
        }

        internal void VerifyPartsArrayType(CardPartsArrayEnum key, Type partType)
        {
            // Check the base type
            if (key == CardPartsArrayEnum.IanaNames || key == CardPartsArrayEnum.NonstandardNames)
                VerifyBasePartType(partType);
            else
                VerifyPartType(partType);

            // Get the type
            string prefix = VcardParserTools.GetPrefixFromPartsArrayEnum(key);
            var type = VcardParserTools.GetPartType(prefix, CardVersion, CardKindStr);

            // Get the parts enumeration according to the type
            if (partType != typeof(BaseCardPartInfo))
            {
                // We don't need the base, but a derivative of it. Check it.
                var partsArrayEnum = (CardPartsArrayEnum)type.enumeration;
                LoggingTools.Debug("Comparing {0} and {1}", key, partsArrayEnum);
                if (key != partsArrayEnum)
                    throw new InvalidOperationException($"Parts array enumeration [{key}] is different from the expected one [{partsArrayEnum}] according to type {typeof(BaseCardPartInfo).Name}.");
            }
        }

        /// <summary>
        /// Saves the contact to the returned string
        /// </summary>
        public override string ToString() =>
            SaveToString();

        /// <summary>
        /// Saves the contact to the returned string
        /// </summary>
        /// <param name="validate">Whether to validate all the fields or not</param>
        public string ToString(bool validate) =>
            SaveToString(validate);

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
                CommonComparison.ExtraPartsEnumEqual(source.extraParts, target.extraParts) &&
                CardPartComparison.PartsArrayEnumEqual(source.partsArray, target.partsArray) &&
                CardPartComparison.StringsEqual(source.strings, target.strings)
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 1365540608;
            hashCode = hashCode * -1521134295 + EqualityComparer<List<Card>>.Default.GetHashCode(nestedCards);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CardPartsArrayEnum, List<BaseCardPartInfo>>>.Default.GetHashCode(partsArray);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CardStringsEnum, List<ValueInfo<string>>>>.Default.GetHashCode(strings);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(Card a, Card b)
            => a.Equals(b);

        /// <inheritdoc/>
        public static bool operator !=(Card a, Card b)
            => !a.Equals(b);

        /// <summary>
        /// Creates a new empty card
        /// </summary>
        /// <param name="version">vCard version to use</param>
        /// <exception cref="ArgumentException"></exception>
        public Card(Version version)
        {
            if (!VcardParserTools.VerifySupportedVersion(version))
                throw new ArgumentException($"Invalid vCard version {version} specified. The supported versions are 2.1, 3.0, 4.0, and 5.0.");
            this.version = version;
        }
    }
}
