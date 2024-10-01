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

namespace VisualCard.Parsers.Recurrence
{
    /// <summary>
    /// Recurrence rule instance
    /// </summary>
    [DebuggerDisplay("RecurRule (v{Version}, {Frequency}) | I: {Interval}, D: {Duration}")]
    public class RecurrenceRule
    {
        // Frequency and interval
        internal RecurrenceRuleFrequency frequency;
        internal int interval = 1;

        // General
        internal Version ruleVersion = new(1, 0);
        internal int duration = 2;
        internal DateTimeOffset endDate;

        /// <summary>
        /// Rule version
        /// </summary>
        public Version Version =>
            ruleVersion;

        /// <summary>
        /// Indicates the frequency of the rule
        /// </summary>
        public RecurrenceRuleFrequency Frequency =>
            frequency;

        /// <summary>
        /// Repeat frequency
        /// </summary>
        public int Interval =>
            interval;

        /// <summary>
        /// Number of occurrences that this rule generates
        /// </summary>
        public int Duration =>
            duration;

        /// <summary>
        /// Specifies whether this date is the last time to repeat
        /// </summary>
        public DateTimeOffset EndDate =>
            endDate;

        #region Version 1.0 rules
        // Time period (daily)
        internal List<(bool isEnd, TimeSpan time)> timePeriods = [];

        // Day times (weekly)
        internal List<(bool isEnd, DayOfWeek time)> dayTimes = [];

        // Monthly (relative and absolute)
        internal List<(bool isEnd, (int occurrence, bool negative))> monthlyOccurrences = [];
        internal List<(bool isEnd, (int dayNum, bool negative, bool isLastDay))> monthlyDayNumbers = [];

        // Yearly (in a month and in a day)
        internal List<(bool isEnd, int monthNum)> yearlyMonthNumbers = [];
        internal List<(bool isEnd, int dayNum)> yearlyDayNumbers = [];

        /// <summary>
        /// Time periods for daily frequency (<see cref="RecurrenceRuleFrequency.Daily"/>). isEnd indicates the end
        /// marker.
        /// </summary>
        /// <remarks>
        /// For v2.0 recurrence rules, seconds list (<see cref="V2Seconds"/>), minutes list, and/or hours list is equivalent to this property.
        /// </remarks>
        public (bool isEnd, TimeSpan time)[] V1TimePeriods =>
            [.. timePeriods];

        /// <summary>
        /// Days of week for weekly frequency (<see cref="RecurrenceRuleFrequency.Weekly"/>). isEnd indicates the end
        /// marker.
        /// </summary>
        /// <remarks>
        /// For v2.0 recurrence rules, days list is equivalent to this property.
        /// </remarks>
        public (bool isEnd, DayOfWeek time)[] V1DayTimes =>
            [.. dayTimes];

        /// <summary>
        /// Monthly occurrences (<see cref="RecurrenceRuleFrequency.MonthlyPos"/>). isEnd indicates the end marker.
        /// </summary>
        /// <remarks>
        /// For v2.0 recurrence rules, positions list is equivalent to this property.
        /// </remarks>
        public (bool isEnd, (int occurrence, bool negative))[] V1MonthlyOccurrences =>
            [.. monthlyOccurrences];

        /// <summary>
        /// Monthly by day numbers (<see cref="RecurrenceRuleFrequency.MonthlyDay"/>). isEnd indicates the end marker.
        /// </summary>
        /// <remarks>
        /// For v2.0 recurrence rules, days of month list is equivalent to this property.
        /// </remarks>
        public (bool isEnd, (int dayNum, bool negative, bool isLastDay))[] V1MonthlyDayNumbers =>
            [.. monthlyDayNumbers];

        /// <summary>
        /// Yearly by month numbers (<see cref="RecurrenceRuleFrequency.YearlyMonth"/>). isEnd indicates the end marker.
        /// </summary>
        /// <remarks>
        /// For v2.0 recurrence rules, months list is equivalent to this property.
        /// </remarks>
        public (bool isEnd, int monthNum)[] V1YearlyMonthNumbers =>
            [.. yearlyMonthNumbers];

