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
using System.Diagnostics;
using System.Linq;
using Terminaux.Colors.Data;
using Terminaux.Writer.ConsoleWriters;
using VisualCard.Calendar;
using CalendarInfo = VisualCard.Calendar.Parts.Calendar;
using VisualCard.Calendar.Parts.Implementations.Event;
using VisualCard.Calendar.Parts.Enums;

namespace VisualCard.ShowCalendars
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                TextWriterColor.WriteColor("Path to calendar file is required.", ConsoleColors.Red);
            }
            else
            {
                // If one of the arguments is a switch to trigger printing, set it
                bool print = !args.Contains("-noprint");
                bool save = args.Contains("-save");
                bool dbg = args.Contains("-debug");
                args = args.Except(["-noprint", "-save", "-debug"]).ToArray();

                // If debug, wait for debugger
                if (dbg)
                    Debugger.Launch();

                // Initialize stopwatch
                Stopwatch elapsed = new();
                elapsed.Start();

                // Parse all calendars
                CalendarInfo[] calendars = CalendarTools.GetCalendars(args[0]);

                // If told to save them, do it
                foreach (var calendar in calendars)
                {
                    if (save)
                        calendar.SaveTo($"calendar_{DateTime.Now:dd-MM-yyyy_HH-mm-ss_ffffff}.vcs");
                }

                // If not printing, exit
                elapsed.Stop();
                if (!print)
                {
                    TextWriterColor.Write("Elapsed time: {0}", elapsed.Elapsed.ToString());
                    return;
                }

                // Show calendar information
                foreach (CalendarInfo Calendar in calendars)
                {
                    TextWriterColor.WriteColor("----------------------------", ConsoleColors.Green);

                    // List essentials
                    var starts = Calendar.GetPartsArray<DateStartInfo>();
                    var ends = Calendar.GetPartsArray<DateEndInfo>();
                    if (starts.Length > 0)
                        TextWriterColor.Write("Calendar start date: {0}", starts[0].DateStart ?? new());
                    if (ends.Length > 0)
                        TextWriterColor.Write("Calendar end date:   {0}", ends[0].DateEnd ?? new());
                    TextWriterColor.Write("Calendar product ID: {0}", Calendar.GetString(CalendarStringsEnum.ProductId));
                    TextWriterColor.Write("Calendar UUID:       {0}", Calendar.UniqueId);

                    // Print VCalendar
                    string raw = Calendar.SaveToString();
                    TextWriterColor.WriteColor(
                        "\nRaw VCalendar\n" +
                        "---------\n"
                        , ConsoleColors.Green
                    );
                    TextWriterColor.Write(raw);
                }
                TextWriterColor.Write("Elapsed time: {0}", elapsed.Elapsed.ToString());
            }
        }
    }
}
