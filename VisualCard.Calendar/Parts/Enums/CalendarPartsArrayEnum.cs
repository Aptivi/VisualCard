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
    /// Enumeration for available parts that are not strings but are arrays
    /// </summary>
    public enum CalendarPartsArrayEnum
    {
        /// <summary>
        /// Calendar attachments (event, todo, journal, or alarm)
        /// </summary>
        Attach,
        /// <summary>
        /// Calendar categories (event, todo, or journal)
        /// </summary>
        Categories,
        /// <summary>
        /// Calendar comments (event, to-do, journal, or free/busy + daylight/standard)
        /// </summary>
        Comment,
        /// <summary>
        /// Calendar geographic location (event or to-do)
        /// </summary>
        Geography,
        /// <summary>
        /// Calendar location (event or to-do)
        /// </summary>
        Location,
        /// <summary>
        /// Calendar resources (event or to-do)
        /// </summary>
        Resources,
        /// <summary>
        /// Calendar attendees (event, todo, journal, free/busy, or alarm)
        /// </summary>
        Attendee,
        /// <summary>
        /// Calendar date of creation (event, todo, or journal)
        /// </summary>
        DateCreated,
        /// <summary>
        /// Calendar date of creation (vCalendar 1.0) (event or todo)
        /// </summary>
        DateCreatedAlt,
        /// <summary>
        /// Event date start
        /// </summary>
        DateStart,
        /// <summary>
        /// Event date end
        /// </summary>
        DateEnd,
        /// <summary>
        /// Date stamp (event, todo, journal, or free/busy)
        /// </summary>
        DateStamp,
        /// <summary>
        /// X-nonstandard names (event, todo, journal, free/busy, or alarm)
        /// </summary>
        NonstandardNames,
    }
}
