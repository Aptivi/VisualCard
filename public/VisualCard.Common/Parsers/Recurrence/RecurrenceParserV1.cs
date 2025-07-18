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

using System;
using System.Collections.Generic;
using System.Globalization;
using Textify.General;
using VisualCard.Common.Diagnostics;
using VisualCard.Common.Languages;

namespace VisualCard.Common.Parsers.Recurrence
{
    /// <summary>
    /// Recurrence rule parser
    /// </summary>
    public static partial class RecurrenceParser
    {
        /// <summary>
        /// Parses the recurrence rule that is formatted with Basic Recurrence Rule Grammar of XAPIA's CSA
        /// </summary>
        /// <param name="rule">Recurrence rule</param>
        /// <returns>Parsed recurrence rules</returns>
        public static RecurrenceRule[] ParseRuleV1(string rule)
        {
            List<RecurrenceRule> rules = [];
            LoggingTools.Info("Parsing recurrence v1 rule: {0}", rule);

            // Sanity check
            if (string.IsNullOrEmpty(rule))
                throw new ArgumentNullException(nameof(rule), LanguageTools.GetLocalized("VISUALCARD_COMMON_PARSERS_RECURRENCE_COMMON_EXCEPTION_NORULE"));
            if (rule.Length < 2)
                throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_COMMON_PARSERS_RECURRENCE_V1_EXCEPTION_TOOSHORT"));

            // Split the spaces in the rule, as the Rule Grammar requires spaces when splitting designators
            string[] designators = rule.Split(' ');
            LoggingTools.Info("Found {0} designators from rule", designators.Length);
            RecurrenceRule parsedRule = new();
            bool firstTime = true;
            for (int i = 0; i < designators.Length; i++)
            {
                string designator = designators[i];
                LoggingTools.Debug("Designator {0}", designator);

                // Check if this designator is a frequency designator
                if (IsFrequencyDesignator(designator) && !IsWeekDay(designator))
                {
                    // We'll start with this:
                    //   - <minuteop> [<enddate>]
                    //                -OR-
                    //   - <daily> [<enddate>]
                    //                -OR-
                    //   - <weekly> [<enddate>]
                    //                -OR-
                    //   - <monthlybypos> [<enddate>]
                    //                -OR-
                    //   - <monthlybyday> [<enddate>]
                    //                -OR-
                    //   - <yearlybymonth> [<enddate>]
                    //                -OR-
                    //   - <yearlybyday> [<enddate>]
                    // Check to see if we already have a rule being parsed, and add it to the list
                    LoggingTools.Debug("First time adding frequency is {0}", firstTime);
                    if (!firstTime)
                    {
                        LoggingTools.Info("Adding parsed rule with frequency {0}", parsedRule.Frequency);
                        rules.Add(parsedRule);
                        parsedRule = new();
                    }

                    // Extract the frequency and the interval, like this for monthly-by-day frequency:
                    //   - MD5
                    //     | +-> Repetition interval
                    //     +---> Frequency
                    var (frequency, interval) = GetFrequency(designator);
                    LoggingTools.Debug("Extracted frequency and interval {0}, {1}", frequency, interval);
                    parsedRule.frequency = frequency;
                    parsedRule.interval = interval;
                    firstTime = false;
                    LoggingTools.Info("Parsing next designator...");
                    continue;
                }

                // Get the duration
                if (designator[0] == '#')
                {
                    LoggingTools.Info("Designator {0} is a duration becuase designator[0] is {1}...", designator, designator[0]);
                    string durationStr = designator.Substring(1);
                    if (!int.TryParse(durationStr, out int duration))
                    {
                        LoggingTools.Error("Designator {0} contains invalid duration.", durationStr);
                        throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_COMMON_PARSERS_RECURRENCE_V1_EXCEPTION_INVALIDDURATION") + $" {durationStr}, {designator}");
                    }
                    LoggingTools.Debug("Duration is {1}.", duration);
                    parsedRule.duration = duration;
                    LoggingTools.Info("Parsing next designator...");
                    continue;
                }

                // Determine whether there is an end marker
                bool isEndMarker = designator[designator.Length - 1] == '$';
                string filtered = isEndMarker ? designator.Substring(0, designator.Length - 1) : designator;
                LoggingTools.Debug("End marker is {0} [coming from {1}] and filtered is {2}...", isEndMarker, designator[designator.Length - 1], filtered);

                // Is this filtered designator a time period of hhmm?
                if (filtered.Length == 4)
                {
                    // time     ::= <hhmm>[<endmarker>]
                    // timelist ::= <time> {<timelist>}
                    LoggingTools.Info("Designator is a time period of hhmm [{0}]", filtered);
                    if (TimeSpan.TryParseExact($"{filtered[0]}{filtered[1]}:{filtered[2]}{filtered[3]}", "hh\\:mm", CultureInfo.InvariantCulture, out TimeSpan span))
                    {
                        LoggingTools.Debug("Got time span [{0}]", span);
                        parsedRule.timePeriods.Add((isEndMarker, span));
                    }
                    else
                    {
                        LoggingTools.Error("Invalid time [{0}]", filtered);
                        throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_COMMON_PARSERS_RECURRENCE_V1_EXCEPTION_INVALIDTIME").FormatString(filtered));
                    }
                    LoggingTools.Info("Parsing next designator...");
                    continue;
                }

                // Is this filtered designator a weekday?
                if (IsWeekDay(filtered))
                {
                    // weekday  ::= <SU|MO|TU|WE|TH|FR|SA>[<endmarker>]
                    DayOfWeek day;
                    LoggingTools.Info("Parsing day of week {0}...", filtered);
                    if (filtered == "SU")
                        day = DayOfWeek.Sunday;
                    else if (filtered == "MO")
                        day = DayOfWeek.Monday;
                    else if (filtered == "TU")
                        day = DayOfWeek.Tuesday;
                    else if (filtered == "WE")
                        day = DayOfWeek.Wednesday;
                    else if (filtered == "TH")
                        day = DayOfWeek.Thursday;
                    else if (filtered == "FR")
                        day = DayOfWeek.Friday;
                    else if (filtered == "SA")
                        day = DayOfWeek.Saturday;
                    else
                    {
                        LoggingTools.Error("Day of week {0} is invalid.", filtered);
                        throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_COMMON_PARSERS_RECURRENCE_V1_EXCEPTION_INVALIDDAYOFWEEK").FormatString(filtered));
                    }
                    LoggingTools.Debug("Day of week from {0} is {1} and end marker is {2}.", filtered, day, isEndMarker);
                    parsedRule.dayTimes.Add((isEndMarker, day));
                    LoggingTools.Info("Parsing next designator...");
                    continue;
                }

                // Is this designator an optional enddate?
                if (CommonTools.TryParsePosixDateTime(filtered, out DateTimeOffset endDate) && i == designators.Length - 1)
                {
                    LoggingTools.Info("Designator {0} is a POSIX date/time representing {1}.", filtered, endDate);

                    // Check to see if this rule is the only rule
                    if (rules.Count == 0)
                    {
                        parsedRule.endDate = endDate;
                        LoggingTools.Debug("Added end date {0} to parsed rule.", endDate);
                    }
                    else
                    {
                        rules[0].endDate = endDate;
                        LoggingTools.Debug("Added end date {0} to first parsed rule.", endDate);
                    }
                    LoggingTools.Info("Parsing next designator...");
                    continue;
                }

                // Now, here's when things get tricky. We need to change how we work according to the type of
                // frequency, but there are times when we have to deal with ambiguity, so handle it accordingly.
                // For instance, daynumber and month can be ambiguous unless they're explicitly specified, and
                // there is no explicit specification in the whole grammar. Fortunately, ambiguous properties
                // can't coexist, so we don't have to add complexity to determine whether we're really dealing
                // with one type or another.
                //
                // This complexity starts with monthlybypos (MonthlyPos).
                LoggingTools.Info("Prcessing frequency {0} [filtered length: {1}]...", parsedRule.frequency, filtered.Length);
                switch (parsedRule.frequency)
                {
                    case RecurrenceRuleFrequency.MonthlyPos:
                        // We need to parse the occurrence list... However, since we already have time and weekday,
                        // we just need to deal with the occurrence list ranging from 1 to 5 with plus or minus sign
                        // just like this:
                        //
                        //  - occurrence      ::= <1-5><plus>[<endmarker>] | <1-5><minus>[<endmarker>]
                        //  - occurrencelist  ::= <occurrence> {<occurrencelist>}
                        //
                        // Fortunately, we've also sucked the end marker, so no need to check for it. However, the plus
                        // or the minus sign is REQUIRED here according to the recurrence rule grammar.
                        //
                        // This is dealing with monthly repetition based on a relative day.
                        LoggingTools.Info("Filtered length must be two");
                        if (filtered.Length != 2)
                            throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_COMMON_PARSERS_RECURRENCE_V1_EXCEPTION_MONTHLYPOS_ARGMISMATCH").FormatString(filtered.Length) + $": {filtered}");

                        // Check the number and the sign
                        char occurrenceNumber = filtered[0];
                        char occurrenceSign = filtered[1];
                        bool occurrenceNegative = occurrenceSign == '-';
                        LoggingTools.Debug("Number from [1-5] {0}, sign {1}, negative {2}", occurrenceNumber, occurrenceSign, occurrenceNegative);
                        if (!int.TryParse($"{occurrenceNumber}", out int occurrence))
                            throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_COMMON_PARSERS_RECURRENCE_V1_EXCEPTION_MONTHLYPOS_OCCURRENCENUMNOTNUMERIC") + $" {occurrenceNumber}: {filtered}");
                        if (occurrence < 1 || occurrence > 5)
                            throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_COMMON_PARSERS_RECURRENCE_V1_EXCEPTION_MONTHLYPOS_OCCURRENCENUMOUTOFRANGE") + $" {occurrence}: {filtered}");
                        if (occurrenceSign != '+' && !occurrenceNegative)
                            throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_COMMON_PARSERS_RECURRENCE_V1_EXCEPTION_MONTHLYPOS_OCCURRENCESIGNINCORRECT") + $" {occurrenceSign}: {filtered}");

                        // Add the parsed occurrence
                        parsedRule.monthlyOccurrences.Add((isEndMarker, (occurrence, occurrenceNegative)));
                        LoggingTools.Debug("Added monthly occurrence {0}, {1}, {2}.", isEndMarker, occurrence, occurrenceNegative);
                        break;
                    case RecurrenceRuleFrequency.MonthlyDay:
                        // We need to parse the month day number list...
                        //
                        //  - lastday         ::= LD
                        //  - daynumber       ::= <1-31>[<plus>|<minus>][<endmarker>] | <lastday>
                        //  - daynumberlist   ::= daynumber {<daynumber>}
                        //
                        // The plus or the minus sign is OPTIONAL here according to the recurrence rule grammar, but it's
                        // always the last character if provided. LD is 2 characters long, but has its own special meaning.
                        // We don't care about the number of days in a month here.
                        //
                        // This is dealing with monthly repetition based on an absolute day.
                        LoggingTools.Info("Filtered length must be between 1 and 3");
                        if (filtered.Length < 1 || filtered.Length > 3)
                            throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_COMMON_PARSERS_RECURRENCE_V1_EXCEPTION_MONTHLYDAY_ARGMISMATCH").FormatString(filtered.Length) + $": {filtered}");

                        // Check for sign
                        char dayNumberSign = filtered[filtered.Length - 1];
                        bool hasSign = !char.IsNumber(dayNumberSign);
                        bool dayNumberNegative = dayNumberSign == '-';
                        LoggingTools.Debug("Number sign {0} [{1}], negative {2}", dayNumberSign, hasSign, dayNumberNegative);

                        // Check the number
                        bool dayNumberLast = filtered == "LD";
                        string dayNumberStr = hasSign && !dayNumberLast ? filtered.Substring(0, filtered.Length - 1) : filtered;
                        LoggingTools.Debug("Day number {0} [last: {1}]", dayNumberStr, dayNumberLast);
                        int dayNumber = 0;
                        if (!dayNumberLast)
                        {
                            if (!int.TryParse($"{dayNumberStr}", out dayNumber))
                                throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_COMMON_PARSERS_RECURRENCE_V1_EXCEPTION_MONTHLYDAY_DAYNUMNOTNUMERIC") + $" {dayNumberStr}: {filtered}");
                            if (dayNumber < 1 || dayNumber > 31)
                                throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_COMMON_PARSERS_RECURRENCE_V1_EXCEPTION_MONTHLYDAY_DAYNUMOUTOFRANGE") + $" {dayNumber}: {filtered}");
                            if (dayNumberSign != '+' && !dayNumberNegative && hasSign)
                                throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_COMMON_PARSERS_RECURRENCE_V1_EXCEPTION_MONTHLYDAY_DAYSIGNINCORRECT") + $" {dayNumberSign}: {filtered}");
                        }

                        // Add the parsed day number
                        parsedRule.monthlyDayNumbers.Add((isEndMarker, (dayNumber, dayNumberNegative, dayNumberLast)));
                        LoggingTools.Debug("Adding {0}, {1}, {2} with end marker {3}", dayNumber, dayNumberNegative, dayNumberLast, isEndMarker);
                        break;
                    case RecurrenceRuleFrequency.YearlyMonth:
                        // We need to parse the month number list...
                        //
                        //  - month           ::= <1-12>[<endmarker>]
                        //  - monthlist       ::= <month> {<monthlist>}
                        //
                        // This is dealing with yearly month repeat, for example, a ninth month is September.
                        LoggingTools.Info("Filtered length must be between 1 and 2");
                        if (filtered.Length < 1 || filtered.Length > 2)
                            throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_COMMON_PARSERS_RECURRENCE_V1_EXCEPTION_YEARLYMONTH_ARGMISMATCH").FormatString(filtered.Length) + $": {filtered}");

                        // Check the number
                        LoggingTools.Debug("Month number [1-12] to parse: {0}", filtered);
                        if (!int.TryParse($"{filtered}", out int monthNumber))
                            throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_COMMON_PARSERS_RECURRENCE_V1_EXCEPTION_YEARLYMONTH_MONTHNUMNOTNUMERIC") + $" {filtered}: {filtered}");
                        if (monthNumber < 1 || monthNumber > 12)
                            throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_COMMON_PARSERS_RECURRENCE_V1_EXCEPTION_YEARLYMONTH_MONTHNUMOUTOFRANGE") + $" {monthNumber}: {filtered}");

                        // Add the parsed month number
                        parsedRule.yearlyMonthNumbers.Add((isEndMarker, monthNumber));
                        LoggingTools.Debug("Adding month number {0} with end marker {3}", monthNumber, isEndMarker);
                        break;
                    case RecurrenceRuleFrequency.YearlyDay:
                        // We need to parse the day number list...
                        //
                        //  - day             ::= <1-366>[<endmarker>]
                        //  - daylist         ::= <day> {<daylist>}
                        //
                        // This is dealing with yearly day repeat, for example, a 258th day in normal years is September 15th.
                        // The client needs to handle leap years, since they have one extra day, which is February 29th or the
                        // 60th day.
                        LoggingTools.Info("Filtered length must be between 1 and 3");
                        if (filtered.Length < 1 || filtered.Length > 3)
                            throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_COMMON_PARSERS_RECURRENCE_V1_EXCEPTION_YEARLYMONTH_ARGMISMATCH").FormatString(filtered.Length) + $": {filtered}");

                        // Check the number
                        LoggingTools.Debug("Yearly day number [1-366] to parse: {0}", filtered);
                        if (!int.TryParse($"{filtered}", out int yearlyDayNumber))
                            throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_COMMON_PARSERS_RECURRENCE_V1_EXCEPTION_YEARLYDAY_DAYNUMNOTNUMERIC") + $" {filtered}: {filtered}");
                        if (yearlyDayNumber < 1 || yearlyDayNumber > 366)
                            throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_COMMON_PARSERS_RECURRENCE_V1_EXCEPTION_YEARLYDAY_DAYNUMOUTOFRANGE") + $" {yearlyDayNumber}: {filtered}");

                        // Add the parsed day number
                        parsedRule.yearlyDayNumbers.Add((isEndMarker, yearlyDayNumber));
                        LoggingTools.Debug("Adding yearly day number {0} with end marker {3}", yearlyDayNumber, isEndMarker);
                        break;
                }
            }
            rules.Add(parsedRule);
            LoggingTools.Info("All rules added: {0}", rules.Count);

            // Return the rules
            return [.. rules];
        }

