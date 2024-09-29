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

namespace VisualCard.Calendar.Parts.Implementations
{
    /// <summary>
    /// Calendar excluded date info
    /// </summary>
    [DebuggerDisplay("ExDate = {ExDates}")]
    public class ExDateInfo : BaseCalendarPartInfo, IEquatable<ExDateInfo>
    {
        /// <summary>
        /// The excluded date list
        /// </summary>
        public string[]? ExDates { get; }

        internal static BaseCalendarPartInfo FromStringVcalendarStatic(string value, string[] finalArgs, string[] elementTypes, string valueType, Version cardVersion) =>
            new ExDateInfo().FromStringVcalendarInternal(value, finalArgs, elementTypes, valueType, cardVersion);

        internal override string ToStringVcalendarInternal(Version cardVersion) =>
            $"{string.Join(cardVersion.Major == 1 ? ";" : ",", ExDates)}";

        internal override BaseCalendarPartInfo FromStringVcalendarInternal(string value, string[] finalArgs, string[] elementTypes, string valueType, Version cardVersion)
        {
            // Populate the fields
            var exDates = Regex.Unescape(value).Split(cardVersion.Major == 1 ? ';' : ',');

            // Add the fetched information
            ExDateInfo _time = new([], elementTypes, valueType, exDates);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((ExDateInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="ExDateInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(ExDateInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="ExDateInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="ExDateInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(ExDateInfo source, ExDateInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.ExDates == target.ExDates
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 498518712;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]?>.Default.GetHashCode(ExDates);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(ExDateInfo left, ExDateInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(ExDateInfo left, ExDateInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCalendarPartInfo source, BaseCalendarPartInfo target) =>
            (ExDateInfo)source == (ExDateInfo)target;

        internal ExDateInfo() { }

        internal ExDateInfo(string[] arguments, string[] elementTypes, string valueType, string[] exDates) :
            base(arguments, elementTypes, valueType)
        {
            ExDates = exDates;
        }
    }
}