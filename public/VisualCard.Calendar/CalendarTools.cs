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

using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using Textify.General;
using VisualCard.Calendar.Parsers;
using VisualCard.Parsers;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parsers;
using VisualCard.Common.Diagnostics;
using VisualCard.Calendar.Languages;

namespace VisualCard.Calendar
{
    /// <summary>
    /// Module for VCalendar management
    /// </summary>
    public static class CalendarTools
    {
        /// <summary>
        /// ISO 9070 Formal Public Identifier (FPI) for clipboard format type
        /// </summary>
        public const string FPI = "+//ISBN 1-887687-00-9::versit::PDI//vCalendar";

        /// <summary>
        /// Gets the list of parsers for single/multiple contacts from the string
        /// </summary>
        /// <param name="calendarText">Contacts text</param>
        /// <returns>List of contact parsers for single or multiple contacts</returns>
        public static Parts.Calendar[] GetCalendarsFromString(string calendarText)
        {
            // Open the stream to parse multiple contact versions (required to parse more than one contact)
            MemoryStream CalendarFs = new(Encoding.Default.GetBytes(calendarText));
            StreamReader CalendarReader = new(CalendarFs);
            return GetCalendars(CalendarReader);
        }

        /// <summary>
        /// Gets the list of parsers for single/multiple contacts from the path
        /// </summary>
        /// <param name="Path">Path to the contacts file</param>
        /// <returns>List of contact parsers for single or multiple contacts</returns>
        public static Parts.Calendar[] GetCalendars(string Path)
        {
            // Open the stream to parse multiple contact versions (required to parse more than one contact)
            FileStream CalendarFs = new(Path, FileMode.Open, FileAccess.Read);
            StreamReader CalendarReader = new(CalendarFs);
            return GetCalendars(CalendarReader);
        }

        /// <summary>
        /// Gets the list of parsers for single/multiple contacts from the stream
        /// </summary>
        /// <param name="stream">Stream containing the contacts</param>
        /// <returns>List of contact parsers for single or multiple contacts</returns>
        public static Parts.Calendar[] GetCalendars(StreamReader stream)
        {
            // Variables and flags
            List<VCalendarParser> FinalParsers = [];
            List<Parts.Calendar> FinalCalendars = [];
            bool BeginSpotted = false;
            bool VersionSpotted = false;
            bool EndSpotted = false;

            // Parse the lines of the calendar file
            string CalendarLine;
            List<(int, string)> lines = [];
            Version CalendarVersion = new();
            int lineNumber = 0;
            while (!stream.EndOfStream)
            {
                bool append = false;
                lineNumber++;

                // Read the line
                CalendarLine = stream.ReadLine();

                // Get the property info
                string prefix = "";
                string value = CalendarLine;
                if (!CalendarLine.StartsWith("\t") && !CalendarLine.StartsWith(" ") && !string.IsNullOrWhiteSpace(CalendarLine))
                {
                    try
                    {
                        var prop = new PropertyInfo(CalendarLine);
                        if (prop.CanContinueMultiline)
                            CalendarLine = CalendarLine.Remove(CalendarLine.Length - 1, 1);
                        while (prop.CanContinueMultiline)
                        {
                            prop.rawValue.Remove(prop.rawValue.Length - 1, 1);
                            string nextLine = stream.ReadLine();
                            prop.rawValue.Append(nextLine);

                            // Add it to the current line for later processing
                            CalendarLine += nextLine;
                            if (CalendarLine[CalendarLine.Length - 1] == '=')
                                CalendarLine = CalendarLine.Remove(CalendarLine.Length - 1, 1);
                        }
                        prefix = prop.Prefix;
                        value = prop.Value;
                    }
                    catch (Exception ex)
                    {
                        LoggingTools.Warning(ex, "Line may not be valid: {0}", CalendarLine);
                        value = CalendarLine;
                    }
                }

                // Process the line for begin, version, and end specifiers
                if (string.IsNullOrEmpty(CalendarLine))
                    continue;
                else if ((!prefix.EqualsNoCase(CommonConstants._beginSpecifier) &&
                          !prefix.EqualsNoCase(CommonConstants._versionSpecifier) &&
                          !prefix.EqualsNoCase(CommonConstants._endSpecifier)) ||
                        ((prefix.EqualsNoCase(CommonConstants._beginSpecifier) ||
                          prefix.EqualsNoCase(CommonConstants._endSpecifier)) &&
                         !value.EqualsNoCase(VCalendarConstants._objectVCalendarSpecifier)))
                    append = true;
                if (append)
                    lines.Add((lineNumber, CalendarLine));

                // All vCalendars must begin with BEGIN:VCALENDAR
                if (!prefix.EqualsNoCase(CommonConstants._beginSpecifier) && !value.EqualsNoCase(VCalendarConstants._objectVCalendarSpecifier) && !BeginSpotted)
                    throw new InvalidDataException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_TOOLS_EXCEPTION_INVALIDVCALENDAR"));
                else if (!BeginSpotted)
                {
                    BeginSpotted = true;
                    VersionSpotted = false;
                    EndSpotted = false;
                    continue;
                }

                // Now that the beginning of the calendar tag is spotted, parse the version as we need to know how to select the appropriate parser.
                // All vCalendars are required to have their own version directly after the BEGIN:VCALENDAR tag
                if (prefix.EqualsNoCase(CommonConstants._versionSpecifier) &&
                    !value.EqualsNoCase("1.0") && !value.EqualsNoCase("2.0") &&
                    !VersionSpotted)
                    throw new InvalidDataException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_TOOLS_EXCEPTION_INVALIDVCALENDARVERSION").FormatString(CalendarLine));
                else if (!VersionSpotted && prefix.EqualsNoCase(CommonConstants._versionSpecifier))
                {
                    VersionSpotted = true;
                    CalendarVersion = new(value);
                    continue;
                }

                // If the ending tag is spotted, reset everything.
                if (prefix.EqualsNoCase(CommonConstants._endSpecifier) && value.EqualsNoCase(VCalendarConstants._objectVCalendarSpecifier) && !EndSpotted)
                {
                    EndSpotted = true;

                    // Make a new parser instance
                    VCalendarParser CalendarParser = new([.. lines], CalendarVersion);
                    FinalParsers.Add(CalendarParser);

                    // Clear the content in case we want to make a second contact
                    lines.Clear();
                    BeginSpotted = false;
                }
            }

            // Close the stream to avoid stuck file handle
            stream.Close();

            // Throw if the calendar ended prematurely
            if (!EndSpotted)
                throw new InvalidDataException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_TOOLS_EXCEPTION_ENDEDPREMATURELY"));

            // Now, assuming that all calendars and their parsers are valid, parse all of them
            foreach (var parser in FinalParsers)
            {
                var calendar = parser.Parse();
                FinalCalendars.Add(calendar);
            }
            return [.. FinalCalendars];
        }
    }
}
