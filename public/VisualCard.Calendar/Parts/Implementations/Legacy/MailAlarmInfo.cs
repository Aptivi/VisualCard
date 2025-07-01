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
using VisualCard.Common.Parsers;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts;

namespace VisualCard.Calendar.Parts.Implementations.Legacy
{
    /// <summary>
    /// Event note alarm information
    /// </summary>
    [DebuggerDisplay("Mail Alarm = @ {RunTime}, {SnoozeTime}, {RepeatCount} times, {Address}, {Note}")]
    public class MailAlarmInfo : BaseCalendarPartInfo, IEquatable<MailAlarmInfo>
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
        /// Mail address string
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Note alarm string
        /// </summary>
        public string? Note { get; set; }

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
            (BaseCalendarPartInfo)new MailAlarmInfo().FromStringInternal(value, property, altId, elementTypes, calendarVersion);

        internal override string ToStringInternal(Version calendarVersion)
        {
            string posixRunTime = CommonTools.SavePosixDate(RunTime);
            return $"{posixRunTime};{SnoozeTime};{RepeatCount};{Address};{Note}";
        }

        internal override BasePartInfo FromStringInternal(string value, PropertyInfo property, int altId, string[] elementTypes, Version calendarVersion)
        {
            // Get the values
            string[] split = value.Split(';');
            if (split.Length != 5)
                throw new ArgumentException("When splitting mail alarm information, the split value is {0} instead of 5.".FormatString(split.Length));
            string unprocessedRunTime = split[0];
            string snoozeTime = split[1];
            string unprocessedRepeat = split[2];
            string address = split[3];
            string note = split[4];

            // Process the run time and the repeat times
            DateTimeOffset runTime = CommonTools.ParsePosixDateTime(unprocessedRunTime);
            int repeat = 0;
            if (!string.IsNullOrWhiteSpace(unprocessedRepeat) && !int.TryParse(unprocessedRepeat, out repeat))
                throw new ArgumentException("Invalid repeat times");

            // Populate the fields
            MailAlarmInfo info = new(property, elementTypes, runTime, snoozeTime, repeat, address, note);
            return info;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((MailAlarmInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="MailAlarmInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(MailAlarmInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="MailAlarmInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="MailAlarmInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(MailAlarmInfo source, MailAlarmInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.RunTime == target.RunTime &&
                source.SnoozeTime == target.SnoozeTime &&
                source.RepeatCount == target.RepeatCount &&
                source.Address == target.Address &&
                source.Note == target.Note
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 554810744;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + RunTime.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(SnoozeTime);
            hashCode = hashCode * -1521134295 + RepeatCount.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(Address);
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(Note);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(MailAlarmInfo left, MailAlarmInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(MailAlarmInfo left, MailAlarmInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BasePartInfo source, BasePartInfo target) =>
            (MailAlarmInfo)source == (MailAlarmInfo)target;

        internal MailAlarmInfo() { }

        internal MailAlarmInfo(PropertyInfo? property, string[] elementTypes, DateTimeOffset runTime, string snoozeTime, int repeat, string address, string note) :
            base(property, elementTypes)
        {
            RunTime = runTime;
            SnoozeTime = snoozeTime;
            RepeatCount = repeat;
            Address = address;
            Note = note;
        }
    }
}
