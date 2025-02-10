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

using System.Collections.Generic;
using System.Linq;

namespace VisualCard.Common.Parts.Comparers
{
    internal static class CommonComparison
    {
        internal static bool CompareLists<TValue>(
            IList<TValue> source,
            IList<TValue> target)
        {
            // Verify the lists
            if (!VerifyLists(source, target))
                return false;

            // Now, compare between two parts
            List<bool> results = [];
            for (int i = 0; i < target.Count; i++)
            {
                TValue sourcePart = source[i];
                TValue targetPart = target[i];
                bool equals = sourcePart?.Equals(targetPart) ?? false;
                results.Add(equals);
            }
            return !results.Contains(false);
        }

        internal static bool ContainsAll<TValue>(
            IEnumerable<TValue> source,
            IEnumerable<TValue> target)
        {
            // Check to see if the target list contains all source elements
            List<bool> results = [];
            for (int i = 0; i < target.Count(); i++)
            {
                TValue targetPart = target.ElementAt(i);
                bool equals = source?.Contains(targetPart) ?? false;
                results.Add(equals);
            }
            return !results.Contains(false);
        }

        internal static bool VerifyLists<TValue>(
            IList<TValue> source,
            IList<TValue> target)
        {
            if (source == null || target == null)
                return false;

            if (source.Count != target.Count)
                return false;
            return true;
        }

        internal static bool VerifyDicts<TKey, TValue>(
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
