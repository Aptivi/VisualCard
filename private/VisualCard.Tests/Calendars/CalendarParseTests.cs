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
using System.Collections.Generic;
using System.IO;
using VisualCard.Extras.Converters;
using VisualCard.Exceptions;
using VisualCard.Parts;
using VisualCard.Extras.Misc;
using VisualCard.Tests.Calendars.Data;
using VisualCard.Calendar.Exceptions;
using VisualCard.Calendar;

namespace VisualCard.Tests.Calendars
{
    [TestClass]
    public class CalendarParseTests
    {
        [TestMethod]
        [DynamicData(nameof(CalendarData.singleVCalendarCalendarShorts), typeof(CalendarData))]
        public void ParseDifferentCalendarsShorts(string calendarText) =>
            ParseDifferentCalendarsInternal(calendarText);

        [TestMethod]
        [DynamicData(nameof(CalendarData.singleVCalendarCalendars), typeof(CalendarData))]
        public void ParseDifferentCalendars(string calendarText) =>
            ParseDifferentCalendarsInternal(calendarText);

        [TestMethod]
        [DynamicData(nameof(CalendarData.multipleVCalendarCalendars), typeof(CalendarData))]
        public void ParseDifferentCalendarsMultiple(string calendarText) =>
            ParseDifferentCalendarsInternal(calendarText);

        internal void ParseDifferentCalendarsInternal(string calendarText)
        {
            Calendar.Parts.Calendar[] calendars;
            Should.NotThrow(() => calendars = CalendarTools.GetCalendarsFromString(calendarText));
        }

        [TestMethod]
        [DynamicData(nameof(CalendarData.singleVCalendarCalendarShorts), typeof(CalendarData))]
        public void ReparseDifferentCalendarsShorts(string calendarText) =>
            ReparseDifferentCalendarsInternal(calendarText);

        [TestMethod]
        [DynamicData(nameof(CalendarData.singleVCalendarCalendars), typeof(CalendarData))]
        public void ReparseDifferentCalendars(string calendarText) =>
            ReparseDifferentCalendarsInternal(calendarText);

        [TestMethod]
        [DynamicData(nameof(CalendarData.multipleVCalendarCalendars), typeof(CalendarData))]
        public void ReparseDifferentCalendarsMultiple(string calendarText) =>
            ReparseDifferentCalendarsInternal(calendarText);

        internal void ReparseDifferentCalendarsInternal(string calendarText)
        {
            Calendar.Parts.Calendar[] calendars = [];
            Calendar.Parts.Calendar[] secondCalendars;
            Should.NotThrow(() => calendars = CalendarTools.GetCalendarsFromString(calendarText));

            // Save all the calendars to strings and re-parse
            foreach (Calendar.Parts.Calendar calendar in calendars)
            {
                string saved = Should.NotThrow(calendar.SaveToString);
                Should.NotThrow(() => secondCalendars = CalendarTools.GetCalendarsFromString(saved));
            }
        }

        [TestMethod]
        [DynamicData(nameof(CalendarData.singleVCalendarCalendarShorts), typeof(CalendarData))]
        public void ParseDifferentCalendarsAndTestEqualityShorts(string calendarText) =>
            ParseDifferentCalendarsAndTestEqualityInternal(calendarText);

        [TestMethod]
        [DynamicData(nameof(CalendarData.singleVCalendarCalendars), typeof(CalendarData))]
        public void ParseDifferentCalendarsAndTestEquality(string calendarText) =>
            ParseDifferentCalendarsAndTestEqualityInternal(calendarText);

        [TestMethod]
        [DynamicData(nameof(CalendarData.multipleVCalendarCalendars), typeof(CalendarData))]
        public void ParseDifferentCalendarsAndTestEqualityMultiple(string calendarText) =>
            ParseDifferentCalendarsAndTestEqualityInternal(calendarText);

        internal void ParseDifferentCalendarsAndTestEqualityInternal(string calendarText)
        {
            Calendar.Parts.Calendar[] calendars = [];
            Calendar.Parts.Calendar[] secondCalendars = [];

            // Parse the calendars
            Should.NotThrow(() => calendars = CalendarTools.GetCalendarsFromString(calendarText));
            Should.NotThrow(() => secondCalendars = CalendarTools.GetCalendarsFromString(calendarText));

            // Test equality with available data
            List<bool> foundCalendars = [];
            foreach (Calendar.Parts.Calendar calendar in calendars)
            {
                bool found = false;
                foreach (Calendar.Parts.Calendar second in secondCalendars)
                    if (second == calendar)
                    {
                        found = true;
                        break;
                    }
                foundCalendars.Add(found);
            }
            foundCalendars.ShouldAllBe((b) => b);
        }

