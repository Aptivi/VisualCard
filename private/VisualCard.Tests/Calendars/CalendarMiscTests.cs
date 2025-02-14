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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.IO;
using Textify.General;
using VisualCard.Calendar.Parts;
using VisualCard.Calendar.Parts.Enums;
using VisualCard.Calendar.Parts.Implementations.Event;

namespace VisualCard.Tests.Calendars
{
    [TestClass]
    public class CalendarMiscTests
    {
        [TestMethod]
        public void TestCreateNewCalendar10()
        {
            var calendar = new Calendar.Parts.Calendar(new(1, 0));
            calendar.Strings.Count.ShouldBe(0);
            calendar.Integers.Count.ShouldBe(0);
            calendar.PartsArray.Count.ShouldBe(0);
            string[] savedLines = calendar.SaveToString().SplitNewLines(false);
            savedLines[0].ShouldBe("BEGIN:VCALENDAR");
            savedLines[1].ShouldBe("VERSION:1.0");
            savedLines[2].ShouldBe("END:VCALENDAR");
        }
        
        [TestMethod]
        public void TestCreateNewMinimalEventCalendar10()
        {
            var calendar = new Calendar.Parts.Calendar(new(1, 0));
            calendar.Strings.Count.ShouldBe(0);
            calendar.Integers.Count.ShouldBe(0);
            calendar.PartsArray.Count.ShouldBe(0);
            var calendarEvent = new CalendarEvent(new(1, 0));
            calendar.AddEvent(calendarEvent);
            string[] savedLines = calendar.SaveToString(true).SplitNewLines(false);
            savedLines[0].ShouldBe("BEGIN:VCALENDAR");
            savedLines[1].ShouldBe("VERSION:1.0");
            savedLines[2].ShouldBe("BEGIN:VEVENT");
            savedLines[3].ShouldBe("END:VEVENT");
            savedLines[4].ShouldBe("END:VCALENDAR");
        }
        
        [TestMethod]
        public void TestCreateNewMinimalTodoCalendar10()
        {
            var calendar = new Calendar.Parts.Calendar(new(1, 0));
            calendar.Strings.Count.ShouldBe(0);
            calendar.Integers.Count.ShouldBe(0);
            calendar.PartsArray.Count.ShouldBe(0);
            var calendarTodo = new CalendarTodo(new(1, 0));
            calendar.AddTodo(calendarTodo);
            string[] savedLines = calendar.SaveToString(true).SplitNewLines(false);
            savedLines[0].ShouldBe("BEGIN:VCALENDAR");
            savedLines[1].ShouldBe("VERSION:1.0");
            savedLines[2].ShouldBe("BEGIN:VTODO");
            savedLines[3].ShouldBe("END:VTODO");
            savedLines[4].ShouldBe("END:VCALENDAR");
        }

        [TestMethod]
        public void TestValidateNewCalendar10()
        {
            var calendar = new Calendar.Parts.Calendar(new(1, 0));
            calendar.Strings.Count.ShouldBe(0);
            calendar.Integers.Count.ShouldBe(0);
            calendar.PartsArray.Count.ShouldBe(0);
            Should.NotThrow(calendar.Validate);
        }

        [TestMethod]
        public void TestCreateNewCalendar20()
        {
            var calendar = new Calendar.Parts.Calendar(new(2, 0));
            calendar.Strings.Count.ShouldBe(0);
            calendar.Integers.Count.ShouldBe(0);
            calendar.PartsArray.Count.ShouldBe(0);
            string[] savedLines = calendar.SaveToString().SplitNewLines(false);
            savedLines[0].ShouldBe("BEGIN:VCALENDAR");
            savedLines[1].ShouldBe("VERSION:2.0");
            savedLines[2].ShouldBe("END:VCALENDAR");
        }

