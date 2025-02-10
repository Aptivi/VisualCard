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

using VisualCard.Common.Parts.Implementations;
using VisualCard.Parts;

namespace VisualCard.Calendar.Parts
{
    internal static class CalendarBuilderTools
    {
        internal static string BuildArguments(BaseCalendarPartInfo partInfo, string defaultType, string defaultValue)
        {
            string extraKeyName =
                (partInfo is XNameInfo xName ? xName.XKeyName :
                 partInfo is ExtraInfo exName ? exName.KeyName : "") ?? "";
            return CardBuilderTools.BuildArguments(partInfo.ElementTypes, partInfo.ValueType, partInfo.Property, extraKeyName, defaultType, defaultValue);
        }

        internal static string BuildArguments<TValue>(ValueInfo<TValue> partInfo, string defaultType, string defaultValue) =>
            CardBuilderTools.BuildArguments(partInfo.ElementTypes, partInfo.ValueType, partInfo.Property, "", defaultType, defaultValue);
    }
}
