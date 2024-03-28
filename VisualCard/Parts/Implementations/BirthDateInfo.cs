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
    /// Contact birth date info
    /// </summary>
    [DebuggerDisplay("Birth date = {BirthDate}")]
    public class BirthDateInfo : BaseCardPartInfo, IEquatable<BirthDateInfo>
    {
        /// <summary>
        /// The contact's birth date
        /// </summary>
        public DateTime? BirthDate { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, int altId, Version cardVersion, StreamReader cardContentReader) =>
            new BirthDateInfo().FromStringVcardInternal(value, altId, cardVersion, cardContentReader);

        internal static BaseCardPartInfo FromStringVcardWithTypeStatic(string value, string[] finalArgs, int altId, Version cardVersion, StreamReader cardContentReader) =>
            new BirthDateInfo().FromStringVcardWithTypeInternal(value, finalArgs, altId, cardVersion, cardContentReader);

        internal override string ToStringVcardInternal(Version cardVersion) =>
            $"{VcardConstants._birthSpecifier}:{BirthDate:yyyyMMdd}";

        internal override BaseCardPartInfo FromStringVcardInternal(string value, int altId, Version cardVersion, StreamReader cardContentReader)
        {
            // Get the value
            string bdayValue = value.Substring(VcardConstants._birthSpecifier.Length + 1);

            // Populate the fields
            return InstallInfo(bdayValue, altId, cardVersion);
        }

        internal override BaseCardPartInfo FromStringVcardWithTypeInternal(string value, string[] finalArgs, int altId, Version cardVersion, StreamReader cardContentReader)
        {
            // Get the value
            string bdayValue = value.Substring(value.IndexOf(VcardConstants._argumentDelimiter) + 1);

            // Populate the fields
            return InstallInfo(bdayValue, finalArgs, altId, cardVersion);
        }

        private BirthDateInfo InstallInfo(string value, int altId, Version cardVersion) =>
            InstallInfo(value, [], altId, cardVersion);

        private BirthDateInfo InstallInfo(string value, string[] finalArgs, int altId, Version cardVersion)
        {
            // Populate field
            DateTime bday;
            if (int.TryParse(value, out _) && value.Length == 8)
            {
                int birthNum = int.Parse(value);
                var birthDigits = VcardParserTools.GetDigits(birthNum).ToList();
                int birthYear = birthDigits[0] * 1000 + birthDigits[1] * 100 + birthDigits[2] * 10 + birthDigits[3];
                int birthMonth = birthDigits[4] * 10 + birthDigits[5];
                int birthDay = birthDigits[6] * 10 + birthDigits[7];
                bday = new DateTime(birthYear, birthMonth, birthDay);
            }
            else
                bday = DateTime.Parse(value);

            // Add the fetched information
            bool altIdSupported = cardVersion.Major >= 4;
            BirthDateInfo _time = new(altIdSupported ? altId : 0, altIdSupported ? finalArgs : [], bday);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            base.Equals(obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="BirthDateInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(BirthDateInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="BirthDateInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="BirthDateInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(BirthDateInfo source, BirthDateInfo target)
        {
            // We can't perform this operation on null.
            if (source is null)
                return false;

            // Check all the properties
            return
                source.AltArguments.SequenceEqual(target.AltArguments) &&
                source.AltId == target.AltId &&
                source.BirthDate == target.BirthDate
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -480211805;
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AltArguments);
            hashCode = hashCode * -1521134295 + BirthDate.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(BirthDateInfo left, BirthDateInfo right) =>
            EqualityComparer<BirthDateInfo>.Default.Equals(left, right);

        /// <inheritdoc/>
        public static bool operator !=(BirthDateInfo left, BirthDateInfo right) =>
            !(left == right);

        internal BirthDateInfo() { }

        internal BirthDateInfo(int altId, string[] altArguments, DateTime? birth)
        {
            AltId = altId;
            AltArguments = altArguments;
            BirthDate = birth;
        }
    }
}
