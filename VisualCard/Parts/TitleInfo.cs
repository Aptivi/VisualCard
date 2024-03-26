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
using System.Text.RegularExpressions;
using VisualCard.Parsers;

namespace VisualCard.Parts
{
    /// <summary>
    /// Contact job title info
    /// </summary>
    [DebuggerDisplay("Job title = {ContactTitle}")]
    public class TitleInfo : IEquatable<TitleInfo>
    {
        /// <summary>
        /// Alternative ID. Zero if unspecified.
        /// </summary>
        public int AltId { get; }
        /// <summary>
        /// Arguments that follow the AltId
        /// </summary>
        public string[] AltArguments { get; }
        /// <summary>
        /// The contact's title
        /// </summary>
        public string ContactTitle { get; }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            base.Equals(obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="TitleInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(TitleInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="TitleInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="TitleInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(TitleInfo source, TitleInfo target)
        {
            // We can't perform this operation on null.
            if (source is null)
                return false;

            // Check all the properties
            return
                source.AltArguments.SequenceEqual(target.AltArguments) &&
                source.AltId == target.AltId &&
                source.ContactTitle == target.ContactTitle
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -1478940212;
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AltArguments);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContactTitle);
            return hashCode;
        }

        internal string ToStringVcardTwo()
        {
            return
                $"{VcardConstants._titleSpecifier}:" +
                $"{ContactTitle}";
        }

        internal string ToStringVcardThree()
        {
            return
                $"{VcardConstants._titleSpecifier}:" +
                $"{ContactTitle}";
        }

        internal string ToStringVcardFour()
        {
            bool installAltId = AltId >= 0 && AltArguments.Length > 0;
            return
                $"{(installAltId ? $"{VcardConstants._titleSpecifier};" : $"{VcardConstants._titleSpecifier}:")}" +
                $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                $"{(installAltId ? string.Join(VcardConstants._fieldDelimiter.ToString(), AltArguments) + VcardConstants._argumentDelimiter : "")}" +
                $"{ContactTitle}";
        }

        internal string ToStringVcardFive() =>
            ToStringVcardFour();

        internal static TitleInfo FromStringVcardTwo(string value)
        {
            // Get the value
            string titleValue = value.Substring(VcardConstants._titleSpecifier.Length + 1);

            // Populate field
            string _title = Regex.Unescape(titleValue);
            TitleInfo title = new(0, [], _title);
            return title;
        }

        internal static TitleInfo FromStringVcardThree(string value)
        {
            // Get the value
            string titleValue = value.Substring(VcardConstants._titleSpecifier.Length + 1);

            // Populate field
            string _title = Regex.Unescape(titleValue);
            TitleInfo title = new(0, [], _title);
            return title;
        }

        internal static TitleInfo FromStringVcardFour(string value, int altId)
        {
            // Get the value
            string titleValue = value.Substring(VcardConstants._titleSpecifier.Length + 1);

            // Populate field
            string _title = Regex.Unescape(titleValue);
            TitleInfo title = new(altId, [], _title);
            return title;
        }

        internal static TitleInfo FromStringVcardFourWithType(string value, List<string> finalArgs, int altId)
        {
            // Get the value
            string titleValue = value.Substring(VcardConstants._titleSpecifier.Length + 1);
            string[] splitTitleParts = titleValue.Split(VcardConstants._argumentDelimiter);

            // Populate field
            string _title = Regex.Unescape(splitTitleParts[1]);
            TitleInfo title = new(altId, [.. finalArgs], _title);
            return title;
        }

        internal static TitleInfo FromStringVcardFive(string value, int altId) =>
            FromStringVcardFour(value, altId);

        internal static TitleInfo FromStringVcardFiveWithType(string value, List<string> finalArgs, int altId) =>
            FromStringVcardFourWithType(value, finalArgs, altId);

        internal TitleInfo() { }

        internal TitleInfo(int altId, string[] altArguments, string contactTitle)
        {
            AltId = altId;
            AltArguments = altArguments;
            ContactTitle = contactTitle;
        }
    }
}
