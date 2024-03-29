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
        private readonly BaseVcardParser _parser;
        private readonly Dictionary<PartsEnum, BaseCardPartInfo> parts = [];
        private readonly Dictionary<PartsArrayEnum, List<BaseCardPartInfo>> partsArray = [];
        private readonly Dictionary<StringsEnum, string> strings = [];

        /// <summary>
        /// The VCard version
        /// </summary>
        public Version CardVersion =>
            Parser.CardVersion;

        internal BaseVcardParser Parser =>
            _parser;

        /// <summary>
        /// Saves the contact file to the path
        /// </summary>
        /// <param name="path">Path to the VCard file that is going to be created</param>
        public void SaveTo(string path) =>
            Parser.SaveTo(path, this);

        /// <summary>
        /// Saves the contact to the returned string
        /// </summary>
        public string SaveToString() =>
            Parser.SaveToString(this);

        /// <summary>
        /// Gets a part array from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>An array of values or an empty part array []</returns>
        public BaseCardPartInfo[] GetPartsArray(PartsArrayEnum key)
        {
            // Check for version support
            if (!VcardParserTools.EnumArrayTypeSupported(key, CardVersion))
                return null;

            // Get the fallback value
            BaseCardPartInfo[] fallback = [];

            // Check to see if the partarray has a value or not
            bool hasValue = partsArray.TryGetValue(key, out List<BaseCardPartInfo> value);
            if (!hasValue)
                return fallback;

            // Now, return the value
            return [.. value];
        }

        /// <summary>
        /// Gets a part from a specified key
        /// </summary>
        /// <param name="key">A key to use</param>
        /// <returns>A value or an empty part if any other type doesn't exist</returns>
        public BaseCardPartInfo GetPart(PartsEnum key)
        {
            // Check for version support
            if (!VcardParserTools.EnumTypeSupported(key, CardVersion))
                return null;

            // Get the fallback value
            BaseCardPartInfo fallback = default;

            // Check to see if the part has a value or not
            bool hasValue = parts.TryGetValue(key, out BaseCardPartInfo value);
            if (!hasValue)
                return fallback;

            // Now, return the value
            return value;
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
        /// Saves the contact to the returned string
        /// </summary>
        public override string ToString() =>
            SaveToString();

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            base.Equals(obj);

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
            if (source is null)
                return false;

            // Check all the properties
            return
                PartComparison.PartsEnumEqual(source.parts, target.parts) &&
                PartComparison.PartsArrayEnumEqual(source.partsArray, target.partsArray) &&
                PartComparison.StringsEqual(source.strings, target.strings)
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -1645291684;
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<PartsEnum, BaseCardPartInfo>>.Default.GetHashCode(parts);
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
                partsArray[key].Add(value);
        }

        internal void SetPart(PartsEnum key, BaseCardPartInfo value)
        {
            if (value is null)
                return;

            // Get the appropriate type and check it
            string prefix = VcardParserTools.GetPrefixFromPartsEnum(key);
            var enumType = VcardParserTools.GetPartType(prefix).enumType;
            if (value.GetType() != enumType)
                return;

            // If we don't have this key yet, add it.
            if (!parts.ContainsKey(key))
                parts.Add(key, value);
            else
                parts[key] = value;
        }

        internal void SetString(StringsEnum key, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            // If we don't have this key yet, add it.
            if (!strings.ContainsKey(key))
                strings.Add(key, value);
            else
                strings[key] = value;
        }

        internal Card(BaseVcardParser parser) =>
            _parser = parser;
    }
}
