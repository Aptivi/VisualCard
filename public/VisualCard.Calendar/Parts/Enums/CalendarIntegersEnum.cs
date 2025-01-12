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

namespace VisualCard.Calendar.Parts.Enums
{
    /// <summary>
    /// Enumeration for available parts that are integers
    /// </summary>
    public enum CalendarIntegersEnum
    {
        /// <summary>
        /// The calendar's priority (event or to-do)
        /// </summary>
        Priority,
        /// <summary>
        /// The calendar's sequence (event, to-do, or journal)
        /// </summary>
        Sequence,
        /// <summary>
        /// To-do percent completion
        /// </summary>
        PercentComplete,
        /// <summary>
        /// Alarm repetition rate
        /// </summary>
        Repeat,
        /// <summary>
        /// Recurrence times
        /// </summary>
        RecurrTimes,
    }
}
