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
using VisualCard.Parsers.Recurrence;

namespace VisualCard.Tests.Recurrence
{
    [TestClass]
    public class RecurrenceV2ParseTests
    {
        [TestMethod]
        [DataRow("FREQ=DAILY;COUNT=10")]
        [DataRow("FREQ=DAILY;UNTIL=19971224T000000Z")]
        [DataRow("FREQ=DAILY;INTERVAL=2")]
        [DataRow("FREQ=DAILY;INTERVAL=10;COUNT=5")]
        [DataRow("FREQ=YEARLY;UNTIL=20000131T140000Z;BYMONTH=1;BYDAY=SU,MO,TU,WE,TH,FR,SA")]
        [DataRow("FREQ=WEEKLY;COUNT=10")]
        [DataRow("FREQ=WEEKLY;UNTIL=19971224T000000Z")]
        [DataRow("FREQ=WEEKLY;INTERVAL=2;WKST=SU")]
        [DataRow("FREQ=WEEKLY;UNTIL=19971007T000000Z;WKST=SU;BYDAY=TU,TH")]
        [DataRow("FREQ=WEEKLY;INTERVAL=2;UNTIL=19971224T000000Z;WKST=SU;BYDAY=MO,WE,FR")]
        [DataRow("FREQ=WEEKLY;INTERVAL=2;COUNT=8;WKST=SU;BYDAY=TU,TH")]
        [DataRow("FREQ=MONTHLY;COUNT=10;BYDAY=1FR")]
        [DataRow("FREQ=MONTHLY;UNTIL=19971224T000000Z;BYDAY=1FR")]
        [DataRow("FREQ=MONTHLY;INTERVAL=2;COUNT=10;BYDAY=1SU,-1SU")]
        [DataRow("FREQ=MONTHLY;COUNT=6;BYDAY=-2MO")]
        [DataRow("FREQ=MONTHLY;BYMONTHDAY=-3")]
        [DataRow("FREQ=MONTHLY;COUNT=10;BYMONTHDAY=2,15")]
        [DataRow("FREQ=MONTHLY;COUNT=10;BYMONTHDAY=1,-1")]
        [DataRow("FREQ=MONTHLY;INTERVAL=18;COUNT=10;BYMONTHDAY=10,11,12,13,14,15")]
        [DataRow("FREQ=MONTHLY;INTERVAL=2;BYDAY=TU")]
        [DataRow("FREQ=YEARLY;COUNT=10;BYMONTH=6,7")]
        [DataRow("FREQ=YEARLY;INTERVAL=2;COUNT=10;BYMONTH=1,2,3")]
        [DataRow("FREQ=YEARLY;INTERVAL=3;COUNT=10;BYYEARDAY=1,100,200")]
        [DataRow("FREQ=YEARLY;BYDAY=20MO")]
        [DataRow("FREQ=YEARLY;BYWEEKNO=20;BYDAY=MO")]
        [DataRow("FREQ=YEARLY;BYMONTH=3;BYDAY=TH")]
        [DataRow("FREQ=YEARLY;BYDAY=TH;BYMONTH=6,7,8")]
        [DataRow("FREQ=MONTHLY;BYDAY=FR;BYMONTHDAY=13")]
        [DataRow("FREQ=MONTHLY;BYDAY=SA;BYMONTHDAY=7,8,9,10,11,12,13")]
        [DataRow("FREQ=YEARLY;INTERVAL=4;BYMONTH=11;BYDAY=TU;BYMONTHDAY=2,3,4,5,6,7,8")]
        [DataRow("FREQ=MONTHLY;COUNT=3;BYDAY=TU,WE,TH;BYSETPOS=3")]
        [DataRow("FREQ=MONTHLY;BYDAY=MO,TU,WE,TH,FR;BYSETPOS=-2")]
        [DataRow("FREQ=HOURLY;INTERVAL=3;UNTIL=19970902T170000Z")]
        [DataRow("FREQ=MINUTELY;INTERVAL=15;COUNT=6")]
        [DataRow("FREQ=MINUTELY;INTERVAL=90;COUNT=4")]
        [DataRow("FREQ=MINUTELY;INTERVAL=20;BYHOUR=9,10,11,12,13,14,15,16")]
        [DataRow("FREQ=WEEKLY;INTERVAL=2;COUNT=4;BYDAY=TU,SU;WKST=MO")]
        [DataRow("FREQ=WEEKLY;INTERVAL=2;COUNT=4;BYDAY=TU,SU;WKST=SU")]
        [DataRow("FREQ=MONTHLY;BYMONTHDAY=15,30;COUNT=5")]
        public void ParseV2Recurrence(string rule)
        {
            var ruleInstance = RecurrenceParser.ParseRuleV2(rule);
            ruleInstance.ShouldNotBeNull();
            ruleInstance.Version.ShouldBe(new(2, 0));
        }

