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
using VisualCard.Calendar.Parts.Enums;

namespace VisualCard.Calendar.Parts.Comparers
{
    internal static class PartComparison
    {
        internal static bool PartsArrayEnumEqual(
            IDictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> source,
            IDictionary<CalendarPartsArrayEnum, List<BaseCalendarPartInfo>> target)
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
                bool exists = target.TryGetValue(kvp.Key, out List<BaseCalendarPartInfo> parts);
                if (!exists)
                    return false;

                // Verify the lists
                if (!VerifyLists(kvp.Value, parts))
                    return false;

                // Now, compare between two parts
                List<bool> results = [];
                for (int i = 0; i < parts.Count; i++)
                {
                    BaseCalendarPartInfo sourcePart = kvp.Value[i];
                    BaseCalendarPartInfo targetPart = parts[i];
                    bool equals = sourcePart == targetPart;
                    results.Add(equals);
                }
                return !results.Contains(false);
            });
            return equal;
        }

        internal static bool StringsEqual(
            IDictionary<CalendarStringsEnum, string> source,
            IDictionary<CalendarStringsEnum, string> target)
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
                bool exists = target.TryGetValue(kvp.Key, out string part);
                bool partsEqual = kvp.Value == part;
                return exists && partsEqual;
            });
            return equal;
        }

        internal static bool IntegersEqual(
            IDictionary<CalendarIntegersEnum, double> source,
            IDictionary<CalendarIntegersEnum, double> target)
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
                bool exists = target.TryGetValue(kvp.Key, out double part);
                bool partsEqual = kvp.Value == part;
                return exists && partsEqual;
            });
            return equal;
        }

        internal static bool CompareCalendarComponents<TComponent>(
            IList<TComponent> source,
            IList<TComponent> target)
            where TComponent : Calendar
        {
            if (!VerifyLists(source, target))
                return false;

            // If they are really equal using the equals operator, return true.
            if (source == target)
                return true;

            // Now, test the equality
            List<bool> results = [];
            for (int i = 0; i < source.Count; i++)
            {
                TComponent sourcePart = source[i];
                TComponent targetPart = target[i];
                bool equals = sourcePart == targetPart;
                results.Add(equals);
            }
            return !results.Contains(false);
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
