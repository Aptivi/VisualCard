/*
 * MIT License
 *
 * Copyright (c) 2021-2022 Aptivi
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VisualCard.Parsers;

namespace VisualCard.Parts
{
    /// <summary>
    /// Contact organization info
    /// </summary>
    public class OrganizationInfo : IEquatable<OrganizationInfo>
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
                source.AltId == target.AltId &&
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

        internal string ToStringVcardTwo()
        {
            bool installType = OrgTypes.Length > 0 && OrgTypes[0].ToUpper() != "WORK";
            return
                $"{VcardConstants._orgSpecifier}{(installType ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter)}" +
                $"{(installType ? $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", OrgTypes)}{VcardConstants._argumentDelimiter}" : "")}" +
                $"{Name}{VcardConstants._fieldDelimiter}" +
                $"{Unit}{VcardConstants._fieldDelimiter}" +
                $"{Role}";
        }

        internal string ToStringVcardThree()
        {
            bool installType = OrgTypes.Length > 0 && OrgTypes[0].ToUpper() != "WORK";
            return
                $"{VcardConstants._orgSpecifier}{(installType ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter)}" +
                $"{(installType ? $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", OrgTypes)}{VcardConstants._argumentDelimiter}" : "")}" +
                $"{Name}{VcardConstants._fieldDelimiter}" +
                $"{Unit}{VcardConstants._fieldDelimiter}" +
                $"{Role}";
        }

        internal string ToStringVcardFour()
        {
            bool installAltId = AltId >= 0 && AltArguments.Length > 0;
            bool installType = (installAltId || OrgTypes.Length > 0) && OrgTypes[0].ToUpper() != "WORK";
            return
                $"{VcardConstants._orgSpecifier}{(installType || installAltId ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter)}" +
                $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + (installType ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter) : "")}" +
                $"{(installType ? $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", OrgTypes)}{VcardConstants._argumentDelimiter}" : "")}" +
                $"{Name}{VcardConstants._fieldDelimiter}" +
                $"{Unit}{VcardConstants._fieldDelimiter}" +
                $"{Role}";
        }

        internal static OrganizationInfo FromStringVcardTwo(string value)
        {
            // Get the value
            string orgValue = value.Substring(VcardConstants._orgSpecifier.Length + 1);
            string[] splitOrg = orgValue.Split(VcardConstants._fieldDelimiter);

            // Populate the fields
            string[] splitTypes = new string[] { "WORK" };
            string _orgName = Regex.Unescape(splitOrg[0]);
            string _orgUnit = Regex.Unescape(splitOrg.Length >= 2 ? splitOrg[1] : "");
            string _orgUnitRole = Regex.Unescape(splitOrg.Length >= 3 ? splitOrg[2] : "");
            OrganizationInfo _org = new(0, Array.Empty<string>(), _orgName, _orgUnit, _orgUnitRole, splitTypes);
            return _org;
        }

        internal static OrganizationInfo FromStringVcardTwoWithType(string value)
        {
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
            string _orgName = Regex.Unescape(splitOrganizationValues[0]);
            string _orgUnit = Regex.Unescape(splitOrganizationValues.Length >= 2 ? splitOrganizationValues[1] : "");
            string _orgUnitRole = Regex.Unescape(splitOrganizationValues.Length >= 3 ? splitOrganizationValues[2] : "");
            string[] _orgTypes = VcardParserTools.GetTypes(splitOrg, "WORK", false);
            OrganizationInfo _org = new(0, Array.Empty<string>(), _orgName, _orgUnit, _orgUnitRole, _orgTypes);
            return _org;
        }

        internal static OrganizationInfo FromStringVcardThree(string value)
        {
            // Get the value
            string orgValue = value.Substring(VcardConstants._orgSpecifier.Length + 1);
            string[] splitOrg = orgValue.Split(VcardConstants._fieldDelimiter);

            // Populate the fields
            string[] splitTypes = new string[] { "WORK" };
            string _orgName = Regex.Unescape(splitOrg[0]);
            string _orgUnit = Regex.Unescape(splitOrg.Length >= 2 ? splitOrg[1] : "");
            string _orgUnitRole = Regex.Unescape(splitOrg.Length >= 3 ? splitOrg[2] : "");
            OrganizationInfo _org = new(0, Array.Empty<string>(), _orgName, _orgUnit, _orgUnitRole, splitTypes);
            return _org;
        }

        internal static OrganizationInfo FromStringVcardThreeWithType(string value)
        {
            // Get the value
            string orgValue = value.Substring(VcardConstants._orgSpecifier.Length + 1);
            string[] splitOrg = orgValue.Split(VcardConstants._argumentDelimiter);
            if (splitOrg.Length < 2)
                throw new InvalidDataException("Organization field must specify exactly two values (Type (must be prepended with TYPE=), and address information)");

            // Check the provided organization
            string[] splitOrganizationValues = splitOrg[1].Split(VcardConstants._fieldDelimiter);
            if (splitOrganizationValues.Length < 3)
                throw new InvalidDataException("Organization information must specify exactly three values (name, unit, and role)");

            // Populate the fields
            string _orgName = Regex.Unescape(splitOrganizationValues[0]);
            string _orgUnit = Regex.Unescape(splitOrganizationValues.Length >= 2 ? splitOrganizationValues[1] : "");
            string _orgUnitRole = Regex.Unescape(splitOrganizationValues.Length >= 3 ? splitOrganizationValues[2] : "");
            string[] _orgTypes = VcardParserTools.GetTypes(splitOrg, "WORK", true);
            OrganizationInfo _org = new(0, Array.Empty<string>(), _orgName, _orgUnit, _orgUnitRole, _orgTypes);
            return _org;
        }

        internal static OrganizationInfo FromStringVcardFour(string value, int altId)
        {
            // Get the value
            string orgValue = value.Substring(VcardConstants._orgSpecifier.Length + 1);
            string[] splitOrg = orgValue.Split(VcardConstants._fieldDelimiter);

            // Populate the fields
            string[] splitTypes = new string[] { "WORK" };
            string _orgName = Regex.Unescape(splitOrg[0]);
            string _orgUnit = Regex.Unescape(splitOrg.Length >= 2 ? splitOrg[1] : "");
            string _orgUnitRole = Regex.Unescape(splitOrg.Length >= 3 ? splitOrg[2] : "");
            OrganizationInfo _org = new(altId, Array.Empty<string>(), _orgName, _orgUnit, _orgUnitRole, splitTypes);
            return _org;
        }

        internal static OrganizationInfo FromStringVcardFourWithType(string value, List<string> finalArgs, int altId)
        {
            // Get the value
            string orgValue = value.Substring(VcardConstants._orgSpecifier.Length + 1);
            string[] splitOrg = orgValue.Split(VcardConstants._argumentDelimiter);
            if (splitOrg.Length < 2)
                throw new InvalidDataException("Organization field must specify exactly two values (Type (must be prepended with TYPE=), and address information)");

            // Check the provided organization
            string[] splitOrganizationValues = splitOrg[1].Split(VcardConstants._fieldDelimiter);
            if (splitOrganizationValues.Length < 3)
                throw new InvalidDataException("Organization information must specify exactly three values (name, unit, and role)");

            // Populate the fields
            string _orgName = Regex.Unescape(splitOrganizationValues[0]);
            string _orgUnit = Regex.Unescape(splitOrganizationValues.Length >= 2 ? splitOrganizationValues[1] : "");
            string _orgUnitRole = Regex.Unescape(splitOrganizationValues.Length >= 3 ? splitOrganizationValues[2] : "");
            string[] _orgTypes = VcardParserTools.GetTypes(splitOrg, "WORK", true);
            OrganizationInfo _org = new(altId, finalArgs.ToArray(), _orgName, _orgUnit, _orgUnitRole, _orgTypes);
            return _org;
        }

        internal OrganizationInfo() { }

        internal OrganizationInfo(int altId, string[] altArguments, string name, string unit, string role, string[] orgTypes)
        {
            AltId = altId;
            AltArguments = altArguments;
            Name = name;
            Unit = unit;
            Role = role;
            OrgTypes = orgTypes;
        }
    }
}
