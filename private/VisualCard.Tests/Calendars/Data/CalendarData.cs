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
    public static class CalendarData
    {
        internal static readonly string singleVCalendarTwoCalendarShort =
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
            END:VEVENT
            END:VCALENDAR
            """
        ;

        internal static readonly string singleVCalendarOneCalendarShort =
            """
            BEGIN:VCALENDAR
            VERSION:1.0
            BEGIN:VEVENT
            UID:uid1@example.com
            DTSTART:1996-09-18T14:30:00
            DTEND:1996-09-20T22:00:00
            END:VEVENT
            END:VCALENDAR
            """
        ;

        internal static readonly string singleVCalendarTwoCalendar =
            """
            BEGIN:VCALENDAR
            PRODID:-//xyz Corp//NONSGML PDA Calendar Version 1.0//EN
            VERSION:2.0
            CALSCALE:GREGORIAN
            BEGIN:VEVENT
            DTSTAMP:1996-07-04T12:00:00
            UID:uid1@example.com
            ORGANIZER:mailto:jsmith@example.com
            DTSTART:1996-09-18T14:30:00
            DTEND:1996-09-20T22:00:00
            STATUS:CONFIRMED
            CATEGORIES:CONFERENCE
            SUMMARY:Networld+Interop Conference
            DESCRIPTION:Networld+Interop Conference
              and Exhibit\nAtlanta World Congress Center\n
             Atlanta\, Georgia
            POPULAR:True
            X-TRENDING:True
            GEO:37.24;-17.87
            REQUEST-STATUS:4.1;Event conflict.  Date-time is busy.
            REQUEST-STATUS:3.7;Invalid calendar user;ORGANIZER:
             mailto:jsmith@example.com
            X-TESTFIELD;ENCODING=QUOTED-PRINTABLE:;;=48=61=64=6A=71=69=68=70=70=74=61=
            =6E=20=3B=41;=48=48=48=48=48=48=20=20=42;;=31=31=31=31=31=31;=53=53=53=53=
            =53=53;=48=48=48=48=48=48=48=48=48=48=48=48=20=48=48=0A=4D=4D=4D=4D=4D=4D=
            =4D=4D=42=20=31=31=31=31=31=20=31=39=0A=53=53=53=53=53=53=53=53=53
            END:VEVENT
            END:VCALENDAR
            """
        ;

        internal static readonly string singleVCalendarOneCalendar =
            """
            BEGIN:VCALENDAR
            VERSION:1.0
            BEGIN:VEVENT
            UID:uid1@example.com
            DTSTART:1996-09-18T14:30:00
            DTEND:1996-09-20T22:00:00
            STATUS:CONFIRMED
            CATEGORIES:CONFERENCE
            SUMMARY:Networld+Interop Conference
            DESCRIPTION:Networld+Interop Conference
              and Exhibit\nAtlanta World Congress Center\n
             Atlanta\, Georgia
            GEO:37.24,-17.87
            X-TESTFIELD;ENCODING=QUOTED-PRINTABLE:;;=48=61=64=6A=71=69=68=70=70=74=61=
            =6E=20=3B=41;=48=48=48=48=48=48=20=20=42;;=31=31=31=31=31=31;=53=53=53=53=
            =53=53=48=48=48=48=48=48=48=48=48=48=48=48=20=48=48=0A=4D=4D=4D=4D=4D=4D=
            =4D=4D=42=20=31=31=31=31=31=20=31=39=0A=53=53=53=53=53=53=53=53=53
            END:VEVENT
            END:VCALENDAR
            """
        ;

        internal static readonly string multipleVCalendarTwoCalendars =
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
            END:VEVENT
            END:VCALENDAR

            BEGIN:VCALENDAR
            METHOD:xyz
            VERSION:2.0
            PRODID:-//ABC Corporation//NONSGML My Product//EN
            BEGIN:VEVENT
            DTSTAMP:1997-03-24T12:00:00
            SEQUENCE:0
            UID:uid3@example.com
            ORGANIZER:mailto:jdoe@example.com
            ATTENDEE;RSVP=TRUE:mailto:jsmith@example.com
            DTSTART:1997-03-24T12:30:00
            DTEND:1997-03-24T21:00:00
            CATEGORIES:MEETING,PROJECT
            CLASS:PUBLIC
            SUMMARY:Calendaring Interoperability Planning Meeting
            DESCRIPTION:Discuss how we can test c&s interoperability\n
             using iCalendar and other IETF standards.
            LOCATION:LDB Lobby
            ATTACH;FMTTYPE=application/postscript:ftp://example.com/pub/
             conf/bkgrnd.ps
            END:VEVENT
            END:VCALENDAR
            """
        ;

        internal static readonly string multipleVCalendarOneCalendars =
            """
            BEGIN:VCALENDAR
            VERSION:1.0
            BEGIN:VEVENT
            UID:uid1@example.com
            DTSTART:1996-09-18T14:30:00
            DTEND:1996-09-20T22:00:00
            END:VEVENT
            END:VCALENDAR

            BEGIN:VCALENDAR
            VERSION:1.0
            DAYLIGHT:TRUE;-09;19960407T115959;19961027T100000;PST;PDT
            BEGIN:VEVENT
            UID:<jsmith.part7.19960817T083000.xyzMail@host3.com>
            CATEGORIES:MEETING
            STATUS:NEEDS ACTION
            DTSTART:19960401T073000Z
            DTEND:19960401T083000Z
            SUMMARY:Steve's Proposal Review
            DESCRIPTION:Steve and John to review newest proposal material
            CLASS:PRIVATE
            DALARM:19960415T235000;PT5M;2;Your Taxes Are Due !!!
            AALARM;TYPE=WAVE;VALUE=URL:19960415T235959; ; ; file:///mmedia/taps.wav
            PALARM;VALUE=URL:19960415T235000;PT5M;2;file:///myapps/shockme.exe
            MALARM:19960415T235000;PT5M;2;Address;Note
            END:VEVENT
            BEGIN:VTODO
            RELATED-TO:<jsmith.part7.19960817T083000.xyzMail@host3.com>
            SUMMARY:John to pay for lunch
            DUE:19960401T083000Z
            STATUS:NEEDS ACTION
            END:VTODO
            END:VCALENDAR
            """
        ;

        /// <summary>
        /// Test VCalendar single calendar contents (shorts)
        /// </summary>
        public static IEnumerable<object[]> singleVCalendarCalendarShorts =>
        [
            [
                singleVCalendarTwoCalendarShort,
            ],
            [
                singleVCalendarOneCalendarShort,
            ],
        ];

        /// <summary>
        /// Test VCalendar single calendar contents
        /// </summary>
        public static IEnumerable<object[]> singleVCalendarCalendars =>
        [
            [
                singleVCalendarTwoCalendar,
            ],
            [
                singleVCalendarOneCalendar,
            ],
        ];

        /// <summary>
        /// Test VCalendar multiple calendar contents
        /// </summary>
        public static IEnumerable<object[]> multipleVCalendarCalendars =>
        [
            [
                multipleVCalendarTwoCalendars,
            ],
            [
                multipleVCalendarOneCalendars,
            ],
        ];
    }
}
