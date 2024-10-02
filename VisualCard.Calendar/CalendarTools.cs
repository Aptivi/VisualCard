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

using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using Textify.General;
using VisualCard.Calendar.Parsers;
using VisualCard.Parsers;

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
            StringBuilder CalendarContent = new();
            Version CalendarVersion = new();
            while (!stream.EndOfStream)
            {
                bool append = false;

                // Skip empty lines
                CalendarLine = stream.ReadLine();
                if (string.IsNullOrEmpty(CalendarLine))
                {
                    if (!stream.EndOfStream)
                        continue;
                }
                else if (!CalendarLine.EqualsNoCase(VCalendarConstants._beginText) &&
                         !CalendarLine.ToUpper().StartsWith(VcardConstants._versionSpecifier) &&
                         !CalendarLine.EqualsNoCase(VCalendarConstants._endText))
                    append = true;
                if (append)
                    CalendarContent.Append(CalendarLine);

                // All vCalendars must begin with BEGIN:VCALENDAR
                if (!CalendarLine.EqualsNoCase(VCalendarConstants._beginText) && !BeginSpotted)
                    throw new InvalidDataException($"This is not a valid vCalendar file.");
                else if (!BeginSpotted)
                {
                    BeginSpotted = true;
                    VersionSpotted = false;
                    EndSpotted = false;
                    continue;
                }

                // Now that the beginning of the calendar tag is spotted, parse the version as we need to know how to select the appropriate parser.
                // All vCalendars are required to have their own version directly after the BEGIN:VCALENDAR tag
                if (CalendarLine.ToUpper().StartsWith(VcardConstants._versionSpecifier) &&
                    !CalendarLine.EqualsNoCase($"{VcardConstants._versionSpecifier}:1.0") &&
                    !CalendarLine.EqualsNoCase($"{VcardConstants._versionSpecifier}:2.0") &&
                    !VersionSpotted)
                    throw new InvalidDataException($"This has an invalid vCalendar version {CalendarLine}.");
                else if (!VersionSpotted && CalendarLine.ToUpper().StartsWith(VcardConstants._versionSpecifier))
                {
                    VersionSpotted = true;
                    CalendarVersion = new(CalendarLine.Substring(8));
                    continue;
                }

                // If the ending tag is spotted, reset everything.
                if (CalendarLine.EqualsNoCase(VCalendarConstants._endText) && !EndSpotted)
                {
                    EndSpotted = true;

                    // Make a new parser instance
                    string content = CalendarContent.ToString();
                    string[] contentLines = content.SplitNewLines();
                    VCalendarParser CalendarParser = new(contentLines, CalendarVersion);
                    FinalParsers.Add(CalendarParser);

                    // Clear the content in case we want to make a second contact
                    CalendarContent.Clear();
                    BeginSpotted = false;
                }
                else if (append)
                    CalendarContent.AppendLine();
            }

            // Close the stream to avoid stuck file handle
            stream.Close();

            // Throw if the calendar ended prematurely
            if (!EndSpotted)
                throw new InvalidDataException("Calendar ended prematurely without the ending tag");

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
