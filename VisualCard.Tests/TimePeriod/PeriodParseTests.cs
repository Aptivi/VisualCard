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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using VisualCard.Parsers;

namespace VisualCard.Tests.TimePeriod
{
    [TestClass]
    public class PeriodParseTests
    {
        [TestMethod]
        [DataRow("19970101T180000Z/19970102T070000Z")]
        [DataRow("19970101T180000Z/PT5H30M")]
        public void ParsePeriods(string rule)
        {
            var span = VcardCommonTools.GetTimePeriod(rule);
            span.StartDate.ShouldNotBe(new());
            span.EndDate.ShouldNotBe(new());
            span.Duration.ShouldNotBe(new());
        }

        [TestMethod]
        public void ParsePeriodWithDateDate()
        {
            var span = VcardCommonTools.GetTimePeriod("19970101T180000Z/19970102T070000Z");

            // We can't test against result because it's uninferrable due to CPU timings.
            span.StartDate.ShouldNotBe(new());
            span.EndDate.ShouldNotBe(new());
            span.Duration.ShouldNotBe(new());
            span.Duration.Days.ShouldBe(0);
            span.Duration.Hours.ShouldBe(13);
            span.Duration.Minutes.ShouldBe(0);
            span.Duration.Seconds.ShouldBe(0);
        }

        [TestMethod]
        public void ParsePeriodWithDateDuration()
        {
            var span = VcardCommonTools.GetTimePeriod("19970101T180000Z/PT5H30M");

            // We can't test against result because it's uninferrable due to CPU timings.
            span.StartDate.ShouldNotBe(new());
            span.EndDate.ShouldNotBe(new());
            span.Duration.ShouldNotBe(new());
            span.Duration.Days.ShouldBe(0);
            span.Duration.Hours.ShouldBe(5);
            span.Duration.Minutes.ShouldBe(30);
            span.Duration.Seconds.ShouldBe(0);
        }
    }
}
