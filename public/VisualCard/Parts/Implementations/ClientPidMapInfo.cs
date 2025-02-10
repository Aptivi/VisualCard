//
// VisualCard  Copyright (C) 2021-2025  Aptivi
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
using Textify.General;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Client PID map info
    /// </summary>
    [DebuggerDisplay("Client PID Map = {PidNum};{PidUri}")]
    public class ClientPidMapInfo : BaseCardPartInfo, IEquatable<ClientPidMapInfo>
    {
        /// <summary>
        /// Client PID number
        /// </summary>
        public int PidNum { get; set; }
        
        /// <summary>
        /// Client PID URI
        /// </summary>
        public string? PidUri { get; set; }

        internal static BaseCardPartInfo FromStringStatic(string value, PropertyInfo property, int altId, string[] elementTypes, string group, string valueType, Version cardVersion) =>
            (BaseCardPartInfo)new ClientPidMapInfo().FromStringInternal(value, property, altId, elementTypes, group, valueType, cardVersion);

        internal override string ToStringInternal(Version cardVersion) =>
            $"{PidNum};{PidUri ?? ""}";

        internal override BasePartInfo FromStringInternal(string value, PropertyInfo property, int altId, string[] elementTypes, string group, string valueType, Version cardVersion)
        {
            // Split the client PID map info
            if (!value.Contains(";"))
                throw new InvalidDataException($"Client PID map representation is invalid: {value}");

            // Parse the info
            string pidNumStr = value.Substring(0, value.IndexOf(";"));
            string pidUriStr = value.RemoveSuffix($"{pidNumStr};");
            if (!int.TryParse(pidNumStr, out int pidNum))
                throw new InvalidDataException($"PID number {pidNumStr} is invalid");
            if (!Uri.TryCreate(pidUriStr, UriKind.Absolute, out Uri uri))
                throw new InvalidDataException($"PID URI {pidUriStr} is invalid");
            pidUriStr = uri.ToString();

            // Populate the fields
            ClientPidMapInfo _source = new(altId, property, elementTypes, group, valueType, pidNum, pidUriStr);
            return _source;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((ClientPidMapInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="ClientPidMapInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(ClientPidMapInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="ClientPidMapInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="ClientPidMapInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(ClientPidMapInfo source, ClientPidMapInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.PidNum == target.PidNum &&
                source.PidUri == target.PidUri
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -659626044;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + PidNum.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(PidUri);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(ClientPidMapInfo left, ClientPidMapInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(ClientPidMapInfo left, ClientPidMapInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BasePartInfo source, BasePartInfo target) =>
            ((ClientPidMapInfo)source) == ((ClientPidMapInfo)target);

        internal ClientPidMapInfo() { }

        internal ClientPidMapInfo(int altId, PropertyInfo? property, string[] elementTypes, string group, string valueType, int pidNum, string pidUri) :
            base(property, altId, elementTypes, group, valueType)
        {
            PidNum = pidNum;
            PidUri = pidUri;
        }
    }
}
