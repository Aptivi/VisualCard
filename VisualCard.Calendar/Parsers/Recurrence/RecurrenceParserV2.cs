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

using System;
using System.Collections.Generic;
using System.Linq;
using VisualCard.Parsers;

namespace VisualCard.Calendar.Parsers.Recurrence
{
    /// <summary>
    /// Recurrence rule parser
    /// </summary>
    public static partial class RecurrenceParser
    {
        /// <summary>
        /// Parses the recurrence rule that is formatted with vCalendar's recurrence rule syntax in section 3.3.10
        /// </summary>
        /// <param name="rule">Recurrence rule</param>
        /// <returns>Parsed recurrence rule</returns>
        public static RecurrenceRule ParseRuleV2(string rule)
        {
            // Sanity check
            if (string.IsNullOrEmpty(rule))
                throw new ArgumentNullException(nameof(rule), "There is no rule.");

            // Split the semicolons to represent part of the rule and check for frequency
            string[] ruleParts = rule.Split(';');
            bool hasFreq = ruleParts.Any((part) => part.Contains("FREQ"));
            if (!hasFreq)
                throw new ArgumentException("Frequency is not specified.");

            // Make a rule instance
            RecurrenceRule recurrenceRule = new()
            {
                ruleVersion = new(2, 0)
            };

            // Loop for each rule part
            List<string> processedKeys = [];
            foreach (string part in ruleParts)
            {
                // Check for equals sign
                if (!part.Contains("="))
                    throw new ArgumentException($"A rule part needs an equal sign, {part}.");

                // Get the key name and the value representation
                //   - ( "FREQ" "=" freq ), and so on.
                string keyName = part.Substring(0, part.IndexOf("="));
                string valueRepresentation = keyName.Length + 2 > part.Length ? "" : part.Substring(keyName.Length + 1);

                // Add this key to the processed keys list after checking for duplicates, but before checking for
                // UNTIL and COUNT occurrences since they can't coexist with each other.
                if (processedKeys.Contains(keyName))
                    throw new ArgumentException($"Key {keyName} already exists, {part}");
                processedKeys.Add(keyName);
                if (processedKeys.Contains("UNTIL") && processedKeys.Contains("COUNT"))
                    throw new ArgumentException($"Keys UNTIL and COUNT can't coexist with each other, {part}");

                // Parse everything according to the key name
                switch (keyName)
                {
                    case "FREQ":
                        // ( "FREQ" "=" freq )
                        //  freq        = "SECONDLY" / "MINUTELY" / "HOURLY" / "DAILY"
                        //              / "WEEKLY" / "MONTHLY" / "YEARLY"
                        RecurrenceRuleFrequency freq =
                            valueRepresentation == "SECONDLY" ? RecurrenceRuleFrequency.Second :
                            valueRepresentation == "MINUTELY" ? RecurrenceRuleFrequency.Minute :
                            valueRepresentation == "HOURLY" ? RecurrenceRuleFrequency.Hourly :
                            valueRepresentation == "DAILY" ? RecurrenceRuleFrequency.Daily :
                            valueRepresentation == "WEEKLY" ? RecurrenceRuleFrequency.Weekly :
                            valueRepresentation == "MONTHLY" ? RecurrenceRuleFrequency.Monthly :
                            valueRepresentation == "YEARLY" ? RecurrenceRuleFrequency.Yearly :
                            throw new ArgumentException($"Frequency {valueRepresentation} is invalid, {part}");
                        recurrenceRule.frequency = freq;
                        break;
                    case "UNTIL":
                        // ( "UNTIL" "=" enddate )
                        //  enddate     = date / date-time
                        DateTimeOffset dateTime = VcardCommonTools.ParsePosixDate(valueRepresentation);
                        recurrenceRule.endDate = dateTime;
                        break;
                    case "COUNT":
                        // ( "COUNT" "=" 1*DIGIT )
                        if (!int.TryParse(valueRepresentation, out int duration))
                            throw new ArgumentException($"Duration {valueRepresentation} is invalid, {part}");
                        recurrenceRule.duration = duration;
                        break;
                    case "INTERVAL":
                        // ( "INTERVAL" "=" 1*DIGIT )
                        if (!int.TryParse(valueRepresentation, out int interval))
                            throw new ArgumentException($"Interval {valueRepresentation} is invalid, {part}");
                        recurrenceRule.interval = interval;
                        break;
                    case "BYSECOND":
                        // ( "BYSECOND" "=" byseclist )
                        // byseclist    = ( seconds *("," seconds) )
                        // seconds      = 1*2DIGIT       ;0 to 60
                        string[] secondsList = valueRepresentation.Split(',');
                        foreach (string secondStr in secondsList)
                        {
                            if (!int.TryParse(secondStr, out int seconds))
                                throw new ArgumentException($"Seconds {secondStr} is invalid, {part}");
                            if (seconds < 0 || seconds > 60)
                                throw new ArgumentException($"Seconds {seconds} is out of range [0-60], {part}");
                            recurrenceRule.secondsList.Add(seconds);
                        }
                        break;
                    case "BYMINUTE":
                        // ( "BYMINUTE" "=" byminlist )
                        // byminlist    = ( minutes *("," minutes) )
                        // minutes      = 1*2DIGIT       ;0 to 59
                        string[] minutesList = valueRepresentation.Split(',');
                        foreach (string minuteStr in minutesList)
                        {
                            if (!int.TryParse(minuteStr, out int minutes))
                                throw new ArgumentException($"Minutes {minuteStr} is invalid, {part}");
                            if (minutes < 0 || minutes > 59)
                                throw new ArgumentException($"Minutes {minutes} is out of range [0-59], {part}");
                            recurrenceRule.minutesList.Add(minutes);
                        }
                        break;
                    case "BYHOUR":
                        // ( "BYHOUR" "=" byhrlist )
                        // byhrlist     = ( hour *("," hour) )
                        // hour         = 1*2DIGIT       ;0 to 23
                        string[] hoursList = valueRepresentation.Split(',');
                        foreach (string hourStr in hoursList)
                        {
                            if (!int.TryParse(hourStr, out int hours))
                                throw new ArgumentException($"Hours {hourStr} is invalid, {part}");
                            if (hours < 0 || hours > 23)
                                throw new ArgumentException($"Hours {hours} is out of range [0-23], {part}");
                            recurrenceRule.hoursList.Add(hours);
                        }
                        break;
                    case "BYDAY":
                        // ( "BYDAY" "=" bywdaylist )
                        // bywdaylist   = ( weekdaynum *("," weekdaynum) )
                        // weekdaynum   = [[plus / minus] ordwk] weekday
                        // ordwk        = 1*2DIGIT       ;1 to 53
                        // weekday      = "SU" / "MO" / "TU" / "WE" / "TH" / "FR" / "SA"
                        string[] daysList = valueRepresentation.Split(',');
                        foreach (string dayStr in daysList)
                        {
                            int weekNum = 0;
                            DayOfWeek dayOfWeek;

                            // We could have an ordwk instance that could have been prefixed by a plus or a minus sign.
                            // Check to see if we have a plus or a minus sign, then check the digit after.
                            string finalDayStr = dayStr;
                            bool isNegative = finalDayStr[0] == '-';
                            bool hasSignAtFirst = finalDayStr[0] == '+' || isNegative;
                            if (hasSignAtFirst)
                            {
                                if (!char.IsNumber(finalDayStr[1]))
                                    throw new ArgumentException($"Not a number {finalDayStr[1]} after sign {finalDayStr[0]}, {finalDayStr}, {part}");
                                finalDayStr = finalDayStr.Substring(1);
                            }

                            // Check for week number
                            if (char.IsNumber(finalDayStr[0]))
                            {
                                // Get the split index between digit and the week number
                                int splitIdx;
                                for (splitIdx = 0; splitIdx < finalDayStr.Length; splitIdx++)
                                {
                                    if (!char.IsNumber(finalDayStr[splitIdx]))
                                        break;
                                }
                                string weekNumStr = finalDayStr.Substring(0, splitIdx);
                                if (!int.TryParse(weekNumStr, out weekNum))
                                    throw new ArgumentException($"Not a week number {weekNumStr}, {finalDayStr}, {part}");
                                if (weekNum < 1 || weekNum > 53)
                                    throw new ArgumentException($"Week number {weekNum} is out of range [1-53], {part}");
                                finalDayStr = finalDayStr.Substring(splitIdx);
                            }

                            // Check for weekday
                            if (finalDayStr.Length != 2)
                                throw new ArgumentException($"Week day needs to have exactly 2 characters {finalDayStr}, {part}");
                            if (finalDayStr == "SU")
                                dayOfWeek = DayOfWeek.Sunday;
                            else if (finalDayStr == "MO")
                                dayOfWeek = DayOfWeek.Monday;
                            else if (finalDayStr == "TU")
                                dayOfWeek = DayOfWeek.Tuesday;
                            else if (finalDayStr == "WE")
                                dayOfWeek = DayOfWeek.Wednesday;
                            else if (finalDayStr == "TH")
                                dayOfWeek = DayOfWeek.Thursday;
                            else if (finalDayStr == "FR")
                                dayOfWeek = DayOfWeek.Friday;
                            else if (finalDayStr == "SA")
                                dayOfWeek = DayOfWeek.Saturday;
                            else
                                throw new ArgumentException($"Not a week day {finalDayStr}, {part}");

                            // Add the result
                            recurrenceRule.daysList.Add((isNegative, weekNum, dayOfWeek));
                        }
                        break;
                    case "BYMONTHDAY":
                        // ( "BYMONTHDAY" "=" bymodaylist )
                        // bymodaylist  = ( monthdaynum *("," monthdaynum) )
                        // monthdaynum  = [plus / minus] ordmoday
                        // ordmoday     = 1*2DIGIT       ;1 to 31
                        string[] monthDaysList = valueRepresentation.Split(',');
                        foreach (string monthDayStr in monthDaysList)
                        {
                            // We could have an ordmoday instance that could have been prefixed by a plus or a minus sign.
                            // Check to see if we have a plus or a minus sign, then check the digit after.
                            string finalMonthStr = monthDayStr;
                            bool isNegative = finalMonthStr[0] == '-';
                            bool hasSignAtFirst = finalMonthStr[0] == '+' || isNegative;
                            if (hasSignAtFirst)
                            {
                                if (!char.IsNumber(finalMonthStr[1]))
                                    throw new ArgumentException($"Not a number {finalMonthStr[1]} after sign {finalMonthStr[0]}, {finalMonthStr}, {part}");
                                finalMonthStr = finalMonthStr.Substring(1);
                            }

                            // Check for day of month number
                            if (!int.TryParse(finalMonthStr, out int dayOfMonth))
                                throw new ArgumentException($"Not a day of month number {finalMonthStr}, {part}");
                            if (dayOfMonth < 1 || dayOfMonth > 31)
                                throw new ArgumentException($"Day of year {dayOfMonth} is out of range [1-31], {part}");

                            // Add the result
                            recurrenceRule.daysOfMonthList.Add((isNegative, dayOfMonth));
                        }
                        break;
                    case "BYYEARDAY":
                        // ( "BYYEARDAY" "=" byyrdaylist )
                        // byyrdaylist  = ( yeardaynum *("," yeardaynum) )
                        // yeardaynum   = [plus / minus] ordyrday
                        // ordyrday     = 1*3DIGIT      ;1 to 366
                        string[] yearsList = valueRepresentation.Split(',');
                        foreach (string yearStr in yearsList)
                        {
                            // We could have an ordyrday instance that could have been prefixed by a plus or a minus sign.
                            // Check to see if we have a plus or a minus sign, then check the digit after.
                            string finalYearStr = yearStr;
                            bool isNegative = finalYearStr[0] == '-';
                            bool hasSignAtFirst = finalYearStr[0] == '+' || isNegative;
                            if (hasSignAtFirst)
                            {
                                if (!char.IsNumber(finalYearStr[1]))
                                    throw new ArgumentException($"Not a number {finalYearStr[1]} after sign {finalYearStr[0]}, {finalYearStr}, {part}");
                                finalYearStr = finalYearStr.Substring(1);
                            }

                            // Check for day of year number
                            if (!int.TryParse(finalYearStr, out int dayOfYear))
                                throw new ArgumentException($"Not a day of year number {finalYearStr}, {part}");
                            if (dayOfYear < 1 || dayOfYear > 366)
                                throw new ArgumentException($"Day of year {dayOfYear} is out of range [1-366], {part}");

                            // Add the result
                            recurrenceRule.daysOfYearList.Add((isNegative, dayOfYear));
                        }
                        break;
                    case "BYWEEKNO":
                        // ( "BYWEEKNO" "=" bywknolist )
                        // bywknolist   = ( weeknum *("," weeknum) )
                        // weeknum      = [plus / minus] ordwk
                        // ordwk        = 1*2DIGIT       ;1 to 53
                        string[] weeksList = valueRepresentation.Split(',');
                        foreach (string weekStr in weeksList)
                        {
                            // We could have an ordwk instance that could have been prefixed by a plus or a minus sign.
                            // Check to see if we have a plus or a minus sign, then check the digit after.
                            string finalWeekStr = weekStr;
                            bool isNegative = finalWeekStr[0] == '-';
                            bool hasSignAtFirst = finalWeekStr[0] == '+' || isNegative;
                            if (hasSignAtFirst)
                            {
                                if (!char.IsNumber(finalWeekStr[1]))
                                    throw new ArgumentException($"Not a number {finalWeekStr[1]} after sign {finalWeekStr[0]}, {finalWeekStr}, {part}");
                                finalWeekStr = finalWeekStr.Substring(1);
                            }

                            // Check for week number
                            if (!int.TryParse(finalWeekStr, out int weekNum))
                                throw new ArgumentException($"Not a week number {finalWeekStr}, {part}");
                            if (weekNum < 1 || weekNum > 53)
                                throw new ArgumentException($"Week number {weekNum} is out of range [1-53], {part}");

                            // Add the result
                            recurrenceRule.weeksList.Add((isNegative, weekNum));
                        }
                        break;
                    case "BYMONTH":
                        // ( "BYMONTH" "=" bymolist )
                        // bymolist     = ( monthnum *("," monthnum) )
                        // monthnum     = 1*2DIGIT       ;1 to 12
                        string[] monthsList = valueRepresentation.Split(',');
                        foreach (string monthStr in monthsList)
                        {
                            // Check for month number
                            if (!int.TryParse(monthStr, out int monthNum))
                                throw new ArgumentException($"Not a month number {monthStr}, {part}");
                            if (monthNum < 1 || monthNum > 12)
                                throw new ArgumentException($"Month number {monthNum} is out of range [1-12], {part}");

                            // Add the result
                            recurrenceRule.monthsList.Add(monthNum);
                        }
                        break;
                    case "BYSETPOS":
                        // ( "BYSETPOS" "=" bysplist )
                        // bysplist     = ( setposday *("," setposday) )
                        // setposday    = yeardaynum
                        // yeardaynum   = [plus / minus] ordyrday
                        // ordyrday     = 1*3DIGIT      ;1 to 366
                        string[] positionsList = valueRepresentation.Split(',');
                        foreach (string positionStr in positionsList)
                        {
                            // We could have an ordyrday instance that could have been prefixed by a plus or a minus sign.
                            // Check to see if we have a plus or a minus sign, then check the digit after.
                            string finalPositionStr = positionStr;
                            bool isNegative = finalPositionStr[0] == '-';
                            bool hasSignAtFirst = finalPositionStr[0] == '+' || isNegative;
                            if (hasSignAtFirst)
                            {
                                if (!char.IsNumber(finalPositionStr[1]))
                                    throw new ArgumentException($"Not a number {finalPositionStr[1]} after sign {finalPositionStr[0]}, {finalPositionStr}, {part}");
                                finalPositionStr = finalPositionStr.Substring(1);
                            }

                            // Check for position number
                            if (!int.TryParse(finalPositionStr, out int position))
                                throw new ArgumentException($"Not a position number {finalPositionStr}, {part}");
                            if (position < 1 || position > 366)
                                throw new ArgumentException($"Position {position} is out of range [1-366], {part}");

                            // Add the result
                            recurrenceRule.positionsList.Add((isNegative, position));
                        }
                        break;
                    case "WKST":
                        // ( "WKST" "=" weekday )
                        // weekday      = "SU" / "MO" / "TU" / "WE" / "TH" / "FR" / "SA"
                        DayOfWeek start;
                        if (valueRepresentation.Length != 2)
                            throw new ArgumentException($"Week day needs to have exactly 2 characters {valueRepresentation}, {part}");
                        if (valueRepresentation == "SU")
                            start = DayOfWeek.Sunday;
                        else if (valueRepresentation == "MO")
                            start = DayOfWeek.Monday;
                        else if (valueRepresentation == "TU")
                            start = DayOfWeek.Tuesday;
                        else if (valueRepresentation == "WE")
                            start = DayOfWeek.Wednesday;
                        else if (valueRepresentation == "TH")
                            start = DayOfWeek.Thursday;
                        else if (valueRepresentation == "FR")
                            start = DayOfWeek.Friday;
                        else if (valueRepresentation == "SA")
                            start = DayOfWeek.Saturday;
                        else
                            throw new ArgumentException($"Not a week day {valueRepresentation}, {part}");
                        recurrenceRule.weekStart = start;
                        break;
                    default:
                        throw new ArgumentException($"Not a valid key name {keyName}, {part}");
                }
            }

            // Return the rule, because v2.0 doesn't support nested rules within the same rule string
            return recurrenceRule;
        }
    }
}
