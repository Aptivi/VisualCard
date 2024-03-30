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
using VisualCard.Parts.Implementations.Tools;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact gender info
    /// </summary>
    [DebuggerDisplay("Gender = {Gender}")]
    public class GenderInfo : BaseCardPartInfo, IEquatable<GenderInfo>
    {
        /// <summary>
        /// The contact's gender
        /// </summary>
        public Gender Gender { get; }
        /// <summary>
        /// The contact's gender description
        /// </summary>
        public string GenderDescription { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, int altId, Version cardVersion) =>
            new GenderInfo().FromStringVcardInternal(value, altId, cardVersion);

        internal static BaseCardPartInfo FromStringVcardWithTypeStatic(string value, string[] finalArgs, int altId, Version cardVersion) =>
            new GenderInfo().FromStringVcardWithTypeInternal(value, finalArgs, altId, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion) =>
            $"{VcardConstants._genderSpecifier}{VcardConstants._argumentDelimiter}" +
            (Gender != Gender.Unspecified ? Gender.ToString()[0] : "") +
            (!string.IsNullOrEmpty(GenderDescription) ? $"{VcardConstants._fieldDelimiter}{GenderDescription}" : "");

        internal override BaseCardPartInfo FromStringVcardInternal(string value, int altId, Version cardVersion)
        {
            // Get the value
            string genderValue = value.Substring(VcardConstants._genderSpecifier.Length + 1);

            // Populate the fields
            return InstallInfo(genderValue);
        }

        internal override BaseCardPartInfo FromStringVcardWithTypeInternal(string value, string[] finalArgs, int altId, Version cardVersion) =>
            FromStringVcardInternal(value, altId, cardVersion);

        private GenderInfo InstallInfo(string value)
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
                ""  => Gender.Unspecified,
                _ =>
                    throw new InvalidDataException($"Invalid gender string {genderString}")
            };

            // Add the fetched information
            GenderInfo _gender = new(0, [], gender, genderDescription);
            return _gender;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            base.Equals(obj);

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
            if (source is null)
                return false;

            // Check all the properties
            return
                source.AltArguments.SequenceEqual(target.AltArguments) &&
                source.AltId == target.AltId &&
                source.Gender == target.Gender &&
                source.GenderDescription == target.GenderDescription
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -446160983;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AltArguments);
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + Gender.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(GenderDescription);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(GenderInfo left, GenderInfo right) =>
            EqualityComparer<GenderInfo>.Default.Equals(left, right);

        /// <inheritdoc/>
        public static bool operator !=(GenderInfo left, GenderInfo right) =>
            !(left == right);

        internal GenderInfo() { }

        internal GenderInfo(int altId, string[] altArguments, Gender gender, string genderDescription)
        {
            AltId = altId;
            AltArguments = altArguments;
            Gender = gender;
            GenderDescription = genderDescription;
        }
    }
}
