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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Textify.General;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts;
using VisualCard.Languages;

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
        public Card[]? AgentCards { get; set; }

        internal static BaseCardPartInfo FromStringStatic(string value, PropertyInfo property, int altId, string[] elementTypes, Version cardVersion) =>
            (BaseCardPartInfo)new AgentInfo().FromStringInternal(value, property, altId, elementTypes, cardVersion);

        internal override string ToStringInternal(Version cardVersion)
        {
            if (AgentCards is null)
                return "";
            var agents = new StringBuilder();

            foreach (var a in AgentCards)
            {
                agents.Append(
                    $"{string.Join("\\n", a.SaveToString().SplitNewLines())}"
                );
            }
            return agents.ToString();
        }

        internal override BasePartInfo FromStringInternal(string value, PropertyInfo property, int altId, string[] elementTypes, Version cardVersion)
        {
            // Check the provided agent
            if (string.IsNullOrEmpty(value))
                throw new InvalidDataException(LanguageTools.GetLocalized("VISUALCARD_PARTS_EXCEPTION_AGENT_NEEDSARGS"));

            // Populate the fields
            string _agentVcard = Regex.Unescape(value).Replace("\\n", "\n").Replace("\\N", "\n");
            var _agentVcardParsers = CardTools.GetCardsFromString(_agentVcard);
            AgentInfo _agent = new(altId, property, elementTypes, _agentVcardParsers);
            return _agent;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((AgentInfo)obj);

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
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.AgentCards == target.AgentCards
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -582546693;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Card[]?>.Default.GetHashCode(AgentCards);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(AgentInfo left, AgentInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(AgentInfo left, AgentInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BasePartInfo source, BasePartInfo target) =>
            ((AgentInfo)source) == ((AgentInfo)target);

        internal AgentInfo() :
            base()
        { }

        internal AgentInfo(int altId, PropertyInfo? property, string[] elementTypes, Card[] agentCard) :
            base(property, altId, elementTypes)
        {
            AgentCards = agentCard;
        }
    }
}
