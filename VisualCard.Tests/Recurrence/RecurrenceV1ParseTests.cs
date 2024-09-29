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
using VisualCard.Calendar.Parsers.Recurrence;

namespace VisualCard.Tests.Recurrence
{
    [TestClass]
    public class RecurrenceV1ParseTests
    {
        [TestMethod]
        [DataRow("M60 #12", 1)]
        [DataRow("M5 #12", 1)]
        [DataRow("D1 #5", 1)]
        [DataRow("D1 #5 M10 #6", 2)]
        [DataRow("D2", 1)]
        [DataRow("D2 0600 1200 1500 #2", 1)]
        [DataRow("D2 0600 1200$ 1500 #3", 1)]
        [DataRow("D7 0600 #5 M15 #4", 2)]
        [DataRow("D7 0600$ #4 M15 #4", 2)]
        [DataRow("D7 0600 #1 M15 #4", 2)]
        [DataRow("W1 #4", 1)]
        [DataRow("W2 MO$ TU #2", 1)]
        [DataRow("W1 TU TH #3 M5 #2", 2)]
        [DataRow("W1 TU 1200 TH 1130 #10 M30", 2)]
        [DataRow("W1 TU$ 1200 TH 1130 #10 M30", 2)]
        [DataRow("W1 TU 1200$ 1230 TH 1130 1200 #10", 1)]
        [DataRow("MP1 #12", 1)]
        [DataRow("MP2 1+ 1- FR #3", 1)]
        [DataRow("MD1 2- #5", 1)]
        [DataRow("MP1 2- MO #6", 1)]
        [DataRow("MD1 3- #0", 1)]
        [DataRow("MD1 7- #12", 1)]
        [DataRow("MP2 1+$ 1- FR #3", 1)]
        [DataRow("MP6 1+ MO #5 D1 #5", 2)]
        [DataRow("MP6 1+ MO #5 D2 0600 1200 1500 #10", 2)]
        [DataRow("MP6 1+ MO #5 D2 0600 1200 1500 #10 M5 #3", 3)]
        [DataRow("MP6 1+ MO 2- TH #5 M5 #2", 2)]
        [DataRow("MP6 1+ SU MO 1200 2+ TU WE 1300 3+ TH FR 1400 #4", 1)]
        [DataRow("MD1 7 #12", 1)]
        [DataRow("MD1 7 14 21 28 #12", 1)]
        [DataRow("MD1 10 20 #24 D1 0600 1200 1600 #5 M15 #4", 3)]
        [DataRow("YM1 1 6 12 #5 MP1 1+ MO 1- FR", 2)]
        [DataRow("YM2 6 #3 MD1 12", 2)]
        [DataRow("YM1 1 3$ 8 #5 MD1 7 14$ 21 28", 2)]
        [DataRow("YM1 6 9 10 #10 MP1 1+ 2+ 3+ 4+ 1- SA SU #1", 2)]
        [DataRow("YM1 6 #10 W1 TU TH 1100 1300 #4", 2)]
        [DataRow("YD1 1 100 200 300 #4", 1)]
        [DataRow("YD1 1 100 #5 D1 #5", 2)]
        [DataRow("YD1 1 100 D1 #5 19990102T000000Z", 2)]
        public void ParseV1Recurrence(string rule, int expectedCount)
        {
            var rules = RecurrenceParser.ParseRuleV1(rule);
            rules.Length.ShouldBe(expectedCount);
        }
    }
}
