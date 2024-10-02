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
using System.Linq;
using System.Text;
using VisualCard.Parsers;
using VisualCard.Parts.Implementations;

namespace VisualCard.Parts
{
    internal static class CardBuilderTools
    {
        internal static string BuildArguments(BaseCardPartInfo partInfo, Version cardVersion, string defaultType, string defaultValue)
        {
            // Filter the list of types and values first
            string[] finalElementTypes = partInfo.ElementTypes.Where((type) => !type.Equals(defaultType, StringComparison.OrdinalIgnoreCase)).ToArray();
            string finalValue = partInfo.ValueType.Equals(defaultValue, StringComparison.OrdinalIgnoreCase) ? "" : partInfo.ValueType;

            // Check to see if we've been provided arguments
            bool installAltId = partInfo.AltId >= 0 && partInfo.Arguments.Length > 0 && cardVersion.Major >= 4;
            bool noSemicolon = partInfo.AltId < 0 && partInfo.Arguments.Length == 0 && finalElementTypes.Length == 0 && string.IsNullOrEmpty(finalValue);
            string extraKeyName =
                (partInfo is XNameInfo xName ? xName.XKeyName :
                 partInfo is ExtraInfo exName ? exName.KeyName : "") ?? "";
            if (noSemicolon)
                return extraKeyName + VcardConstants._argumentDelimiter.ToString();

            // Now, initialize the argument builder
            StringBuilder argumentsBuilder = new(extraKeyName + VcardConstants._fieldDelimiter.ToString());
            bool installArguments = partInfo.Arguments.Length > 0;
            bool installElementTypes = finalElementTypes.Length > 0;
            bool installValueType = !string.IsNullOrEmpty(finalValue);

            // First, install the AltId parameter if it exists
            if (installAltId)
            {
                argumentsBuilder.Append(VcardConstants._altIdArgumentSpecifier + partInfo.AltId);
                noSemicolon = !installArguments && !installElementTypes && !installValueType;
                if (noSemicolon)
                {
                    argumentsBuilder.Append(VcardConstants._argumentDelimiter.ToString());
                    return argumentsBuilder.ToString();
                }
                else
                    argumentsBuilder.Append(VcardConstants._fieldDelimiter.ToString());
            }

            // Then, install the element types parameter if it exists
            if (installElementTypes)
            {
                argumentsBuilder.Append(VcardConstants._typeArgumentSpecifier + string.Join(",", finalElementTypes));
                noSemicolon = !installArguments && !installValueType;
                if (noSemicolon)
                {
                    argumentsBuilder.Append(VcardConstants._argumentDelimiter.ToString());
                    return argumentsBuilder.ToString();
                }
                else
                    argumentsBuilder.Append(VcardConstants._fieldDelimiter.ToString());
            }

            // Then, install the value type parameter if it exists
            if (installValueType)
            {
                argumentsBuilder.Append(VcardConstants._valueArgumentSpecifier + string.Join(",", finalValue));
                noSemicolon = !installArguments;
                if (noSemicolon)
                {
                    argumentsBuilder.Append(VcardConstants._argumentDelimiter.ToString());
                    return argumentsBuilder.ToString();
                }
                else
                    argumentsBuilder.Append(VcardConstants._fieldDelimiter.ToString());
            }

            // Finally, install the remaining arguments if they exist and contain keys and values
            if (installArguments)
            {
                string[] finalArguments = partInfo.Arguments
                    .Where((arg) => !string.IsNullOrWhiteSpace(arg.Value))
                    .Select((arg) => $"{arg.Key}={(arg.CaseSensitive ? $"\"{arg.Value}\"" : arg.Value)}").ToArray();
                argumentsBuilder.Append(string.Join(VcardConstants._fieldDelimiter.ToString(), finalArguments));
            }

            // We've reached the end.
            argumentsBuilder.Append(VcardConstants._argumentDelimiter.ToString());
            return argumentsBuilder.ToString();
        }
    }
}