        [TestMethod]
        [DynamicData(nameof(CalendarData.singleVCalendarCalendarShorts), typeof(CalendarData))]
        public void ParseDifferentCalendarsSaveToStringAndTestEqualityShorts(string calendarText) =>
            ParseDifferentCalendarsSaveToStringAndTestEqualityInternal(calendarText);

        [TestMethod]
        [DynamicData(nameof(CalendarData.singleVCalendarCalendars), typeof(CalendarData))]
        public void ParseDifferentCalendarsSaveToStringAndTestEquality(string calendarText) =>
            ParseDifferentCalendarsSaveToStringAndTestEqualityInternal(calendarText);

        [TestMethod]
        [DynamicData(nameof(CalendarData.multipleVCalendarCalendars), typeof(CalendarData))]
        public void ParseDifferentCalendarsSaveToStringAndTestEqualityMultiple(string calendarText) =>
            ParseDifferentCalendarsSaveToStringAndTestEqualityInternal(calendarText);

        public void ParseDifferentCalendarsSaveToStringAndTestEqualityInternal(string calendarText)
        {
            List<Calendar.Parts.Calendar> savedCalendars = [];
            Calendar.Parts.Calendar[] calendars = [];
            Calendar.Parts.Calendar[] secondCalendars = [];

            // Parse the calendars
            Should.NotThrow(() => calendars = CalendarTools.GetCalendarsFromString(calendarText));
            Should.NotThrow(() => secondCalendars = CalendarTools.GetCalendarsFromString(calendarText));

            // Save all the calendars to strings and re-parse
            foreach (Calendar.Parts.Calendar calendar in calendars)
            {
                string saved = Should.NotThrow(calendar.SaveToString);
                Should.NotThrow(() => secondCalendars = CalendarTools.GetCalendarsFromString(saved));
            }

            // Test equality with available data
            List<bool> foundCalendars = [];
            foreach (Calendar.Parts.Calendar calendar in savedCalendars)
            {
                bool found = false;
                foreach (Calendar.Parts.Calendar second in secondCalendars)
                    if (second == calendar)
                    {
                        found = true;
                        break;
                    }
                foundCalendars.Add(found);
            }
            foundCalendars.ShouldAllBe((b) => b);
        }

        [TestMethod]
        [DynamicData(nameof(CalendarDataBogus.invalidCalendarsParser), typeof(CalendarDataBogus))]
        public void InvalidCalendarShouldThrowWhenParsingData(string calendarText) =>
            Should.Throw<InvalidDataException>(() => CalendarTools.GetCalendarsFromString(calendarText));

        [TestMethod]
        [DynamicData(nameof(CalendarDataBogus.invalidCalendars), typeof(CalendarDataBogus))]
        public void InvalidCalendarShouldThrowWhenParsingVCalendar(string calendarText) =>
            Should.Throw<VCalendarParseException>(() => CalendarTools.GetCalendarsFromString(calendarText));

        [TestMethod]
        [DynamicData(nameof(CalendarDataBogus.seemsValidCalendars), typeof(CalendarDataBogus))]
        public void BogusButSeemsValidShouldNotThrowWhenParsing(string calendarText)
        {
            Calendar.Parts.Calendar[] calendars = [];
            Should.NotThrow(() => calendars = CalendarTools.GetCalendarsFromString(calendarText));
            foreach (Calendar.Parts.Calendar calendar in calendars)
                calendar.ShouldNotBeNull();
        }

        [TestMethod]
        [DynamicData(nameof(CalendarDataBogus.seemsValidCalendars), typeof(CalendarDataBogus))]
        public void BogusButSeemsValidShouldNotThrowWhenParsingAndReparsing(string calendarText)
        {
            Calendar.Parts.Calendar[] calendars = [];
            Calendar.Parts.Calendar[] secondCalendars = [];
            Should.NotThrow(() => calendars = CalendarTools.GetCalendarsFromString(calendarText));
            foreach (Calendar.Parts.Calendar calendar in calendars)
                calendar.ShouldNotBeNull();

            // Save all the calendars to strings and re-parse
            foreach (Calendar.Parts.Calendar calendar in calendars)
            {
                string saved = Should.NotThrow(calendar.SaveToString);
                Should.NotThrow(() => secondCalendars = CalendarTools.GetCalendarsFromString(saved));
            }
            foreach (Calendar.Parts.Calendar calendar in secondCalendars)
                calendar.ShouldNotBeNull();
        }
    }
}
