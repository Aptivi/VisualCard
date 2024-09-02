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
                CalendarStringsEnum.CalScale => calendarVersion.Major == 2,
                CalendarStringsEnum.Method => calendarVersion.Major == 2,
                CalendarStringsEnum.Class => true,
                CalendarStringsEnum.Uid => true,
                CalendarStringsEnum.Organizer => true,
                CalendarStringsEnum.Status => true,
                CalendarStringsEnum.Summary => true,
                CalendarStringsEnum.Description => true,
                CalendarStringsEnum.Transparency => true,
                _ =>
                    throw new InvalidOperationException("Invalid string enumeration type to get supported value"),
            };
        
        internal static bool IntegerSupported(CalendarIntegersEnum integersEnum, Version calendarVersion) =>
            integersEnum switch
            {
                CalendarIntegersEnum.Priority => calendarVersion.Major == 2,
                CalendarIntegersEnum.Sequence => true,
                _ =>
                    throw new InvalidOperationException("Invalid integer enumeration type to get supported value"),
            };

        internal static bool EnumArrayTypeSupported(CalendarPartsArrayEnum partsArrayEnum, Version calendarVersion) =>
            partsArrayEnum switch
            {
                CalendarPartsArrayEnum.Attach => true,
                CalendarPartsArrayEnum.Categories => true,
                CalendarPartsArrayEnum.Comment => calendarVersion.Major == 2,
                CalendarPartsArrayEnum.Geography => true,
                CalendarPartsArrayEnum.Location => calendarVersion.Major == 2,
                CalendarPartsArrayEnum.Resources => true,
                CalendarPartsArrayEnum.Attendee => true,
                CalendarPartsArrayEnum.DateCreated => true,
                CalendarPartsArrayEnum.DateCreatedAlt => true,
                CalendarPartsArrayEnum.DateStart => true,
                CalendarPartsArrayEnum.DateEnd => true,
                CalendarPartsArrayEnum.NonstandardNames => true,
                CalendarPartsArrayEnum.DateStamp => calendarVersion.Major == 2,
                _ =>
                    throw new InvalidOperationException("Invalid parts array enumeration type to get supported value"),
            };

        internal static string GetPrefixFromStringsEnum(CalendarStringsEnum stringsEnum) =>
            stringsEnum switch
            {
                CalendarStringsEnum.ProductId => VCalendarConstants._productIdSpecifier,
                CalendarStringsEnum.CalScale => VCalendarConstants._calScaleSpecifier,
                CalendarStringsEnum.Method => VCalendarConstants._methodSpecifier,
                CalendarStringsEnum.Class => VCalendarConstants._classSpecifier,
                CalendarStringsEnum.Uid => VCalendarConstants._uidSpecifier,
                CalendarStringsEnum.Organizer => VCalendarConstants._organizerSpecifier,
                CalendarStringsEnum.Status => VCalendarConstants._statusSpecifier,
                CalendarStringsEnum.Summary => VCalendarConstants._summarySpecifier,
                CalendarStringsEnum.Description => VCalendarConstants._descriptionSpecifier,
                CalendarStringsEnum.Transparency => VCalendarConstants._transparencySpecifier,
                _ =>
                    throw new NotImplementedException($"String enumeration {stringsEnum} is not implemented.")
            };

        internal static string GetPrefixFromIntegersEnum(CalendarIntegersEnum integersEnum) =>
            integersEnum switch
            {
                CalendarIntegersEnum.Priority => VCalendarConstants._prioritySpecifier,
                CalendarIntegersEnum.Sequence => VCalendarConstants._sequenceSpecifier,
                _ =>
                    throw new NotImplementedException($"Integer enumeration {integersEnum} is not implemented.")
            };

        internal static string GetPrefixFromPartsArrayEnum(CalendarPartsArrayEnum partsArrayEnum) =>
            partsArrayEnum switch
            {
                CalendarPartsArrayEnum.Attach => VCalendarConstants._attachSpecifier,
                CalendarPartsArrayEnum.Categories => VCalendarConstants._categoriesSpecifier,
                CalendarPartsArrayEnum.Comment => VCalendarConstants._commentSpecifier,
                CalendarPartsArrayEnum.Geography => VCalendarConstants._geoSpecifier,
                CalendarPartsArrayEnum.Location => VCalendarConstants._locationSpecifier,
                CalendarPartsArrayEnum.Resources => VCalendarConstants._resourcesSpecifier,
                CalendarPartsArrayEnum.Attendee => VCalendarConstants._attendeeSpecifier,
                CalendarPartsArrayEnum.DateCreated => VCalendarConstants._createdSpecifier,
                CalendarPartsArrayEnum.DateCreatedAlt => VCalendarConstants._created1Specifier,
                CalendarPartsArrayEnum.DateStart => VCalendarConstants._dateStartSpecifier,
                CalendarPartsArrayEnum.DateEnd => VCalendarConstants._dateEndSpecifier,
                CalendarPartsArrayEnum.DateStamp => VCalendarConstants._dateStampSpecifier,
                _ =>
                    throw new NotImplementedException($"Array enumeration {partsArrayEnum} is not implemented.")
            };

        internal static (CalendarPartsArrayEnum, PartCardinality) GetPartsArrayEnumFromType(Type partsArrayType, Version calendarVersion)
        {
            if (partsArrayType == null)
                throw new NotImplementedException("Type is not provided.");

            // Now, iterate through every type
            if (partsArrayType == typeof(AttachInfo))
                return (CalendarPartsArrayEnum.Attach, PartCardinality.Any);
            else if (partsArrayType == typeof(CategoriesInfo))
                return (CalendarPartsArrayEnum.Categories, PartCardinality.MayBeOne);
            else if (partsArrayType == typeof(CommentInfo))
                return (CalendarPartsArrayEnum.Comment, PartCardinality.Any);
            else if (partsArrayType == typeof(GeoInfo))
                return (CalendarPartsArrayEnum.Geography, PartCardinality.Any);
            else if (partsArrayType == typeof(LocationInfo))
                return (CalendarPartsArrayEnum.Location, PartCardinality.Any);
            else if (partsArrayType == typeof(ResourcesInfo))
                return (CalendarPartsArrayEnum.Resources, PartCardinality.MayBeOne);
            else if (partsArrayType == typeof(AttendeeInfo))
                return (CalendarPartsArrayEnum.Attendee, PartCardinality.Any);
            else if (partsArrayType == typeof(DateCreatedInfo))
                return (calendarVersion.Major == 1 ? CalendarPartsArrayEnum.DateCreatedAlt : CalendarPartsArrayEnum.DateCreated, PartCardinality.MayBeOne);
            else if (partsArrayType == typeof(DateStartInfo))
                return (CalendarPartsArrayEnum.DateStart, PartCardinality.ShouldBeOne);
            else if (partsArrayType == typeof(DateEndInfo))
                return (CalendarPartsArrayEnum.DateEnd, PartCardinality.MayBeOne);
            else if (partsArrayType == typeof(DateStampInfo))
                return (CalendarPartsArrayEnum.DateStamp, PartCardinality.MayBeOne);
            throw new NotImplementedException($"Type {partsArrayType.Name} doesn't represent any part array.");
        }

        internal static (PartType type, object enumeration, Type enumType, Func<string, string[], string[], string, Version, BaseCalendarPartInfo> fromStringFunc, string defaultType, string defaultValue, string[] allowedExtraTypes) GetPartType(string prefix) =>
            prefix switch
            {
                VCalendarConstants._attachSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.Attach, typeof(AttachInfo), AttachInfo.FromStringVcalendarStatic, "", "", []),
                VCalendarConstants._categoriesSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.Categories, typeof(CategoriesInfo), CategoriesInfo.FromStringVcalendarStatic, "", "", []),
                VCalendarConstants._commentSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.Comment, typeof(CommentInfo), CommentInfo.FromStringVcalendarStatic, "", "", []),
                VCalendarConstants._geoSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.Geography, typeof(GeoInfo), GeoInfo.FromStringVcalendarStatic, "", "", []),
                VCalendarConstants._locationSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.Location, typeof(LocationInfo), LocationInfo.FromStringVcalendarStatic, "", "", []),
                VCalendarConstants._resourcesSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.Resources, typeof(ResourcesInfo), ResourcesInfo.FromStringVcalendarStatic, "", "", []),
                VCalendarConstants._attendeeSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.Attendee, typeof(AttendeeInfo), AttendeeInfo.FromStringVcalendarStatic, "", "", []),
                VCalendarConstants._createdSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.DateCreated, typeof(DateCreatedInfo), DateCreatedInfo.FromStringVcalendarStatic, "", "", []),
                VCalendarConstants._created1Specifier => (PartType.PartsArray, CalendarPartsArrayEnum.DateCreatedAlt, typeof(DateCreatedInfo), DateCreatedInfo.FromStringVcalendarStatic, "", "", []),
                VCalendarConstants._dateStartSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.DateStart, typeof(DateStartInfo), DateStartInfo.FromStringVcalendarStatic, "", "", []),
                VCalendarConstants._dateEndSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.DateEnd, typeof(DateEndInfo), DateEndInfo.FromStringVcalendarStatic, "", "", []),
                VCalendarConstants._dateStampSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.DateStamp, typeof(DateStampInfo), DateStampInfo.FromStringVcalendarStatic, "", "", []),
                VCalendarConstants._xSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.NonstandardNames, typeof(XNameInfo), XNameInfo.FromStringVcalendarStatic, "", "", []),
                VCalendarConstants._productIdSpecifier => (PartType.Strings, CalendarStringsEnum.ProductId, null, null, "", "", []),
                VCalendarConstants._calScaleSpecifier => (PartType.Strings, CalendarStringsEnum.CalScale, null, null, "", "", []),
                VCalendarConstants._methodSpecifier => (PartType.Strings, CalendarStringsEnum.Method, null, null, "", "", []),
                VCalendarConstants._classSpecifier => (PartType.Strings, CalendarStringsEnum.Class, null, null, "", "", []),
                VCalendarConstants._uidSpecifier => (PartType.Strings, CalendarStringsEnum.Uid, null, null, "", "", []),
                VCalendarConstants._organizerSpecifier => (PartType.Strings, CalendarStringsEnum.Organizer, null, null, "", "", []),
                VCalendarConstants._statusSpecifier => (PartType.Strings, CalendarStringsEnum.Status, null, null, "", "", []),
                VCalendarConstants._summarySpecifier => (PartType.Strings, CalendarStringsEnum.Summary, null, null, "", "", []),
                VCalendarConstants._descriptionSpecifier => (PartType.Strings, CalendarStringsEnum.Description, null, null, "", "", []),
                VCalendarConstants._transparencySpecifier => (PartType.Strings, CalendarStringsEnum.Transparency, null, null, "", "", []),
                VCalendarConstants._prioritySpecifier => (PartType.Integers, CalendarIntegersEnum.Priority, null, null, "", "", []),
                VCalendarConstants._sequenceSpecifier => (PartType.Integers, CalendarIntegersEnum.Sequence, null, null, "", "", []),
                _ =>
                    throw new InvalidOperationException($"Unknown prefix {prefix}"),
            };
#pragma warning restore IDE0060
    }
}
