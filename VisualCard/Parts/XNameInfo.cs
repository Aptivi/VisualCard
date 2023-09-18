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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VisualCard.Parsers;

namespace VisualCard.Parts
{
    /// <summary>
    /// Contact non-standard field entry information
    /// </summary>
    public class XNameInfo : IEquatable<XNameInfo>
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

        internal string ToStringVcardTwo()
        {
            return
                $"{VcardConstants._xSpecifier}" +
                $"{XKeyName}{(XKeyTypes.Length > 0 ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter)}" +
                $"{(XKeyTypes.Length > 0 ? string.Join(VcardConstants._fieldDelimiter.ToString(), XKeyTypes) + VcardConstants._argumentDelimiter : "")}" +
                $"{string.Join(VcardConstants._fieldDelimiter.ToString(), XValues)}";
        }

        internal string ToStringVcardThree()
        {
            return
                $"{VcardConstants._xSpecifier}" +
                $"{XKeyName}{(XKeyTypes.Length > 0 ? VcardConstants._fieldDelimiter : VcardConstants._argumentDelimiter)}" +
                $"{(XKeyTypes.Length > 0 ? string.Join(VcardConstants._fieldDelimiter.ToString(), XKeyTypes) + VcardConstants._argumentDelimiter : "")}" +
                $"{string.Join(VcardConstants._fieldDelimiter.ToString(), XValues)}";
        }

        internal string ToStringVcardFour()
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

        internal static XNameInfo FromStringVcardTwo(string value)
        {
            string xValue = value.Substring(VcardConstants._xSpecifier.Length);
            string[] splitX = xValue.Split(VcardConstants._argumentDelimiter);
            string _xName = splitX[0].Contains(VcardConstants._fieldDelimiter.ToString()) ?
                            splitX[0].Substring(0, splitX[0].IndexOf(VcardConstants._fieldDelimiter)) :
                            splitX[0];
            string[] _xTypes = splitX[0].Contains(VcardConstants._fieldDelimiter.ToString()) ?
                               splitX[0].Substring(splitX[0].IndexOf(VcardConstants._fieldDelimiter) + 1)
                                        .Split(VcardConstants._fieldDelimiter) :
                               Array.Empty<string>();
            string[] _xValues = splitX[1].Split(VcardConstants._fieldDelimiter);
            XNameInfo _x = new(0, Array.Empty<string>(), _xName, _xValues, _xTypes);
            return _x;
        }

        internal static XNameInfo FromStringVcardThree(string value)
        {
            // Get the value
            string xValue = value.Substring(VcardConstants._xSpecifier.Length);
            string[] splitX = xValue.Split(VcardConstants._argumentDelimiter);

            // Populate the name
            string _xName = splitX[0].Contains(VcardConstants._fieldDelimiter.ToString()) ?
                            splitX[0].Substring(0, splitX[0].IndexOf(VcardConstants._fieldDelimiter)) :
                            splitX[0];

            // Populate the fields
            string[] _xTypes = splitX[0].Contains(VcardConstants._fieldDelimiter.ToString()) ?
                               splitX[0].Substring(splitX[0].IndexOf(VcardConstants._fieldDelimiter) + 1)
                                        .Split(VcardConstants._fieldDelimiter) :
                               Array.Empty<string>();
            string[] _xValues = splitX[1].Split(VcardConstants._fieldDelimiter);
            XNameInfo _x = new(0, Array.Empty<string>(), _xName, _xValues, _xTypes);
            return _x;
        }

        internal static XNameInfo FromStringVcardFour(string value, List<string> finalArgs, int altId)
        {
            // Get the value
            string xValue = value.Substring(VcardConstants._xSpecifier.Length);
            string[] splitX = xValue.Split(VcardConstants._argumentDelimiter);

            // Populate the name
            string _xName = splitX[0].Contains(VcardConstants._fieldDelimiter.ToString()) ?
                            splitX[0].Substring(0, splitX[0].IndexOf(VcardConstants._fieldDelimiter)) :
                            splitX[0];

            // Populate the fields
            string[] _xTypes = splitX[0].Contains(VcardConstants._fieldDelimiter.ToString()) ?
                               splitX[0].Substring(splitX[0].IndexOf(VcardConstants._fieldDelimiter) + 1)
                                        .Split(VcardConstants._fieldDelimiter) :
                               Array.Empty<string>();
            string[] _xValues = splitX[1].Split(VcardConstants._fieldDelimiter);
            XNameInfo _x = new(altId, finalArgs.ToArray(), _xName, _xValues, _xTypes);
            return _x;
        }

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
