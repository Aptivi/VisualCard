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
using System.Linq;
using System.Text;
using Textify.General;
using VisualCard.Common.Diagnostics;

namespace VisualCard.Common.Parsers.Arguments
{
    /// <summary>
    /// Argument info for properties
    /// </summary>
    [DebuggerDisplay("{Key} = [{string.Join(\", \", AllValues)}]")]
    public class ArgumentInfo : IEquatable<ArgumentInfo>
    {
        private readonly string key = "";
        private readonly (bool caseSensitive, string value)[] values = [];

        /// <summary>
        /// Argument key name
        /// </summary>
        public string Key =>
            key;

        /// <summary>
        /// Argument values (without double quotes) and their case sensitivity indicators
        /// </summary>
        public (bool caseSensitive, string value)[] Values =>
            values;

        /// <summary>
        /// Argument values (without double quotes)
        /// </summary>
        public string[] AllValues =>
            Values.Select((tuple) => tuple.value).ToArray();

        /// <summary>
        /// Matches the argument value
        /// </summary>
        /// <param name="value">Value to match</param>
        /// <returns>True if they equal (case sensitive if the value's case sensitivity is true; case insensitive if otherwise); false otherwise.</returns>
        public bool MatchValue(string value)
        {
            bool equals = false;
            LoggingTools.Info("Matching {0}...", value);
            foreach ((bool caseSensitive, string targetValue) in Values)
            {
                LoggingTools.Debug("Processing {0} [CS: {1}]...", targetValue, caseSensitive);
                equals =
                    caseSensitive ?
                    targetValue == value :
                    targetValue.EqualsNoCase(value);
                LoggingTools.Debug("Equals is {0} [{1}, CS: {2}].", equals, targetValue, caseSensitive);
                if (equals)
                    break;
            }
            LoggingTools.Info("Found! {0}", value);
            return equals;
        }

        internal string BuildArguments()
        {
            var argBuilder = new StringBuilder();
            string key = Key;
            LoggingTools.Info("Key: {0}", key);
            if (!string.IsNullOrEmpty(key))
            {
                LoggingTools.Debug("Appending key argument {0}", key);
                argBuilder.Append($"{key}=");
            }
            for (int i = 0; i < Values.Length; i++)
            {
                (bool caseSensitive, string value) value = Values[i];
                argBuilder.Append($"{(value.caseSensitive ? $"\"{value.value}\"" : value.value)}");
                LoggingTools.Debug("Added value {0} [CS: {1}]", value.value, value.caseSensitive);
                if (i < Values.Length - 1)
                {
                    LoggingTools.Debug("Value index is {0} and is less than {1}", i, Values.Length - 1);
                    argBuilder.Append(CommonConstants._valueDelimiter);
                }
            }
            LoggingTools.Info("Argument: {0}", argBuilder.ToString());
            return argBuilder.ToString();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((ArgumentInfo)obj);

        /// <summary>
        /// Checks to see if both the cards are equal
        /// </summary>
        /// <param name="other">The target <see cref="ArgumentInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the card elements are equal. Otherwise, false.</returns>
        public bool Equals(ArgumentInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the cards are equal
        /// </summary>
        /// <param name="source">The source <see cref="ArgumentInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="ArgumentInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the card elements are equal. Otherwise, false.</returns>
        public bool Equals(ArgumentInfo source, ArgumentInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.Key.Equals(source.Key, StringComparison.OrdinalIgnoreCase) &&
                target.Values.Any((tuple) => source.MatchValue(tuple.value))
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -933998265;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Key);
            hashCode = hashCode * -1521134295 + EqualityComparer<(bool caseSensitive, string value)[]>.Default.GetHashCode(Values);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(ArgumentInfo a, ArgumentInfo b)
            => a.Equals(b);

        /// <inheritdoc/>
        public static bool operator !=(ArgumentInfo a, ArgumentInfo b)
            => !a.Equals(b);

        /// <summary>
        /// Builds the argument info class
        /// </summary>
        /// <param name="kvp">Key and value pair that describes one argument</param>
        public ArgumentInfo(string kvp)
        {
            LoggingTools.Info("Key-value pair: {0}", kvp);
            if (kvp.Contains(CommonConstants._argumentValueDelimiter))
            {
                LoggingTools.Debug("Splitting with idx {0} as medium...", kvp.IndexOf(CommonConstants._argumentValueDelimiter));
                string keyStr = kvp.Substring(0, kvp.IndexOf(CommonConstants._argumentValueDelimiter));
                string valueStr = kvp.RemovePrefix($"{keyStr}{CommonConstants._argumentValueDelimiter}").Trim();
                LoggingTools.Debug("Key and Value: {0}, {1}", keyStr, valueStr);
                var info = new ArgumentInfo(keyStr.Trim(), valueStr);
                key = info.key;
                values = info.values;
            }
            else
            {
                LoggingTools.Warning("Key is not provided! {0} | Assuming value...", kvp);
                var info = new ArgumentInfo("", kvp.Trim());
                key = "";
                values = info.values;
            }
            LoggingTools.Info("Installed values: {0}", values.Length);
        }

        /// <summary>
        /// Builds the argument info class
        /// </summary>
        /// <param name="key">Key name</param>
        /// <param name="value">Key value</param>
        public ArgumentInfo(string key, string value)
        {
            // First, split the values and check for quotes
            string[] valuesArray = value.SplitEncloseDoubleQuotesNoRelease(',');
            LoggingTools.Info("Key: {0}, {1} values from {2}", key, valuesArray.Length, value);
            var values = new(bool caseSensitive, string value)[valuesArray.Length];
            for (int i = 0; i < valuesArray.Length; i++)
            {
                string valueArray = valuesArray[i];
                var quoteType = valueArray.GetEnclosedDoubleQuotesType();
                LoggingTools.Debug("Value from array: {0}, quote type: {1}", valueArray, quoteType);
                if (quoteType == EnclosedDoubleQuotesType.DoubleQuotes)
                {
                    values[i].caseSensitive = true;
                    values[i].value = valueArray.ReleaseDoubleQuotes();
                    LoggingTools.Debug("Released double quotes from value: {0}, turned on case sensitivity.", values[i].value);
                }
                else
                {
                    values[i].value = valueArray;
                    LoggingTools.Debug("No quotes: {0}, no case sensitivity.", values[i].value);
                }
            }

            // Install the key and the values
            this.key = key;
            this.values = values;
            LoggingTools.Info("Installed {0} values in {1}", values.Length, key);
        }
    }
}
