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

using System.Collections.Generic;

namespace VisualCard.Tests.Calendars.Data
{
    public static class CalendarDataBogus
    {
        private static readonly string vCalendarTwoNoProdid =
            """
            BEGIN:VCALENDAR
            VERSION:2.0
            CALSCALE:GREGORIAN
            BEGIN:VEVENT
            DTSTAMP:1996-07-04T12:00:00
            UID:uid1@example.com
            DTSTART:1996-09-18T14:30:00
            DTEND:1996-09-20T22:00:00
            END:VEVENT
            END:VCALENDAR
            """
        ;

        private static readonly string vCalendarTwoWithUnsupportedParts =
            """
            BEGIN:VCALENDAR
            PRODID:-//xyz Corp//NONSGML PDA Calendar Version 1.0//EN
            VERSION:2.0
            CALSCALE:GREGORIAN
            BEGIN:VEVENT
            DTSTAMP:1996-07-04T12:00:00
            UID:uid1@example.com
            DTSTART:1996-09-18T14:30:00
            DTEND:1996-09-20T22:00:00
            POPULAR:True
            X-TRENDING:True
            END:VEVENT
            END:VCALENDAR
            """
        ;

        private static readonly string vCalendarTwoWithIndirectVersionSpecify =
            """
            BEGIN:VCALENDAR
            PRODID:-//xyz Corp//NONSGML PDA Calendar Version 1.0//EN
            CALSCALE:GREGORIAN
            BEGIN:VEVENT
            DTSTAMP:1996-07-04T12:00:00
            UID:uid1@example.com
            DTSTART:1996-09-18T14:30:00
            DTEND:1996-09-20T22:00:00
            POPULAR:True
            X-TRENDING:True
            END:VEVENT
            VERSION:2.0
            END:VCALENDAR
            """
        ;

        private static readonly string vCalendarZeroByte =
            """

            """
        ;

        private static readonly string vCalendarNonexistentVersion =
            """
            BEGIN:VCALENDAR
            VERSION:0.0
            CALSCALE:GREGORIAN
            BEGIN:VEVENT
            DTSTAMP:1996-07-04T12:00:00
            UID:uid1@example.com
            DTSTART:1996-09-18T14:30:00
            DTEND:1996-09-20T22:00:00
            END:VEVENT
            END:VCALENDAR
            """
        ;

        private static readonly string vCalendarTwoBarren =
            """
            BEGIN:VCALENDAR
            VERSION:2.0
            END:VCALENDAR
            """
        ;

        private static readonly string vCalendarOneBarren =
            """
            BEGIN:VCALENDAR
            VERSION:1.0
            END:VCALENDAR
            """
        ;

        private static readonly string vCalendarDtEndConflict =
            """
            BEGIN:VCALENDAR
            PRODID:-//xyz Corp//NONSGML PDA Calendar Version 1.0//EN
            VERSION:2.0
            CALSCALE:GREGORIAN
            BEGIN:VEVENT
            DTSTAMP:1996-07-04T12:00:00
            UID:uid1@example.com
            DTSTART:1996-09-18T14:30:00
            DTEND:1996-09-20T22:00:00
            DURATION:PT15M
            END:VEVENT
            END:VCALENDAR
            """
        ;

        private static readonly string vCalendarEndMismatch =
            """
            BEGIN:VCALENDAR
            PRODID:-//xyz Corp//NONSGML PDA Calendar Version 1.0//EN
            VERSION:2.0
            CALSCALE:GREGORIAN
            BEGIN:VEVENT
            DTSTAMP:1996-07-04T12:00:00
            UID:uid1@example.com
            DTSTART:1996-09-18T14:30:00
            DTEND:1996-09-20T22:00:00
            END:VTODO
            END:VCALENDAR
            """
        ;

        /// <summary>
        /// All of the calendars in this field should fail immediately upon processing the test calendars in the
        /// <see cref="VCalendarParser.Parse()"/> function. This throws VCalendarParseException.
        /// </summary>
        public static IEnumerable<object[]> invalidCalendars =>
        [
            [
                vCalendarEndMismatch,
            ],
        ];

        /// <summary>
        /// All of the calendars in this field with invalid syntax or omitted requirements may be accepted by the
        /// <see cref="VCalendarParser.Parse()"/> function.
        /// </summary>
        public static IEnumerable<object[]> seemsValidCalendars =>
        [
            [
                vCalendarTwoWithUnsupportedParts,
            ],
            [
                vCalendarTwoWithIndirectVersionSpecify,
            ],
        ];

        /// <summary>
        /// All of the calendars in this field should fail immediately upon calling <see cref="VCalendarParser.Parse()"/>.
        /// These usually resemble calendars with invalid syntax. This throws InvalidDataException.
        /// </summary>
        public static IEnumerable<object[]> invalidCalendarsParser =>
        [
            [
                vCalendarTwoNoProdid,
            ],
            [
                vCalendarTwoBarren,
            ],
            [
                vCalendarOneBarren,
            ],
            [
                vCalendarNonexistentVersion,
            ],
            [
                vCalendarZeroByte,
            ],
            [
                vCalendarDtEndConflict,
            ],
        ];
    }
}
