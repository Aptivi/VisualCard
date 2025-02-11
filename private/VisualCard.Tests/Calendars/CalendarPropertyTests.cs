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
using VisualCard.Calendar;
using VisualCard.Calendar.Parts.Enums;
using VisualCard.Calendar.Parts.Implementations;
using VisualCard.Common.Parts.Enums;
using VisualCard.Common.Parts.Implementations;
using VisualCard.Tests.Calendars.Data;

namespace VisualCard.Tests.Calendars
{
    [TestClass]
    public class CalendarPropertyTests
    {
        [TestMethod]
        public void TestCalendarPropertyAddString()
        {
            var calendars = CalendarTools.GetCalendarsFromString(CalendarData.singleVCalendarTwoCalendarShort);
            var calendar = calendars[0];
            var eventChunk = calendar.Events[0];
            eventChunk.AddString(CalendarStringsEnum.Url, "https://example.com/event");
            eventChunk.GetString(CalendarStringsEnum.Url).ShouldNotBeEmpty();
            var note = eventChunk.GetString(CalendarStringsEnum.Url)[0];
            note.Value.ShouldBe("https://example.com/event");
            string calendarStr = calendar.SaveToString();
            calendarStr.ShouldContain("https://example.com/event");
        }
        
        [TestMethod]
        public void TestCalendarPropertyAddNonstandardInRoot()
        {
            var calendars = CalendarTools.GetCalendarsFromString(CalendarData.singleVCalendarTwoCalendarShort);
            var calendar = calendars[0];
            calendar.AddPartToArray(CalendarPartsArrayEnum.NonstandardNames, "T;E;S;T", "", "CHARACTERS");
            calendar.GetExtraPartsArray(PartsArrayEnum.NonstandardNames).ShouldNotBeEmpty();
            var name = calendar.GetExtraPartsArray<XNameInfo>()[0];
            name.XKeyName.ShouldBe("CHARACTERS");
            name.XValues.ShouldNotBeNull();
            name.XValues.Length.ShouldBe(4);
            name.XValues[0].ShouldBe("T");
            name.XValues[1].ShouldBe("E");
            name.XValues[2].ShouldBe("S");
            name.XValues[3].ShouldBe("T");
            string calendarStr = calendar.SaveToString();
            calendarStr.ShouldContain("X-CHARACTERS");
            calendarStr.ShouldContain("T;E;S;T");
        }
        
        [TestMethod]
        public void TestCalendarPropertyAddNonstandardInEvent()
        {
            var calendars = CalendarTools.GetCalendarsFromString(CalendarData.singleVCalendarTwoCalendarShort);
            var calendar = calendars[0];
            var eventChunk = calendar.Events[0];
            eventChunk.AddPartToArray(CalendarPartsArrayEnum.NonstandardNames, "T;E;S;T", "", "CHARACTERS");
            eventChunk.GetExtraPartsArray(PartsArrayEnum.NonstandardNames).ShouldNotBeEmpty();
            var name = eventChunk.GetExtraPartsArray<XNameInfo>()[0];
            name.XKeyName.ShouldBe("CHARACTERS");
            name.XValues.ShouldNotBeNull();
            name.XValues.Length.ShouldBe(4);
            name.XValues[0].ShouldBe("T");
            name.XValues[1].ShouldBe("E");
            name.XValues[2].ShouldBe("S");
            name.XValues[3].ShouldBe("T");
            string calendarStr = calendar.SaveToString();
            calendarStr.ShouldContain("X-CHARACTERS");
            calendarStr.ShouldContain("T;E;S;T");
        }
        
        [TestMethod]
        public void TestCalendarPropertyAddIanaInRoot()
        {
            var calendars = CalendarTools.GetCalendarsFromString(CalendarData.singleVCalendarTwoCalendarShort);
            var calendar = calendars[0];
            calendar.AddPartToArray(CalendarPartsArrayEnum.IanaNames, "T;E;S;T", "", "CHARACTERS");
            calendar.GetExtraPartsArray(PartsArrayEnum.IanaNames).ShouldNotBeEmpty();
            var name = calendar.GetExtraPartsArray<ExtraInfo>()[0];
            name.KeyName.ShouldBe("CHARACTERS");
            name.Values.ShouldNotBeNull();
            name.Values.Length.ShouldBe(4);
            name.Values[0].ShouldBe("T");
            name.Values[1].ShouldBe("E");
            name.Values[2].ShouldBe("S");
            name.Values[3].ShouldBe("T");
            string calendarStr = calendar.SaveToString();
            calendarStr.ShouldContain("CHARACTERS");
            calendarStr.ShouldContain("T;E;S;T");
        }
        
