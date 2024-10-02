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

using System;
using VisualCard.Calendar.Parts;
using VisualCard.Calendar.Parts.Implementations;
using VisualCard.Calendar.Parts.Enums;
using VisualCard.Calendar.Parts.Implementations.Event;
using VisualCard.Parts.Enums;
using System.Linq;
using VisualCard.Calendar.Parts.Implementations.TimeZone;
using VisualCard.Calendar.Parts.Implementations.Todo;
using VisualCard.Calendar.Parts.Implementations.Legacy;
using System.IO;
using VisualCard.Calendar.Parts.Implementations.FreeBusy;
using VisualCard.Parsers.Arguments;
using VisualCard.Parsers;

namespace VisualCard.Calendar.Parsers
{
    internal class VCalendarParserTools
    {
        internal static bool StringSupported(CalendarStringsEnum stringsEnum, Version calendarVersion, Type componentType) =>
            stringsEnum switch
            {
                CalendarStringsEnum.ProductId => true,
                CalendarStringsEnum.CalScale => calendarVersion.Major == 2,
                CalendarStringsEnum.Method => calendarVersion.Major == 2,
                CalendarStringsEnum.Class => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal)),
                CalendarStringsEnum.Uid => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarFreeBusy)),
                CalendarStringsEnum.Organizer => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarFreeBusy)),
                CalendarStringsEnum.Status => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal)),
                CalendarStringsEnum.Summary => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarAlarm)),
                CalendarStringsEnum.Description => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarAlarm)),
                CalendarStringsEnum.Transparency => TypeMatch(componentType, typeof(CalendarEvent)),
                CalendarStringsEnum.Action => calendarVersion.Major == 2 && TypeMatch(componentType, typeof(CalendarAlarm)),
                CalendarStringsEnum.Trigger => calendarVersion.Major == 2 && TypeMatch(componentType, typeof(CalendarAlarm)),
                CalendarStringsEnum.TimeZoneId => calendarVersion.Major == 2 && TypeMatch(componentType, typeof(CalendarTimeZone)),
                CalendarStringsEnum.TimeZoneUrl => calendarVersion.Major == 2 && TypeMatch(componentType, typeof(CalendarTimeZone)),
                CalendarStringsEnum.Recursion => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarStandard), typeof(CalendarDaylight)),
                CalendarStringsEnum.ExRule => calendarVersion.Major == 1 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo)),
                CalendarStringsEnum.RecursionId => calendarVersion.Major == 2 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal)),
                CalendarStringsEnum.Url => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarFreeBusy)),
                CalendarStringsEnum.TimeZone => TypeMatch(componentType, typeof(Parts.Calendar)),
                CalendarStringsEnum.Comment => calendarVersion.Major == 2 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarFreeBusy), typeof(CalendarStandard), typeof(CalendarDaylight)),
                CalendarStringsEnum.Location => calendarVersion.Major == 2 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo)),
                CalendarStringsEnum.Attendee => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarFreeBusy), typeof(CalendarAlarm)),
                CalendarStringsEnum.Duration => calendarVersion.Major == 2 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarAlarm)),
                CalendarStringsEnum.TimeZoneName => calendarVersion.Major == 2 && TypeMatch(componentType, typeof(CalendarTimeZone)),
                CalendarStringsEnum.RelatedTo => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal)),
                CalendarStringsEnum.Contact => calendarVersion.Major == 2 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarFreeBusy)),
                _ =>
                    throw new InvalidOperationException("Invalid string enumeration type to get supported value"),
            };
        
        internal static bool IntegerSupported(CalendarIntegersEnum integersEnum, Version calendarVersion, Type componentType) =>
            integersEnum switch
            {
                CalendarIntegersEnum.Priority => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo)),
                CalendarIntegersEnum.Sequence => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal)),
                CalendarIntegersEnum.PercentComplete => TypeMatch(componentType, typeof(CalendarTodo)),
                CalendarIntegersEnum.Repeat => TypeMatch(componentType, typeof(CalendarAlarm)),
                CalendarIntegersEnum.RecurrTimes => calendarVersion.Major == 1 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo)),
                _ =>
                    throw new InvalidOperationException("Invalid integer enumeration type to get supported value"),
            };

        internal static bool EnumArrayTypeSupported(CalendarPartsArrayEnum partsArrayEnum, Version calendarVersion, Type componentType) =>
            partsArrayEnum switch
            {
                CalendarPartsArrayEnum.Attach => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarAlarm)),
                CalendarPartsArrayEnum.Categories => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal)),
                CalendarPartsArrayEnum.Geography => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo)),
                CalendarPartsArrayEnum.Resources => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo)),
                CalendarPartsArrayEnum.DateCreated => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal)),
                CalendarPartsArrayEnum.DateCreatedAlt => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo)),
                CalendarPartsArrayEnum.DateStart => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarFreeBusy), typeof(CalendarStandard), typeof(CalendarDaylight)),
                CalendarPartsArrayEnum.DateEnd => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarFreeBusy)),
                CalendarPartsArrayEnum.DateCompleted => TypeMatch(componentType, typeof(CalendarTodo)),
                CalendarPartsArrayEnum.DueDate => TypeMatch(componentType, typeof(CalendarTodo)),
                CalendarPartsArrayEnum.DateStamp => calendarVersion.Major == 2 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarFreeBusy)),
                CalendarPartsArrayEnum.TimeZoneOffsetFrom => calendarVersion.Major == 2 && TypeMatch(componentType, typeof(CalendarStandard), typeof(CalendarDaylight)),
                CalendarPartsArrayEnum.TimeZoneOffsetTo => calendarVersion.Major == 2 && TypeMatch(componentType, typeof(CalendarStandard), typeof(CalendarDaylight)),
                CalendarPartsArrayEnum.RecDate => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarStandard), typeof(CalendarDaylight)),
                CalendarPartsArrayEnum.ExDate => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarStandard), typeof(CalendarDaylight)),
                CalendarPartsArrayEnum.Daylight => calendarVersion.Major == 1 && TypeMatch(componentType, typeof(Parts.Calendar)),
                CalendarPartsArrayEnum.AudioAlarm => calendarVersion.Major == 1 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo)),
                CalendarPartsArrayEnum.DisplayAlarm => calendarVersion.Major == 1 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo)),
                CalendarPartsArrayEnum.MailAlarm => calendarVersion.Major == 1 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo)),
                CalendarPartsArrayEnum.ProcedureAlarm => calendarVersion.Major == 1 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo)),
                CalendarPartsArrayEnum.LastModified => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarTimeZone)),
                CalendarPartsArrayEnum.FreeBusy => calendarVersion.Major == 2 && TypeMatch(componentType, typeof(CalendarFreeBusy)),
                CalendarPartsArrayEnum.RequestStatus => calendarVersion.Major == 2 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarFreeBusy)),

                // Extensions are allowed
                _ => true,
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
                CalendarStringsEnum.Action => VCalendarConstants._actionSpecifier,
                CalendarStringsEnum.Trigger => VCalendarConstants._triggerSpecifier,
                CalendarStringsEnum.TimeZoneId => VCalendarConstants._tzidSpecifier,
                CalendarStringsEnum.TimeZoneUrl => VCalendarConstants._tzUrlSpecifier,
                CalendarStringsEnum.Recursion => VCalendarConstants._recurseSpecifier,
                CalendarStringsEnum.ExRule => VCalendarConstants._exRuleSpecifier,
                CalendarStringsEnum.RecursionId => VCalendarConstants._recurIdSpecifier,
                CalendarStringsEnum.Url => VCalendarConstants._urlSpecifier,
                CalendarStringsEnum.TimeZone => VCalendarConstants._tzSpecifier,
                CalendarStringsEnum.Comment => VCalendarConstants._commentSpecifier,
                CalendarStringsEnum.Location => VCalendarConstants._locationSpecifier,
                CalendarStringsEnum.Attendee => VCalendarConstants._attendeeSpecifier,
                CalendarStringsEnum.Duration => VCalendarConstants._durationSpecifier,
                CalendarStringsEnum.TimeZoneName => VCalendarConstants._tzNameSpecifier,
                CalendarStringsEnum.RelatedTo => VCalendarConstants._relationshipSpecifier,
                CalendarStringsEnum.Contact => VCalendarConstants._contactSpecifier,
                _ =>
                    throw new NotImplementedException($"String enumeration {stringsEnum} is not implemented.")
            };

        internal static string GetPrefixFromIntegersEnum(CalendarIntegersEnum integersEnum) =>
            integersEnum switch
            {
                CalendarIntegersEnum.Priority => VCalendarConstants._prioritySpecifier,
                CalendarIntegersEnum.Sequence => VCalendarConstants._sequenceSpecifier,
                CalendarIntegersEnum.PercentComplete => VCalendarConstants._percentCompletionSpecifier,
                CalendarIntegersEnum.Repeat => VCalendarConstants._repeatSpecifier,
                CalendarIntegersEnum.RecurrTimes => VCalendarConstants._rNumSpecifier,
                _ =>
                    throw new NotImplementedException($"Integer enumeration {integersEnum} is not implemented.")
            };

        internal static string GetPrefixFromPartsArrayEnum(CalendarPartsArrayEnum partsArrayEnum) =>
            partsArrayEnum switch
            {
                CalendarPartsArrayEnum.Attach => VCalendarConstants._attachSpecifier,
                CalendarPartsArrayEnum.Categories => VCalendarConstants._categoriesSpecifier,
                CalendarPartsArrayEnum.Geography => VCalendarConstants._geoSpecifier,
                CalendarPartsArrayEnum.Resources => VCalendarConstants._resourcesSpecifier,
                CalendarPartsArrayEnum.DateCreated => VCalendarConstants._createdSpecifier,
                CalendarPartsArrayEnum.DateCreatedAlt => VCalendarConstants._created1Specifier,
                CalendarPartsArrayEnum.DateStart => VCalendarConstants._dateStartSpecifier,
                CalendarPartsArrayEnum.DateEnd => VCalendarConstants._dateEndSpecifier,
                CalendarPartsArrayEnum.DateCompleted => VCalendarConstants._dateCompletedSpecifier,
                CalendarPartsArrayEnum.DueDate => VCalendarConstants._dueDateSpecifier,
                CalendarPartsArrayEnum.DateStamp => VCalendarConstants._dateStampSpecifier,
                CalendarPartsArrayEnum.TimeZoneOffsetFrom => VCalendarConstants._tzOffsetFromSpecifier,
                CalendarPartsArrayEnum.TimeZoneOffsetTo => VCalendarConstants._tzOffsetToSpecifier,
                CalendarPartsArrayEnum.RecDate => VCalendarConstants._recDateSpecifier,
                CalendarPartsArrayEnum.ExDate => VCalendarConstants._exDateSpecifier,
                CalendarPartsArrayEnum.Daylight => VCalendarConstants._daylightSpecifier,
                CalendarPartsArrayEnum.AudioAlarm => VCalendarConstants._aAlarmSpecifier,
                CalendarPartsArrayEnum.DisplayAlarm => VCalendarConstants._dAlarmSpecifier,
                CalendarPartsArrayEnum.MailAlarm => VCalendarConstants._mAlarmSpecifier,
                CalendarPartsArrayEnum.ProcedureAlarm => VCalendarConstants._pAlarmSpecifier,
                CalendarPartsArrayEnum.LastModified => VCalendarConstants._lastModSpecifier,
                CalendarPartsArrayEnum.FreeBusy => VCalendarConstants._freeBusySpecifier,
                CalendarPartsArrayEnum.RequestStatus => VCalendarConstants._requestStatusSpecifier,

                // Extensions are allowed
                CalendarPartsArrayEnum.NonstandardNames => VcardConstants._xSpecifier,
                _ => ""
            };

        internal static PartCardinality GetStringsEnumFromType(CalendarStringsEnum strings)
        {
            // Now, iterate through every type
            return strings switch
            {
                CalendarStringsEnum.Comment or 
                CalendarStringsEnum.Location or
                CalendarStringsEnum.Attendee or
                CalendarStringsEnum.TimeZoneName or
                CalendarStringsEnum.RelatedTo or
                CalendarStringsEnum.Contact => PartCardinality.Any,
                CalendarStringsEnum.Duration => PartCardinality.MayBeOne,
                _ => throw new ArgumentException($"There is no string enum info for {strings}"),
            };
        }

        internal static PartCardinality GetIntegersEnumFromType(CalendarIntegersEnum integers)
        {
            // Now, iterate through every type
            return integers switch
            {
                CalendarIntegersEnum.PercentComplete => PartCardinality.MayBeOne,
                CalendarIntegersEnum.Priority => PartCardinality.MayBeOne,
                CalendarIntegersEnum.RecurrTimes => PartCardinality.MayBeOne,
                CalendarIntegersEnum.Repeat => PartCardinality.MayBeOne,
                CalendarIntegersEnum.Sequence => PartCardinality.MayBeOne,
                _ => throw new ArgumentException($"There is no string enum info for {integers}"),
            };
        }

        internal static (CalendarPartsArrayEnum, PartCardinality) GetPartsArrayEnumFromType(Type partsArrayType, Version calendarVersion)
        {
            if (partsArrayType == null)
                throw new NotImplementedException("Type is not provided.");

            // Now, iterate through every type
            if (partsArrayType == typeof(AttachInfo))
                return (CalendarPartsArrayEnum.Attach, PartCardinality.Any);
            else if (partsArrayType == typeof(CategoriesInfo))
                return (CalendarPartsArrayEnum.Categories, PartCardinality.MayBeOne);
            else if (partsArrayType == typeof(GeoInfo))
                return (CalendarPartsArrayEnum.Geography, PartCardinality.Any);
            else if (partsArrayType == typeof(ResourcesInfo))
                return (CalendarPartsArrayEnum.Resources, PartCardinality.MayBeOne);
            else if (partsArrayType == typeof(DateCreatedInfo))
                return (calendarVersion.Major == 1 ? CalendarPartsArrayEnum.DateCreatedAlt : CalendarPartsArrayEnum.DateCreated, PartCardinality.MayBeOne);
            else if (partsArrayType == typeof(DateStartInfo))
                return (CalendarPartsArrayEnum.DateStart, PartCardinality.ShouldBeOne);
            else if (partsArrayType == typeof(DateEndInfo))
                return (CalendarPartsArrayEnum.DateEnd, PartCardinality.MayBeOne);
            else if (partsArrayType == typeof(DateCompletedInfo))
                return (CalendarPartsArrayEnum.DateCompleted, PartCardinality.MayBeOne);
            else if (partsArrayType == typeof(DueDateInfo))
                return (CalendarPartsArrayEnum.DueDate, PartCardinality.MayBeOne);
            else if (partsArrayType == typeof(DateStampInfo))
                return (CalendarPartsArrayEnum.DateStamp, PartCardinality.MayBeOne);
            else if (partsArrayType == typeof(TimeZoneOffsetFromInfo))
                return (CalendarPartsArrayEnum.TimeZoneOffsetFrom, PartCardinality.MayBeOne);
            else if (partsArrayType == typeof(TimeZoneOffsetToInfo))
                return (CalendarPartsArrayEnum.TimeZoneOffsetTo, PartCardinality.MayBeOne);
            else if (partsArrayType == typeof(RecDateInfo))
                return (CalendarPartsArrayEnum.RecDate, PartCardinality.Any);
            else if (partsArrayType == typeof(ExDateInfo))
                return (CalendarPartsArrayEnum.ExDate, PartCardinality.Any);
            else if (partsArrayType == typeof(DaylightInfo))
                return (CalendarPartsArrayEnum.Daylight, PartCardinality.MayBeOne);
            else if (partsArrayType == typeof(LastModifiedInfo))
                return (CalendarPartsArrayEnum.LastModified, PartCardinality.MayBeOne);
            else if (partsArrayType == typeof(CalendarFreeBusyInfo))
                return (CalendarPartsArrayEnum.FreeBusy, PartCardinality.Any);
            else if (partsArrayType == typeof(RequestStatusInfo))
                return (CalendarPartsArrayEnum.RequestStatus, PartCardinality.Any);

            // Extensions are allowed
            else if (partsArrayType == typeof(XNameInfo))
                return (CalendarPartsArrayEnum.NonstandardNames, PartCardinality.Any);
            return (CalendarPartsArrayEnum.IanaNames, PartCardinality.Any);
        }

        internal static (PartType type, object enumeration, Type? enumType, Func<string, ArgumentInfo[], string[], string, string, Version, BaseCalendarPartInfo>? fromStringFunc, string defaultType, string defaultValue, string defaultValueType, string[] allowedExtraTypes, string[] allowedValues) GetPartType(string prefix, string objectType, Version calendarVersion)
        {
            string[] allowedStatuses =
                objectType == VCalendarConstants._objectVEventSpecifier && calendarVersion.Major == 2 ? ["TENTATIVE", "CONFIRMED", "CANCELLED"] :
                objectType == VCalendarConstants._objectVEventSpecifier && calendarVersion.Major == 1 ? ["NEEDS ACTION", "SENT", "TENTATIVE", "CONFIRMED", "DECLINED", "DELEGATED"] :
                objectType == VCalendarConstants._objectVTodoSpecifier && calendarVersion.Major == 2 ? ["NEEDS-ACTION", "COMPLETED", "IN-PROCESS", "CANCELLED"] :
                objectType == VCalendarConstants._objectVTodoSpecifier && calendarVersion.Major == 1 ? ["NEEDS ACTION", "SENT", "ACCEPTED", "COMPLETED", "DECLINED", "DELEGATED"] :
                objectType == VCalendarConstants._objectVJournalSpecifier ? ["DRAFT", "FINAL", "CANCELLED"] :
                [];
            return prefix switch
            {
                VCalendarConstants._attachSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.Attach, typeof(AttachInfo), AttachInfo.FromStringVcalendarStatic, "", "", "uri", [], []),
                VCalendarConstants._categoriesSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.Categories, typeof(CategoriesInfo), CategoriesInfo.FromStringVcalendarStatic, "", "", "text", [], []),
                VCalendarConstants._geoSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.Geography, typeof(GeoInfo), GeoInfo.FromStringVcalendarStatic, "", "", "float", [], []),
                VCalendarConstants._resourcesSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.Resources, typeof(ResourcesInfo), ResourcesInfo.FromStringVcalendarStatic, "", "", "text", [], []),
                VCalendarConstants._createdSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.DateCreated, typeof(DateCreatedInfo), DateCreatedInfo.FromStringVcalendarStatic, "", "", "date-time", [], []),
                VCalendarConstants._created1Specifier => (PartType.PartsArray, CalendarPartsArrayEnum.DateCreatedAlt, typeof(DateCreatedInfo), DateCreatedInfo.FromStringVcalendarStatic, "", "", "date-time", [], []),
                VCalendarConstants._dateStartSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.DateStart, typeof(DateStartInfo), DateStartInfo.FromStringVcalendarStatic, "", "", "date-time", [], []),
                VCalendarConstants._dateEndSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.DateEnd, typeof(DateEndInfo), DateEndInfo.FromStringVcalendarStatic, "", "", "date-time", [], []),
                VCalendarConstants._dateCompletedSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.DateCompleted, typeof(DateCompletedInfo), DateCompletedInfo.FromStringVcalendarStatic, "", "", "date-time", [], []),
                VCalendarConstants._dueDateSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.DueDate, typeof(DueDateInfo), DueDateInfo.FromStringVcalendarStatic, "", "", "date-time", [], []),
                VCalendarConstants._dateStampSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.DateStamp, typeof(DateStampInfo), DateStampInfo.FromStringVcalendarStatic, "", "", "date-time", [], []),
                VCalendarConstants._tzOffsetFromSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.TimeZoneOffsetFrom, typeof(TimeZoneOffsetFromInfo), TimeZoneOffsetFromInfo.FromStringVcalendarStatic, "", "", "utc-offset", [], []),
                VCalendarConstants._tzOffsetToSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.TimeZoneOffsetTo, typeof(TimeZoneOffsetToInfo), TimeZoneOffsetToInfo.FromStringVcalendarStatic, "", "", "utc-offset", [], []),
                VCalendarConstants._recDateSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.RecDate, typeof(RecDateInfo), RecDateInfo.FromStringVcalendarStatic, "", "", "date-time", [], []),
                VCalendarConstants._exDateSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.ExDate, typeof(ExDateInfo), ExDateInfo.FromStringVcalendarStatic, "", "", "date-time", [], []),
                VCalendarConstants._daylightSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.Daylight, typeof(DaylightInfo), DaylightInfo.FromStringVcalendarStatic, "", "", "text", [], []),
                VCalendarConstants._aAlarmSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.AudioAlarm, typeof(AudioAlarmInfo), AudioAlarmInfo.FromStringVcalendarStatic, "PCM", "", "text", ["PCM", "WAVE", "AIFF"], []),
                VCalendarConstants._dAlarmSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.DisplayAlarm, typeof(DisplayAlarmInfo), DisplayAlarmInfo.FromStringVcalendarStatic, "", "", "text", [], []),
                VCalendarConstants._mAlarmSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.MailAlarm, typeof(MailAlarmInfo), MailAlarmInfo.FromStringVcalendarStatic, "", "", "text", [], []),
                VCalendarConstants._pAlarmSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.ProcedureAlarm, typeof(ProcedureAlarmInfo), ProcedureAlarmInfo.FromStringVcalendarStatic, "", "", "text", [], []),
                VCalendarConstants._lastModSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.LastModified, typeof(LastModifiedInfo), LastModifiedInfo.FromStringVcalendarStatic, "", "", "date-time", [], []),
                VCalendarConstants._freeBusySpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.FreeBusy, typeof(CalendarFreeBusyInfo), CalendarFreeBusyInfo.FromStringVcalendarStatic, "", "", "period", [], []),
                VCalendarConstants._requestStatusSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.RequestStatus, typeof(RequestStatusInfo), RequestStatusInfo.FromStringVcalendarStatic, "", "", "text", [], []),
                VCalendarConstants._productIdSpecifier => (PartType.Strings, CalendarStringsEnum.ProductId, null, null, "", "", "text", [], []),
                VCalendarConstants._calScaleSpecifier => (PartType.Strings, CalendarStringsEnum.CalScale, null, null, "", "", "text", [], []),
                VCalendarConstants._methodSpecifier => (PartType.Strings, CalendarStringsEnum.Method, null, null, "", "", "text", [], []),
                VCalendarConstants._classSpecifier => (PartType.Strings, CalendarStringsEnum.Class, null, null, "", "", "text", [], []),
                VCalendarConstants._uidSpecifier => (PartType.Strings, CalendarStringsEnum.Uid, null, null, "", "", "text", [], []),
                VCalendarConstants._organizerSpecifier => (PartType.Strings, CalendarStringsEnum.Organizer, null, null, "", "", "cal-address", [], []),
                VCalendarConstants._statusSpecifier => (PartType.Strings, CalendarStringsEnum.Status, null, null, "", "", "text", [], allowedStatuses),
                VCalendarConstants._summarySpecifier => (PartType.Strings, CalendarStringsEnum.Summary, null, null, "", "", "text", [], []),
                VCalendarConstants._descriptionSpecifier => (PartType.Strings, CalendarStringsEnum.Description, null, null, "", "", "text", [], []),
                VCalendarConstants._transparencySpecifier => (PartType.Strings, CalendarStringsEnum.Transparency, null, null, "", "", calendarVersion.Major == 2 ? "text" : "integer", [], calendarVersion.Major == 2 ? ["TRANSPARENT", "OPAQUE"] : []),
                VCalendarConstants._actionSpecifier => (PartType.Strings, CalendarStringsEnum.Action, null, null, "", "", "text", [], []),
                VCalendarConstants._triggerSpecifier => (PartType.Strings, CalendarStringsEnum.Trigger, null, null, "", "", "duration", [], []),
                VCalendarConstants._tzidSpecifier => (PartType.Strings, CalendarStringsEnum.TimeZoneId, null, null, "", "", "text", [], []),
                VCalendarConstants._tzUrlSpecifier => (PartType.Strings, CalendarStringsEnum.TimeZoneUrl, null, null, "", "", "uri", [], []),
                VCalendarConstants._recurseSpecifier => (PartType.Strings, CalendarStringsEnum.Recursion, null, null, "", "", "recur", [], []),
                VCalendarConstants._exRuleSpecifier => (PartType.Strings, CalendarStringsEnum.ExRule, null, null, "", "", "recur", [], []),
                VCalendarConstants._recurIdSpecifier => (PartType.Strings, CalendarStringsEnum.RecursionId, null, null, "", "", "date-time", [], []),
                VCalendarConstants._urlSpecifier => (PartType.Strings, CalendarStringsEnum.Url, null, null, "", "", "uri", [], []),
                VCalendarConstants._tzSpecifier => (PartType.Strings, CalendarStringsEnum.TimeZone, null, null, "", "", "tz-offset", [], []),
                VCalendarConstants._commentSpecifier => (PartType.Strings, CalendarStringsEnum.Comment, null, null, "", "", "text", [], []),
                VCalendarConstants._locationSpecifier => (PartType.Strings, CalendarStringsEnum.Location, null, null, "", "", "text", [], []),
                VCalendarConstants._attendeeSpecifier => (PartType.Strings, CalendarStringsEnum.Attendee, null, null, "", "", "cal-address", ["VCARD"], []),
                VCalendarConstants._durationSpecifier => (PartType.Strings, CalendarStringsEnum.Duration, null, null, "", "", "duration", [], []),
                VCalendarConstants._tzNameSpecifier => (PartType.Strings, CalendarStringsEnum.TimeZoneName, null, null, "", "", "text", [], []),
                VCalendarConstants._relationshipSpecifier => (PartType.Strings, CalendarStringsEnum.RelatedTo, null, null, "", "", "text", [], []),
                VCalendarConstants._contactSpecifier => (PartType.Strings, CalendarStringsEnum.Contact, null, null, "", "", "text", [], []),
                VCalendarConstants._prioritySpecifier => (PartType.Integers, CalendarIntegersEnum.Priority, null, null, "", "", "integer", [], []),
                VCalendarConstants._sequenceSpecifier => (PartType.Integers, CalendarIntegersEnum.Sequence, null, null, "", "", "integer", [], []),
                VCalendarConstants._percentCompletionSpecifier => (PartType.Integers, CalendarIntegersEnum.PercentComplete, null, null, "", "", "integer", [], []),
                VCalendarConstants._repeatSpecifier => (PartType.Integers, CalendarIntegersEnum.Repeat, null, null, "", "", "integer", [], []),
                VCalendarConstants._rNumSpecifier => (PartType.Integers, CalendarIntegersEnum.RecurrTimes, null, null, "", "", "integer", [], []),

                // Extensions are allowed
                VcardConstants._xSpecifier => (PartType.PartsArray, CalendarPartsArrayEnum.NonstandardNames, typeof(XNameInfo), XNameInfo.FromStringVcalendarStatic, "", "", "", [], []),
                _ => (PartType.PartsArray, CalendarPartsArrayEnum.IanaNames, typeof(ExtraInfo), ExtraInfo.FromStringVcalendarStatic, "", "", "", [], []),
            };
        }

        internal static string GetObjectTypeFromComponent<TComponent>(TComponent component)
            where TComponent : Parts.Calendar =>
            GetObjectTypeFromType(component.GetType(), component is CalendarOtherComponent other ? other.ComponentName : "");

        internal static string GetObjectTypeFromType(Type type, string specifier = "") =>
            type.Name switch
            {
                nameof(Parts.Calendar) => VCalendarConstants._objectVCalendarSpecifier,
                nameof(CalendarEvent) => VCalendarConstants._objectVEventSpecifier,
                nameof(CalendarTodo) => VCalendarConstants._objectVTodoSpecifier,
                nameof(CalendarJournal) => VCalendarConstants._objectVJournalSpecifier,
                nameof(CalendarFreeBusy) => VCalendarConstants._objectVFreeBusySpecifier,
                nameof(CalendarTimeZone) => VCalendarConstants._objectVTimeZoneSpecifier,
                nameof(CalendarStandard) => VCalendarConstants._objectVStandardSpecifier,
                nameof(CalendarDaylight) => VCalendarConstants._objectVDaylightSpecifier,
                nameof(CalendarAlarm) => VCalendarConstants._objectVAlarmSpecifier,
                nameof(CalendarOtherComponent) => specifier,
                _ => throw new InvalidDataException("Type is not a valid component"),
            };

        private static bool TypeMatch(Type componentType, params Type[] expectedTypes) =>
            expectedTypes.Contains(componentType);
    }
}
