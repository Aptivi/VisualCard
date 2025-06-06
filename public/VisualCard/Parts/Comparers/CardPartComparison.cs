﻿//
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
using System.Linq;
using VisualCard.Common.Diagnostics;
using VisualCard.Common.Parts;
using VisualCard.Common.Parts.Comparers;
using VisualCard.Parts.Enums;

namespace VisualCard.Parts.Comparers
{
    internal static class CardPartComparison
    {
        internal static bool PartsArrayEnumEqual(
            IDictionary<CardPartsArrayEnum, List<BaseCardPartInfo>> source,
            IDictionary<CardPartsArrayEnum, List<BaseCardPartInfo>> target)
        {
            // Verify the dictionaries
            if (!CommonComparison.VerifyDicts(source, target))
                return false;

            // If they are really equal using the equals operator, return true.
            if (source == target)
                return true;

            // Now, test the equality
            bool equal = source.All(kvp =>
            {
                bool exists = target.TryGetValue(kvp.Key, out var parts);
                if (!exists)
                    return false;

                // Compare between the lists
                return CommonComparison.CompareLists(kvp.Value, parts);
            });
            LoggingTools.Info("As a result, equal is {0}", equal);
            return equal;
        }
        
        internal static bool StringsEqual(
            IDictionary<CardStringsEnum, List<ValueInfo<string>>> source,
            IDictionary<CardStringsEnum, List<ValueInfo<string>>> target)
        {
            // Verify the dictionaries
            if (!CommonComparison.VerifyDicts(source, target))
                return false;

            // If they are really equal using the equals operator, return true.
            if (source == target)
                return true;

            // Now, test the equality
            bool equal = source.All(kvp =>
            {
                bool exists = target.TryGetValue(kvp.Key, out var parts);
                if (!exists)
                    return false;

                // Compare between the lists
                return CommonComparison.CompareLists(kvp.Value, parts);
            });
            LoggingTools.Info("As a result, equal is {0}", equal);
            return equal;
        }
    }
}
