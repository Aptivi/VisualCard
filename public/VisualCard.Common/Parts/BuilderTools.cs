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
using VisualCard.Common.Diagnostics;
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
            return BuildArguments(partInfo.ElementTypes, partInfo.ValueType, partInfo.Arguments, extraKeyName, defaultType, defaultValue);
        }

        internal static string BuildArguments<TValue>(ValueInfo<TValue> partInfo, string defaultType, string defaultValue) =>
            BuildArguments(partInfo.ElementTypes, partInfo.ValueType, partInfo.Arguments, "", defaultType, defaultValue);

        internal static string BuildArguments(string[] elementTypes, string valueType, ArgumentInfo[] arguments, string extraKeyName, string defaultType, string defaultValue)
        {
            LoggingTools.Debug("Building {0} arguments with {1} element types [{2}], type {3}, {4}, {5}, {6}...", arguments.Length, elementTypes.Length, string.Join(", ", elementTypes), valueType, extraKeyName, defaultType, defaultValue);

            // Filter the list of types and values first
            string[] finalElementTypes = elementTypes.Where((type) => !type.Equals(defaultType, StringComparison.OrdinalIgnoreCase)).ToArray();
            string finalValue = valueType.Equals(defaultValue, StringComparison.OrdinalIgnoreCase) ? "" : valueType;
            LoggingTools.Debug("{0} element types after processing [{1}], final value {2}", finalElementTypes.Length, string.Join(", ", finalElementTypes), finalValue);

            // Check to see if we've been provided arguments
            bool noSemicolon = arguments.Length == 0 && finalElementTypes.Length == 0 && string.IsNullOrEmpty(finalValue);
            LoggingTools.Debug("Caller provided arguments? {0}", !noSemicolon);
            if (noSemicolon)
            {
                LoggingTools.Debug("Returning with extra key name and argument delimiter: {0}", extraKeyName + CommonConstants._argumentDelimiter.ToString());
                return extraKeyName + CommonConstants._argumentDelimiter.ToString();
            }

            // Now, initialize the argument builder
            StringBuilder argumentsBuilder = new(extraKeyName + CommonConstants._fieldDelimiter.ToString());
            bool installArguments = arguments.Length > 0;
            LoggingTools.Debug("Install {0} arguments? {1}", arguments.Length, installArguments);

            // Install the remaining arguments if they exist and contain keys and values
            if (installArguments)
            {
                List<string> finalArguments = [];
                foreach (var arg in arguments)
                {
                    string args = arg.BuildArguments();
                    LoggingTools.Debug("Installing argument {0}...", args);
                    finalArguments.Add(args);
                }
                argumentsBuilder.Append(string.Join(CommonConstants._fieldDelimiter.ToString(), finalArguments));
                LoggingTools.Info("Installed {0} arguments", arguments.Length);
            }

            // We've reached the end.
            LoggingTools.Debug("Adding delimiter...");
            argumentsBuilder.Append(CommonConstants._argumentDelimiter.ToString());
            return CommonTools.MakeStringBlock(argumentsBuilder.ToString());
        }
    }
}
