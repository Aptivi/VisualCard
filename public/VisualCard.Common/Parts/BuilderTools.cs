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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VisualCard.Common.Parsers;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts.Implementations;

namespace VisualCard.Common.Parts
{
    internal static class BuilderTools
    {
        internal static string BuildArguments(BasePartInfo partInfo, string defaultType, string defaultValue)
        {
            string extraKeyName =
                (partInfo is XNameInfo xName ? xName.XKeyName :
                 partInfo is ExtraInfo exName ? exName.KeyName : "") ?? "";
            return BuildArguments(partInfo.ElementTypes, partInfo.ValueType, partInfo.Property, extraKeyName, defaultType, defaultValue);
        }

        internal static string BuildArguments<TValue>(ValueInfo<TValue> partInfo, string defaultType, string defaultValue) =>
            BuildArguments(partInfo.ElementTypes, partInfo.ValueType, partInfo.Property, "", defaultType, defaultValue);

        internal static string BuildArguments(string[] elementTypes, string valueType, PropertyInfo? property, string extraKeyName, string defaultType, string defaultValue)
        {
            var arguments = property?.Arguments ?? [];

            // Filter the list of types and values first
            string[] finalElementTypes = elementTypes.Where((type) => !type.Equals(defaultType, StringComparison.OrdinalIgnoreCase)).ToArray();
            string finalValue = valueType.Equals(defaultValue, StringComparison.OrdinalIgnoreCase) ? "" : valueType;

            // Check to see if we've been provided arguments
            bool noSemicolon = arguments.Length == 0 && finalElementTypes.Length == 0 && string.IsNullOrEmpty(finalValue);
            if (noSemicolon)
                return extraKeyName + CommonConstants._argumentDelimiter.ToString();

            // Now, initialize the argument builder
            StringBuilder argumentsBuilder = new(extraKeyName + CommonConstants._fieldDelimiter.ToString());
            bool installArguments = arguments.Length > 0;

            // Install the remaining arguments if they exist and contain keys and values
            if (installArguments)
            {
                List<string> finalArguments = [];
                foreach (var arg in arguments)
                    finalArguments.Add(arg.BuildArguments());
                argumentsBuilder.Append(string.Join(CommonConstants._fieldDelimiter.ToString(), finalArguments));
            }

            // We've reached the end.
            argumentsBuilder.Append(CommonConstants._argumentDelimiter.ToString());
            return CommonTools.MakeStringBlock(argumentsBuilder.ToString());
        }
    }
}
