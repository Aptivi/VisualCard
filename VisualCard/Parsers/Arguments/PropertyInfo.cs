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
using System.Linq;
using Textify.General;

namespace VisualCard.Parsers.Arguments
{
    /// <summary>
    /// Property info class
    /// </summary>
    public class PropertyInfo
    {
        private string rawValue = "";
        private string prefix = "";
        private string group = "";
        private ArgumentInfo[] arguments = [];
        private readonly Version version;

        /// <summary>
        /// Raw value
        /// </summary>
        public string Value
        {
            get => rawValue;
            set => rawValue = value;
        }

        /// <summary>
        /// Property prefix
        /// </summary>
        public string Prefix
        {
            get => prefix;
            set => prefix = value;
        }

        /// <summary>
        /// Property group
        /// </summary>
        public string Group
        {
            get => group;
            set => group = value;
        }

        /// <summary>
        /// Argument info instances. It includes AltId, type, and value
        /// </summary>
        public ArgumentInfo[] Arguments
        {
            get => arguments;
            set => arguments = value;
        }

        /// <summary>
        /// Argument info instances. It doesn't include AltId, type, and value
        /// </summary>
        public ArgumentInfo[] ArgumentsFiltered
        {
            get
            {
                var args = Arguments.Where((arg) =>
                    (arg.Key != VcardConstants._altIdArgumentSpecifier &&
                     arg.Key != VcardConstants._valueArgumentSpecifier &&
                     arg.Key != VcardConstants._typeArgumentSpecifier) ||
                    (version.Major == 2 && string.IsNullOrEmpty(arg.Key)));
                return Arguments.Except(args).ToArray();
            }
        }

        internal PropertyInfo(string line, Version version)
        {
            // Now, parse this value
            if (!line.Contains($"{VcardConstants._argumentDelimiter}"))
                throw new ArgumentException("The line must contain an argument delimiter.");
            string value = line.Substring(line.IndexOf(VcardConstants._argumentDelimiter) + 1);
            string prefixWithArgs = line.Substring(0, line.IndexOf(VcardConstants._argumentDelimiter));
            string prefix = (prefixWithArgs.Contains($"{VcardConstants._fieldDelimiter}") ? prefixWithArgs.Substring(0, prefixWithArgs.IndexOf($"{VcardConstants._fieldDelimiter}")) : prefixWithArgs).ToUpper();
            string args = prefixWithArgs.Contains($"{VcardConstants._fieldDelimiter}") ? prefixWithArgs.Substring(prefix.Length + 1) : "";
            string[] splitArgs = args.Split([VcardConstants._fieldDelimiter], StringSplitOptions.RemoveEmptyEntries);
            var finalArgs = splitArgs.Select((arg) => new ArgumentInfo(arg)).ToArray();

            // Extract the group name
            string group = prefix.Contains(".") ? prefix.Substring(0, prefix.IndexOf(".")) : "";
            prefix = prefix.RemovePrefix($"{group}.");

            // Check to see if this is a nonstandard prefix
            bool xNonstandard = prefix.StartsWith(VcardConstants._xSpecifier);
            prefix = xNonstandard ? VcardConstants._xSpecifier : prefix;

            // Install values
            Value = value;
            Prefix = prefix;
            Arguments = finalArgs;
            Group = group;
            this.version = version;
        }
    }
}
