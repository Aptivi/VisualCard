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

namespace VisualCard.Parts
{
    /// <summary>
    /// Base card part class
    /// </summary>
    [DebuggerDisplay("Base card part = ALTID: {AltId}")]
    public abstract class BaseCardPartInfo : IEquatable<BaseCardPartInfo>
    {
        /// <summary>
        /// Arguments that follow the AltId
        /// </summary>
        public virtual string[] AltArguments { get; internal set; }

        /// <summary>
        /// Alternative ID. Zero if unspecified.
        /// </summary>
        public virtual int AltId { get; internal set; }

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="BaseCardPartInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(BaseCardPartInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="BaseCardPartInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="BaseCardPartInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(BaseCardPartInfo source, BaseCardPartInfo target)
        {
            // We can't perform this operation on null.
            if (source is null)
                return false;

            // Check all the properties
            return
                source.AltArguments.SequenceEqual(target.AltArguments) &&
                source.AltId == target.AltId
            ;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            base.Equals(obj);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -2100286935;
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AltArguments);
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(BaseCardPartInfo left, BaseCardPartInfo right) =>
            EqualityComparer<BaseCardPartInfo>.Default.Equals(left, right);

        /// <inheritdoc/>
        public static bool operator !=(BaseCardPartInfo left, BaseCardPartInfo right) =>
            !(left == right);

        internal abstract BaseCardPartInfo FromStringVcardInternal(string value, int altId, Version cardVersion);

        internal abstract BaseCardPartInfo FromStringVcardWithTypeInternal(string value, string[] finalArgs, int altId, Version cardVersion);

        internal abstract string ToStringVcardInternal(Version cardVersion);
    }
}
