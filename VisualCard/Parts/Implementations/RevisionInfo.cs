﻿//
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
using System.Diagnostics;
using VisualCard.Parsers;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Card revision info
    /// </summary>
    [DebuggerDisplay("Revision = {Revision}")]
    public class RevisionInfo : BaseCardPartInfo, IEquatable<RevisionInfo>
    {
        /// <summary>
        /// The card's revision
        /// </summary>
        public DateTime? Revision { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion) =>
            new RevisionInfo().FromStringVcardInternal(value, finalArgs, altId, elementTypes, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion) =>
            $"{Revision:yyyy-MM-dd HH:mm:ss}";

        internal override BaseCardPartInfo FromStringVcardInternal(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion)
        {
            // Get the value
            string revValue = value.Substring(VcardConstants._revSpecifier.Length + 1);

            // Populate the fields
            DateTime rev = DateTime.Parse(revValue);

            // Add the fetched information
            bool altIdSupported = cardVersion.Major >= 4;
            RevisionInfo _time = new(altIdSupported ? altId : 0, finalArgs, elementTypes, valueType, rev);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((RevisionInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="RevisionInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(RevisionInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="RevisionInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="RevisionInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(RevisionInfo source, RevisionInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.Revision == target.Revision
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 47832270;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Revision.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(RevisionInfo left, RevisionInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(RevisionInfo left, RevisionInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCardPartInfo source, BaseCardPartInfo target) =>
            ((RevisionInfo)source) == ((RevisionInfo)target);

        internal RevisionInfo() { }

        internal RevisionInfo(int altId, string[] arguments, string[] elementTypes, string valueType, DateTime? birth)
        {
            AltId = altId;
            Arguments = arguments;
            ElementTypes = elementTypes;
            ValueType = valueType;
            Revision = birth;
        }
    }
}
