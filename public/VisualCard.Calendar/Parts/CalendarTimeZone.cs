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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Textify.General;
using VisualCard.Calendar.Parsers;
using VisualCard.Calendar.Parts.Comparers;
using VisualCard.Calendar.Parts.Enums;
using VisualCard.Common.Parts;
using VisualCard.Common.Parts.Comparers;
using VisualCard.Common.Parts.Enums;
using VisualCard.Common.Parts.Implementations;

namespace VisualCard.Calendar.Parts
{
    /// <summary>
    /// A vCalendar card instance
    /// </summary>
    [DebuggerDisplay("vCalendar timezone version {CalendarVersion.ToString()}, parts: (A [{partsArray.Count}] | S [{strings.Count}] | I [{integers.Count}] | E [{extraParts.Count}])")]
    public class CalendarTimeZone : Calendar, IEquatable<CalendarTimeZone>
    {
        internal readonly List<CalendarStandard> standards = [];
        internal readonly List<CalendarDaylight> daylights = [];

        /// <summary>
        /// Standard time list
        /// </summary>
        public CalendarStandard[] StandardTimeList =>
            [.. standards];

        /// <summary>
        /// Daylight time list
        /// </summary>
        public CalendarDaylight[] DaylightTimeList =>
            [.. daylights];

        /// <summary>
        /// Adds a standard time to the calendar time zone info (To validate, you'll need to call <see cref="Calendar.Validate"/>).
        /// </summary>
        /// <param name="standardInstance">Instance of a calendar standard time info</param>
        public void AddStandardTime(CalendarStandard standardInstance) =>
            standards.Add(standardInstance);

        /// <summary>
        /// Deletes a standard time to the calendar time zone info (To validate, you'll need to call <see cref="Calendar.Validate"/>).
        /// </summary>
        /// <param name="standardInstance">Instance of a calendar standard time info</param>
        public void DeleteStandardTime(CalendarStandard standardInstance) =>
            standards.Remove(standardInstance);

        /// <summary>
        /// Adds a daylight time to the calendar time zone info (To validate, you'll need to call <see cref="Calendar.Validate"/>).
        /// </summary>
        /// <param name="daylightInstance">Instance of a calendar daylight time info</param>
        public void AddDaylightTime(CalendarDaylight daylightInstance) =>
            daylights.Add(daylightInstance);

        /// <summary>
        /// Deletes a daylight time to the calendar time zone info (To validate, you'll need to call <see cref="Calendar.Validate"/>).
        /// </summary>
        /// <param name="daylightInstance">Instance of a calendar daylight time info</param>
        public void DeleteDaylightTime(CalendarDaylight daylightInstance) =>
            daylights.Remove(daylightInstance);

        /// <summary>
        /// Saves this parsed calendar timezone info to the string
        /// </summary>
        /// <param name="validate">Whether to validate all the fields or not</param>
        public override string SaveToString(bool validate = false) =>
            SaveToString(CalendarVersion, VCalendarConstants._objectVTimeZoneSpecifier, validate);

        /// <summary>
        /// Saves the calendar timezone info to the returned string
        /// </summary>
        public override string ToString() =>
            SaveToString();

        /// <summary>
        /// Saves the calendar timezone info to the returned string
        /// </summary>
        /// <param name="validate">Whether to validate all the fields or not</param>
        public override string ToString(bool validate) =>
            SaveToString(validate);

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((CalendarTimeZone)obj);

        /// <summary>
        /// Checks to see if both the cards are equal
        /// </summary>
        /// <param name="other">The target <see cref="Calendar"/> instance to check to see if they equal</param>
        /// <returns>True if all the card elements are equal. Otherwise, false.</returns>
        public bool Equals(CalendarTimeZone other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the cards are equal
        /// </summary>
        /// <param name="source">The source <see cref="Calendar"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="Calendar"/> instance to check to see if they equal</param>
        /// <returns>True if all the card elements are equal. Otherwise, false.</returns>
        public bool Equals(CalendarTimeZone source, CalendarTimeZone target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                CommonComparison.ExtraPartsEnumEqual(source.extraParts, target.extraParts) &&
                CalendarPartComparison.PartsArrayEnumEqual(source.partsArray, target.partsArray) &&
                CalendarPartComparison.StringsEqual(source.strings, target.strings) &&
                CalendarPartComparison.IntegersEqual(source.integers, target.integers) &&
                CalendarPartComparison.CompareCalendarComponents(source.standards, target.standards) &&
                CalendarPartComparison.CompareCalendarComponents(source.daylights, target.daylights)
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -1226557388;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<List<CalendarStandard>>.Default.GetHashCode(standards);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<CalendarDaylight>>.Default.GetHashCode(daylights);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<PartsArrayEnum, List<BasePartInfo>>>.Default.GetHashCode(extraParts);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>>>.Default.GetHashCode(partsArray);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CalendarStringsEnum, List<ValueInfo<string>>>>.Default.GetHashCode(strings);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CalendarIntegersEnum, List<ValueInfo<double>>>>.Default.GetHashCode(integers);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(CalendarTimeZone a, CalendarTimeZone b)
            => a.Equals(b);

        /// <inheritdoc/>
        public static bool operator !=(CalendarTimeZone a, CalendarTimeZone b)
            => !a.Equals(b);

        /// <summary>
        /// Makes an empty calendar time zone info
        /// </summary>
        /// <param name="version">vCalendar version to use</param>
        /// <exception cref="ArgumentException"></exception>
        public CalendarTimeZone(Version version) :
            base(version)
        {
            if (version.Major != 2 && version.Minor != 0)
                throw new ArgumentException("Invalid vCalendar version {0} specified. The supported version is 2.0.".FormatString(version));
        }
    }
}
