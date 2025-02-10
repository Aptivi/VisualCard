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
using System.IO;
using System.Linq;
using VisualCard.Parsers;
using VisualCard.Parts.Implementations.Tools;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact gender info
    /// </summary>
    [DebuggerDisplay("Gender = {Gender} [{GenderDescription}]")]
    public class GenderInfo : BaseCardPartInfo, IEquatable<GenderInfo>
    {
        /// <summary>
        /// The contact's gender
        /// </summary>
        public Gender Gender { get; set; }
        /// <summary>
        /// The contact's gender description
        /// </summary>
        public string? GenderDescription { get; set; }

        internal static BaseCardPartInfo FromStringStatic(string value, PropertyInfo property, int altId, string[] elementTypes, string group, string valueType, Version cardVersion) =>
            (BaseCardPartInfo)new GenderInfo().FromStringInternal(value, property, altId, elementTypes, group, valueType, cardVersion);

        internal override string ToStringInternal(Version cardVersion) =>
            (Gender != Gender.Unspecified ? Gender.ToString()[0] : "") +
            (!string.IsNullOrEmpty(GenderDescription) ? $"{VcardConstants._fieldDelimiter}{GenderDescription}" : "");

        internal override BasePartInfo FromStringInternal(string value, PropertyInfo property, int altId, string[] elementTypes, string group, string valueType, Version cardVersion)
        {
            // Populate field
            string genderString = value;
            string genderDescription = "";

            // Check to see if we have the description or not (as in GENDER:F;grrrl or GENDER:F)
            bool hasGenderDescription = value.Contains(VcardConstants._fieldDelimiter);
            if (hasGenderDescription)
            {
                // We have the description!
                genderString = value.Substring(0, value.IndexOf(VcardConstants._fieldDelimiter));
                genderDescription = value.Substring(value.IndexOf(VcardConstants._fieldDelimiter) + 1);
            }

            // Now, translate the gender string to its enum equivalent
            Gender gender = genderString switch
            {
                "M" => Gender.Male,
                "F" => Gender.Female,
                "O" => Gender.Other,
                "N" => Gender.NotApplicable,
                "U" => Gender.Unknown,
                "" => Gender.Unspecified,
                _ =>
                    throw new InvalidDataException($"Invalid gender string {genderString}")
            };

            // Add the fetched information
            GenderInfo _gender = new(-1, property, elementTypes, group, valueType, gender, genderDescription);
            return _gender;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((GenderInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="GenderInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(GenderInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="GenderInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="GenderInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(GenderInfo source, GenderInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.Gender == target.Gender &&
                source.GenderDescription == target.GenderDescription
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 1213594384;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Gender.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(GenderDescription);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(GenderInfo left, GenderInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(GenderInfo left, GenderInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BasePartInfo source, BasePartInfo target) =>
            ((GenderInfo)source) == ((GenderInfo)target);

        internal GenderInfo() { }

        internal GenderInfo(int altId, PropertyInfo? property, string[] elementTypes, string group, string valueType, Gender gender, string genderDescription) :
            base(property, altId, elementTypes, group, valueType)
        {
            Gender = gender;
            GenderDescription = genderDescription;
        }
    }
}
