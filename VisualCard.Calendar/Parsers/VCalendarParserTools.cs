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
using System;
using System.IO;
using System.Linq;
using System.Text;
using VisualCard.Calendar.Parts;
using VisualCard.Calendar.Parts.Implementations;
using VisualCard.Calendar.Parts.Enums;
using VisualCard.Calendar.Parts.Implementations.Event;
using VisualCard.Parts.Enums;

namespace VisualCard.Calendar.Parsers
{
    internal class VCalendarParserTools
    {
#pragma warning disable IDE0060
        internal static bool StringSupported(CalendarStringsEnum stringsEnum, Version calendarVersion) =>
            stringsEnum switch
            {
                CalendarStringsEnum.ProductId => true,
                CalendarStringsEnum.Uid => true,
                CalendarStringsEnum.Organizer => true,
                CalendarStringsEnum.Status => true,
                CalendarStringsEnum.Summary => true,
                CalendarStringsEnum.Description => true,
                CalendarStringsEnum.CalScale => calendarVersion.Major == 2,
                CalendarStringsEnum.Method => calendarVersion.Major == 2,
                _ =>
                    throw new InvalidOperationException("Invalid string enumeration type to get supported value"),
            };

        internal static bool EnumArrayTypeSupported(CalendarPartsArrayEnum partsArrayEnum, Version calendarVersion) =>
            partsArrayEnum switch
            {
                CalendarPartsArrayEnum.DateStart => true,
                CalendarPartsArrayEnum.DateEnd => true,
                CalendarPartsArrayEnum.DateStamp => calendarVersion.Major == 2,
                CalendarPartsArrayEnum.Categories => true,
                CalendarPartsArrayEnum.NonstandardNames => true,
                _ =>
                    throw new InvalidOperationException("Invalid parts array enumeration type to get supported value"),
            };

        internal static string GetPrefixFromStringsEnum(CalendarStringsEnum stringsEnum) =>
            stringsEnum switch
            {
                CalendarStringsEnum.ProductId => VCalendarConstants._productIdSpecifier,
                CalendarStringsEnum.Uid => VCalendarConstants._uidSpecifier,
                CalendarStringsEnum.Organizer => VCalendarConstants._organizerSpecifier,
                CalendarStringsEnum.Status => VCalendarConstants._statusSpecifier,
                CalendarStringsEnum.Summary => VCalendarConstants._summarySpecifier,
                CalendarStringsEnum.Description => VCalendarConstants._descriptionSpecifier,
                CalendarStringsEnum.CalScale => VCalendarConstants._calScaleSpecifier,
                CalendarStringsEnum.Method => VCalendarConstants._methodSpecifier,
                _ =>
                    throw new NotImplementedException($"String enumeration {stringsEnum} is not implemented.")
            };

        internal static string GetPrefixFromPartsArrayEnum(CalendarPartsArrayEnum partsArrayEnum) =>
            partsArrayEnum switch
            {
                CalendarPartsArrayEnum.DateStart => VCalendarConstants._dateStartSpecifier,
                CalendarPartsArrayEnum.DateEnd => VCalendarConstants._dateEndSpecifier,
                CalendarPartsArrayEnum.DateStamp => VCalendarConstants._dateStampSpecifier,
                CalendarPartsArrayEnum.Categories => VCalendarConstants._categoriesSpecifier,
                _ =>
                    throw new NotImplementedException($"String enumeration {partsArrayEnum} is not implemented.")
            };

        internal static (CalendarPartsArrayEnum, PartCardinality) GetPartsArrayEnumFromType(Type partsArrayType, Version calendarVersion)
        {
            if (partsArrayType == null)
                throw new NotImplementedException("Type is not provided.");

            // Now, iterate through every type
            if (partsArrayType == typeof(DateStartInfo))
                return (CalendarPartsArrayEnum.DateStart, PartCardinality.ShouldBeOne);
            else if (partsArrayType == typeof(DateEndInfo))
                return (CalendarPartsArrayEnum.DateEnd, PartCardinality.MayBeOne);
            else if (partsArrayType == typeof(DateStampInfo))
                return (CalendarPartsArrayEnum.DateStamp, PartCardinality.MayBeOne);
            else if (partsArrayType == typeof(CategoriesInfo))
                return (CalendarPartsArrayEnum.Categories, PartCardinality.MayBeOne);
            throw new NotImplementedException($"Type {partsArrayType.Name} doesn't represent any part array.");
        }

        internal static (PartType type, object enumeration, Type enumType, Func<string, string[], string[], string, Version, BaseCalendarPartInfo> fromStringFunc, string defaultType, string defaultValue, string[] allowedExtraTypes) GetPartType(string prefix) =>
            prefix switch
            {
                VCalendarConstants._dateStartSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.DateStart, typeof(DateStartInfo), DateStartInfo.FromStringVcalendarStatic, "", "", []),
                VCalendarConstants._dateEndSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.DateEnd, typeof(DateEndInfo), DateEndInfo.FromStringVcalendarStatic, "", "", []),
                VCalendarConstants._dateStampSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.DateStamp, typeof(DateStampInfo), DateStampInfo.FromStringVcalendarStatic, "", "", []),
                VCalendarConstants._categoriesSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.Categories, typeof(CategoriesInfo), CategoriesInfo.FromStringVcalendarStatic, "", "", []),
                VCalendarConstants._xSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.NonstandardNames, typeof(XNameInfo), XNameInfo.FromStringVcalendarStatic, "", "", []),
                VCalendarConstants._productIdSpecifier => (PartType.Strings, CalendarStringsEnum.ProductId, null, null, "", "", []),
                VCalendarConstants._uidSpecifier => (PartType.Strings, CalendarStringsEnum.Uid, null, null, "", "", []),
                VCalendarConstants._organizerSpecifier => (PartType.Strings, CalendarStringsEnum.Organizer, null, null, "", "", []),
                VCalendarConstants._statusSpecifier => (PartType.Strings, CalendarStringsEnum.Status, null, null, "", "", []),
                VCalendarConstants._summarySpecifier => (PartType.Strings, CalendarStringsEnum.Summary, null, null, "", "", []),
                VCalendarConstants._descriptionSpecifier => (PartType.Strings, CalendarStringsEnum.Description, null, null, "", "", []),
                VCalendarConstants._calScaleSpecifier => (PartType.Strings, CalendarStringsEnum.CalScale, null, null, "", "", []),
                VCalendarConstants._methodSpecifier => (PartType.Strings, CalendarStringsEnum.Method, null, null, "", "", []),
                _ =>
                    throw new InvalidOperationException($"Unknown prefix {prefix}"),
            };
#pragma warning restore IDE0060
    }
}