        [TestMethod]
        public void TestCreateNewMinimalEventCalendar20()
        {
            var calendar = new Calendar.Parts.Calendar(new(2, 0));
            calendar.Strings.Count.ShouldBe(0);
            calendar.Integers.Count.ShouldBe(0);
            calendar.PartsArray.Count.ShouldBe(0);
            calendar.AddString(CalendarStringsEnum.ProductId, "-//Aptivi//VisualCard//EN");
            calendar.Strings.Count.ShouldBe(1);
            calendar.GetString(CalendarStringsEnum.ProductId)[0].Value.ShouldBe("-//Aptivi//VisualCard//EN");
            var calendarEvent = new CalendarEvent(new(2, 0));
            calendarEvent.AddPartToArray(CalendarPartsArrayEnum.DateStamp, "2025-05-26T12:00:00Z");
            calendarEvent.AddPartToArray(CalendarPartsArrayEnum.DateStart, "2025-05-26T14:00:00Z");
            calendarEvent.PartsArray.Count.ShouldBe(2);
            var dateStamp = calendarEvent.GetPartsArray<DateStampInfo>()[0].DateStamp;
            dateStamp.Year.ShouldBe(2025);
            dateStamp.Month.ShouldBe(5);
            dateStamp.Day.ShouldBe(26);
            dateStamp.Hour.ShouldBe(12);
            var dateStart = calendarEvent.GetPartsArray<DateStartInfo>()[0].DateStart;
            dateStart.Year.ShouldBe(2025);
            dateStart.Month.ShouldBe(5);
            dateStart.Day.ShouldBe(26);
            dateStart.Hour.ShouldBe(14);
            calendarEvent.AddString(CalendarStringsEnum.Uid, "userid@festival.com");
            calendarEvent.Strings.Count.ShouldBe(1);
            calendarEvent.GetString(CalendarStringsEnum.Uid)[0].Value.ShouldBe("userid@festival.com");
            calendar.AddEvent(calendarEvent);
            string[] savedLines = calendar.SaveToString(true).SplitNewLines(false);
            savedLines[0].ShouldBe("BEGIN:VCALENDAR");
            savedLines[1].ShouldBe("VERSION:2.0");
            savedLines[2].ShouldBe("PRODID:-//Aptivi//VisualCard//EN");
            savedLines[3].ShouldBe("BEGIN:VEVENT");
            savedLines[4].ShouldBe("UID:userid@festival.com");
            savedLines[5].ShouldBe("DTSTAMP:20250526T120000Z");
            savedLines[6].ShouldBe("DTSTART:20250526T140000Z");
            savedLines[7].ShouldBe("END:VEVENT");
            savedLines[8].ShouldBe("END:VCALENDAR");
        }

        [TestMethod]
        public void TestCreateNewMinimalTodoCalendar20()
        {
            var calendar = new Calendar.Parts.Calendar(new(2, 0));
            calendar.Strings.Count.ShouldBe(0);
            calendar.Integers.Count.ShouldBe(0);
            calendar.PartsArray.Count.ShouldBe(0);
            calendar.AddString(CalendarStringsEnum.ProductId, "-//Aptivi//VisualCard//EN");
            calendar.Strings.Count.ShouldBe(1);
            calendar.GetString(CalendarStringsEnum.ProductId)[0].Value.ShouldBe("-//Aptivi//VisualCard//EN");
            var calendarTodo = new CalendarTodo(new(2, 0));
            calendarTodo.AddPartToArray(CalendarPartsArrayEnum.DateStamp, "2025-05-26T12:00:00Z");
            calendarTodo.PartsArray.Count.ShouldBe(1);
            var dateStamp = calendarTodo.GetPartsArray<DateStampInfo>()[0].DateStamp;
            dateStamp.Year.ShouldBe(2025);
            dateStamp.Month.ShouldBe(5);
            dateStamp.Day.ShouldBe(26);
            dateStamp.Hour.ShouldBe(12);
            calendarTodo.AddString(CalendarStringsEnum.Uid, "userid@festival.com");
            calendarTodo.Strings.Count.ShouldBe(1);
            calendarTodo.GetString(CalendarStringsEnum.Uid)[0].Value.ShouldBe("userid@festival.com");
            calendar.AddTodo(calendarTodo);
            string[] savedLines = calendar.SaveToString(true).SplitNewLines(false);
            savedLines[0].ShouldBe("BEGIN:VCALENDAR");
            savedLines[1].ShouldBe("VERSION:2.0");
            savedLines[2].ShouldBe("PRODID:-//Aptivi//VisualCard//EN");
            savedLines[3].ShouldBe("BEGIN:VTODO");
            savedLines[4].ShouldBe("UID:userid@festival.com");
            savedLines[5].ShouldBe("DTSTAMP:20250526T120000Z");
            savedLines[6].ShouldBe("END:VTODO");
            savedLines[7].ShouldBe("END:VCALENDAR");
        }

        [TestMethod]
        public void TestValidateNewCalendar20()
        {
            var calendar = new Calendar.Parts.Calendar(new(2, 0));
            calendar.Strings.Count.ShouldBe(0);
            calendar.Integers.Count.ShouldBe(0);
            calendar.PartsArray.Count.ShouldBe(0);
            Should.Throw(calendar.Validate, typeof(InvalidDataException));
        }
    }
}