        [TestMethod]
        public void TestCalendarPropertyAddIanaInEvent()
        {
            var calendars = CalendarTools.GetCalendarsFromString(CalendarData.singleVCalendarTwoCalendarShort);
            var calendar = calendars[0];
            var eventChunk = calendar.Events[0];
            eventChunk.AddPartToArray(CalendarPartsArrayEnum.IanaNames, "T;E;S;T", "", "CHARACTERS");
            eventChunk.GetExtraPartsArray(PartsArrayEnum.IanaNames).ShouldNotBeEmpty();
            var name = eventChunk.GetExtraPartsArray<ExtraInfo>()[0];
            name.KeyName.ShouldBe("CHARACTERS");
            name.Values.ShouldNotBeNull();
            name.Values.Length.ShouldBe(4);
            name.Values[0].ShouldBe("T");
            name.Values[1].ShouldBe("E");
            name.Values[2].ShouldBe("S");
            name.Values[3].ShouldBe("T");
            string calendarStr = calendar.SaveToString();
            calendarStr.ShouldContain("CHARACTERS");
            calendarStr.ShouldContain("T;E;S;T");
        }

        [TestMethod]
        public void TestCalendarPropertySetStringInRoot()
        {
            var calendars = CalendarTools.GetCalendarsFromString(CalendarData.singleVCalendarTwoCalendarShort);
            var calendar = calendars[0];
            var fullName = calendar.GetString(CalendarStringsEnum.CalScale)[0];
            fullName.Value.ShouldBe("GREGORIAN");
            fullName.Value = "HIJRI";
            fullName.Value.ShouldBe("HIJRI");
            string calendarStr = calendar.SaveToString();
            calendarStr.ShouldContain("HIJRI");
        }
        
        [TestMethod]
        public void TestCalendarPropertySetStringInEvent()
        {
            var calendars = CalendarTools.GetCalendarsFromString(CalendarData.singleVCalendarTwoCalendarShort);
            var calendar = calendars[0];
            var eventChunk = calendar.Events[0];
            var uid = eventChunk.GetString(CalendarStringsEnum.Uid)[0];
            uid.Value.ShouldBe("uid1@example.com");
            uid.Value = "a.doherty@unimol.com";
            uid.Value.ShouldBe("a.doherty@unimol.com");
            string calendarStr = calendar.SaveToString();
            calendarStr.ShouldContain("a.doherty@unimol.com");
        }

        [TestMethod]
        public void TestCalendarPropertyRemoveStringInRoot()
        {
            var calendars = CalendarTools.GetCalendarsFromString(CalendarData.singleVCalendarTwoCalendar);
            var calendar = calendars[0];
            var calScale = calendar.GetString(CalendarStringsEnum.CalScale)[0];
            calScale.Value.ShouldBe("GREGORIAN");
            calendar.DeleteString(CalendarStringsEnum.CalScale, 0);
            calendar.GetString(CalendarStringsEnum.CalScale).ShouldBeEmpty();
            string calendarStr = calendar.SaveToString();
            calendarStr.ShouldNotContain("GREGORIAN");
        }

        [TestMethod]
        public void TestCalendarPropertyRemoveStringInEvent()
        {
            var calendars = CalendarTools.GetCalendarsFromString(CalendarData.singleVCalendarTwoCalendar);
            var calendar = calendars[0];
            var eventChunk = calendar.Events[0];
            var status = eventChunk.GetString(CalendarStringsEnum.Status)[0];
            status.Value.ShouldBe("CONFIRMED");
            eventChunk.DeleteString(CalendarStringsEnum.Status, 0);
            eventChunk.GetString(CalendarStringsEnum.Status).ShouldBeEmpty();
            string calendarStr = calendar.SaveToString();
            calendarStr.ShouldNotContain("CONFIRMED");
        }

        [TestMethod]
        public void TestCalendarPropertyRemovePartsArrayInEvent()
        {
            var calendars = CalendarTools.GetCalendarsFromString(CalendarData.singleVCalendarTwoCalendar);
            var calendar = calendars[0];
            var eventChunk = calendar.Events[0];
            var status = eventChunk.GetPartsArray<RequestStatusInfo>()[0];
            status.RequestStatus.ShouldBe((4, 1, 0));
            eventChunk.DeletePartsArray<RequestStatusInfo>(0);
            eventChunk.GetPartsArray<RequestStatusInfo>().Length.ShouldBe(1);
            status = eventChunk.GetPartsArray<RequestStatusInfo>()[0];
            status.RequestStatus.ShouldBe((3, 7, 0));
            eventChunk.DeletePartsArray<RequestStatusInfo>(0);
            eventChunk.GetPartsArray<RequestStatusInfo>().ShouldBeEmpty();
            string calendarStr = calendar.SaveToString();
            calendarStr.ShouldNotContain("REQUEST-STATUS");
        }

        [TestMethod]
        public void TestCalendarPropertyRemoveEvent()
        {
            var calendars = CalendarTools.GetCalendarsFromString(CalendarData.singleVCalendarTwoCalendar);
            var calendar = calendars[0];
            calendar.Events.Length.ShouldBe(1);
            var eventChunk = calendar.Events[0];
            calendar.DeleteEvent(eventChunk);
            calendar.Events.ShouldBeEmpty();
            string calendarStr = calendar.SaveToString();
            calendarStr.ShouldNotContain("BEGIN:VEVENT");
        }
    }
}
