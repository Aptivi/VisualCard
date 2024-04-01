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
using System.Xml;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact XML info
    /// </summary>
    [DebuggerDisplay("XML = {XML}")]
    public class XmlInfo : BaseCardPartInfo, IEquatable<XmlInfo>
    {
        /// <summary>
        /// The contact's XML field
        /// </summary>
        public XmlDocument Xml { get; }
        
        /// <summary>
        /// The contact's XML string that generated the <see cref="Xml"/> property
        /// </summary>
        public string XmlString { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion) =>
            new XmlInfo().FromStringVcardInternal(value, finalArgs, altId, elementTypes, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion) =>
            XmlString;

        internal override BaseCardPartInfo FromStringVcardInternal(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion)
        {
            // Check to see if the XML document is valid or not
            string finalXml =
                $"""
                <?xml version="1.0" encoding="UTF-8"?>
                <visualcardroot>
                {value.Replace("\\n", "\n")}
                </visualcardroot>
                """;
            XmlDocument doc = new();
            doc.LoadXml(finalXml);

            // Add the fetched information
            bool altIdSupported = cardVersion.Major >= 4;
            XmlInfo _xml = new(altIdSupported ? altId : 0, finalArgs, elementTypes, valueType, doc, value);
            return _xml;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((XmlInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="XmlInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(XmlInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="XmlInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="XmlInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(XmlInfo source, XmlInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.Xml == target.Xml
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 572884467;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<XmlDocument>.Default.GetHashCode(Xml);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(XmlString);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(XmlInfo left, XmlInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(XmlInfo left, XmlInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCardPartInfo source, BaseCardPartInfo target) =>
            ((XmlInfo)source) == ((XmlInfo)target);

        internal XmlInfo() { }

        internal XmlInfo(int altId, string[] arguments, string[] elementTypes, string valueType, XmlDocument xml, string xmlString) :
            base(arguments, altId, elementTypes, valueType)
        {
            Xml = xml;
            XmlString = xmlString;
        }
    }
}
