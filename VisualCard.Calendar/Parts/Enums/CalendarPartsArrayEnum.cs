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
        /// Date start (event, to-do, free/busy, or standard/daylight timezone components)
        /// </summary>
        DateStart,
        /// <summary>
        /// Date end (event or free/busy)
        /// </summary>
        DateEnd,
        /// <summary>
        /// Date stamp (event, todo, journal, or free/busy)
        /// </summary>
        DateStamp,
        /// <summary>
        /// Time zone name
        /// </summary>
        TimeZoneName,
        /// <summary>
        /// Recurrence date and time (event, todo, journal, or standard/daylight timezone components)
        /// </summary>
        RecDate,
        /// <summary>
        /// Excluded date and time (event, todo, journal, or standard/daylight timezone components)
        /// </summary>
        ExDate,
        /// <summary>
        /// To-do date completion
        /// </summary>
        DateCompleted,
        /// <summary>
        /// To-do due date
        /// </summary>
        DueDate,
        /// <summary>
        /// Time zone offset from
        /// </summary>
        TimeZoneOffsetFrom,
        /// <summary>
        /// Time zone offset to
        /// </summary>
        TimeZoneOffsetTo,
        /// <summary>
        /// Daylight saving time info (vCalendar 1.0)
        /// </summary>
        Daylight,
        /// <summary>
        /// Event/to-do alarm (audio) (vCalendar 1.0)
        /// </summary>
        AudioAlarm,
        /// <summary>
        /// Event/to-do alarm (display) (vCalendar 1.0)
        /// </summary>
        DisplayAlarm,
        /// <summary>
        /// Event/to-do alarm (mail) (vCalendar 1.0)
        /// </summary>
        MailAlarm,
        /// <summary>
        /// Event/to-do alarm (procedure) (vCalendar 1.0)
        /// </summary>
        ProcedureAlarm,
        /// <summary>
        /// The calendar's relationship (event, todo, or journal)
        /// </summary>
        RelatedTo,
        /// <summary>
        /// The calendar's relationship (event, todo, journal, or time zone)
        /// </summary>
        LastModified,
        /// <summary>
        /// Free/busy periods
        /// </summary>
        FreeBusy,
        /// <summary>
        /// Duration (event, todo, or alarm)
        /// </summary>
        Duration,
        /// <summary>
        /// Request status (event, todo, journal, or free/busy)
        /// </summary>
        RequestStatus,
        /// <summary>
        /// Contact information (event, todo, journal, or free/busy)
        /// </summary>
        Contact,
        /// <summary>
        /// The calendar's extended IANA options (usually starts with SOMETHING:Value1;Value2...) (event, todo, journal, free/busy, or alarm)
        /// </summary>
        IanaNames = int.MaxValue - 2,
        /// <summary>
        /// The calendar's extended options (usually starts with X-SOMETHING:Value1;Value2...) (event, todo, journal, free/busy, or alarm)
        /// </summary>
        NonstandardNames = int.MaxValue - 1,
    }
}
