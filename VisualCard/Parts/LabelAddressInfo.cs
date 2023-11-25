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
                $"{VcardConstants._addressSpecifier};" +
                $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", AddressTypes)}{VcardConstants._argumentDelimiter}" +
                $"{DeliveryLabel}";
        }

        internal string ToStringVcardThree()
        {
            return
                $"{VcardConstants._addressSpecifier};" +
                $"{VcardConstants._typeArgumentSpecifier}{string.Join(",", AddressTypes)}{VcardConstants._argumentDelimiter}" +
                $"{DeliveryLabel}";
        }

        internal static LabelAddressInfo FromStringVcardTwo(string value)
        {
            // Get the value
            string adrValue = value.Substring(VcardConstants._addressSpecifier.Length + 1);
            string[] splitAdr = adrValue.Split(VcardConstants._argumentDelimiter);

            // Check the provided address
            string[] splitAddressValues = splitAdr[0].Split(VcardConstants._fieldDelimiter);
            if (splitAddressValues.Length < 1)
                throw new InvalidDataException("Label address information must specify exactly one value (address label)");

            // Populate the fields
            string[] _addressTypes = new string[] { "HOME" };
            string _addressLabel = Regex.Unescape(splitAddressValues[0]);
            LabelAddressInfo _address = new(0, [], _addressTypes, _addressLabel);
            return _address;
        }

        internal static LabelAddressInfo FromStringVcardTwoWithType(string value)
        {
            // Get the value
            string adrValue = value.Substring(VcardConstants._addressSpecifier.Length + 1);
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
            string adrValue = value.Substring(VcardConstants._addressSpecifier.Length + 1);
            string[] splitAdr = adrValue.Split(VcardConstants._argumentDelimiter);

            // Check the provided address
            string[] splitAddressValues = splitAdr[0].Split(VcardConstants._fieldDelimiter);
            if (splitAddressValues.Length < 1)
                throw new InvalidDataException("Label address information must specify exactly one value (address label)");

            // Populate the fields
            string[] _addressTypes = new string[] { "HOME" };
            string _addressLabel = Regex.Unescape(splitAddressValues[0]);
            LabelAddressInfo _address = new(0, [], _addressTypes, _addressLabel);
            return _address;
        }

        internal static LabelAddressInfo FromStringVcardThreeWithType(string value)
        {
            // Get the value
            string adrValue = value.Substring(VcardConstants._addressSpecifier.Length + 1);
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
