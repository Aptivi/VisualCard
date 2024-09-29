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

namespace VisualCard.Calendar.Parsers.Recurrence
{
    /// <summary>
    /// Recurrence rule instance
    /// </summary>
    public class RecurrenceRule
    {
        // Frequency and interval
        internal RecurrenceRuleFrequency frequency;
        internal int interval;

        // General
        internal int duration = 2;
        internal DateTimeOffset endDate;

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
    }
}
