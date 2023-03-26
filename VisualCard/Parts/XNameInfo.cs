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
    public class XNameInfo : IEquatable<XNameInfo>
    {
        /// <summary>
        /// Alternative ID. Zero if unspecified.
        /// </summary>
        public int AltId { get; }
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
                source.XKeyTypes.SequenceEqual(target.XKeyTypes) &&
                source.XValues.SequenceEqual(target.XValues) &&
                source.AltId == target.AltId &&
                source.XKeyName == target.XKeyName
            ;
        }

        public override int GetHashCode()
        {
            int hashCode = 174715714;
            hashCode = hashCode * -1521134295 + AltId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(XKeyName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(XKeyTypes);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(XValues);
            return hashCode;
        }

        internal XNameInfo() { }

        internal XNameInfo(int altId, string xKeyName, string[] xValues, string[] xKeyTypes)
        {
            AltId = altId;
            XKeyName = xKeyName;
            XValues = xValues;
            XKeyTypes = xKeyTypes;
        }
    }
}
