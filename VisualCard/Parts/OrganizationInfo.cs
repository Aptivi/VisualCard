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
using System.Linq;

namespace VisualCard.Parts
{
    public class OrganizationInfo : IEquatable<OrganizationInfo>
    {
        /// <summary>
        /// Alternative ID. Zero if unspecified.
        /// </summary>
        public int AltId { get; }
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
                source.AltId == target.AltId &&
                source.Name == target.Name &&
                source.Unit == target.Unit &&
                source.Role == target.Role
            ;
        }

        public override int GetHashCode()
        {
            int hashCode = -1277624027;
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(OrgTypes);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Unit);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Role);
            return hashCode;
        }

        internal OrganizationInfo() { }

        internal OrganizationInfo(int altId, string name, string unit, string role, string[] orgTypes)
        {
            AltId = altId;
            Name = name;
            Unit = unit;
            Role = role;
            OrgTypes = orgTypes;
        }
    }
}
