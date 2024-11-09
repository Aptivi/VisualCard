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
using System.Linq;
using Textify.General;
using VisualCard.Parts.Comparers;

namespace VisualCard.Parsers.Arguments
{
    /// <summary>
    /// Property info class
    /// </summary>
    [DebuggerDisplay("Property: {Arguments.Length} args, {Prefix} [G: {Group}] = {Value}")]
    public class PropertyInfo : IEquatable<PropertyInfo?>
    {
        private readonly string rawValue = "";
        private readonly string prefix = "";
        private readonly string group = "";
        private readonly ArgumentInfo[] arguments = [];

        /// <summary>
        /// Raw value
        /// </summary>
        public string Value =>
            rawValue;

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
                PartComparison.CompareLists(source.arguments, target.arguments)
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
            if (!line.Contains($"{VcardConstants._argumentDelimiter}"))
                throw new ArgumentException("The line must contain an argument delimiter.");
            line = line.Trim();
            string value = line.Substring(line.IndexOf(VcardConstants._argumentDelimiter) + 1).Trim();
            string prefixWithArgs = line.Substring(0, line.IndexOf(VcardConstants._argumentDelimiter)).Trim();
            string prefix = (prefixWithArgs.Contains($"{VcardConstants._fieldDelimiter}") ? prefixWithArgs.Substring(0, prefixWithArgs.IndexOf($"{VcardConstants._fieldDelimiter}")) : prefixWithArgs).Trim().ToUpper();
            string args = prefixWithArgs.Contains($"{VcardConstants._fieldDelimiter}") ? prefixWithArgs.Substring(prefix.Length + 1) : "";
            string[] splitArgs = args.Split([VcardConstants._fieldDelimiter], StringSplitOptions.RemoveEmptyEntries);
            var finalArgs = splitArgs.Select((arg) => new ArgumentInfo(arg)).ToArray();

            // Extract the group name
            string group = prefix.Contains(".") ? prefix.Substring(0, prefix.LastIndexOf(".")) : "";
            prefix = prefix.RemovePrefix($"{group}.");

            // Check to see if this is a nonstandard prefix
            bool xNonstandard = prefix.StartsWith(VcardConstants._xSpecifier);
            prefix = xNonstandard ? VcardConstants._xSpecifier : prefix;

            // Install values
            this.rawValue = value;
            this.prefix = prefix;
            this.arguments = finalArgs;
            this.group = group.Trim();
        }
    }
}