        /// <summary>
        /// Yearly by day numbers (<see cref="RecurrenceRuleFrequency.YearlyDay"/>). isEnd indicates the end marker.
        /// </summary>
        /// <remarks>
        /// For v2.0 recurrence rules, days of year list is equivalent to this property.
        /// </remarks>
        public (bool isEnd, int dayNum)[] V1YearlyDayNumbers =>
            [.. yearlyDayNumbers];
        #endregion

        #region Version 2.0 rules
        internal List<int> secondsList = [];
        internal List<int> minutesList = [];
        internal List<int> hoursList = [];
        internal List<(bool negative, int weekNum, DayOfWeek time)> daysList = [];
        internal List<(bool negative, int dayOfMonth)> daysOfMonthList = [];
        internal List<(bool negative, int dayOfYear)> daysOfYearList = [];
        internal List<(bool negative, int weekNum)> weeksList = [];
        internal List<int> monthsList = [];
        internal List<(bool negative, int position)> positionsList = [];
        internal DayOfWeek weekStart = DayOfWeek.Sunday;

        /// <summary>
        /// A day that defines the start of the week.
        /// </summary>
        public DayOfWeek StartWeekday =>
            weekStart;

        /// <summary>
        /// List of seconds (<see cref="RecurrenceRuleFrequency.Second"/>).
        /// </summary>
        /// <remarks>
        /// For v1.0 recurrence rules, time period list (<see cref="V1TimePeriods"/>) is equivalent to this property.
        /// </remarks>
        public int[] V2Seconds =>
            [.. secondsList];

        /// <summary>
        /// List of minutes (<see cref="RecurrenceRuleFrequency.Minute"/>).
        /// </summary>
        /// <remarks>
        /// For v1.0 recurrence rules, time period list (<see cref="V1TimePeriods"/>) is equivalent to this property.
        /// </remarks>
        public int[] V2Minutes =>
            [.. minutesList];

        /// <summary>
        /// List of hours (<see cref="RecurrenceRuleFrequency.Hourly"/>).
        /// </summary>
        /// <remarks>
        /// For v1.0 recurrence rules, time period list (<see cref="V1TimePeriods"/>) is equivalent to this property.
        /// </remarks>
        public int[] V2Hours =>
            [.. hoursList];

        /// <summary>
        /// List of days (<see cref="RecurrenceRuleFrequency.Daily"/>).
        /// </summary>
        /// <remarks>
        /// For v1.0 recurrence rules, day time list (<see cref="V1DayTimes"/>) is equivalent to this property.
        /// </remarks>
        public (bool negative, int weekNum, DayOfWeek time)[] V2Days =>
            [.. daysList];

        /// <summary>
        /// List of days of month (<see cref="RecurrenceRuleFrequency.Monthly"/>).
        /// </summary>
        /// <remarks>
        /// For v1.0 recurrence rules, monthly days list (<see cref="V1MonthlyDayNumbers"/>) is equivalent to this property.
        /// </remarks>
        public (bool negative, int dayOfMonth)[] V2MonthlyDays =>
            [.. daysOfMonthList];

        /// <summary>
        /// List of days of year (<see cref="RecurrenceRuleFrequency.Yearly"/>).
        /// </summary>
        /// <remarks>
        /// For v1.0 recurrence rules, yearly days list (<see cref="V1YearlyDayNumbers"/>) is equivalent to this property.
        /// </remarks>
        public (bool negative, int dayOfYear)[] V2YearlyDays =>
            [.. daysOfYearList];

        /// <summary>
        /// List of week numbers.
        /// </summary>
        public (bool negative, int weekNum)[] V2WeekNumbers =>
            [.. weeksList];

        /// <summary>
        /// List of month numbers (<see cref="RecurrenceRuleFrequency.Yearly"/>).
        /// </summary>
        /// <remarks>
        /// For v1.0 recurrence rules, yearly months list (<see cref="V1YearlyMonthNumbers"/>) is equivalent to this property.
        /// </remarks>
        public int[] V2MonthNumbers =>
            [.. monthsList];

        /// <summary>
        /// List of positions.
        /// </summary>
        /// <remarks>
        /// For v1.0 recurrence rules, monthly occurrence list (<see cref="V1MonthlyOccurrences"/>) is almost equivalent to this property.
        /// </remarks>
        public (bool negative, int position)[] V2Positions =>
            [.. positionsList];
        #endregion
    }
}
