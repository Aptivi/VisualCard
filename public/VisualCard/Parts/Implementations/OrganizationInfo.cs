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
using System.Text.RegularExpressions;
using VisualCard.Parsers;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts;
using VisualCard.Common.Parsers;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact organization info
    /// </summary>
    [DebuggerDisplay("OrgName = {Name}, {Unit}, {Role}")]
    public class OrganizationInfo : BaseCardPartInfo, IEquatable<OrganizationInfo>
    {
        /// <summary>
        /// The contact's organization name
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// The contact's organization unit
        /// </summary>
        public string? Unit { get; set; }
        /// <summary>
        /// The contact's organization unit's role
        /// </summary>
        public string? Role { get; set; }

        internal static BaseCardPartInfo FromStringStatic(string value, PropertyInfo property, int altId, string[] elementTypes, Version cardVersion) =>
            (BaseCardPartInfo)new OrganizationInfo().FromStringInternal(value, property, altId, elementTypes, cardVersion);

        internal override string ToStringInternal(Version cardVersion) =>
            $"{Name}{CommonConstants._fieldDelimiter}" +
            $"{Unit}{CommonConstants._fieldDelimiter}" +
            $"{Role}";

        internal override BasePartInfo FromStringInternal(string value, PropertyInfo property, int altId, string[] elementTypes, Version cardVersion)
        {
            string[] splitOrg = value.Split(CommonConstants._fieldDelimiter);

            // Populate the fields
            string _orgName = Regex.Unescape(splitOrg[0]);
            string _orgUnit = Regex.Unescape(splitOrg.Length >= 2 ? splitOrg[1] : "");
            string _orgUnitRole = Regex.Unescape(splitOrg.Length >= 3 ? splitOrg[2] : "");
            OrganizationInfo _org = new(altId, property, elementTypes, _orgName, _orgUnit, _orgUnitRole);
            return _org;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((OrganizationInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="OrganizationInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(OrganizationInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="OrganizationInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="OrganizationInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(OrganizationInfo source, OrganizationInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.Name == target.Name &&
                source.Unit == target.Unit &&
                source.Role == target.Role
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 1382759124;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(Unit);
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(Role);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(OrganizationInfo left, OrganizationInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(OrganizationInfo left, OrganizationInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BasePartInfo source, BasePartInfo target) =>
            ((OrganizationInfo)source) == ((OrganizationInfo)target);

        internal OrganizationInfo() { }

        internal OrganizationInfo(int altId, PropertyInfo? property, string[] elementTypes, string name, string unit, string role) :
            base(property, altId, elementTypes)
        {
            Name = name;
            Unit = unit;
            Role = role;
        }
    }
}
