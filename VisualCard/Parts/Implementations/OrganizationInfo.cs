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
using System.Text.RegularExpressions;
using VisualCard.Parsers;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact organization info
    /// </summary>
    [DebuggerDisplay("OrgName = {Name}, {Unit}, {Role}")]
    public class OrganizationInfo : BaseCardPartInfo, IEquatable<OrganizationInfo>
    {
        /// <summary>
        /// The contact's organization types
        /// </summary>
        public string[] OrgTypes { get; }
        /// <summary>
        /// The contact's organization name
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The contact's organization unit
        /// </summary>
        public string Unit { get; }
        /// <summary>
        /// The contact's organization unit's role
        /// </summary>
        public string Role { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion) =>
            new OrganizationInfo().FromStringVcardInternal(value, finalArgs, altId, elementTypes, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;
            if (altIdSupported)
            {
                bool installAltId = AltId >= 0 && Arguments.Length > 0;
                bool installType = (installAltId || OrgTypes.Length > 0) && OrgTypes[0].ToUpper() != "WORK";
                return
                    $"{VcardConstants._orgSpecifier}{(installType || installAltId ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter)}" +
                    $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + (installType ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter) : "")}" +
                    $"{(installType ? $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", OrgTypes)}{VcardConstants._argumentDelimiter}" : "")}" +
                    $"{Name}{VcardConstants._fieldDelimiter}" +
                    $"{Unit}{VcardConstants._fieldDelimiter}" +
                    $"{Role}";
            }
            else
            {
                bool installType = OrgTypes.Length > 0 && OrgTypes[0].ToUpper() != "WORK";
                return
                    $"{VcardConstants._orgSpecifier}{(installType ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter)}" +
                    $"{(installType ? $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", OrgTypes)}{VcardConstants._argumentDelimiter}" : "")}" +
                    $"{Name}{VcardConstants._fieldDelimiter}" +
                    $"{Unit}{VcardConstants._fieldDelimiter}" +
                    $"{Role}";
            }
        }

        internal override BaseCardPartInfo FromStringVcardInternal(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion)
        {
            // Get the value
            string orgValue = value.Substring(VcardConstants._orgSpecifier.Length + 1);
            string[] splitOrg = orgValue.Split(VcardConstants._fieldDelimiter);

            // Populate the fields
            return InstallInfo(splitOrg, ["WORK"], altId, cardVersion);
        }

        internal override BaseCardPartInfo FromStringVcardWithTypeInternal(string value, string[] finalArgs, int altId, Version cardVersion)
        {
            bool specifierRequired = cardVersion.Major >= 3;

            // Get the value
            string orgValue = value.Substring(VcardConstants._orgSpecifier.Length + 1);
            string[] splitOrg = orgValue.Split(VcardConstants._argumentDelimiter);
            if (splitOrg.Length < 2)
                throw new InvalidDataException("Organization field must specify exactly two values (Type, and address information)");

            // Check the provided organization
            string[] splitOrganizationValues = splitOrg[1].Split(VcardConstants._fieldDelimiter);
            if (splitOrganizationValues.Length < 3)
                throw new InvalidDataException("Organization information must specify exactly three values (name, unit, and role)");

            // Populate the fields
            string[] _orgTypes = VcardParserTools.GetTypes(splitOrg, "WORK", specifierRequired);
            return InstallInfo(splitOrganizationValues, _orgTypes, finalArgs, altId, cardVersion);
        }

        private OrganizationInfo InstallInfo(string[] splitOrg, string[] _orgTypes, int altId, Version cardVersion) =>
            InstallInfo(splitOrg, _orgTypes, [], altId, cardVersion);

        private OrganizationInfo InstallInfo(string[] splitOrg, string[] _orgTypes, string[] finalArgs, int altId, Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;

            // Populate the fields
            string _orgName = Regex.Unescape(splitOrg[0]);
            string _orgUnit = Regex.Unescape(splitOrg.Length >= 2 ? splitOrg[1] : "");
            string _orgUnitRole = Regex.Unescape(splitOrg.Length >= 3 ? splitOrg[2] : "");
            OrganizationInfo _org = new(altIdSupported ? altId : 0, finalArgs, elementTypes, valueType, _orgName, _orgUnit, _orgUnitRole, _orgTypes);
            return _org;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            base.Equals(obj);

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
            if (source is null)
                return false;

            // Check all the properties
            return
                source.OrgTypes.SequenceEqual(target.OrgTypes) &&
                source.AltArguments.SequenceEqual(target.AltArguments) &&
                base.Equals(source, target) &&
                source.Name == target.Name &&
                source.Unit == target.Unit &&
                source.Role == target.Role
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 374840165;
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AltArguments);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(OrgTypes);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Unit);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Role);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(OrganizationInfo left, OrganizationInfo right) =>
            EqualityComparer<OrganizationInfo>.Default.Equals(left, right);

        /// <inheritdoc/>
        public static bool operator !=(OrganizationInfo left, OrganizationInfo right) =>
            !(left == right);

        internal OrganizationInfo() { }

        internal OrganizationInfo(int altId, string[] arguments, string[] elementTypes, string valueType, string name, string unit, string role, string[] orgTypes)
        {
            AltId = altId;
            Arguments = arguments;
            Name = name;
            Unit = unit;
            Role = role;
            OrgTypes = orgTypes;
        }
    }
}
