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
using System.Text;
using VisualCard.Calendar.Parsers;
using VisualCard.Calendar.Parts.Implementations;

namespace VisualCard.Calendar.Parts
{
    internal static class CalendarBuilderTools
    {
        internal static string BuildArguments(BaseCalendarPartInfo partInfo, string defaultType, string defaultValue)
        {
            // Filter the list of types and values first
            string valueType = partInfo.ValueType ?? "";
            var valueArguments = partInfo.Arguments ?? [];
            string[] finalElementTypes = partInfo.ElementTypes.Where((type) => !type.Equals(defaultType, StringComparison.OrdinalIgnoreCase)).ToArray();
            string finalValue = valueType.Equals(defaultValue, StringComparison.OrdinalIgnoreCase) ? "" : valueType;

            // Check to see if we've been provided arguments
            bool noSemicolon = valueArguments.Length == 0 && finalElementTypes.Length == 0 && string.IsNullOrEmpty(finalValue);
            string extraKeyName =
                (partInfo is XNameInfo xName ? xName.XKeyName :
                 partInfo is ExtraInfo exName ? exName.KeyName : "") ?? "";
            if (noSemicolon)
                return extraKeyName + VCalendarConstants._argumentDelimiter.ToString();

            // Now, initialize the argument builder
            StringBuilder argumentsBuilder = new(extraKeyName + VCalendarConstants._fieldDelimiter.ToString());
            bool installArguments = valueArguments.Length > 0;
            bool installElementTypes = finalElementTypes.Length > 0;
            bool installValueType = !string.IsNullOrEmpty(finalValue);

            // Install the element types parameter if it exists
            if (installElementTypes)
            {
                argumentsBuilder.Append(VCalendarConstants._typeArgumentSpecifier + string.Join(",", finalElementTypes));
                noSemicolon = !installArguments && !installValueType;
                if (noSemicolon)
                {
                    argumentsBuilder.Append(VCalendarConstants._argumentDelimiter.ToString());
                    return argumentsBuilder.ToString();
                }
                else
                    argumentsBuilder.Append(VCalendarConstants._fieldDelimiter.ToString());
            }

            // Then, install the value type parameter if it exists
            if (installValueType)
            {
                argumentsBuilder.Append(VCalendarConstants._valueArgumentSpecifier + string.Join(",", finalValue));
                noSemicolon = !installArguments;
                if (noSemicolon)
                {
                    argumentsBuilder.Append(VCalendarConstants._argumentDelimiter.ToString());
                    return argumentsBuilder.ToString();
                }
                else
                    argumentsBuilder.Append(VCalendarConstants._fieldDelimiter.ToString());
            }

            // Finally, install the remaining arguments if they exist and contain keys and values
            if (installArguments)
            {
                List<string> finalArguments = [];
                foreach (var arg in valueArguments)
                    finalArguments.Add(arg.BuildArguments());
                argumentsBuilder.Append(string.Join(VCalendarConstants._fieldDelimiter.ToString(), finalArguments));
            }

            // We've reached the end.
            argumentsBuilder.Append(VCalendarConstants._argumentDelimiter.ToString());
            return argumentsBuilder.ToString();
        }
    }
}
