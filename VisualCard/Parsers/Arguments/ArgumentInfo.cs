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
using Textify.General;

namespace VisualCard.Parsers.Arguments
{
    /// <summary>
    /// Argument info for properties
    /// </summary>
    public class ArgumentInfo : IEquatable<ArgumentInfo>
    {
        private readonly string key = "";
        private readonly string value = "";
        private readonly bool caseSensitive;

        /// <summary>
        /// Argument key name
        /// </summary>
        public string Key =>
            key;

        /// <summary>
        /// Argument value (without double quotes)
        /// </summary>
        public string Value =>
            value;

        /// <summary>
        /// Whether the value is case sensitive
        /// </summary>
        public bool CaseSensitive =>
            caseSensitive;

        /// <summary>
        /// Matches the argument value
        /// </summary>
        /// <param name="value">Value to match</param>
        /// <returns>True if they equal (case sensitive if <see cref="CaseSensitive"/> is true; case insensitive if otherwise); false otherwise.</returns>
        public bool MatchValue(string value) =>
            CaseSensitive ?
            value == Value :
            Value.Equals(value, StringComparison.OrdinalIgnoreCase);

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
                source.MatchValue(target.Value)
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 206514262;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Key);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(ArgumentInfo a, ArgumentInfo b)
            => a.Equals(b);

        /// <inheritdoc/>
        public static bool operator !=(ArgumentInfo a, ArgumentInfo b)
            => !a.Equals(b);

        internal ArgumentInfo(string key, string value)
        {
            this.key = key;
            this.value = value;
            if (value.GetEnclosedDoubleQuotesType() == EnclosedDoubleQuotesType.DoubleQuotes)
            {
                caseSensitive = true;
                this.value = this.value.ReleaseDoubleQuotes();
            }
        }
    }
}
