﻿//
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

namespace VisualCard.Tests.Calendars
{
    [TestClass]
    public class CalendarMiscTests
    {
        [TestMethod]
        public void TestCreateNewCalendar()
        {
            var calendar = new Calendar.Parts.Calendar(new(2, 0));
            calendar.Strings.Count.ShouldBe(0);
            calendar.PartsArray.Count.ShouldBe(0);
            string[] savedLines = calendar.SaveToString().SplitNewLines(false);
            savedLines[0].ShouldBe("BEGIN:VCALENDAR");
            savedLines[1].ShouldBe("VERSION:2.0");
            savedLines[2].ShouldBe("END:VCALENDAR");
        }

        [TestMethod]
        public void TestValidateNewCalendar()
        {
            var calendar = new Calendar.Parts.Calendar(new(2, 0));
            calendar.Strings.Count.ShouldBe(0);
            calendar.PartsArray.Count.ShouldBe(0);
            Should.Throw(calendar.Validate, typeof(InvalidDataException));
        }
    }
}
