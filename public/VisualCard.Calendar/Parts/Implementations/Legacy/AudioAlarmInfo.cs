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
using Textify.General;
using VisualCard.Calendar.Languages;
using VisualCard.Common.Parsers;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts;

namespace VisualCard.Calendar.Parts.Implementations.Legacy
{
    /// <summary>
    /// Event audio alarm information
    /// </summary>
    [DebuggerDisplay("Audio Alarm = @ {RunTime}, {SnoozeTime}, {RepeatCount} times, {AudioResource}")]
    public class AudioAlarmInfo : BaseCalendarPartInfo, IEquatable<AudioAlarmInfo>
    {
        /// <summary>
        /// Alarm run time
        /// </summary>
        public DateTimeOffset RunTime { get; set; }

        /// <summary>
        /// Alarm snooze time
        /// </summary>
        public string? SnoozeTime { get; set; }

        /// <summary>
        /// Alarm repeat count
        /// </summary>
        public int RepeatCount { get; set; }

        /// <summary>
        /// Audio alarm resource
        /// </summary>
        public string? AudioResource { get; set; }

        /// <summary>
        /// Snooze duration. Throws exception if there is no snooze time, so check accordingly.
        /// </summary>
        public TimeSpan SnoozeDuration =>
            CommonTools.GetDurationSpan(SnoozeTime ?? "").span;

        /// <summary>
        /// Snooze date/time. Throws exception if there is no snooze time, so check accordingly.
        /// </summary>
        public DateTimeOffset SnoozeIn =>
            CommonTools.GetDurationSpan(SnoozeTime ?? "").result;

        internal static BaseCalendarPartInfo FromStringStatic(string value, PropertyInfo property, int altId, string[] elementTypes, Version calendarVersion) =>
            (BaseCalendarPartInfo)new AudioAlarmInfo().FromStringInternal(value, property, altId, elementTypes, calendarVersion);

        internal override string ToStringInternal(Version calendarVersion)
        {
            string posixRunTime = CommonTools.SavePosixDate(RunTime);
            return $"{posixRunTime};{SnoozeTime};{RepeatCount};{AudioResource}";
        }

        internal override BasePartInfo FromStringInternal(string value, PropertyInfo property, int altId, string[] elementTypes, Version calendarVersion)
        {
            // Get the values
            string[] split = value.Split(';');
            if (split.Length != 4)
                throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_LEGACY_AUDIOALARMINFO_ARGMISMATCH").FormatString(split.Length));
            string unprocessedRunTime = split[0];
            string snoozeTime = split[1];
            string unprocessedRepeat = split[2];
            string audioResource = split[3];

            // Process the run time and the repeat times
            DateTimeOffset runTime = CommonTools.ParsePosixDateTime(unprocessedRunTime);
            int repeat = 0;
            if (!string.IsNullOrWhiteSpace(unprocessedRepeat) && !int.TryParse(unprocessedRepeat, out repeat))
                throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_LEGACY_AUDIOALARMINFO_REPEATTIMES"));

            // Populate the fields
            AudioAlarmInfo info = new(property, elementTypes, runTime, snoozeTime, repeat, audioResource);
            return info;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((AudioAlarmInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="AudioAlarmInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(AudioAlarmInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="AudioAlarmInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="AudioAlarmInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(AudioAlarmInfo source, AudioAlarmInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.RunTime == target.RunTime &&
                source.SnoozeTime == target.SnoozeTime &&
                source.RepeatCount == target.RepeatCount &&
                source.AudioResource == target.AudioResource
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 191071641;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + RunTime.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(SnoozeTime);
            hashCode = hashCode * -1521134295 + RepeatCount.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(AudioResource);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(AudioAlarmInfo left, AudioAlarmInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(AudioAlarmInfo left, AudioAlarmInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BasePartInfo source, BasePartInfo target) =>
            (AudioAlarmInfo)source == (AudioAlarmInfo)target;

        internal AudioAlarmInfo() { }

        internal AudioAlarmInfo(PropertyInfo? property, string[] elementTypes, DateTimeOffset runTime, string snoozeTime, int repeat, string audioResource) :
            base(property, elementTypes)
        {
            RunTime = runTime;
            SnoozeTime = snoozeTime;
            RepeatCount = repeat;
            AudioResource = audioResource;
        }
    }
}
