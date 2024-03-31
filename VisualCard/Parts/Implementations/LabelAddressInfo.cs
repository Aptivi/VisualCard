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
using System.Text.RegularExpressions;
using VisualCard.Parsers;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact address information
    /// </summary>
    [DebuggerDisplay("LabelAddress = {DeliveryLabel}")]
    public class LabelAddressInfo : BaseCardPartInfo, IEquatable<LabelAddressInfo>
    {
        /// <summary>
        /// The contact's delivery address label
        /// </summary>
        public string DeliveryLabel { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion) =>
            new LabelAddressInfo().FromStringVcardInternal(value, finalArgs, altId, elementTypes, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion) =>
            DeliveryLabel;

        internal override BaseCardPartInfo FromStringVcardInternal(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion)
        {
            bool altIdSupported = cardVersion.Major >= 4;

            // Populate the fields
            string _addressLabel = Regex.Unescape(value);
            LabelAddressInfo _address = new(altIdSupported ? altId : 0, finalArgs, elementTypes, valueType, _addressLabel);
            return _address;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((LabelAddressInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="LabelAddressInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(LabelAddressInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="LabelAddressInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="LabelAddressInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(LabelAddressInfo source, LabelAddressInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.DeliveryLabel == target.DeliveryLabel
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 1203542083;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DeliveryLabel);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(LabelAddressInfo left, LabelAddressInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(LabelAddressInfo left, LabelAddressInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCardPartInfo source, BaseCardPartInfo target) =>
            ((LabelAddressInfo)source) == ((LabelAddressInfo)target);

        internal LabelAddressInfo() { }

        internal LabelAddressInfo(int altId, string[] arguments, string[] elementTypes, string valueType, string label) :
            base(arguments, altId, elementTypes, valueType)
        {
            DeliveryLabel = label;
        }
    }
}
