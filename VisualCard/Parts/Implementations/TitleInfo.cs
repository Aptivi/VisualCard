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
using System.Text.RegularExpressions;
using VisualCard.Parsers;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact job title info
    /// </summary>
    [DebuggerDisplay("Job title = {ContactTitle}")]
    public class TitleInfo : BaseCardPartInfo, IEquatable<TitleInfo>
    {
        /// <summary>
        /// The contact's title
        /// </summary>
        public string ContactTitle { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion) =>
            new TitleInfo().FromStringVcardInternal(value, finalArgs, altId, elementTypes, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion) =>
            ContactTitle;

        internal override BaseCardPartInfo FromStringVcardInternal(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;
            string _title = Regex.Unescape(value);
            TitleInfo _titleInfo = new(altIdSupported ? altId : 0, finalArgs, elementTypes, valueType, _title);
            return _titleInfo;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((TitleInfo)obj);

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
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.ContactTitle == target.ContactTitle
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -345092951;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContactTitle);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(TitleInfo left, TitleInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(TitleInfo left, TitleInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCardPartInfo source, BaseCardPartInfo target) =>
            ((TitleInfo)source) == ((TitleInfo)target);

        internal TitleInfo() { }

        internal TitleInfo(int altId, string[] arguments, string[] elementTypes, string valueType, string contactTitle) :
            base(arguments, altId, elementTypes, valueType)
        {
            ContactTitle = contactTitle;
        }
    }
}
