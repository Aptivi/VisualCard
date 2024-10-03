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
using VisualCard.Parsers;
using VisualCard.Parsers.Arguments;
using VisualCard.Parts.Implementations;

namespace VisualCard.Parts
{
    internal static class CardBuilderTools
    {
        internal static string BuildArguments(BaseCardPartInfo partInfo, string defaultType, string defaultValue)
        {
            string extraKeyName =
                (partInfo is XNameInfo xName ? xName.XKeyName :
                 partInfo is ExtraInfo exName ? exName.KeyName : "") ?? "";
            return BuildArguments(partInfo.ElementTypes, partInfo.ValueType, partInfo.Arguments, extraKeyName, defaultType, defaultValue);
        }

        internal static string BuildArguments<TValue>(CardValueInfo<TValue> partInfo, string defaultType, string defaultValue) =>
            BuildArguments(partInfo.ElementTypes, partInfo.ValueType, partInfo.Arguments, "", defaultType, defaultValue);

        internal static string BuildArguments(string[] elementTypes, string valueType, ArgumentInfo[] arguments, string extraKeyName, string defaultType, string defaultValue)
        {
            // Filter the list of types and values first
            string[] finalElementTypes = elementTypes.Where((type) => !type.Equals(defaultType, StringComparison.OrdinalIgnoreCase)).ToArray();
            string finalValue = valueType.Equals(defaultValue, StringComparison.OrdinalIgnoreCase) ? "" : valueType;

            // Check to see if we've been provided arguments
            bool noSemicolon = arguments.Length == 0 && finalElementTypes.Length == 0 && string.IsNullOrEmpty(finalValue);
            if (noSemicolon)
                return extraKeyName + VcardConstants._argumentDelimiter.ToString();

            // Now, initialize the argument builder
            StringBuilder argumentsBuilder = new(extraKeyName + VcardConstants._fieldDelimiter.ToString());
            bool installArguments = arguments.Length > 0;

            // Install the remaining arguments if they exist and contain keys and values
            if (installArguments)
            {
                List<string> finalArguments = [];
                foreach (var arg in arguments)
                    finalArguments.Add(arg.BuildArguments());
                argumentsBuilder.Append(string.Join(VcardConstants._fieldDelimiter.ToString(), finalArguments));
            }

            // We've reached the end.
            argumentsBuilder.Append(VcardConstants._argumentDelimiter.ToString());
            return VcardCommonTools.MakeStringBlock(argumentsBuilder.ToString());
        }
    }
}
