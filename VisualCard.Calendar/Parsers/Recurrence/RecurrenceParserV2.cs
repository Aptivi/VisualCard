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

namespace VisualCard.Calendar.Parsers.Recurrence
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
        public static RecurrenceRule[] ParseRuleV2(string rule)
        {
            List<RecurrenceRule> rules = [];

            // Sanity check
            if (string.IsNullOrEmpty(rule))
                throw new ArgumentNullException(nameof(rule), "There is no rule.");

            // Return the rules
            // TODO: Fill this up!
            return [.. rules];
        }
    }
}
