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
using System.Text;
using System.Text.RegularExpressions;
using Textify.General;
using VisualCard.Parsers.Arguments;

namespace VisualCard.Calendar.Parts.Implementations
{
    /// <summary>
    /// Calendar request status info
    /// </summary>
    [DebuggerDisplay("Request Status = {RequestStatus}")]
    public class RequestStatusInfo : BaseCalendarPartInfo, IEquatable<RequestStatusInfo>
    {
        /// <summary>
        /// The calendar's request status code
        /// </summary>
        public (int, int, int) RequestStatus { get; }
        
        /// <summary>
        /// The calendar's request status description
        /// </summary>
        public string? RequestStatusDesc { get; }
        
        /// <summary>
        /// The calendar's request status external data
        /// </summary>
        public string? RequestStatusExtData { get; }

        internal static BaseCalendarPartInfo FromStringVcalendarStatic(string value, PropertyInfo property, string[] elementTypes, string group, string valueType, Version cardVersion) =>
            new RequestStatusInfo().FromStringVcalendarInternal(value, property, elementTypes, group, valueType, cardVersion);

        internal override string ToStringVcalendarInternal(Version cardVersion)
        {
            var statusBuilder = new StringBuilder();

            // Build the status code
            statusBuilder.Append(RequestStatus.Item1 + "." + RequestStatus.Item2);
            if (RequestStatus.Item3 > 0)
                statusBuilder.Append("." + RequestStatus.Item3);

            // Build the status description
            statusBuilder.Append($";{RequestStatusDesc ?? ""}");

            // Build the status external data if found
            string extData = RequestStatusExtData ?? "";
            if (!string.IsNullOrEmpty(extData))
                statusBuilder.Append($";{extData}");
            return statusBuilder.ToString();
        }

        internal override BaseCalendarPartInfo FromStringVcalendarInternal(string value, PropertyInfo property, string[] elementTypes, string group, string valueType, Version cardVersion)
        {
            // Get the request and split it with the semicolon two times
            var requestStatus = Regex.Unescape(value);
            if (!requestStatus.Contains(";"))
                throw new InvalidDataException("There is no semicolon to split between status number and status description and, possibly, the external data pair.");

            // Get the status number
            string statusStr = requestStatus.Substring(0, requestStatus.IndexOf(";"));
            if (!statusStr.Contains("."))
                throw new InvalidDataException("There is no dot to split two or three status codes.");
            string[] statusSplit = statusStr.Split('.');
            if (statusSplit.Length < 2 || statusSplit.Length > 3)
                throw new InvalidDataException($"After splitting, got {statusSplit.Length} items instead of 2 or 3.");
            (int, int, int) statusTuple = default;
            if (!int.TryParse(statusSplit[0], out statusTuple.Item1))
                throw new InvalidDataException($"Status is not a number. First: {statusSplit[0]}.");
            if (!int.TryParse(statusSplit[1], out statusTuple.Item2))
                throw new InvalidDataException($"Status is not a number. Second: {statusSplit[1]}.");
            if (statusSplit.Length == 3 && !int.TryParse(statusSplit[2], out statusTuple.Item3))
                throw new InvalidDataException($"Status is not a number. Third: {statusSplit[2]}.");

            // Get the property pair and split it
            string pair = requestStatus.RemovePrefix($"{statusStr};");

            // Get the status number
            string statusDesc = pair.Contains(";") ? pair.Substring(0, pair.IndexOf(";")) : pair;
            string statusExtData = pair.Contains(";") ? pair.RemovePrefix($"{statusDesc};") : "";

            // Add the fetched information
            RequestStatusInfo _time = new(property, elementTypes, group, valueType, statusTuple, statusDesc, statusExtData);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((RequestStatusInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="RequestStatusInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(RequestStatusInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="RequestStatusInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="RequestStatusInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(RequestStatusInfo source, RequestStatusInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.RequestStatus == target.RequestStatus
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 1940289686;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + RequestStatus.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(RequestStatusDesc);
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(RequestStatusExtData);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(RequestStatusInfo left, RequestStatusInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(RequestStatusInfo left, RequestStatusInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCalendarPartInfo source, BaseCalendarPartInfo target) =>
            (RequestStatusInfo)source == (RequestStatusInfo)target;

        internal RequestStatusInfo() { }

        internal RequestStatusInfo(PropertyInfo? property, string[] elementTypes, string group, string valueType, (int, int, int) requestStatus, string statusDesc, string statusExtData) :
            base(property, elementTypes, group, valueType)
        {
            RequestStatus = requestStatus;
            RequestStatusDesc = statusDesc;
            RequestStatusExtData = statusExtData;
        }
    }
}
