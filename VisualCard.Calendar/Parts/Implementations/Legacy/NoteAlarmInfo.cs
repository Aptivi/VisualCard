﻿//
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
using VisualCard.Parsers;

namespace VisualCard.Calendar.Parts.Implementations.Legacy
{
    /// <summary>
    /// Event note alarm information
    /// </summary>
    [DebuggerDisplay("NoteAlarm = {Flag}")]
    public class NoteAlarmInfo : BaseCalendarPartInfo, IEquatable<NoteAlarmInfo>
    {
        /// <summary>
        /// Alarm run time
        /// </summary>
        public DateTime RunTime { get; }

        /// <summary>
        /// Alarm snooze time
        /// </summary>
        public string? SnoozeTime { get; }

        /// <summary>
        /// Alarm repeat count
        /// </summary>
        public int RepeatCount { get; }

        /// <summary>
        /// Address string
        /// </summary>
        public string? Address { get; }

        /// <summary>
        /// Note alarm string
        /// </summary>
        public string? Note { get; }

        internal static BaseCalendarPartInfo FromStringVcalendarStatic(string value, string[] finalArgs, string[] elementTypes, string valueType, Version calendarVersion) =>
            new NoteAlarmInfo().FromStringVcalendarInternal(value, finalArgs, elementTypes, valueType, calendarVersion);

        internal override string ToStringVcalendarInternal(Version calendarVersion)
        {
            string posixRunTime = VcardParserTools.SavePosixDate(RunTime);
            return $"{posixRunTime};{SnoozeTime};{RepeatCount};{Note}";
        }

        internal override BaseCalendarPartInfo FromStringVcalendarInternal(string value, string[] finalArgs, string[] elementTypes, string valueType, Version calendarVersion)
        {
            // Get the values
            string[] split = value.Split(';');
            if (split.Length != 5)
                throw new ArgumentException($"When splitting note alarm information, the split value is {split.Length} instead of 5.");
            string unprocessedRunTime = split[0];
            string snoozeTime = split[1];
            string unprocessedRepeat = split[2];
            string address = split[3];
            string note = split[4];

            // Process the run time and the repeat times
            DateTime runTime = VcardParserTools.ParsePosixDate(unprocessedRunTime);
            int repeat = 0;
            if (!string.IsNullOrWhiteSpace(unprocessedRepeat) && !int.TryParse(unprocessedRepeat, out repeat))
                throw new ArgumentException("Invalid repeat times");

            // Populate the fields
            NoteAlarmInfo info = new(finalArgs, elementTypes, valueType, runTime, snoozeTime, repeat, address, note);
            return info;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((NoteAlarmInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="NoteAlarmInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(NoteAlarmInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="NoteAlarmInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="NoteAlarmInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(NoteAlarmInfo source, NoteAlarmInfo target)
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
        public static bool operator ==(NoteAlarmInfo left, NoteAlarmInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(NoteAlarmInfo left, NoteAlarmInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCalendarPartInfo source, BaseCalendarPartInfo target) =>
            (NoteAlarmInfo)source == (NoteAlarmInfo)target;

        internal NoteAlarmInfo() { }

        internal NoteAlarmInfo(string[] arguments, string[] elementTypes, string valueType, DateTime runTime, string snoozeTime, int repeat, string address, string note) :
            base(arguments, elementTypes, valueType)
        {
            RunTime = runTime;
            SnoozeTime = snoozeTime;
            RepeatCount = repeat;
            Address = address;
            Note = note;
        }
    }
}