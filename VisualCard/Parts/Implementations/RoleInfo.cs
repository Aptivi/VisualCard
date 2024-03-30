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
    /// Contact role info
    /// </summary>
    [DebuggerDisplay("Role = {ContactRole}")]
    public class RoleInfo : BaseCardPartInfo, IEquatable<RoleInfo>
    {
        /// <summary>
        /// The contact's role
        /// </summary>
        public string ContactRole { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, int altId, Version cardVersion) =>
            new RoleInfo().FromStringVcardInternal(value, altId, cardVersion);

        internal static BaseCardPartInfo FromStringVcardWithTypeStatic(string value, string[] finalArgs, int altId, Version cardVersion) =>
            new RoleInfo().FromStringVcardWithTypeInternal(value, finalArgs, altId, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;
            if (altIdSupported)
            {
                bool installAltId = AltId >= 0 && AltArguments.Length > 0;
                return
                    $"{VcardConstants._roleSpecifier}{(installAltId ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter)}" +
                    $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                    $"{(installAltId ? string.Join(VcardConstants._fieldDelimiter.ToString(), AltArguments) + VcardConstants._argumentDelimiter : "")}" +
                    $"{ContactRole}";
            }
            else
            {
                return
                    $"{VcardConstants._roleSpecifier};" +
                    $"{ContactRole}";
            }
        }

        internal override BaseCardPartInfo FromStringVcardInternal(string value, int altId, Version cardVersion)
        {
            // Get the value
            string roleValue = value.Substring(VcardConstants._roleSpecifier.Length + 1);

            // Populate the fields
            return InstallInfo(roleValue, altId, cardVersion);
        }

        internal override BaseCardPartInfo FromStringVcardWithTypeInternal(string value, string[] finalArgs, int altId, Version cardVersion)
        {
            // Get the value
            string roleValue = value.Substring(VcardConstants._roleSpecifier.Length + 1);

            // Populate the fields
            return InstallInfo(roleValue, finalArgs, altId, cardVersion);
        }

        private RoleInfo InstallInfo(string roleValue, int altId, Version cardVersion) =>
            InstallInfo(roleValue, [], altId, cardVersion);

        private RoleInfo InstallInfo(string roleValue, string[] finalArgs, int altId, Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;

            // Populate the fields
            RoleInfo _telephone = new(altIdSupported ? altId : 0, altIdSupported ? finalArgs : [], roleValue);
            return _telephone;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            base.Equals(obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="RoleInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(RoleInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="RoleInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="RoleInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(RoleInfo source, RoleInfo target)
        {
            // We can't perform this operation on null.
            if (source is null)
                return false;

            // Check all the properties
            return
                source.AltArguments.SequenceEqual(target.AltArguments) &&
                source.AltId == target.AltId &&
                source.ContactRole == target.ContactRole
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -1215418912;
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AltArguments);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContactRole);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(RoleInfo left, RoleInfo right) =>
            EqualityComparer<RoleInfo>.Default.Equals(left, right);

        /// <inheritdoc/>
        public static bool operator !=(RoleInfo left, RoleInfo right) =>
            !(left == right);

        internal RoleInfo() { }

        internal RoleInfo(int altId, string[] altArguments, string contactRole)
        {
            AltId = altId;
            AltArguments = altArguments;
            ContactRole = contactRole;
        }
    }
}
