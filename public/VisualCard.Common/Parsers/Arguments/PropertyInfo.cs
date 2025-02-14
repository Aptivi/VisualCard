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
using VisualCard.Common.Parts.Comparers;

namespace VisualCard.Common.Parsers.Arguments
{
    /// <summary>
    /// Property info class
    /// </summary>
    [DebuggerDisplay("Property: {Arguments.Length} args, {Prefix} [G: {Group}] = {Value}")]
    internal class PropertyInfo : IEquatable<PropertyInfo?>
    {
        internal readonly StringBuilder rawValue = new();
        private readonly string prefix = "";
        private readonly string group = "";
        private readonly ArgumentInfo[] arguments = [];

        /// <summary>
        /// Raw value
        /// </summary>
        public string Value =>
            rawValue.ToString();

        /// <summary>
        /// Property prefix
        /// </summary>
        public string Prefix =>
            prefix;

        /// <summary>
        /// Property group (dots are for nested groups)
        /// </summary>
        public string Group =>
            group;

        /// <summary>
        /// Nested property groups
        /// </summary>
        public string[] NestedGroups =>
            group.Split(['.'], StringSplitOptions.RemoveEmptyEntries);

        /// <summary>
        /// Argument info instances. It includes AltId, type, and value
        /// </summary>
        public ArgumentInfo[] Arguments =>
            arguments;

        /// <summary>
        /// Property encoding
        /// </summary>
        public string Encoding
            => CommonTools.GetValuesString(arguments, "", CommonConstants._encodingArgumentSpecifier);

        /// <summary>
        /// Property type
        /// </summary>
        public string Type
            => CommonTools.GetValuesString(arguments, "", CommonConstants._typeArgumentSpecifier);

        /// <summary>
        /// Property value type
        /// </summary>
        public string ValueType { get; set; }

        internal bool CanContinueMultiline
            => Encoding == CommonConstants._quotedPrintable && Value?.Length > 0 && Value[Value.Length - 1] == '=';

        /// <summary>
        /// Checks to see if both the property info instances are equal
        /// </summary>
        /// <param name="other">The target <see cref="PropertyInfo"/> instance to check to see if they equal</param>
        /// <returns>True if both instances are equal. Otherwise, false.</returns>
        public bool Equals(PropertyInfo? other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the property info instances are equal
        /// </summary>
        /// <param name="source">The source <see cref="PropertyInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="PropertyInfo"/> instance to check to see if they equal</param>
        /// <returns>True if both instances are equal. Otherwise, false.</returns>
        public bool Equals(PropertyInfo? source, PropertyInfo? target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.prefix == target.prefix &&
                source.group == target.group &&
                CommonComparison.CompareLists(source.arguments, target.arguments)
            ;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            Equals((PropertyInfo?)obj);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 106740708;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(prefix);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(group);
            hashCode = hashCode * -1521134295 + EqualityComparer<ArgumentInfo[]>.Default.GetHashCode(arguments);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(PropertyInfo? left, PropertyInfo? right) =>
            left?.Equals(right) ?? false;

        /// <inheritdoc/>
        public static bool operator !=(PropertyInfo? left, PropertyInfo? right) =>
            !(left == right);

        internal PropertyInfo(string line)
        {
            // Now, parse this value
            LoggingTools.Info("Line passed: {0}", line);
            if (!line.Contains($"{CommonConstants._argumentDelimiter}"))
            {
                LoggingTools.Error("Invalid line! No argument delimiter ':' found!");
                throw new ArgumentException("The line must contain an argument delimiter.");
            }
            line = line.Trim();
            string value = line.Substring(line.IndexOf(CommonConstants._argumentDelimiter) + 1).Trim();
            string prefixWithArgs = line.Substring(0, line.IndexOf(CommonConstants._argumentDelimiter)).Trim();
            string prefix = (prefixWithArgs.Contains($"{CommonConstants._fieldDelimiter}") ? prefixWithArgs.Substring(0, prefixWithArgs.IndexOf($"{CommonConstants._fieldDelimiter}")) : prefixWithArgs).Trim().ToUpper();
            string args = prefixWithArgs.Contains($"{CommonConstants._fieldDelimiter}") ? prefixWithArgs.Substring(prefix.Length + 1) : "";
            string[] splitArgs = args.Split([CommonConstants._fieldDelimiter], StringSplitOptions.RemoveEmptyEntries);
            var finalArgs = splitArgs.Select((arg) => new ArgumentInfo(arg)).ToArray();
            LoggingTools.Debug("Value: {0}, Prefix: [{1}, {2}], Args: {3} [{4} arguments, {5} processed arguments]", value, prefixWithArgs, prefix, args, splitArgs.Length, finalArgs.Length);

            // Extract the group name
            string group = prefix.Contains(".") ? prefix.Substring(0, prefix.LastIndexOf(".")) : "";
            prefix = prefix.RemovePrefix($"{group}.");
            LoggingTools.Debug("Cut group {0}, resulting prefix is {1}", group, prefix);

            // Check to see if this is a nonstandard prefix
            bool xNonstandard = prefix.StartsWith(CommonConstants._xSpecifier);
            prefix = xNonstandard ? CommonConstants._xSpecifier : prefix;
            LoggingTools.Debug("Nonstandard is {0}, resulting prefix is {1}", xNonstandard, prefix);

            // Install values
            rawValue.Append(value);
            this.prefix = prefix;
            arguments = finalArgs;
            this.group = group.Trim();
            ValueType = CommonTools.GetValuesString(arguments, "", CommonConstants._valueArgumentSpecifier);
            LoggingTools.Info("Installed values: {0}, {1} args, {2} [Initial raw: {3}]", prefix, finalArgs.Length, group, rawValue.ToString());
        }
    }
}
