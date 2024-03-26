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

namespace VisualCard.Parts
{
    /// <summary>
    /// Contact address information
    /// </summary>
    [DebuggerDisplay("LabelAddress = {DeliveryLabel}")]
    public class LabelAddressInfo : IEquatable<LabelAddressInfo>
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
        /// The contact's address types
        /// </summary>
        public string[] AddressTypes { get; }
        /// <summary>
        /// The contact's delivery address label
        /// </summary>
        public string DeliveryLabel { get; }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            base.Equals(obj);

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
            if (source is null)
                return false;

            // Check all the properties
            return
                source.AddressTypes.SequenceEqual(target.AddressTypes) &&
                source.AltArguments.SequenceEqual(target.AltArguments) &&
                source.AltId == target.AltId &&
                source.DeliveryLabel == target.DeliveryLabel
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 1313918102;
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AltArguments);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(AddressTypes);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DeliveryLabel);
            return hashCode;
        }

        internal string ToStringVcardTwo()
        {
            return
                $"{VcardConstants._labelSpecifier};" +
                $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", AddressTypes)}{VcardConstants._argumentDelimiter}" +
                $"{DeliveryLabel}";
        }

        internal string ToStringVcardThree()
        {
            return
                $"{VcardConstants._labelSpecifier};" +
                $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", AddressTypes)}{VcardConstants._argumentDelimiter}" +
                $"{DeliveryLabel}";
        }

        internal string ToStringVcardFive()
        {
            bool installAltId = AltId >= 0 && AltArguments.Length > 0;
            return
                $"{VcardConstants._labelSpecifier};" +
                $"{(installAltId ? VcardConstants._altIdArgumentSpecifier + AltId + VcardConstants._fieldDelimiter : "")}" +
                $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", AddressTypes)}{VcardConstants._argumentDelimiter}" +
                $"{DeliveryLabel}";
        }

        internal static LabelAddressInfo FromStringVcardTwo(string value)
        {
            // Get the value
            string adrValue = value.Substring(VcardConstants._labelSpecifier.Length + 1);
            string[] splitAdr = adrValue.Split(VcardConstants._argumentDelimiter);

            // Check the provided address
            string[] splitAddressValues = splitAdr[0].Split(VcardConstants._fieldDelimiter);
            if (splitAddressValues.Length < 1)
                throw new InvalidDataException("Label address information must specify exactly one value (address label)");

            // Populate the fields
            string[] _addressTypes = ["HOME"];
            string _addressLabel = Regex.Unescape(splitAddressValues[0]);
            LabelAddressInfo _address = new(0, [], _addressTypes, _addressLabel);
            return _address;
        }

        internal static LabelAddressInfo FromStringVcardTwoWithType(string value)
        {
            // Get the value
            string adrValue = value.Substring(VcardConstants._labelSpecifier.Length + 1);
            string[] splitAdr = adrValue.Split(VcardConstants._argumentDelimiter);
            if (splitAdr.Length < 2)
                throw new InvalidDataException("Label address field must specify exactly two values (Type (optionally prepended with TYPE=), and address information)");

            // Check the provided address
            string[] splitAddressValues = splitAdr[1].Split(VcardConstants._fieldDelimiter);
            if (splitAddressValues.Length < 1)
                throw new InvalidDataException("Label address information must specify exactly one value (address label)");

            // Populate the fields
            string[] _addressTypes = VcardParserTools.GetTypes(splitAdr, "HOME", false);
            string _addressLabel = Regex.Unescape(splitAddressValues[0]);
            LabelAddressInfo _address = new(0, [], _addressTypes, _addressLabel);
            return _address;
        }

        internal static LabelAddressInfo FromStringVcardThree(string value)
        {
            // Get the value
            string adrValue = value.Substring(VcardConstants._labelSpecifier.Length + 1);
            string[] splitAdr = adrValue.Split(VcardConstants._argumentDelimiter);

            // Check the provided address
            string[] splitAddressValues = splitAdr[0].Split(VcardConstants._fieldDelimiter);
            if (splitAddressValues.Length < 1)
                throw new InvalidDataException("Label address information must specify exactly one value (address label)");

            // Populate the fields
            string[] _addressTypes = ["HOME"];
            string _addressLabel = Regex.Unescape(splitAddressValues[0]);
            LabelAddressInfo _address = new(0, [], _addressTypes, _addressLabel);
            return _address;
        }

        internal static LabelAddressInfo FromStringVcardThreeWithType(string value)
        {
            // Get the value
            string adrValue = value.Substring(VcardConstants._labelSpecifier.Length + 1);
            string[] splitAdr = adrValue.Split(VcardConstants._argumentDelimiter);
            if (splitAdr.Length < 2)
                throw new InvalidDataException("Label address field must specify exactly two values (Type (must be prepended with TYPE=), and address information)");

            // Check the provided address
            string[] splitAddressValues = splitAdr[1].Split(VcardConstants._fieldDelimiter);
            if (splitAddressValues.Length < 1)
                throw new InvalidDataException("Label address information must specify exactly one value (address label)");

            // Populate the fields
            string[] _addressTypes = VcardParserTools.GetTypes(splitAdr, "HOME", true);
            string _addressLabel = Regex.Unescape(splitAddressValues[0]);
            LabelAddressInfo _address = new(0, [], _addressTypes, _addressLabel);
            return _address;
        }

        internal static LabelAddressInfo FromStringVcardFive(string value, int altId)
        {
            // Get the value
            string adrValue = value.Substring(VcardConstants._labelSpecifier.Length + 1);
            string[] splitAdr = adrValue.Split(VcardConstants._argumentDelimiter);

            // Check the provided address
            string[] splitAddressValues = splitAdr[0].Split(VcardConstants._fieldDelimiter);
            if (splitAddressValues.Length < 1)
                throw new InvalidDataException("Label address information must specify exactly one value (address label)");

            // Populate the fields
            string[] _addressTypes = ["HOME"];
            string _addressLabel = Regex.Unescape(splitAddressValues[0]);
            LabelAddressInfo _address = new(altId, [], _addressTypes, _addressLabel);
            return _address;
        }

        internal static LabelAddressInfo FromStringVcardFiveWithType(string value, List<string> finalArgs, int altId)
        {
            // Get the value
            string adrValue = value.Substring(VcardConstants._labelSpecifier.Length + 1);
            string[] splitAdr = adrValue.Split(VcardConstants._argumentDelimiter);
            if (splitAdr.Length < 2)
                throw new InvalidDataException("Label address field must specify exactly two values (Type (must be prepended with TYPE=), and address information)");

            // Check the provided address
            string[] splitAddressValues = splitAdr[1].Split(VcardConstants._fieldDelimiter);
            if (splitAddressValues.Length < 1)
                throw new InvalidDataException("Label address information must specify exactly one value (address label)");

            // Populate the fields
            string[] _addressTypes = VcardParserTools.GetTypes(splitAdr, "HOME", true);
            string _addressLabel = Regex.Unescape(splitAddressValues[0]);
            LabelAddressInfo _address = new(altId, [.. finalArgs], _addressTypes, _addressLabel);
            return _address;
        }

        internal LabelAddressInfo() { }

        internal LabelAddressInfo(int altId, string[] altArguments, string[] addressTypes, string label)
        {
            AltId = altId;
            AltArguments = altArguments;
            AddressTypes = addressTypes;
            DeliveryLabel = label;
        }
    }
}