        [TestMethod]
        public void ParseV2Recurrence01()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=DAILY;COUNT=10");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(10);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Daily);
            rule.interval.ShouldBe(1);
        }

        [TestMethod]
        public void ParseV2Recurrence02()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=DAILY;UNTIL=19971224T000000Z");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(2);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Daily);
            rule.interval.ShouldBe(1);
            rule.endDate.ShouldBe(new(1997, 12, 24, 0, 0, 0, new()));
        }

        [TestMethod]
        public void ParseV2Recurrence03()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=DAILY;INTERVAL=2");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(2);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Daily);
            rule.interval.ShouldBe(2);
        }

        [TestMethod]
        public void ParseV2Recurrence04()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=DAILY;INTERVAL=10;COUNT=5");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(5);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Daily);
            rule.interval.ShouldBe(10);
        }

        [TestMethod]
        public void ParseV2Recurrence05()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=YEARLY;UNTIL=20000131T140000Z;BYMONTH=1;BYDAY=SU,MO,TU,WE,TH,FR,SA");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(2);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Yearly);
            rule.interval.ShouldBe(1);
            rule.endDate.ShouldBe(new(2000, 1, 31, 14, 0, 0, new()));
            rule.monthsList.Count.ShouldBe(1);
            rule.monthsList[0].ShouldBe(1);
            rule.daysList.Count.ShouldBe(7);
            rule.daysList[0].negative.ShouldBeFalse();
            rule.daysList[0].time.ShouldBe(DayOfWeek.Sunday);
            rule.daysList[0].weekNum.ShouldBe(0);
            rule.daysList[1].negative.ShouldBeFalse();
            rule.daysList[1].time.ShouldBe(DayOfWeek.Monday);
            rule.daysList[1].weekNum.ShouldBe(0);
            rule.daysList[2].negative.ShouldBeFalse();
            rule.daysList[2].time.ShouldBe(DayOfWeek.Tuesday);
            rule.daysList[2].weekNum.ShouldBe(0);
            rule.daysList[3].negative.ShouldBeFalse();
            rule.daysList[3].time.ShouldBe(DayOfWeek.Wednesday);
            rule.daysList[3].weekNum.ShouldBe(0);
            rule.daysList[4].negative.ShouldBeFalse();
            rule.daysList[4].time.ShouldBe(DayOfWeek.Thursday);
            rule.daysList[4].weekNum.ShouldBe(0);
            rule.daysList[5].negative.ShouldBeFalse();
            rule.daysList[5].time.ShouldBe(DayOfWeek.Friday);
            rule.daysList[5].weekNum.ShouldBe(0);
            rule.daysList[6].negative.ShouldBeFalse();
            rule.daysList[6].time.ShouldBe(DayOfWeek.Saturday);
            rule.daysList[6].weekNum.ShouldBe(0);
        }

        [TestMethod]
        public void ParseV2Recurrence06()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=WEEKLY;COUNT=10");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(10);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Weekly);
            rule.interval.ShouldBe(1);
        }

        [TestMethod]
        public void ParseV2Recurrence07()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=WEEKLY;UNTIL=19971224T000000Z");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(2);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Weekly);
            rule.interval.ShouldBe(1);
            rule.endDate.ShouldBe(new(1997, 12, 24, 0, 0, 0, new()));
        }

        [TestMethod]
        public void ParseV2Recurrence08()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=WEEKLY;INTERVAL=2;WKST=SU");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(2);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Weekly);
            rule.interval.ShouldBe(2);
            rule.weekStart.ShouldBe(DayOfWeek.Sunday);
        }

        [TestMethod]
        public void ParseV2Recurrence09()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=WEEKLY;UNTIL=19971007T000000Z;WKST=SU;BYDAY=TU,TH");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(2);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Weekly);
            rule.interval.ShouldBe(1);
            rule.weekStart.ShouldBe(DayOfWeek.Sunday);
            rule.endDate.ShouldBe(new(1997, 10, 7, 0, 0, 0, new()));
            rule.daysList.Count.ShouldBe(2);
            rule.daysList[0].negative.ShouldBeFalse();
            rule.daysList[0].time.ShouldBe(DayOfWeek.Tuesday);
            rule.daysList[0].weekNum.ShouldBe(0);
            rule.daysList[1].negative.ShouldBeFalse();
            rule.daysList[1].time.ShouldBe(DayOfWeek.Thursday);
            rule.daysList[1].weekNum.ShouldBe(0);
        }

        [TestMethod]
        public void ParseV2Recurrence10()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=WEEKLY;INTERVAL=2;UNTIL=19971224T000000Z;WKST=SU;BYDAY=MO,WE,FR");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(2);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Weekly);
            rule.interval.ShouldBe(2);
            rule.weekStart.ShouldBe(DayOfWeek.Sunday);
            rule.endDate.ShouldBe(new(1997, 12, 24, 0, 0, 0, new()));
            rule.daysList.Count.ShouldBe(3);
            rule.daysList[0].negative.ShouldBeFalse();
            rule.daysList[0].time.ShouldBe(DayOfWeek.Monday);
            rule.daysList[0].weekNum.ShouldBe(0);
            rule.daysList[1].negative.ShouldBeFalse();
            rule.daysList[1].time.ShouldBe(DayOfWeek.Wednesday);
            rule.daysList[1].weekNum.ShouldBe(0);
            rule.daysList[2].negative.ShouldBeFalse();
            rule.daysList[2].time.ShouldBe(DayOfWeek.Friday);
            rule.daysList[2].weekNum.ShouldBe(0);
        }

        [TestMethod]
        public void ParseV2Recurrence11()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=WEEKLY;INTERVAL=2;COUNT=8;WKST=SU;BYDAY=TU,TH");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(8);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Weekly);
            rule.interval.ShouldBe(2);
            rule.weekStart.ShouldBe(DayOfWeek.Sunday);
            rule.daysList.Count.ShouldBe(2);
            rule.daysList[0].negative.ShouldBeFalse();
            rule.daysList[0].time.ShouldBe(DayOfWeek.Tuesday);
            rule.daysList[0].weekNum.ShouldBe(0);
            rule.daysList[1].negative.ShouldBeFalse();
            rule.daysList[1].time.ShouldBe(DayOfWeek.Thursday);
            rule.daysList[1].weekNum.ShouldBe(0);
        }

        [TestMethod]
        public void ParseV2Recurrence12()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=MONTHLY;COUNT=10;BYDAY=1FR");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(10);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Monthly);
            rule.interval.ShouldBe(1);
            rule.daysList.Count.ShouldBe(1);
            rule.daysList[0].negative.ShouldBeFalse();
            rule.daysList[0].time.ShouldBe(DayOfWeek.Friday);
            rule.daysList[0].weekNum.ShouldBe(1);
        }

        [TestMethod]
        public void ParseV2Recurrence13()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=MONTHLY;UNTIL=19971224T000000Z;BYDAY=1FR");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(2);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Monthly);
            rule.interval.ShouldBe(1);
            rule.endDate.ShouldBe(new(1997, 12, 24, 0, 0, 0, new()));
            rule.daysList.Count.ShouldBe(1);
            rule.daysList[0].negative.ShouldBeFalse();
            rule.daysList[0].time.ShouldBe(DayOfWeek.Friday);
            rule.daysList[0].weekNum.ShouldBe(1);
        }

        [TestMethod]
        public void ParseV2Recurrence14()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=MONTHLY;INTERVAL=2;COUNT=10;BYDAY=1SU,-1SU");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(10);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Monthly);
            rule.interval.ShouldBe(2);
            rule.daysList.Count.ShouldBe(2);
            rule.daysList[0].negative.ShouldBeFalse();
            rule.daysList[0].time.ShouldBe(DayOfWeek.Sunday);
            rule.daysList[0].weekNum.ShouldBe(1);
            rule.daysList[1].negative.ShouldBeTrue();
            rule.daysList[1].time.ShouldBe(DayOfWeek.Sunday);
            rule.daysList[1].weekNum.ShouldBe(1);
        }

        [TestMethod]
        public void ParseV2Recurrence15()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=MONTHLY;COUNT=6;BYDAY=-2MO");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(6);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Monthly);
            rule.interval.ShouldBe(1);
            rule.daysList.Count.ShouldBe(1);
            rule.daysList[0].negative.ShouldBeTrue();
            rule.daysList[0].time.ShouldBe(DayOfWeek.Monday);
            rule.daysList[0].weekNum.ShouldBe(2);
        }

        [TestMethod]
        public void ParseV2Recurrence16()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=MONTHLY;BYMONTHDAY=-3");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(2);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Monthly);
            rule.interval.ShouldBe(1);
            rule.daysOfMonthList.Count.ShouldBe(1);
            rule.daysOfMonthList[0].negative.ShouldBeTrue();
            rule.daysOfMonthList[0].dayOfMonth.ShouldBe(3);
        }

        [TestMethod]
        public void ParseV2Recurrence17()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=MONTHLY;COUNT=10;BYMONTHDAY=2,15");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(10);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Monthly);
            rule.interval.ShouldBe(1);
            rule.daysOfMonthList.Count.ShouldBe(2);
            rule.daysOfMonthList[0].negative.ShouldBeFalse();
            rule.daysOfMonthList[0].dayOfMonth.ShouldBe(2);
            rule.daysOfMonthList[1].negative.ShouldBeFalse();
            rule.daysOfMonthList[1].dayOfMonth.ShouldBe(15);
        }

        [TestMethod]
        public void ParseV2Recurrence18()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=MONTHLY;COUNT=10;BYMONTHDAY=1,-1");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(10);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Monthly);
            rule.interval.ShouldBe(1);
            rule.daysOfMonthList.Count.ShouldBe(2);
            rule.daysOfMonthList[0].negative.ShouldBeFalse();
            rule.daysOfMonthList[0].dayOfMonth.ShouldBe(1);
            rule.daysOfMonthList[1].negative.ShouldBeTrue();
            rule.daysOfMonthList[1].dayOfMonth.ShouldBe(1);
        }

        [TestMethod]
        public void ParseV2Recurrence19()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=MONTHLY;INTERVAL=18;COUNT=10;BYMONTHDAY=10,11,12,13,14,15");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(10);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Monthly);
            rule.interval.ShouldBe(18);
            rule.daysOfMonthList.Count.ShouldBe(6);
            rule.daysOfMonthList[0].negative.ShouldBeFalse();
            rule.daysOfMonthList[0].dayOfMonth.ShouldBe(10);
            rule.daysOfMonthList[1].negative.ShouldBeFalse();
            rule.daysOfMonthList[1].dayOfMonth.ShouldBe(11);
            rule.daysOfMonthList[2].negative.ShouldBeFalse();
            rule.daysOfMonthList[2].dayOfMonth.ShouldBe(12);
            rule.daysOfMonthList[3].negative.ShouldBeFalse();
            rule.daysOfMonthList[3].dayOfMonth.ShouldBe(13);
            rule.daysOfMonthList[4].negative.ShouldBeFalse();
            rule.daysOfMonthList[4].dayOfMonth.ShouldBe(14);
            rule.daysOfMonthList[5].negative.ShouldBeFalse();
            rule.daysOfMonthList[5].dayOfMonth.ShouldBe(15);
        }

        [TestMethod]
        public void ParseV2Recurrence20()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=MONTHLY;INTERVAL=2;BYDAY=TU");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(2);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Monthly);
            rule.interval.ShouldBe(2);
            rule.daysList.Count.ShouldBe(1);
            rule.daysList[0].negative.ShouldBeFalse();
            rule.daysList[0].time.ShouldBe(DayOfWeek.Tuesday);
            rule.daysList[0].weekNum.ShouldBe(0);
        }

        [TestMethod]
        public void ParseV2Recurrence21()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=YEARLY;COUNT=10;BYMONTH=6,7");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(10);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Yearly);
            rule.interval.ShouldBe(1);
            rule.monthsList.Count.ShouldBe(2);
            rule.monthsList[0].ShouldBe(6);
            rule.monthsList[1].ShouldBe(7);
        }

        [TestMethod]
        public void ParseV2Recurrence22()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=YEARLY;INTERVAL=2;COUNT=10;BYMONTH=1,2,3");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(10);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Yearly);
            rule.interval.ShouldBe(2);
            rule.monthsList.Count.ShouldBe(3);
            rule.monthsList[0].ShouldBe(1);
            rule.monthsList[1].ShouldBe(2);
            rule.monthsList[2].ShouldBe(3);
        }

        [TestMethod]
        public void ParseV2Recurrence23()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=YEARLY;INTERVAL=3;COUNT=10;BYYEARDAY=1,100,200");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(10);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Yearly);
            rule.interval.ShouldBe(3);
            rule.daysOfYearList.Count.ShouldBe(3);
            rule.daysOfYearList[0].negative.ShouldBeFalse();
            rule.daysOfYearList[0].dayOfYear.ShouldBe(1);
            rule.daysOfYearList[1].negative.ShouldBeFalse();
            rule.daysOfYearList[1].dayOfYear.ShouldBe(100);
            rule.daysOfYearList[2].negative.ShouldBeFalse();
            rule.daysOfYearList[2].dayOfYear.ShouldBe(200);
        }

        [TestMethod]
        public void ParseV2Recurrence24()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=YEARLY;BYDAY=20MO");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(2);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Yearly);
            rule.interval.ShouldBe(1);
            rule.daysList.Count.ShouldBe(1);
            rule.daysList[0].negative.ShouldBeFalse();
            rule.daysList[0].time.ShouldBe(DayOfWeek.Monday);
            rule.daysList[0].weekNum.ShouldBe(20);
        }

        [TestMethod]
        public void ParseV2Recurrence25()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=YEARLY;BYWEEKNO=20;BYDAY=MO");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(2);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Yearly);
            rule.interval.ShouldBe(1);
            rule.weeksList.Count.ShouldBe(1);
            rule.weeksList[0].negative.ShouldBeFalse();
            rule.weeksList[0].weekNum.ShouldBe(20);
            rule.daysList.Count.ShouldBe(1);
            rule.daysList[0].negative.ShouldBeFalse();
            rule.daysList[0].time.ShouldBe(DayOfWeek.Monday);
            rule.daysList[0].weekNum.ShouldBe(0);
        }

        [TestMethod]
        public void ParseV2Recurrence26()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=YEARLY;BYMONTH=3;BYDAY=TH");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(2);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Yearly);
            rule.interval.ShouldBe(1);
            rule.monthsList.Count.ShouldBe(1);
            rule.monthsList[0].ShouldBe(3);
            rule.daysList.Count.ShouldBe(1);
            rule.daysList[0].negative.ShouldBeFalse();
            rule.daysList[0].time.ShouldBe(DayOfWeek.Thursday);
            rule.daysList[0].weekNum.ShouldBe(0);
        }

        [TestMethod]
        public void ParseV2Recurrence27()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=YEARLY;BYDAY=TH;BYMONTH=6,7,8");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(2);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Yearly);
            rule.interval.ShouldBe(1);
            rule.daysList.Count.ShouldBe(1);
            rule.daysList[0].negative.ShouldBeFalse();
            rule.daysList[0].time.ShouldBe(DayOfWeek.Thursday);
            rule.daysList[0].weekNum.ShouldBe(0);
            rule.monthsList.Count.ShouldBe(3);
            rule.monthsList[0].ShouldBe(6);
            rule.monthsList[1].ShouldBe(7);
            rule.monthsList[2].ShouldBe(8);
        }

        [TestMethod]
        public void ParseV2Recurrence28()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=MONTHLY;BYDAY=FR;BYMONTHDAY=13");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(2);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Monthly);
            rule.interval.ShouldBe(1);
            rule.daysOfMonthList.Count.ShouldBe(1);
            rule.daysOfMonthList[0].negative.ShouldBeFalse();
            rule.daysOfMonthList[0].dayOfMonth.ShouldBe(13);
            rule.daysList.Count.ShouldBe(1);
            rule.daysList[0].negative.ShouldBeFalse();
            rule.daysList[0].time.ShouldBe(DayOfWeek.Friday);
            rule.daysList[0].weekNum.ShouldBe(0);
        }

        [TestMethod]
        public void ParseV2Recurrence29()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=MONTHLY;BYDAY=SA;BYMONTHDAY=7,8,9,10,11,12,13");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(2);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Monthly);
            rule.interval.ShouldBe(1);
            rule.daysOfMonthList.Count.ShouldBe(7);
            rule.daysOfMonthList[0].negative.ShouldBeFalse();
            rule.daysOfMonthList[0].dayOfMonth.ShouldBe(7);
            rule.daysOfMonthList[1].negative.ShouldBeFalse();
            rule.daysOfMonthList[1].dayOfMonth.ShouldBe(8);
            rule.daysOfMonthList[2].negative.ShouldBeFalse();
            rule.daysOfMonthList[2].dayOfMonth.ShouldBe(9);
            rule.daysOfMonthList[3].negative.ShouldBeFalse();
            rule.daysOfMonthList[3].dayOfMonth.ShouldBe(10);
            rule.daysOfMonthList[4].negative.ShouldBeFalse();
            rule.daysOfMonthList[4].dayOfMonth.ShouldBe(11);
            rule.daysOfMonthList[5].negative.ShouldBeFalse();
            rule.daysOfMonthList[5].dayOfMonth.ShouldBe(12);
            rule.daysOfMonthList[6].negative.ShouldBeFalse();
            rule.daysOfMonthList[6].dayOfMonth.ShouldBe(13);
            rule.daysList.Count.ShouldBe(1);
            rule.daysList[0].negative.ShouldBeFalse();
            rule.daysList[0].time.ShouldBe(DayOfWeek.Saturday);
            rule.daysList[0].weekNum.ShouldBe(0);
        }

        [TestMethod]
        public void ParseV2Recurrence30()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=YEARLY;INTERVAL=4;BYMONTH=11;BYDAY=TU;BYMONTHDAY=2,3,4,5,6,7,8");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(2);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Yearly);
            rule.interval.ShouldBe(4);
            rule.daysOfMonthList.Count.ShouldBe(7);
            rule.daysOfMonthList[0].negative.ShouldBeFalse();
            rule.daysOfMonthList[0].dayOfMonth.ShouldBe(2);
            rule.daysOfMonthList[1].negative.ShouldBeFalse();
            rule.daysOfMonthList[1].dayOfMonth.ShouldBe(3);
            rule.daysOfMonthList[2].negative.ShouldBeFalse();
            rule.daysOfMonthList[2].dayOfMonth.ShouldBe(4);
            rule.daysOfMonthList[3].negative.ShouldBeFalse();
            rule.daysOfMonthList[3].dayOfMonth.ShouldBe(5);
            rule.daysOfMonthList[4].negative.ShouldBeFalse();
            rule.daysOfMonthList[4].dayOfMonth.ShouldBe(6);
            rule.daysOfMonthList[5].negative.ShouldBeFalse();
            rule.daysOfMonthList[5].dayOfMonth.ShouldBe(7);
            rule.daysOfMonthList[6].negative.ShouldBeFalse();
            rule.daysOfMonthList[6].dayOfMonth.ShouldBe(8);
            rule.daysList.Count.ShouldBe(1);
            rule.daysList[0].negative.ShouldBeFalse();
            rule.daysList[0].time.ShouldBe(DayOfWeek.Tuesday);
            rule.daysList[0].weekNum.ShouldBe(0);
            rule.monthsList.Count.ShouldBe(1);
            rule.monthsList[0].ShouldBe(11);
        }

        [TestMethod]
        public void ParseV2Recurrence31()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=MONTHLY;COUNT=3;BYDAY=TU,WE,TH;BYSETPOS=3");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(3);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Monthly);
            rule.interval.ShouldBe(1);
            rule.positionsList.Count.ShouldBe(1);
            rule.positionsList[0].negative.ShouldBeFalse();
            rule.positionsList[0].position.ShouldBe(3);
            rule.daysList.Count.ShouldBe(3);
            rule.daysList[0].negative.ShouldBeFalse();
            rule.daysList[0].time.ShouldBe(DayOfWeek.Tuesday);
            rule.daysList[0].weekNum.ShouldBe(0);
            rule.daysList[1].negative.ShouldBeFalse();
            rule.daysList[1].time.ShouldBe(DayOfWeek.Wednesday);
            rule.daysList[1].weekNum.ShouldBe(0);
            rule.daysList[2].negative.ShouldBeFalse();
            rule.daysList[2].time.ShouldBe(DayOfWeek.Thursday);
            rule.daysList[2].weekNum.ShouldBe(0);
        }

        [TestMethod]
        public void ParseV2Recurrence32()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=MONTHLY;BYDAY=MO,TU,WE,TH,FR;BYSETPOS=-2");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(2);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Monthly);
            rule.interval.ShouldBe(1);
            rule.positionsList.Count.ShouldBe(1);
            rule.positionsList[0].negative.ShouldBeTrue();
            rule.positionsList[0].position.ShouldBe(2);
            rule.daysList.Count.ShouldBe(5);
            rule.daysList[0].negative.ShouldBeFalse();
            rule.daysList[0].time.ShouldBe(DayOfWeek.Monday);
            rule.daysList[0].weekNum.ShouldBe(0);
            rule.daysList[1].negative.ShouldBeFalse();
            rule.daysList[1].time.ShouldBe(DayOfWeek.Tuesday);
            rule.daysList[1].weekNum.ShouldBe(0);
            rule.daysList[2].negative.ShouldBeFalse();
            rule.daysList[2].time.ShouldBe(DayOfWeek.Wednesday);
            rule.daysList[2].weekNum.ShouldBe(0);
            rule.daysList[3].negative.ShouldBeFalse();
            rule.daysList[3].time.ShouldBe(DayOfWeek.Thursday);
            rule.daysList[3].weekNum.ShouldBe(0);
            rule.daysList[4].negative.ShouldBeFalse();
            rule.daysList[4].time.ShouldBe(DayOfWeek.Friday);
            rule.daysList[4].weekNum.ShouldBe(0);
        }

        [TestMethod]
        public void ParseV2Recurrence33()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=HOURLY;INTERVAL=3;UNTIL=19970902T170000Z");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(2);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Hourly);
            rule.interval.ShouldBe(3);
            rule.endDate.ShouldBe(new(1997, 9, 2, 17, 0, 0, new()));
        }

        [TestMethod]
        public void ParseV2Recurrence34()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=MINUTELY;INTERVAL=15;COUNT=6");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(6);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Minute);
            rule.interval.ShouldBe(15);
        }

        [TestMethod]
        public void ParseV2Recurrence35()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=MINUTELY;INTERVAL=90;COUNT=4");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(4);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Minute);
            rule.interval.ShouldBe(90);
        }

        [TestMethod]
        public void ParseV2Recurrence36()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=MINUTELY;INTERVAL=20;BYHOUR=9,10,11,12,13,14,15,16");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(2);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Minute);
            rule.interval.ShouldBe(20);
            rule.hoursList.Count.ShouldBe(8);
            rule.hoursList[0].ShouldBe(9);
            rule.hoursList[1].ShouldBe(10);
            rule.hoursList[2].ShouldBe(11);
            rule.hoursList[3].ShouldBe(12);
            rule.hoursList[4].ShouldBe(13);
            rule.hoursList[5].ShouldBe(14);
            rule.hoursList[6].ShouldBe(15);
            rule.hoursList[7].ShouldBe(16);
        }

        [TestMethod]
        public void ParseV2Recurrence37()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=WEEKLY;INTERVAL=2;COUNT=4;BYDAY=TU,SU;WKST=MO");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(4);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Weekly);
            rule.interval.ShouldBe(2);
            rule.weekStart.ShouldBe(DayOfWeek.Monday);
            rule.daysList.Count.ShouldBe(2);
            rule.daysList[0].negative.ShouldBeFalse();
            rule.daysList[0].time.ShouldBe(DayOfWeek.Tuesday);
            rule.daysList[0].weekNum.ShouldBe(0);
            rule.daysList[1].negative.ShouldBeFalse();
            rule.daysList[1].time.ShouldBe(DayOfWeek.Sunday);
            rule.daysList[1].weekNum.ShouldBe(0);
        }

        [TestMethod]
        public void ParseV2Recurrence38()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=WEEKLY;INTERVAL=2;COUNT=4;BYDAY=TU,SU;WKST=SU");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(4);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Weekly);
            rule.interval.ShouldBe(2);
            rule.weekStart.ShouldBe(DayOfWeek.Sunday);
            rule.daysList.Count.ShouldBe(2);
            rule.daysList[0].negative.ShouldBeFalse();
            rule.daysList[0].time.ShouldBe(DayOfWeek.Tuesday);
            rule.daysList[0].weekNum.ShouldBe(0);
            rule.daysList[1].negative.ShouldBeFalse();
            rule.daysList[1].time.ShouldBe(DayOfWeek.Sunday);
            rule.daysList[1].weekNum.ShouldBe(0);
        }

        [TestMethod]
        public void ParseV2Recurrence39()
        {
            var rule = RecurrenceParser.ParseRuleV2("FREQ=MONTHLY;BYMONTHDAY=15,30;COUNT=5");

            // Check the first rule
            rule.Version.ShouldBe(new(2, 0));
            rule.duration.ShouldBe(5);
            rule.frequency.ShouldBe(RecurrenceRuleFrequency.Monthly);
            rule.interval.ShouldBe(1);
            rule.weekStart.ShouldBe(DayOfWeek.Sunday);
            rule.daysOfMonthList.Count.ShouldBe(2);
            rule.daysOfMonthList[0].negative.ShouldBeFalse();
            rule.daysOfMonthList[0].dayOfMonth.ShouldBe(15);
            rule.daysOfMonthList[1].negative.ShouldBeFalse();
            rule.daysOfMonthList[1].dayOfMonth.ShouldBe(30);
        }
    }
}