        private static bool IsFrequencyDesignator(string designator) =>
            designator.Length >= 2 &&
            ((designator[0] == 'Y' && designator[1] == 'D') ||
             (designator[0] == 'Y' && designator[1] == 'M') ||
             (designator[0] == 'M' && designator[1] == 'D') ||
             (designator[0] == 'M' && designator[1] == 'P') ||
             designator[0] == 'W' ||
             designator[0] == 'D' ||
             designator[0] == 'M');

        private static bool IsWeekDay(string designator) =>
            designator.Length >= 2 && char.IsLetter(designator[0]) && char.IsLetter(designator[1]) &&
            ((designator[0] == 'S' && designator[1] == 'U') ||
             (designator[0] == 'M' && designator[1] == 'O') ||
             (designator[0] == 'T' && designator[1] == 'U') ||
             (designator[0] == 'W' && designator[1] == 'E') ||
             (designator[0] == 'T' && designator[1] == 'H') ||
             (designator[0] == 'F' && designator[1] == 'R') ||
             (designator[0] == 'S' && designator[1] == 'A'));

        private static (RecurrenceRuleFrequency frequency, int interval) GetFrequency(string designator)
        {
            if (!IsFrequencyDesignator(designator))
                throw new ArgumentException(designator);

            // Get the frequency
            RecurrenceRuleFrequency freq =
                (designator[0] == 'Y' && designator[1] == 'D') ? RecurrenceRuleFrequency.YearlyDay :
                (designator[0] == 'Y' && designator[1] == 'M') ? RecurrenceRuleFrequency.YearlyMonth :
                (designator[0] == 'M' && designator[1] == 'D') ? RecurrenceRuleFrequency.MonthlyDay :
                (designator[0] == 'M' && designator[1] == 'P') ? RecurrenceRuleFrequency.MonthlyPos :
                designator[0] == 'W' ? RecurrenceRuleFrequency.Weekly :
                designator[0] == 'D' ? RecurrenceRuleFrequency.Daily :
                designator[0] == 'M' ? RecurrenceRuleFrequency.Minute :
                throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_COMMON_PARSERS_RECURRENCE_V1_EXCEPTION_INVALIDFREQUENCY") + $" {designator}");
            LoggingTools.Debug("Frequency is {0}", freq);

            // Get the number index and cut the string so that we have a number
            int numberIdx;
            for (numberIdx = 0; numberIdx < designator.Length; numberIdx++)
            {
                LoggingTools.Debug("Checking {0} to see if it's a digit", designator[numberIdx]);
                if (char.IsDigit(designator[numberIdx]))
                {
                    LoggingTools.Debug("{0} is a digit, breaking...", designator[numberIdx]);
                    break;
                }
            }
            string intervalStr = designator.Substring(numberIdx);
            LoggingTools.Info("Checking {0} for freq {1} to see if it's parsable", intervalStr, freq);
            if (!int.TryParse(intervalStr, out int interval))
                throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_COMMON_PARSERS_RECURRENCE_V1_EXCEPTION_INVALIDFREQUENCYINTERVAL") + $" {intervalStr}, {designator}");
            return (freq, interval);
        }
    }
}
