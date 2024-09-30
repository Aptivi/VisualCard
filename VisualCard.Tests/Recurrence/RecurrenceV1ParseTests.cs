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
using System;
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
            rules[0].Version.ShouldBe(new(1, 0));
        }

        [TestMethod]
        public void ParseV1RecurrenceMinute()
        {
            var rules = RecurrenceParser.ParseRuleV1("M60 #12");
            rules.Length.ShouldBe(1);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.duration.ShouldBe(12);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.Minute);
            rule1.interval.ShouldBe(60);
        }

        [TestMethod]
        public void ParseV1RecurrenceDayOneRule()
        {
            var rules = RecurrenceParser.ParseRuleV1("D2 0600 1200 1500 #2");
            rules.Length.ShouldBe(1);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.duration.ShouldBe(2);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.Daily);
            rule1.interval.ShouldBe(2);
            rule1.timePeriods.Count.ShouldBe(3);
            rule1.timePeriods[0].isEnd.ShouldBeFalse();
            rule1.timePeriods[0].time.ShouldBe(new(6, 0, 0));
            rule1.timePeriods[1].isEnd.ShouldBeFalse();
            rule1.timePeriods[1].time.ShouldBe(new(12, 0, 0));
            rule1.timePeriods[2].isEnd.ShouldBeFalse();
            rule1.timePeriods[2].time.ShouldBe(new(15, 0, 0));
        }

        [TestMethod]
        public void ParseV1RecurrenceDayOneRuleWithEnd()
        {
            var rules = RecurrenceParser.ParseRuleV1("D2 0600 1200$ 1500 #3");
            rules.Length.ShouldBe(1);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.duration.ShouldBe(3);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.Daily);
            rule1.interval.ShouldBe(2);
            rule1.timePeriods.Count.ShouldBe(3);
            rule1.timePeriods[0].isEnd.ShouldBeFalse();
            rule1.timePeriods[0].time.ShouldBe(new(6, 0, 0));
            rule1.timePeriods[1].isEnd.ShouldBeTrue();
            rule1.timePeriods[1].time.ShouldBe(new(12, 0, 0));
            rule1.timePeriods[2].isEnd.ShouldBeFalse();
            rule1.timePeriods[2].time.ShouldBe(new(15, 0, 0));
        }

        [TestMethod]
        public void ParseV1RecurrenceDayComplex()
        {
            var rules = RecurrenceParser.ParseRuleV1("D1 #5 M10 #6");
            rules.Length.ShouldBe(2);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.duration.ShouldBe(5);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.Daily);
            rule1.interval.ShouldBe(1);

            // Check the second rule
            var rule2 = rules[1];
            rule2.Version.ShouldBe(new(1, 0));
            rule2.duration.ShouldBe(6);
            rule2.frequency.ShouldBe(RecurrenceRuleFrequency.Minute);
            rule2.interval.ShouldBe(10);
        }

        [TestMethod]
        public void ParseV1RecurrenceDayComplexWithEnd()
        {
            var rules = RecurrenceParser.ParseRuleV1("D7 0600$ #4 M15 #4");
            rules.Length.ShouldBe(2);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.duration.ShouldBe(4);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.Daily);
            rule1.interval.ShouldBe(7);
            rule1.timePeriods.Count.ShouldBe(1);
            rule1.timePeriods[0].isEnd.ShouldBeTrue();
            rule1.timePeriods[0].time.ShouldBe(new(6, 0, 0));

            // Check the second rule
            var rule2 = rules[1];
            rule2.Version.ShouldBe(new(1, 0));
            rule2.duration.ShouldBe(4);
            rule2.frequency.ShouldBe(RecurrenceRuleFrequency.Minute);
            rule2.interval.ShouldBe(15);
        }

        [TestMethod]
        public void ParseV1RecurrenceWeekOneRule()
        {
            var rules = RecurrenceParser.ParseRuleV1("W1 #4");
            rules.Length.ShouldBe(1);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.duration.ShouldBe(4);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.Weekly);
            rule1.interval.ShouldBe(1);
        }

        [TestMethod]
        public void ParseV1RecurrenceWeekOneRuleWithEnd()
        {
            var rules = RecurrenceParser.ParseRuleV1("W2 MO$ TU #2");
            rules.Length.ShouldBe(1);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.dayTimes.Count.ShouldBe(2);
            rule1.dayTimes[0].isEnd.ShouldBeTrue();
            rule1.dayTimes[0].time.ShouldBe(DayOfWeek.Monday);
            rule1.dayTimes[1].isEnd.ShouldBeFalse();
            rule1.dayTimes[1].time.ShouldBe(DayOfWeek.Tuesday);
            rule1.duration.ShouldBe(2);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.Weekly);
            rule1.interval.ShouldBe(2);
        }

        [TestMethod]
        public void ParseV1RecurrenceWeekComplex()
        {
            var rules = RecurrenceParser.ParseRuleV1("W1 TU 1200 TH 1130 #10 M30");
            rules.Length.ShouldBe(2);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.dayTimes.Count.ShouldBe(2);
            rule1.dayTimes[0].isEnd.ShouldBeFalse();
            rule1.dayTimes[0].time.ShouldBe(DayOfWeek.Tuesday);
            rule1.dayTimes[1].isEnd.ShouldBeFalse();
            rule1.dayTimes[1].time.ShouldBe(DayOfWeek.Thursday);
            rule1.duration.ShouldBe(10);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.Weekly);
            rule1.interval.ShouldBe(1);
            rule1.timePeriods.Count.ShouldBe(2);
            rule1.timePeriods[0].isEnd.ShouldBeFalse();
            rule1.timePeriods[0].time.ShouldBe(new(12, 0, 0));
            rule1.timePeriods[1].isEnd.ShouldBeFalse();
            rule1.timePeriods[1].time.ShouldBe(new(11, 30, 0));

            // Check the second rule
            var rule2 = rules[1];
            rule2.Version.ShouldBe(new(1, 0));
            rule2.duration.ShouldBe(2);
            rule2.frequency.ShouldBe(RecurrenceRuleFrequency.Minute);
            rule2.interval.ShouldBe(30);
        }

        [TestMethod]
		public void ParseV1RecurrenceWeekComplexWithEnd()
        {
            var rules = RecurrenceParser.ParseRuleV1("W1 TU$ 1200 TH 1130 #10 M30");
            rules.Length.ShouldBe(2);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.dayTimes.Count.ShouldBe(2);
            rule1.dayTimes[0].isEnd.ShouldBeTrue();
            rule1.dayTimes[0].time.ShouldBe(DayOfWeek.Tuesday);
            rule1.dayTimes[1].isEnd.ShouldBeFalse();
            rule1.dayTimes[1].time.ShouldBe(DayOfWeek.Thursday);
            rule1.duration.ShouldBe(10);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.Weekly);
            rule1.interval.ShouldBe(1);
            rule1.timePeriods.Count.ShouldBe(2);
            rule1.timePeriods[0].isEnd.ShouldBeFalse();
            rule1.timePeriods[0].time.ShouldBe(new(12, 0, 0));
            rule1.timePeriods[1].isEnd.ShouldBeFalse();
            rule1.timePeriods[1].time.ShouldBe(new(11, 30, 0));

            // Check the second rule
            var rule2 = rules[1];
            rule2.Version.ShouldBe(new(1, 0));
            rule2.duration.ShouldBe(2);
            rule2.frequency.ShouldBe(RecurrenceRuleFrequency.Minute);
            rule2.interval.ShouldBe(30);
        }

        [TestMethod]
		public void ParseV1RecurrenceMonthlyPosOneRule()
        {
            var rules = RecurrenceParser.ParseRuleV1("MP1 #12");
            rules.Length.ShouldBe(1);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.duration.ShouldBe(12);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.MonthlyPos);
            rule1.interval.ShouldBe(1);
        }

        [TestMethod]
		public void ParseV1RecurrenceMonthlyPosOneRuleWithEnd()
        {
            var rules = RecurrenceParser.ParseRuleV1("MP2 1+$ 1- FR #3");
            rules.Length.ShouldBe(1);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.dayTimes.Count.ShouldBe(1);
            rule1.dayTimes[0].isEnd.ShouldBeFalse();
            rule1.dayTimes[0].time.ShouldBe(DayOfWeek.Friday);
            rule1.duration.ShouldBe(3);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.MonthlyPos);
            rule1.interval.ShouldBe(2);
            rule1.monthlyOccurrences.Count.ShouldBe(2);
            rule1.monthlyOccurrences[0].isEnd.ShouldBeTrue();
            rule1.monthlyOccurrences[0].Item2.negative.ShouldBeFalse();
            rule1.monthlyOccurrences[0].Item2.occurrence.ShouldBe(1);
            rule1.monthlyOccurrences[1].isEnd.ShouldBeFalse();
            rule1.monthlyOccurrences[1].Item2.negative.ShouldBeTrue();
            rule1.monthlyOccurrences[1].Item2.occurrence.ShouldBe(1);
        }

        [TestMethod]
		public void ParseV1RecurrenceMonthlyPosComplex()
        {
            var rules = RecurrenceParser.ParseRuleV1("MP6 1+ MO #5 D2 0600 1200 1500 #10");
            rules.Length.ShouldBe(2);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.dayTimes.Count.ShouldBe(1);
            rule1.dayTimes[0].isEnd.ShouldBeFalse();
            rule1.dayTimes[0].time.ShouldBe(DayOfWeek.Monday);
            rule1.duration.ShouldBe(5);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.MonthlyPos);
            rule1.interval.ShouldBe(6);
            rule1.timePeriods.Count.ShouldBe(0);
            rule1.monthlyOccurrences.Count.ShouldBe(1);
            rule1.monthlyOccurrences[0].isEnd.ShouldBeFalse();
            rule1.monthlyOccurrences[0].Item2.negative.ShouldBeFalse();
            rule1.monthlyOccurrences[0].Item2.occurrence.ShouldBe(1);

            // Check the second rule
            var rule2 = rules[1];
            rule2.Version.ShouldBe(new(1, 0));
            rule2.dayTimes.Count.ShouldBe(0);
            rule2.duration.ShouldBe(10);
            rule2.frequency.ShouldBe(RecurrenceRuleFrequency.Daily);
            rule2.interval.ShouldBe(2);
            rule2.monthlyOccurrences.Count.ShouldBe(0);
            rule2.timePeriods.Count.ShouldBe(3);
            rule2.timePeriods[0].isEnd.ShouldBeFalse();
            rule2.timePeriods[0].time.ShouldBe(new(6, 0, 0));
            rule2.timePeriods[1].isEnd.ShouldBeFalse();
            rule2.timePeriods[1].time.ShouldBe(new(12, 0, 0));
            rule2.timePeriods[2].isEnd.ShouldBeFalse();
            rule2.timePeriods[2].time.ShouldBe(new(15, 0, 0));
        }

        [TestMethod]
		public void ParseV1RecurrenceMonthlyPosComplexWithEnd()
        {
            var rules = RecurrenceParser.ParseRuleV1("MP6 1+ MO$ #5 D2 0600 1200 1500 #10");
            rules.Length.ShouldBe(2);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.dayTimes.Count.ShouldBe(1);
            rule1.dayTimes[0].isEnd.ShouldBeTrue();
            rule1.dayTimes[0].time.ShouldBe(DayOfWeek.Monday);
            rule1.duration.ShouldBe(5);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.MonthlyPos);
            rule1.interval.ShouldBe(6);
            rule1.monthlyOccurrences.Count.ShouldBe(1);
            rule1.monthlyOccurrences[0].isEnd.ShouldBeFalse();
            rule1.monthlyOccurrences[0].Item2.negative.ShouldBeFalse();
            rule1.monthlyOccurrences[0].Item2.occurrence.ShouldBe(1);

            // Check the second rule
            var rule2 = rules[1];
            rule2.Version.ShouldBe(new(1, 0));
            rule2.dayTimes.Count.ShouldBe(0);
            rule2.duration.ShouldBe(10);
            rule2.frequency.ShouldBe(RecurrenceRuleFrequency.Daily);
            rule2.interval.ShouldBe(2);
            rule2.timePeriods.Count.ShouldBe(3);
            rule2.timePeriods[0].isEnd.ShouldBeFalse();
            rule2.timePeriods[0].time.ShouldBe(new(6, 0, 0));
            rule2.timePeriods[1].isEnd.ShouldBeFalse();
            rule2.timePeriods[1].time.ShouldBe(new(12, 0, 0));
            rule2.timePeriods[2].isEnd.ShouldBeFalse();
            rule2.timePeriods[2].time.ShouldBe(new(15, 0, 0));
        }

        [TestMethod]
		public void ParseV1RecurrenceMonthlyDayOneRule()
        {
            var rules = RecurrenceParser.ParseRuleV1("MD1 2- #5");
            rules.Length.ShouldBe(1);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.duration.ShouldBe(5);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.MonthlyDay);
            rule1.interval.ShouldBe(1);
            rule1.monthlyDayNumbers.Count.ShouldBe(1);
            rule1.monthlyDayNumbers[0].isEnd.ShouldBeFalse();
            rule1.monthlyDayNumbers[0].Item2.negative.ShouldBeTrue();
            rule1.monthlyDayNumbers[0].Item2.isLastDay.ShouldBeFalse();
            rule1.monthlyDayNumbers[0].Item2.dayNum.ShouldBe(2);
        }

        [TestMethod]
		public void ParseV1RecurrenceMonthlyDayOneRuleWithEnd()
        {
            var rules = RecurrenceParser.ParseRuleV1("MD1 2-$ #5");
            rules.Length.ShouldBe(1);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.duration.ShouldBe(5);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.MonthlyDay);
            rule1.interval.ShouldBe(1);
            rule1.monthlyDayNumbers.Count.ShouldBe(1);
            rule1.monthlyDayNumbers[0].isEnd.ShouldBeTrue();
            rule1.monthlyDayNumbers[0].Item2.negative.ShouldBeTrue();
            rule1.monthlyDayNumbers[0].Item2.isLastDay.ShouldBeFalse();
            rule1.monthlyDayNumbers[0].Item2.dayNum.ShouldBe(2);
        }

        [TestMethod]
		public void ParseV1RecurrenceMonthlyDayComplex()
        {
            var rules = RecurrenceParser.ParseRuleV1("MD1 10 20 #24 D1 0600 1200 1600 #5 M15 #4");
            rules.Length.ShouldBe(3);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.duration.ShouldBe(24);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.MonthlyDay);
            rule1.interval.ShouldBe(1);
            rule1.timePeriods.Count.ShouldBe(0);
            rule1.monthlyDayNumbers.Count.ShouldBe(2);
            rule1.monthlyDayNumbers[0].isEnd.ShouldBeFalse();
            rule1.monthlyDayNumbers[0].Item2.negative.ShouldBeFalse();
            rule1.monthlyDayNumbers[0].Item2.isLastDay.ShouldBeFalse();
            rule1.monthlyDayNumbers[0].Item2.dayNum.ShouldBe(10);
            rule1.monthlyDayNumbers[1].isEnd.ShouldBeFalse();
            rule1.monthlyDayNumbers[1].Item2.negative.ShouldBeFalse();
            rule1.monthlyDayNumbers[1].Item2.isLastDay.ShouldBeFalse();
            rule1.monthlyDayNumbers[1].Item2.dayNum.ShouldBe(20);

            // Check the second rule
            var rule2 = rules[1];
            rule2.Version.ShouldBe(new(1, 0));
            rule2.duration.ShouldBe(5);
            rule2.frequency.ShouldBe(RecurrenceRuleFrequency.Daily);
            rule2.interval.ShouldBe(1);
            rule2.monthlyDayNumbers.Count.ShouldBe(0);
            rule2.timePeriods.Count.ShouldBe(3);
            rule2.timePeriods[0].isEnd.ShouldBeFalse();
            rule2.timePeriods[0].time.ShouldBe(new(6, 0, 0));
            rule2.timePeriods[1].isEnd.ShouldBeFalse();
            rule2.timePeriods[1].time.ShouldBe(new(12, 0, 0));
            rule2.timePeriods[2].isEnd.ShouldBeFalse();
            rule2.timePeriods[2].time.ShouldBe(new(16, 0, 0));

            // Check the third rule
            var rule3 = rules[2];
            rule3.Version.ShouldBe(new(1, 0));
            rule3.duration.ShouldBe(4);
            rule3.frequency.ShouldBe(RecurrenceRuleFrequency.Minute);
            rule3.interval.ShouldBe(15);
            rule3.monthlyDayNumbers.Count.ShouldBe(0);
            rule3.timePeriods.Count.ShouldBe(0);
        }

        [TestMethod]
		public void ParseV1RecurrenceMonthlyDayComplexWithEnd()
        {
            var rules = RecurrenceParser.ParseRuleV1("MD1 10 20 #24 D1 0600$ 1200 1600 #5 M15 #4");
            rules.Length.ShouldBe(3);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.duration.ShouldBe(24);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.MonthlyDay);
            rule1.interval.ShouldBe(1);
            rule1.timePeriods.Count.ShouldBe(0);
            rule1.monthlyDayNumbers.Count.ShouldBe(2);
            rule1.monthlyDayNumbers[0].isEnd.ShouldBeFalse();
            rule1.monthlyDayNumbers[0].Item2.negative.ShouldBeFalse();
            rule1.monthlyDayNumbers[0].Item2.isLastDay.ShouldBeFalse();
            rule1.monthlyDayNumbers[0].Item2.dayNum.ShouldBe(10);
            rule1.monthlyDayNumbers[1].isEnd.ShouldBeFalse();
            rule1.monthlyDayNumbers[1].Item2.negative.ShouldBeFalse();
            rule1.monthlyDayNumbers[1].Item2.isLastDay.ShouldBeFalse();
            rule1.monthlyDayNumbers[1].Item2.dayNum.ShouldBe(20);

            // Check the second rule
            var rule2 = rules[1];
            rule2.Version.ShouldBe(new(1, 0));
            rule2.duration.ShouldBe(5);
            rule2.frequency.ShouldBe(RecurrenceRuleFrequency.Daily);
            rule2.interval.ShouldBe(1);
            rule2.monthlyDayNumbers.Count.ShouldBe(0);
            rule2.timePeriods.Count.ShouldBe(3);
            rule2.timePeriods[0].isEnd.ShouldBeTrue();
            rule2.timePeriods[0].time.ShouldBe(new(6, 0, 0));
            rule2.timePeriods[1].isEnd.ShouldBeFalse();
            rule2.timePeriods[1].time.ShouldBe(new(12, 0, 0));
            rule2.timePeriods[2].isEnd.ShouldBeFalse();
            rule2.timePeriods[2].time.ShouldBe(new(16, 0, 0));

            // Check the third rule
            var rule3 = rules[2];
            rule3.Version.ShouldBe(new(1, 0));
            rule3.duration.ShouldBe(4);
            rule3.frequency.ShouldBe(RecurrenceRuleFrequency.Minute);
            rule3.interval.ShouldBe(15);
            rule3.monthlyDayNumbers.Count.ShouldBe(0);
            rule3.timePeriods.Count.ShouldBe(0);
        }

        [TestMethod]
		public void ParseV1RecurrenceYearlyMonthOneRule()
        {
            var rules = RecurrenceParser.ParseRuleV1("YM2 6 #3");
            rules.Length.ShouldBe(1);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.duration.ShouldBe(3);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.YearlyMonth);
            rule1.interval.ShouldBe(2);
            rule1.yearlyMonthNumbers.Count.ShouldBe(1);
            rule1.yearlyMonthNumbers[0].isEnd.ShouldBeFalse();
            rule1.yearlyMonthNumbers[0].monthNum.ShouldBe(6);
        }

        [TestMethod]
		public void ParseV1RecurrenceYearlyMonthOneRuleWithEnd()
        {
            var rules = RecurrenceParser.ParseRuleV1("YM1 1 3$ 8 #5");
            rules.Length.ShouldBe(1);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.duration.ShouldBe(5);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.YearlyMonth);
            rule1.interval.ShouldBe(1);
            rule1.yearlyMonthNumbers.Count.ShouldBe(3);
            rule1.yearlyMonthNumbers[0].isEnd.ShouldBeFalse();
            rule1.yearlyMonthNumbers[0].monthNum.ShouldBe(1);
            rule1.yearlyMonthNumbers[1].isEnd.ShouldBeTrue();
            rule1.yearlyMonthNumbers[1].monthNum.ShouldBe(3);
            rule1.yearlyMonthNumbers[2].isEnd.ShouldBeFalse();
            rule1.yearlyMonthNumbers[2].monthNum.ShouldBe(8);
        }

        [TestMethod]
		public void ParseV1RecurrenceYearlyMonthComplex()
        {
            var rules = RecurrenceParser.ParseRuleV1("YM1 6 9 10 #10 MP1 1+ 2+ 3+ 4+ 1- SA SU #1");
            rules.Length.ShouldBe(2);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.duration.ShouldBe(10);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.YearlyMonth);
            rule1.interval.ShouldBe(1);
            rule1.yearlyMonthNumbers.Count.ShouldBe(3);
            rule1.yearlyMonthNumbers[0].isEnd.ShouldBeFalse();
            rule1.yearlyMonthNumbers[0].monthNum.ShouldBe(6);
            rule1.yearlyMonthNumbers[1].isEnd.ShouldBeFalse();
            rule1.yearlyMonthNumbers[1].monthNum.ShouldBe(9);
            rule1.yearlyMonthNumbers[2].isEnd.ShouldBeFalse();
            rule1.yearlyMonthNumbers[2].monthNum.ShouldBe(10);

            // Check the second rule
            var rule2 = rules[1];
            rule2.Version.ShouldBe(new(1, 0));
            rule2.dayTimes.Count.ShouldBe(2);
            rule2.dayTimes[0].isEnd.ShouldBeFalse();
            rule2.dayTimes[0].time.ShouldBe(DayOfWeek.Saturday);
            rule2.dayTimes[1].isEnd.ShouldBeFalse();
            rule2.dayTimes[1].time.ShouldBe(DayOfWeek.Sunday);
            rule2.duration.ShouldBe(1);
            rule2.frequency.ShouldBe(RecurrenceRuleFrequency.MonthlyPos);
            rule2.interval.ShouldBe(1);
            rule2.monthlyOccurrences.Count.ShouldBe(5);
            rule2.monthlyOccurrences[0].isEnd.ShouldBeFalse();
            rule2.monthlyOccurrences[0].Item2.negative.ShouldBeFalse();
            rule2.monthlyOccurrences[0].Item2.occurrence.ShouldBe(1);
            rule2.monthlyOccurrences[1].isEnd.ShouldBeFalse();
            rule2.monthlyOccurrences[1].Item2.negative.ShouldBeFalse();
            rule2.monthlyOccurrences[1].Item2.occurrence.ShouldBe(2);
            rule2.monthlyOccurrences[2].isEnd.ShouldBeFalse();
            rule2.monthlyOccurrences[2].Item2.negative.ShouldBeFalse();
            rule2.monthlyOccurrences[2].Item2.occurrence.ShouldBe(3);
            rule2.monthlyOccurrences[3].isEnd.ShouldBeFalse();
            rule2.monthlyOccurrences[3].Item2.negative.ShouldBeFalse();
            rule2.monthlyOccurrences[3].Item2.occurrence.ShouldBe(4);
            rule2.monthlyOccurrences[4].isEnd.ShouldBeFalse();
            rule2.monthlyOccurrences[4].Item2.negative.ShouldBeTrue();
            rule2.monthlyOccurrences[4].Item2.occurrence.ShouldBe(1);
        }

        [TestMethod]
		public void ParseV1RecurrenceYearlyMonthComplexWithEnd()
        {
            var rules = RecurrenceParser.ParseRuleV1("YM1 1 3$ 8 #5 MD1 7 14$ 21 28");
            rules.Length.ShouldBe(2);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.duration.ShouldBe(5);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.YearlyMonth);
            rule1.interval.ShouldBe(1);
            rule1.yearlyMonthNumbers.Count.ShouldBe(3);
            rule1.yearlyMonthNumbers[0].isEnd.ShouldBeFalse();
            rule1.yearlyMonthNumbers[0].monthNum.ShouldBe(1);
            rule1.yearlyMonthNumbers[1].isEnd.ShouldBeTrue();
            rule1.yearlyMonthNumbers[1].monthNum.ShouldBe(3);
            rule1.yearlyMonthNumbers[2].isEnd.ShouldBeFalse();
            rule1.yearlyMonthNumbers[2].monthNum.ShouldBe(8);

            // Check the second rule
            var rule2 = rules[1];
            rule2.Version.ShouldBe(new(1, 0));
            rule2.duration.ShouldBe(2);
            rule2.frequency.ShouldBe(RecurrenceRuleFrequency.MonthlyDay);
            rule2.interval.ShouldBe(1);
            rule2.monthlyDayNumbers.Count.ShouldBe(4);
            rule2.monthlyDayNumbers[0].isEnd.ShouldBeFalse();
            rule2.monthlyDayNumbers[0].Item2.negative.ShouldBeFalse();
            rule2.monthlyDayNumbers[0].Item2.isLastDay.ShouldBeFalse();
            rule2.monthlyDayNumbers[0].Item2.dayNum.ShouldBe(7);
            rule2.monthlyDayNumbers[1].isEnd.ShouldBeTrue();
            rule2.monthlyDayNumbers[1].Item2.negative.ShouldBeFalse();
            rule2.monthlyDayNumbers[1].Item2.isLastDay.ShouldBeFalse();
            rule2.monthlyDayNumbers[1].Item2.dayNum.ShouldBe(14);
            rule2.monthlyDayNumbers[2].isEnd.ShouldBeFalse();
            rule2.monthlyDayNumbers[2].Item2.negative.ShouldBeFalse();
            rule2.monthlyDayNumbers[2].Item2.isLastDay.ShouldBeFalse();
            rule2.monthlyDayNumbers[2].Item2.dayNum.ShouldBe(21);
            rule2.monthlyDayNumbers[3].isEnd.ShouldBeFalse();
            rule2.monthlyDayNumbers[3].Item2.negative.ShouldBeFalse();
            rule2.monthlyDayNumbers[3].Item2.isLastDay.ShouldBeFalse();
            rule2.monthlyDayNumbers[3].Item2.dayNum.ShouldBe(28);
        }

        [TestMethod]
		public void ParseV1RecurrenceYearlyDayOneRule()
        {
            var rules = RecurrenceParser.ParseRuleV1("YD1 1 100 200 300 #4");
            rules.Length.ShouldBe(1);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.duration.ShouldBe(4);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.YearlyDay);
            rule1.interval.ShouldBe(1);
            rule1.yearlyDayNumbers.Count.ShouldBe(4);
            rule1.yearlyDayNumbers[0].isEnd.ShouldBeFalse();
            rule1.yearlyDayNumbers[0].dayNum.ShouldBe(1);
            rule1.yearlyDayNumbers[1].isEnd.ShouldBeFalse();
            rule1.yearlyDayNumbers[1].dayNum.ShouldBe(100);
            rule1.yearlyDayNumbers[2].isEnd.ShouldBeFalse();
            rule1.yearlyDayNumbers[2].dayNum.ShouldBe(200);
            rule1.yearlyDayNumbers[3].isEnd.ShouldBeFalse();
            rule1.yearlyDayNumbers[3].dayNum.ShouldBe(300);
        }

        [TestMethod]
		public void ParseV1RecurrenceYearlyDayOneRuleWithEnd()
        {
            var rules = RecurrenceParser.ParseRuleV1("YD1 1 100 200 300$ #4");
            rules.Length.ShouldBe(1);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.duration.ShouldBe(4);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.YearlyDay);
            rule1.interval.ShouldBe(1);
            rule1.yearlyDayNumbers.Count.ShouldBe(4);
            rule1.yearlyDayNumbers[0].isEnd.ShouldBeFalse();
            rule1.yearlyDayNumbers[0].dayNum.ShouldBe(1);
            rule1.yearlyDayNumbers[1].isEnd.ShouldBeFalse();
            rule1.yearlyDayNumbers[1].dayNum.ShouldBe(100);
            rule1.yearlyDayNumbers[2].isEnd.ShouldBeFalse();
            rule1.yearlyDayNumbers[2].dayNum.ShouldBe(200);
            rule1.yearlyDayNumbers[3].isEnd.ShouldBeTrue();
            rule1.yearlyDayNumbers[3].dayNum.ShouldBe(300);
        }

        [TestMethod]
		public void ParseV1RecurrenceYearlyDayComplex()
        {
            var rules = RecurrenceParser.ParseRuleV1("YD1 1 100 #5 D1 #5");
            rules.Length.ShouldBe(2);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.duration.ShouldBe(5);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.YearlyDay);
            rule1.interval.ShouldBe(1);
            rule1.yearlyDayNumbers.Count.ShouldBe(2);
            rule1.yearlyDayNumbers[0].isEnd.ShouldBeFalse();
            rule1.yearlyDayNumbers[0].dayNum.ShouldBe(1);
            rule1.yearlyDayNumbers[1].isEnd.ShouldBeFalse();
            rule1.yearlyDayNumbers[1].dayNum.ShouldBe(100);

            // Check the second rule
            var rule2 = rules[1];
            rule2.Version.ShouldBe(new(1, 0));
            rule2.duration.ShouldBe(5);
            rule2.frequency.ShouldBe(RecurrenceRuleFrequency.Daily);
            rule2.interval.ShouldBe(1);
        }

        [TestMethod]
		public void ParseV1RecurrenceYearlyDayComplexWithEnd()
        {
            var rules = RecurrenceParser.ParseRuleV1("YD1 1 100$ #5 D1 #5");
            rules.Length.ShouldBe(2);

            // Check the first rule
            var rule1 = rules[0];
            rule1.Version.ShouldBe(new(1, 0));
            rule1.duration.ShouldBe(5);
            rule1.frequency.ShouldBe(RecurrenceRuleFrequency.YearlyDay);
            rule1.interval.ShouldBe(1);
            rule1.yearlyDayNumbers.Count.ShouldBe(2);
            rule1.yearlyDayNumbers[0].isEnd.ShouldBeFalse();
            rule1.yearlyDayNumbers[0].dayNum.ShouldBe(1);
            rule1.yearlyDayNumbers[1].isEnd.ShouldBeTrue();
            rule1.yearlyDayNumbers[1].dayNum.ShouldBe(100);

            // Check the second rule
            var rule2 = rules[1];
            rule2.Version.ShouldBe(new(1, 0));
            rule2.duration.ShouldBe(5);
            rule2.frequency.ShouldBe(RecurrenceRuleFrequency.Daily);
            rule2.interval.ShouldBe(1);
        }
    }
}
