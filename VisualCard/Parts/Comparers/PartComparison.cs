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
using System.Linq;
using VisualCard.Parts.Enums;

namespace VisualCard.Parts.Comparers
{
    internal static class PartComparison
    {
        internal static bool PartsArrayEnumEqual(
            IDictionary<PartsArrayEnum, List<BaseCardPartInfo>> source,
            IDictionary<PartsArrayEnum, List<BaseCardPartInfo>> target)
        {
            // Verify the dictionaries
            if (!VerifyDicts(source, target))
                return false;

            // If they are really equal using the equals operator, return true.
            if (source == target)
                return true;

            // Now, test the equality
            bool equal = source.All(kvp =>
            {
                bool exists = target.TryGetValue(kvp.Key, out List<BaseCardPartInfo> parts);
                if (!exists)
                    return false;

                // Verify the lists
                if (!VerifyLists(kvp.Value, parts))
                    return false;

                // Now, compare between two parts
                List<bool> results = [];
                for (int i = 0; i < parts.Count; i++)
                {
                    BaseCardPartInfo sourcePart = kvp.Value[i];
                    BaseCardPartInfo targetPart = parts[i];
                    bool equals = sourcePart == targetPart;
                    results.Add(equals);
                }
                return !results.Contains(false);
            });
            return equal;
        }
        
        internal static bool StringsEqual(
            IDictionary<StringsEnum, List<CardValueInfo<string>>> source,
            IDictionary<StringsEnum, List<CardValueInfo<string>>> target)
        {
            // Verify the dictionaries
            if (!VerifyDicts(source, target))
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

                // Verify the lists
                if (!VerifyLists(kvp.Value, parts))
                    return false;

                // Now, compare between two parts
                List<bool> results = [];
                for (int i = 0; i < parts.Count; i++)
                {
                    CardValueInfo<string> sourcePart = kvp.Value[i];
                    CardValueInfo<string> targetPart = parts[i];
                    bool equals = sourcePart == targetPart;
                    results.Add(equals);
                }
                return !results.Contains(false);
            });
            return equal;
        }

        private static bool VerifyLists<TValue>(
            IList<TValue> source,
            IList<TValue> target)
        {
            if (source == null || target == null)
                return false;

            if (source.Count != target.Count)
                return false;
            return true;
        }

        private static bool VerifyDicts<TKey, TValue>(
            IDictionary<TKey, TValue> source,
            IDictionary<TKey, TValue> target)
        {
            if (source == null || target == null)
                return false;

            if (source.Count != target.Count)
                return false;
            return true;
        }
    }
}
