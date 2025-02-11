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
using VisualCard.Common.Parsers;

namespace VisualCard.Tests.Durations
{
    [TestClass]
    public class DurationParseTests
    {
        [TestMethod]
        [DataRow("P6W")]
        [DataRow("PT15M")]
        [DataRow("PT1H30M")]
        [DataRow("P2Y10M15DT10H30M20S")]
        [DataRow("P15DT5H0M20S")]
        [DataRow("P7W")]
        public void ParseDurations(string rule)
        {
            var span = CommonTools.GetDurationSpan(rule);
            span.result.ShouldNotBe(new());
            span.span.ShouldNotBe(new());
        }
        
        [TestMethod]
        [DataRow("P6W")]
        [DataRow("PT15M")]
        [DataRow("PT1H30M")]
        [DataRow("P2Y10M15DT10H30M20S")]
        [DataRow("P15DT5H0M20S")]
        [DataRow("P7W")]
        public void ParseDurationsNoUtc(string rule)
        {
            var span = CommonTools.GetDurationSpan(rule, utc: false);
            span.result.ShouldNotBe(new());
            span.span.ShouldNotBe(new());
        }

        [TestMethod]
        [DataRow("-P6W")]
        [DataRow("-PT15M")]
        [DataRow("-PT1H30M")]
        [DataRow("-P2Y10M15DT10H30M20S")]
        [DataRow("-P15DT5H0M20S")]
        [DataRow("-P7W")]
        public void ParseNegativeDurations(string rule)
        {
            var span = CommonTools.GetDurationSpan(rule);
            span.result.ShouldNotBe(new());
            span.span.ShouldNotBe(new());
        }
        
        [TestMethod]
        [DataRow("-P6W")]
        [DataRow("-PT15M")]
        [DataRow("-PT1H30M")]
        [DataRow("-P2Y10M15DT10H30M20S")]
        [DataRow("-P15DT5H0M20S")]
        [DataRow("-P7W")]
        public void ParseNegativeDurationsNoUtc(string rule)
        {
            var span = CommonTools.GetDurationSpan(rule, utc: false);
            span.result.ShouldNotBe(new());
            span.span.ShouldNotBe(new());
        }

        [TestMethod]
        public void ParseDuration()
        {
            var span = CommonTools.GetDurationSpan("P2Y10M15DT10H30M20S");

            // We can't test against result and days because it's uninferrable due to CPU timings.
            span.result.ShouldNotBe(new());
            span.span.ShouldNotBe(new());
            span.span.Hours.ShouldBe(10);
            span.span.Minutes.ShouldBe(30);
            span.span.Seconds.ShouldBe(20);
        }

        [TestMethod]
        public void ParseNegativeDuration()
        {
            var span = CommonTools.GetDurationSpan("-P2Y10M15DT10H30M20S");

            // We can't test against result and days because it's uninferrable due to CPU timings.
            span.result.ShouldNotBe(new());
            span.span.ShouldNotBe(new());
            span.span.Hours.ShouldBe(-10);
            span.span.Minutes.ShouldBe(-30);
            span.span.Seconds.ShouldBe(-20);
        }
    }
}
