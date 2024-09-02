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
    /// Enumeration for available parts that are strings
    /// </summary>
    public enum CalendarStringsEnum
    {
        /// <summary>
        /// The calendar's product ID (top-level)
        /// </summary>
        ProductId,
        /// <summary>
        /// The calendar's scale (top-level)
        /// </summary>
        CalScale,
        /// <summary>
        /// The calendar's method (top-level)
        /// </summary>
        Method,
        /// <summary>
        /// The calendar's classification (event, to-do, or journal)
        /// </summary>
        Class,
        /// <summary>
        /// The calendar's description (event, to-do, journal (1+), or alarm)
        /// </summary>
        Description,
        /// <summary>
        /// The calendar's unique ID
        /// </summary>
        Uid,
        /// <summary>
        /// The calendar's organizer
        /// </summary>
        Organizer,
        /// <summary>
        /// The calendar's status
        /// </summary>
        Status,
        /// <summary>
        /// The calendar's summary
        /// </summary>
        Summary,
    }
}
