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

namespace VisualCard.Parsers.Recurrence
{
    /// <summary>
    /// Recurrence rule frequency
    /// </summary>
    public enum RecurrenceRuleFrequency
    {
        /// <summary>
        /// Particular second repetition (v2)
        /// </summary>
        Second,
        /// <summary>
        /// Particular minute repetition (v1 and v2)
        /// </summary>
        Minute,
        /// <summary>
        /// Particular hour repetition (v2)
        /// </summary>
        Hourly,
        /// <summary>
        /// Daily repetition (v1 and v2)
        /// </summary>
        Daily,
        /// <summary>
        /// Weekly repetition (v1 and v2)
        /// </summary>
        Weekly,
        /// <summary>
        /// Monthly repetition (v2)
        /// </summary>
        Monthly,
        /// <summary>
        /// Yearly repetition (v2)
        /// </summary>
        Yearly,
        /// <summary>
        /// Monthly repetition based on a relative day (v1 and v2)
        /// </summary>
        MonthlyPos,
        /// <summary>
        /// Monthly repetition based on an absolute day (v1 and v2)
        /// </summary>
        MonthlyDay,
        /// <summary>
        /// Yearly month repeat (every year in a specific month, for example, September [Month 9]) (v1)
        /// </summary>
        YearlyMonth,
        /// <summary>
        /// Yearly day repeat (every year in a specific day, for example, September 15th [Day 258 in normal years and 259 in leap years]) (v1 and v2)
        /// </summary>
        YearlyDay,
        /// <summary>
        /// Repetition by set position (v2)
        /// </summary>
        Position,
    }
}
