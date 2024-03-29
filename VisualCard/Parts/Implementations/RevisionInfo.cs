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

        internal static BaseCardPartInfo FromStringVcardStatic(string value, int altId, Version cardVersion, StreamReader cardContentReader) =>
            new RevisionInfo().FromStringVcardInternal(value, altId, cardVersion, cardContentReader);

        internal static BaseCardPartInfo FromStringVcardWithTypeStatic(string value, string[] finalArgs, int altId, Version cardVersion, StreamReader cardContentReader) =>
            new RevisionInfo().FromStringVcardWithTypeInternal(value, finalArgs, altId, cardVersion, cardContentReader);

        internal override string ToStringVcardInternal(Version cardVersion) =>
            $"{VcardConstants._revSpecifier}:{Revision:yyyy-MM-dd HH:mm:ss}";

        internal override BaseCardPartInfo FromStringVcardInternal(string value, int altId, Version cardVersion, StreamReader cardContentReader)
        {
            // Get the value
            string revValue = value.Substring(VcardConstants._revSpecifier.Length + 1);

            // Populate the fields
            return InstallInfo(revValue, altId, cardVersion);
        }

        internal override BaseCardPartInfo FromStringVcardWithTypeInternal(string value, string[] finalArgs, int altId, Version cardVersion, StreamReader cardContentReader) =>
            FromStringVcardInternal(value, altId, cardVersion, cardContentReader);

        private RevisionInfo InstallInfo(string value, int altId, Version cardVersion) =>
            InstallInfo(value, [], altId, cardVersion);

        private RevisionInfo InstallInfo(string value, string[] finalArgs, int altId, Version cardVersion)
        {
            // Populate field
            DateTime rev = DateTime.Parse(value);

            // Add the fetched information
            bool altIdSupported = cardVersion.Major >= 4;
            RevisionInfo _time = new(altIdSupported ? altId : 0, altIdSupported ? finalArgs : [], rev);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            base.Equals(obj);

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
            if (source is null)
                return false;

            // Check all the properties
            return
                source.AltArguments.SequenceEqual(target.AltArguments) &&
                source.AltId == target.AltId &&
                source.Revision == target.Revision
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -480211805;
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AltArguments);
            hashCode = hashCode * -1521134295 + Revision.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(RevisionInfo left, RevisionInfo right) =>
            EqualityComparer<RevisionInfo>.Default.Equals(left, right);

        /// <inheritdoc/>
        public static bool operator !=(RevisionInfo left, RevisionInfo right) =>
            !(left == right);

        internal RevisionInfo() { }

        internal RevisionInfo(int altId, string[] altArguments, DateTime? birth)
        {
            AltId = altId;
            AltArguments = altArguments;
            Revision = birth;
        }
    }
}
