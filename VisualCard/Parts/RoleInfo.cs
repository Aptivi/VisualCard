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
using VisualCard.Parsers;

namespace VisualCard.Parts
{
    /// <summary>
    /// Contact role info
    /// </summary>
    [DebuggerDisplay("Role = {ContactRole}")]
    public class RoleInfo : IEquatable<RoleInfo>
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
        /// The contact's role
        /// </summary>
        public string ContactRole { get; }

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

        internal string ToStringVcardTwo()
        {
            return
                $"{VcardConstants._roleSpecifier};" +
                $"{ContactRole}";
        }

        internal string ToStringVcardThree()
        {
            return
                $"{VcardConstants._roleSpecifier};" +
                $"{ContactRole}";
        }

        internal string ToStringVcardFour()
        {
            bool installAltId = AltId >= 0 && AltArguments.Length > 0;
            return
                $"{VcardConstants._roleSpecifier}{(installAltId ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter)}" +
                $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                $"{(installAltId ? string.Join(VcardConstants._fieldDelimiter.ToString(), AltArguments) + VcardConstants._argumentDelimiter : "")}" +
                $"{ContactRole}";
        }

        internal string ToStringVcardFive() =>
            ToStringVcardFour();

        internal static RoleInfo FromStringVcardTwo(string value)
        {
            // Get the value
            string roleValue = value.Substring(VcardConstants._roleSpecifier.Length + 1);

            // Populate the fields
            RoleInfo _role = new(0, [], roleValue);
            return _role;
        }

        internal static RoleInfo FromStringVcardThree(string value)
        {
            // Get the value
            string roleValue = value.Substring(VcardConstants._roleSpecifier.Length + 1);

            // Populate the fields
            RoleInfo _role = new(0, [], roleValue);
            return _role;
        }

        internal static RoleInfo FromStringVcardFour(string value, int altId)
        {
            // Get the value
            string roleValue = value.Substring(VcardConstants._roleSpecifier.Length + 1);

            // Populate the fields
            RoleInfo _role = new(altId, [], roleValue);
            return _role;
        }

        internal static RoleInfo FromStringVcardFourWithType(string value, List<string> finalArgs, int altId)
        {
            // Get the value
            string roleValue = value.Substring(VcardConstants._roleSpecifier.Length + 1);

            // Populate the fields
            RoleInfo _role = new(altId, [.. finalArgs], roleValue);
            return _role;
        }

        internal static RoleInfo FromStringVcardFive(string value, int altId) =>
            FromStringVcardFour(value, altId);

        internal static RoleInfo FromStringVcardFiveWithType(string value, List<string> finalArgs, int altId) =>
            FromStringVcardFourWithType(value, finalArgs, altId);

        internal RoleInfo() { }

        internal RoleInfo(int altId, string[] altArguments, string contactRole)
        {
            AltId = altId;
            AltArguments = altArguments;
            ContactRole = contactRole;
        }
    }
}
