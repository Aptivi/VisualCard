﻿//
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
using System.Text.RegularExpressions;
using VisualCard.Parsers;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact full name info
    /// </summary>
    [DebuggerDisplay("Full name = {FullName}")]
    public class FullNameInfo : BaseCardPartInfo, IEquatable<FullNameInfo>
    {
        /// <summary>
        /// The contact's full name
        /// </summary>
        public string FullName { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion) =>
            new FullNameInfo().FromStringVcardInternal(value, finalArgs, altId, elementTypes, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion) =>
            FullName;

        internal override BaseCardPartInfo FromStringVcardInternal(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion)
        {
            // Get the value
            string _fullNameStr = Regex.Unescape(value);

            // Populate the fields
            FullNameInfo _fullName = new(altId, finalArgs, elementTypes, valueType, _fullNameStr);
            return _fullName;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((FullNameInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="FullNameInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(FullNameInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="FullNameInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="FullNameInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(FullNameInfo source, FullNameInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.FullName == target.FullName
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -1441119731;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FullName);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(FullNameInfo left, FullNameInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(FullNameInfo left, FullNameInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCardPartInfo source, BaseCardPartInfo target) =>
            ((FullNameInfo)source) == ((FullNameInfo)target);

        internal FullNameInfo() { }

        internal FullNameInfo(int altId, string[] arguments, string[] elementTypes, string valueType, string fullName) :
            base(arguments, altId, elementTypes, valueType)
        {
            FullName = fullName;
        }
    }
}
