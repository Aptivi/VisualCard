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

namespace VisualCard.Calendar.Parsers.Durations
{
    /// <summary>
    /// Duration management tools
    /// </summary>
    public static class DurationTools
    {
        /// <summary>
        /// Gets the date/time offset from the duration specifier that is compliant with the ISO-8601:2004 specification
        /// </summary>
        /// <param name="duration">Duration specifier in the ISO-8601:2004 format</param>
        /// <param name="modern">Whether to disable parsing years and months or not</param>
        /// <param name="utc">Whether to use UTC</param>
        /// <returns>A date/time offset instance and a time span instance from the duration specifier</returns>
        /// <exception cref="ArgumentException"></exception>
        public static (DateTimeOffset result, TimeSpan span) GetDurationSpan(string duration, bool modern = false, bool utc = true)
        {
            // Sanity checks
            duration = duration.Trim();
            if (string.IsNullOrEmpty(duration))
                throw new ArgumentException($"Duration is not provided");

            // Check to see if we've been provided with a sign
            bool isNegative = duration[0] == '-';
            if (duration[0] == '+' || isNegative)
                duration = duration.Substring(1);
            if (duration[0] != 'P')
                throw new ArgumentException($"Duration is invalid: {duration}");
            duration = duration.Substring(1);

            // Populate the date time offset accordingly
            DateTimeOffset rightNow = utc ? DateTimeOffset.UtcNow : DateTimeOffset.Now;
            DateTimeOffset offset = rightNow;
            bool inDate = true;
            while (!string.IsNullOrEmpty(duration))
            {
                // Get the designator index
                int designatorIndex;
                for (designatorIndex = 0; designatorIndex < duration.Length - 1; designatorIndex++)
                    if (!char.IsNumber(duration[designatorIndex]))
                        break;

                // Split the duration according to the designator index
                string digits = duration.Substring(0, designatorIndex);
                string type = duration.Substring(designatorIndex, 1);
                int length = digits.Length + type.Length;

                // Add according to type, but check first for the time designator
                if (type == "T")
                {
                    duration = duration.Substring(length);
                    inDate = false;
                    continue;
                }
                if (!int.TryParse(digits, out int value))
                    throw new ArgumentException($"Digits are not numeric: {digits}, {duration}");
                value = isNegative ? -value : value;
                switch (type)
                {
                    // Year and Month types are only supported in vCalendar 1.0
                    case "Y":
                        if (modern)
                            throw new ArgumentException($"Year specifier is disabled in vCalendar 2.0, {duration}");
                        offset = offset.AddYears(value);
                        break;
                    case "M":
                        if (modern && inDate)
                            throw new ArgumentException($"Month specifier is disabled in vCalendar 2.0, {duration}");
                        if (inDate)
                            offset = offset.AddMonths(value);
                        else
                            offset = offset.AddMinutes(value);
                        break;

                    // Supported in all vCalendars
                    case "W":
                        offset = offset.AddDays(value * 7);
                        break;
                    case "D":
                        offset = offset.AddDays(value);
                        break;
                    case "H":
                        offset = offset.AddHours(value);
                        break;
                    case "S":
                        offset = offset.AddSeconds(value);
                        break;
                    default:
                        throw new ArgumentException($"Type is invalid: {type}, {duration}");
                }
                duration = duration.Substring(length);
            }

            // Return the result
            return (offset, offset - rightNow);
        }
    }
}
