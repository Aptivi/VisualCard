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
using VisualCard.Parsers.Arguments;
using VisualCard.Parts;
using VisualCard.Parts.Enums;

namespace VisualCard.Parsers
{
    internal class VcardPartType
    {
        internal readonly PartType type;
        internal readonly object enumeration;
        internal readonly PartCardinality cardinality;
        internal readonly Func<Version, bool> minimumVersionCondition = (_) => true;
        internal readonly Type? enumType;
        internal readonly Func<string, PropertyInfo, int, string[], string, Version, BaseCardPartInfo>? fromStringFunc;
        internal readonly string defaultType = "";
        internal readonly string defaultValue = "";
        internal readonly string defaultValueType = "";
        internal readonly string[] allowedExtraTypes = [];
        internal readonly string[] allowedValues = [];

        internal VcardPartType(PartType type, object enumeration, PartCardinality cardinality, Func<Version, bool>? minimumVersionCondition, Type? enumType, Func<string, PropertyInfo, int, string[], string, Version, BaseCardPartInfo>? fromStringFunc, string defaultType, string defaultValue, string defaultValueType, string[] allowedExtraTypes, string[] allowedValues)
        {
            this.type = type;
            this.enumeration = enumeration;
            this.cardinality = cardinality;
            this.minimumVersionCondition = minimumVersionCondition ?? new((_) => true);
            this.enumType = enumType;
            this.fromStringFunc = fromStringFunc;
            this.defaultType = defaultType;
            this.defaultValue = defaultValue;
            this.defaultValueType = defaultValueType;
            this.allowedExtraTypes = allowedExtraTypes;
            this.allowedValues = allowedValues;
        }
    }
}
