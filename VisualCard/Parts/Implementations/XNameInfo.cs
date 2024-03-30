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
    /// Contact non-standard field entry information
    /// </summary>
    [DebuggerDisplay("Non-standard = {XKeyName}")]
    public class XNameInfo : BaseCardPartInfo, IEquatable<XNameInfo>
    {
        /// <summary>
        /// X- key name
        /// </summary>
        public string XKeyName { get; }
        /// <summary>
        /// X- values
        /// </summary>
        public string[] XKeyTypes { get; }
        /// <summary>
        /// X- values
        /// </summary>
        public string[] XValues { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, int altId, Version cardVersion) =>
            new XNameInfo().FromStringVcardInternal(value, altId, cardVersion);

        internal static BaseCardPartInfo FromStringVcardWithTypeStatic(string value, string[] finalArgs, int altId, Version cardVersion) =>
            new XNameInfo().FromStringVcardWithTypeInternal(value, finalArgs, altId, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;
            if (altIdSupported)
            {
                bool installAltId = AltId >= 0 && AltArguments.Length > 0;
                bool installType = installAltId && XKeyTypes.Length > 0;
                return
                    $"{VcardConstants._xSpecifier}" +
                    $"{XKeyName}{(installType ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter)}" +
                    $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                    $"{(XKeyTypes.Length > 0 ? string.Join(VcardConstants._fieldDelimiter.ToString(), XKeyTypes) + VcardConstants._argumentDelimiter : "")}" +
                    $"{string.Join(VcardConstants._fieldDelimiter.ToString(), XValues)}";
            }
            else
            {
                return
                    $"{VcardConstants._xSpecifier}" +
                    $"{XKeyName}{(XKeyTypes.Length > 0 ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter)}" +
                    $"{(XKeyTypes.Length > 0 ? string.Join(VcardConstants._fieldDelimiter.ToString(), XKeyTypes) + VcardConstants._argumentDelimiter : "")}" +
                    $"{string.Join(VcardConstants._fieldDelimiter.ToString(), XValues)}";
            }
        }

        internal override BaseCardPartInfo FromStringVcardInternal(string value, int altId, Version cardVersion)
        {
            // Get the value
            string xValue = value.Substring(VcardConstants._xSpecifier.Length);
            string[] splitX = xValue.Split(VcardConstants._argumentDelimiter);

            // Populate the fields
            return InstallInfo(splitX, altId, cardVersion);
        }

        internal override BaseCardPartInfo FromStringVcardWithTypeInternal(string value, string[] finalArgs, int altId, Version cardVersion)
        {
            // Get the value
            string xValue = value.Substring(VcardConstants._xSpecifier.Length);
            string[] splitX = xValue.Split(VcardConstants._argumentDelimiter);

            // Populate the fields
            return InstallInfo(splitX, finalArgs, altId, cardVersion);
        }

        private XNameInfo InstallInfo(string[] splitX, int altId, Version cardVersion) =>
            InstallInfo(splitX, [], altId, cardVersion);

        private XNameInfo InstallInfo(string[] splitX, string[] finalArgs, int altId, Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;

            // Populate the name
            string _xName = splitX[0].Contains(VcardConstants._fieldDelimiter.ToString()) ?
                            splitX[0].Substring(0, splitX[0].IndexOf(VcardConstants._fieldDelimiter)) :
                            splitX[0];

            // Populate the fields
            string[] _xTypes = splitX[0].Contains(VcardConstants._fieldDelimiter.ToString()) ?
                               splitX[0].Substring(splitX[0].IndexOf(VcardConstants._fieldDelimiter) + 1)
                                        .Split(VcardConstants._fieldDelimiter) :
                               [];
            string[] _xValues = splitX[1].Split(VcardConstants._fieldDelimiter);
            XNameInfo _x = new(altIdSupported ? altId : 0, altIdSupported ? finalArgs : [], _xName, _xValues, _xTypes);
            return _x;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            base.Equals(obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="XNameInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(XNameInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="XNameInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="XNameInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(XNameInfo source, XNameInfo target)
        {
            // We can't perform this operation on null.
            if (source is null)
                return false;

            // Check all the properties
            return
                source.AltArguments.SequenceEqual(target.AltArguments) &&
                source.XKeyTypes.SequenceEqual(target.XKeyTypes) &&
                source.XValues.SequenceEqual(target.XValues) &&
                source.AltId == target.AltId &&
                source.XKeyName == target.XKeyName
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 1235403650;
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AltArguments);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(XKeyName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(XKeyTypes);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(XValues);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(XNameInfo left, XNameInfo right) =>
            EqualityComparer<XNameInfo>.Default.Equals(left, right);

        /// <inheritdoc/>
        public static bool operator !=(XNameInfo left, XNameInfo right) =>
            !(left == right);

        internal XNameInfo() { }

        internal XNameInfo(int altId, string[] altArguments, string xKeyName, string[] xValues, string[] xKeyTypes)
        {
            AltId = altId;
            AltArguments = altArguments;
            XKeyName = xKeyName;
            XValues = xValues;
            XKeyTypes = xKeyTypes;
        }
    }
}
