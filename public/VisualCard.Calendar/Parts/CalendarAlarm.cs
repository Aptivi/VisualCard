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
using System.IO;
using System.Linq;
using Textify.General;
using VisualCard.Calendar.Parsers;
using VisualCard.Calendar.Parts.Comparers;
using VisualCard.Calendar.Parts.Enums;
using VisualCard.Common.Diagnostics;
using VisualCard.Common.Parts;
using VisualCard.Common.Parts.Comparers;
using VisualCard.Common.Parts.Enums;
using VisualCard.Common.Parts.Implementations;

namespace VisualCard.Calendar.Parts
{
    /// <summary>
    /// A vCalendar card instance
    /// </summary>
    [DebuggerDisplay("vCalendar alarm version {CalendarVersion.ToString()}, parts: (A [{partsArray.Count}] | S [{strings.Count}] | I [{integers.Count}] | E [{extraParts.Count}])")]
    public class CalendarAlarm : Calendar, IEquatable<CalendarAlarm>
    {
        /// <summary>
        /// Saves this parsed calendar alarm to the string
        /// </summary>
        /// <param name="validate">Whether to validate all the fields or not</param>
        public override string SaveToString(bool validate = false) =>
            SaveToString(CalendarVersion, VCalendarConstants._objectVAlarmSpecifier, validate);

        /// <summary>
        /// Saves the calendar alarm to the returned string
        /// </summary>
        public override string ToString() =>
            SaveToString();

        /// <summary>
        /// Saves the calendar alarm to the returned string
        /// </summary>
        /// <param name="validate">Whether to validate all the fields or not</param>
        public override string ToString(bool validate) =>
            SaveToString(validate);

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((CalendarAlarm)obj);

        /// <summary>
        /// Checks to see if both the cards are equal
        /// </summary>
        /// <param name="other">The target <see cref="Calendar"/> instance to check to see if they equal</param>
        /// <returns>True if all the card elements are equal. Otherwise, false.</returns>
        public bool Equals(CalendarAlarm other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the cards are equal
        /// </summary>
        /// <param name="source">The source <see cref="Calendar"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="Calendar"/> instance to check to see if they equal</param>
        /// <returns>True if all the card elements are equal. Otherwise, false.</returns>
        public bool Equals(CalendarAlarm source, CalendarAlarm target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                CommonComparison.ExtraPartsEnumEqual(source.extraParts, target.extraParts) &&
                CalendarPartComparison.PartsArrayEnumEqual(source.partsArray, target.partsArray) &&
                CalendarPartComparison.StringsEqual(source.strings, target.strings) &&
                CalendarPartComparison.IntegersEqual(source.integers, target.integers)
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 1333672311;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<PartsArrayEnum, List<BasePartInfo>>>.Default.GetHashCode(extraParts);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>>>.Default.GetHashCode(partsArray);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CalendarStringsEnum, List<ValueInfo<string>>>>.Default.GetHashCode(strings);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<CalendarIntegersEnum, List<ValueInfo<double>>>>.Default.GetHashCode(integers);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(CalendarAlarm a, CalendarAlarm b)
            => a.Equals(b);

        /// <inheritdoc/>
        public static bool operator !=(CalendarAlarm a, CalendarAlarm b)
            => !a.Equals(b);

        internal void ValidateAlarm()
        {
            string[] expectedAlarmFields = [VCalendarConstants._actionSpecifier, VCalendarConstants._triggerSpecifier];
            LoggingTools.Debug("Expected alarm fields: {0} [{1}]", expectedAlarmFields.Length, string.Join(", ", expectedAlarmFields));
            if (!ValidateComponent(ref expectedAlarmFields, out string[] actualAlarmFields, this))
                throw new InvalidDataException("The following keys [{0}] are required in the alarm representation. Got [{1}].".FormatString(string.Join(", ", expectedAlarmFields), string.Join(", ", actualAlarmFields)));

            // Check the alarm action
            string[] expectedAudioAlarmFields = [VCalendarConstants._attachSpecifier];
            string[] expectedDisplayAlarmFields = [VCalendarConstants._descriptionSpecifier];
            string[] expectedMailAlarmFields = [VCalendarConstants._descriptionSpecifier, VCalendarConstants._summarySpecifier, VCalendarConstants._attendeeSpecifier];
            LoggingTools.Debug("Expected a-alarm fields: {0} [{1}]", expectedAudioAlarmFields.Length, string.Join(", ", expectedAudioAlarmFields));
            LoggingTools.Debug("Expected d-alarm fields: {0} [{1}]", expectedDisplayAlarmFields.Length, string.Join(", ", expectedDisplayAlarmFields));
            LoggingTools.Debug("Expected m-alarm fields: {0} [{1}]", expectedMailAlarmFields.Length, string.Join(", ", expectedMailAlarmFields));
            var actionList = GetString(CalendarStringsEnum.Action);
            string type = actionList.Length > 0 ? actionList[0].Value : "";
            LoggingTools.Debug("Alarm type: {0} [{1} actions]", type, actionList.Length);
            switch (type)
            {
                case "AUDIO":
                    if (!ValidateComponent(ref expectedAudioAlarmFields, out string[] actualAudioAlarmFields, this))
                        throw new InvalidDataException("The following keys [{0}] are required in the audio alarm representation. Got [{1}].".FormatString(string.Join(", ", expectedAudioAlarmFields), string.Join(", ", actualAudioAlarmFields)));
                    break;
                case "DISPLAY":
                    if (!ValidateComponent(ref expectedDisplayAlarmFields, out string[] actualDisplayAlarmFields, this))
                        throw new InvalidDataException("The following keys [{0}] are required in the display alarm representation. Got [{1}].".FormatString(string.Join(", ", expectedDisplayAlarmFields), string.Join(", ", actualDisplayAlarmFields)));
                    break;
                case "EMAIL":
                    if (!ValidateComponent(ref expectedMailAlarmFields, out string[] actualMailAlarmFields, this))
                        throw new InvalidDataException("The following keys [{0}] are required in the mail alarm representation. Got [{1}].".FormatString(string.Join(", ", expectedMailAlarmFields), string.Join(", ", actualMailAlarmFields)));
                    break;
            }

            // Check to see if there is a repeat property
            var repeatList = GetInteger(CalendarIntegersEnum.Repeat);
            LoggingTools.Debug("{0} repeats", repeatList.Length);
            int repeat = (int)(repeatList.Length > 0 ? repeatList[0].Value : -1);
            string[] expectedRepeatedAlarmFields = [VCalendarConstants._durationSpecifier];
            LoggingTools.Debug("Expected r-alarm fields: {0} [{1}]", expectedRepeatedAlarmFields.Length, string.Join(", ", expectedRepeatedAlarmFields));
            if (repeat >= 1)
            {
                if (!ValidateComponent(ref expectedRepeatedAlarmFields, out string[] actualRepeatedAlarmFields, this))
                    throw new InvalidDataException("The following keys [{0}] are required in the repeated alarm representation. Got [{1}].".FormatString(string.Join(", ", expectedRepeatedAlarmFields), string.Join(", ", actualRepeatedAlarmFields)));
            }
        }

        /// <summary>
        /// Makes an empty calendar alarm
        /// </summary>
        /// <param name="version">vCalendar version to use</param>
        /// <exception cref="ArgumentException"></exception>
        public CalendarAlarm(Version version) :
            base(version)
        {
            if (version.Major != 2 && version.Minor != 0)
                throw new ArgumentException("Invalid vCalendar version {0} specified. The supported version is 2.0.".FormatString(version));
        }
    }
}
