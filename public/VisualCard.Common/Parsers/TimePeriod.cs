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
using System.Diagnostics;
using VisualCard.Common.Diagnostics;
using VisualCard.Common.Languages;

namespace VisualCard.Common.Parsers
{
    /// <summary>
    /// Precise period of time
    /// </summary>
    [DebuggerDisplay("{StartDate} => {EndDate} in {Duration}")]
    public class TimePeriod
    {
        private DateTimeOffset startDate;
        private DateTimeOffset endDate;

        /// <summary>
        /// Start date
        /// </summary>
        public DateTimeOffset StartDate =>
            startDate;

        /// <summary>
        /// End date or a resultant date from the duration
        /// </summary>
        public DateTimeOffset EndDate =>
            endDate;

        /// <summary>
        /// Duration of the time period
        /// </summary>
        public TimeSpan Duration =>
            endDate - startDate;

        /// <summary>
        /// Makes a new TimePeriod instance
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        public TimePeriod(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            LoggingTools.Info("Start date {0}, end date {1}", startDate, endDate);
            if (startDate > endDate)
                throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_COMMON_PARSERS_TOOLS_EXCEPTION_STARTLATERTHANEND"));
            LoggingTools.Debug("Installing start and end date...");
            this.startDate = startDate;
            this.endDate = endDate;
        }
    }
}
