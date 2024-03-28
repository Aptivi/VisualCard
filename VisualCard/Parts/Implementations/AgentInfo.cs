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
using System.Text.RegularExpressions;
using Textify.General;
using VisualCard.Parsers;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact agent information
    /// </summary>
    [DebuggerDisplay("{AgentCards.Length} agents")]
    public class AgentInfo : BaseCardPartInfo, IEquatable<AgentInfo>
    {
        /// <summary>
        /// The contact's agent <see cref="Card"/> instances
        /// </summary>
        public Card[] AgentCards { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, int altId, Version cardVersion, StreamReader cardContentReader) =>
            new AgentInfo().FromStringVcardInternal(value, altId, cardVersion, cardContentReader);

        internal static BaseCardPartInfo FromStringVcardWithTypeStatic(string value, string[] finalArgs, int altId, Version cardVersion, StreamReader cardContentReader) =>
            new AgentInfo().FromStringVcardWithTypeInternal(value, finalArgs, altId, cardVersion, cardContentReader);

        internal override string ToStringVcardInternal(Version cardVersion)
        {
            var agents = new StringBuilder();
            bool altIdSupported = cardVersion.Major >= 4;

            foreach (var a in AgentCards)
            {
                if (altIdSupported)
                {
                    bool installAltId = AltId >= 0 && AltArguments.Length > 0;
                    agents.Append(
                        $"{VcardConstants._agentSpecifier}" +
                        $"{(installAltId ? $"{VcardConstants._fieldDelimiter}{VcardConstants._altIdArgumentSpecifier}" + AltId : "")}" +
                        $"{VcardConstants._argumentDelimiter}" +
                        $"{string.Join("\\n", a.SaveToString().SplitNewLines())}"
                    );
                }
                else
                {
                    agents.Append(
                        $"{VcardConstants._agentSpecifier}{VcardConstants._argumentDelimiter}" +
                        $"{string.Join("\\n", a.SaveToString().SplitNewLines())}"
                    );
                }
            }
            return agents.ToString();
        }

        internal override BaseCardPartInfo FromStringVcardInternal(string value, int altId, Version cardVersion, StreamReader cardContentReader)
        {
            // Get the value
            string agentValue = value.Substring(VcardConstants._agentSpecifier.Length + 1);
            string[] splitAgent = agentValue.Split(VcardConstants._argumentDelimiter);

            // Check the provided agent
            string[] splitAgentValues = splitAgent[0].Split(VcardConstants._fieldDelimiter);
            if (splitAgentValues.Length < 1)
                throw new InvalidDataException("Agent information must specify exactly one value (agent vCard contents that have their lines delimited by \\n)");

            // Populate the fields
            return InstallInfo(agentValue, altId, cardVersion);
        }

        internal override BaseCardPartInfo FromStringVcardWithTypeInternal(string value, string[] finalArgs, int altId, Version cardVersion, StreamReader cardContentReader)
        {
            // Get the value
            string agentValue = value.Substring(VcardConstants._agentSpecifier.Length + 1);
            string[] splitAgent = agentValue.Split(VcardConstants._argumentDelimiter);
            if (splitAgent.Length < 2)
                throw new InvalidDataException("Agent field must specify exactly two values (Type (optionally prepended with TYPE=), and agent information)");

            // Check the provided agent
            string[] splitAgentValues = splitAgent[1].Split(VcardConstants._fieldDelimiter);
            if (splitAgentValues.Length < 1)
                throw new InvalidDataException("Agent information must specify exactly one value (agent vCard contents that have their lines delimited by \\n)");

            // Populate the fields
            return InstallInfo(agentValue, finalArgs, altId, cardVersion);
        }

        private AgentInfo InstallInfo(string value, int altId, Version cardVersion) =>
            InstallInfo(value, [], altId, cardVersion);

        private AgentInfo InstallInfo(string value, string[] finalArgs, int altId, Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;

            // Populate the fields
            string _agentVcard = Regex.Unescape(value).Replace("\\n", "\n");
            var _agentVcardParsers = CardTools.GetCardParsersFromString(_agentVcard);
            var _agentVcardFinal = _agentVcardParsers.Select((parser) => parser.Parse()).ToArray();
            AgentInfo _agent = new(altIdSupported ? altId : 0, altIdSupported ? finalArgs : [], _agentVcardFinal);
            return _agent;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            base.Equals(obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="AgentInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(AgentInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="AgentInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="AgentInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(AgentInfo source, AgentInfo target)
        {
            // We can't perform this operation on null.
            if (source is null)
                return false;

            // Check all the properties
            return
                source.AltArguments.SequenceEqual(target.AltArguments) &&
                source.AltId == target.AltId &&
                source.AgentCards == target.AgentCards
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -1716393954;
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AltArguments);
            hashCode = hashCode * -1521134295 + EqualityComparer<Card[]>.Default.GetHashCode(AgentCards);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(AgentInfo left, AgentInfo right) =>
            EqualityComparer<AgentInfo>.Default.Equals(left, right);

        /// <inheritdoc/>
        public static bool operator !=(AgentInfo left, AgentInfo right) =>
            !(left == right);

        internal AgentInfo() { }

        internal AgentInfo(int altId, string[] altArguments, Card[] agentCard)
        {
            AltId = altId;
            AltArguments = altArguments;
            AgentCards = agentCard;
        }
    }
}
