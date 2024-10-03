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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VisualCard.Parsers;
using VisualCard.Parsers.Arguments;

namespace VisualCard.Calendar.Parts.Implementations
{
    /// <summary>
    /// Calendar recurrence date info
    /// </summary>
    [DebuggerDisplay("{RecDates.Length} recurrence dates")]
    public class RecDateInfo : BaseCalendarPartInfo, IEquatable<RecDateInfo>
    {
        /// <summary>
        /// The recurrence date list
        /// </summary>
        public TimePeriod[]? RecDates { get; }

        internal static BaseCalendarPartInfo FromStringVcalendarStatic(string value, ArgumentInfo[] finalArgs, string[] elementTypes, string group, string valueType, Version cardVersion) =>
            new RecDateInfo().FromStringVcalendarInternal(value, finalArgs, elementTypes, group, valueType, cardVersion);

        internal override string ToStringVcalendarInternal(Version cardVersion)
        {
            if (RecDates is null)
                return "";

            var builder = new StringBuilder();
            if (cardVersion.Major == 1)
                builder.Append(string.Join(";", RecDates.Select((dt) => VcardCommonTools.SavePosixDate(dt.StartDate))));
            else
            {
                string type = ValueType ?? "";
                builder.Append(VcardCommonTools.SavePosixDate(RecDates[0].StartDate));
                if (RecDates[0].StartDate != RecDates[0].EndDate && type.Equals("period", StringComparison.OrdinalIgnoreCase))
                    builder.Append("/" + VcardCommonTools.SavePosixDate(RecDates[0].EndDate));
            }
            return builder.ToString();
        }

        internal override BaseCalendarPartInfo FromStringVcalendarInternal(string value, ArgumentInfo[] finalArgs, string[] elementTypes, string group, string valueType, Version cardVersion)
        {
            // Populate the fields
            TimePeriod[] recDates = [];
            if (cardVersion.Major == 1)
            {
                var recDateStrings = Regex.Unescape(value).Split(';');
                recDates = recDateStrings.Select((date) =>
                {
                    var parsedDate = VcardCommonTools.ParsePosixDateTime(date);
                    return new TimePeriod(parsedDate, parsedDate);
                }).ToArray();
            }
            else
            {
                // Check to see if it's a period
                if (valueType.Equals("period", StringComparison.OrdinalIgnoreCase))
                {
                    var parsedPeriod = VcardCommonTools.GetTimePeriod(value);
                    recDates = [parsedPeriod];
                }
                else if (valueType.Equals("date", StringComparison.OrdinalIgnoreCase))
                {
                    // Not a period. Use date
                    var parsedDate = VcardCommonTools.ParsePosixDate(value);
                    recDates = [new TimePeriod(parsedDate, parsedDate)];
                }
                else
                {
                    // Not a period. Continue using normal date and time
                    var parsedDate = VcardCommonTools.ParsePosixDateTime(value);
                    recDates = [new TimePeriod(parsedDate, parsedDate)];
                }
            }

            // Add the fetched information
            RecDateInfo _time = new([], elementTypes, group, valueType, recDates);
            return _time;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((RecDateInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="RecDateInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(RecDateInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="RecDateInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="RecDateInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(RecDateInfo source, RecDateInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.RecDates == target.RecDates
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 498518712;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<TimePeriod[]?>.Default.GetHashCode(RecDates);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(RecDateInfo left, RecDateInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(RecDateInfo left, RecDateInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCalendarPartInfo source, BaseCalendarPartInfo target) =>
            (RecDateInfo)source == (RecDateInfo)target;

        internal RecDateInfo() { }

        internal RecDateInfo(ArgumentInfo[] arguments, string[] elementTypes, string group, string valueType, TimePeriod[] recDates) :
            base(arguments, elementTypes, group, valueType)
        {
            RecDates = recDates;
        }
    }
}
