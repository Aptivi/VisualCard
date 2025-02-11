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
using VisualCard.Calendar.Parts;
using VisualCard.Calendar.Parts.Implementations;
using VisualCard.Calendar.Parts.Enums;
using VisualCard.Calendar.Parts.Implementations.Event;
using System.Linq;
using VisualCard.Calendar.Parts.Implementations.TimeZone;
using VisualCard.Calendar.Parts.Implementations.Todo;
using VisualCard.Calendar.Parts.Implementations.Legacy;
using VisualCard.Calendar.Parts.Implementations.FreeBusy;
using VisualCard.Parsers;
using VisualCard.Common.Parts.Implementations;
using VisualCard.Common.Parts.Enums;
using VisualCard.Common.Parsers;

namespace VisualCard.Calendar.Parsers
{
    internal class VCalendarParserTools
    {
        internal static Version[] supportedVersions =
        [
            new(1, 0),
            new(2, 0),
        ];

        internal static bool VerifySupportedVersion(Version version)
        {
            int major = version.Major;
            int minor = version.Minor;
            foreach (var supportedVersion in supportedVersions)
            {
                if (supportedVersion.Major == major && supportedVersion.Minor == minor)
                    return true;
            }
            return false;
        }

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
                CalendarPartsArrayEnum.NonstandardNames => CommonConstants._xSpecifier,
                _ => ""
            };

        internal static CalendarPartsArrayEnum GetPartsArrayEnumFromType(Type? partsArrayType, Version cardVersion, Type componentType)
        {
            if (partsArrayType is null)
                throw new NotImplementedException("Type is not provided.");

            // Enumerate through all parts array enums
            var enums = Enum.GetValues(typeof(CalendarPartsArrayEnum));
            foreach (CalendarPartsArrayEnum part in enums)
            {
                string prefix = GetPrefixFromPartsArrayEnum(part);
                var type = GetPartType(prefix, cardVersion, componentType);
                if (type.enumType == partsArrayType)
                    return part;
            }
            return CalendarPartsArrayEnum.IanaNames;
        }

        internal static VCalendarPartType GetPartType(string prefix, Version calendarVersion, Type componentType)
        {
            string[] allowedStatuses =
                componentType == typeof(CalendarEvent) && calendarVersion.Major == 2 ? ["TENTATIVE", "CONFIRMED", "CANCELLED"] :
                componentType == typeof(CalendarEvent) && calendarVersion.Major == 1 ? ["NEEDS ACTION", "SENT", "TENTATIVE", "CONFIRMED", "DECLINED", "DELEGATED"] :
                componentType == typeof(CalendarTodo) && calendarVersion.Major == 2 ? ["NEEDS-ACTION", "COMPLETED", "IN-PROCESS", "CANCELLED"] :
                componentType == typeof(CalendarTodo) && calendarVersion.Major == 1 ? ["NEEDS ACTION", "SENT", "ACCEPTED", "COMPLETED", "DECLINED", "DELEGATED"] :
                componentType == typeof(CalendarJournal) ? ["DRAFT", "FINAL", "CANCELLED"] :
                [];
            return prefix switch
            {
                VCalendarConstants._attachSpecifier => new(PartType.PartsArray, CalendarPartsArrayEnum.Attach, PartCardinality.Any, (_) => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarAlarm)), typeof(AttachInfo), AttachInfo.FromStringStatic, "", "", "uri", [], []),
                VCalendarConstants._categoriesSpecifier => new(PartType.PartsArray, CalendarPartsArrayEnum.Categories, PartCardinality.MayBeOne, (_) => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal)), typeof(CategoriesInfo), CategoriesInfo.FromStringStatic, "", "", "text", [], []),
                VCalendarConstants._geoSpecifier => new(PartType.PartsArray, CalendarPartsArrayEnum.Geography, PartCardinality.Any, (_) => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo)), typeof(GeoInfo), GeoInfo.FromStringStatic, "", "", "float", [], []),
                VCalendarConstants._resourcesSpecifier => new(PartType.PartsArray, CalendarPartsArrayEnum.Resources, PartCardinality.MayBeOne, (_) => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo)), typeof(ResourcesInfo), ResourcesInfo.FromStringStatic, "", "", "text", [], []),
                VCalendarConstants._createdSpecifier => new(PartType.PartsArray, CalendarPartsArrayEnum.DateCreated, PartCardinality.MayBeOne, (_) => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal)), typeof(DateCreatedInfo), DateCreatedInfo.FromStringStatic, "", "", "date-time", [], []),
                VCalendarConstants._created1Specifier => new(PartType.PartsArray, CalendarPartsArrayEnum.DateCreatedAlt, PartCardinality.MayBeOne, (_) => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo)), typeof(DateCreatedInfo), DateCreatedInfo.FromStringStatic, "", "", "date-time", [], []),
                VCalendarConstants._dateStartSpecifier => new(PartType.PartsArray, CalendarPartsArrayEnum.DateStart, PartCardinality.ShouldBeOne, (_) => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarFreeBusy), typeof(CalendarStandard), typeof(CalendarDaylight)), typeof(DateStartInfo), DateStartInfo.FromStringStatic, "", "", "date-time", [], []),
                VCalendarConstants._dateEndSpecifier => new(PartType.PartsArray, CalendarPartsArrayEnum.DateEnd, PartCardinality.MayBeOne, (_) => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarFreeBusy)), typeof(DateEndInfo), DateEndInfo.FromStringStatic, "", "", "date-time", [], []),
                VCalendarConstants._dateCompletedSpecifier => new(PartType.PartsArray, CalendarPartsArrayEnum.DateCompleted, PartCardinality.MayBeOne, (_) => TypeMatch(componentType, typeof(CalendarTodo)), typeof(DateCompletedInfo), DateCompletedInfo.FromStringStatic, "", "", "date-time", [], []),
                VCalendarConstants._dueDateSpecifier => new(PartType.PartsArray, CalendarPartsArrayEnum.DueDate, PartCardinality.MayBeOne, (_) => TypeMatch(componentType, typeof(CalendarTodo)), typeof(DueDateInfo), DueDateInfo.FromStringStatic, "", "", "date-time", [], []),
                VCalendarConstants._dateStampSpecifier => new(PartType.PartsArray, CalendarPartsArrayEnum.DateStamp, PartCardinality.MayBeOne, (ver) => ver.Major == 2 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarFreeBusy)), typeof(DateStampInfo), DateStampInfo.FromStringStatic, "", "", "date-time", [], []),
                VCalendarConstants._tzOffsetFromSpecifier => new(PartType.PartsArray, CalendarPartsArrayEnum.TimeZoneOffsetFrom, PartCardinality.MayBeOne, (ver) => ver.Major == 2 && TypeMatch(componentType, typeof(CalendarStandard), typeof(CalendarDaylight)), typeof(TimeZoneOffsetFromInfo), TimeZoneOffsetFromInfo.FromStringStatic, "", "", "utc-offset", [], []),
                VCalendarConstants._tzOffsetToSpecifier => new(PartType.PartsArray, CalendarPartsArrayEnum.TimeZoneOffsetTo, PartCardinality.MayBeOne, (ver) => ver.Major == 2 && TypeMatch(componentType, typeof(CalendarStandard), typeof(CalendarDaylight)), typeof(TimeZoneOffsetToInfo), TimeZoneOffsetToInfo.FromStringStatic, "", "", "utc-offset", [], []),
                VCalendarConstants._recDateSpecifier => new(PartType.PartsArray, CalendarPartsArrayEnum.RecDate, PartCardinality.Any, (_) => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarStandard), typeof(CalendarDaylight)), typeof(RecDateInfo), RecDateInfo.FromStringStatic, "", "", "date-time", [], []),
                VCalendarConstants._exDateSpecifier => new(PartType.PartsArray, CalendarPartsArrayEnum.ExDate, PartCardinality.Any, (_) => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarStandard), typeof(CalendarDaylight)), typeof(ExDateInfo), ExDateInfo.FromStringStatic, "", "", "date-time", [], []),
                VCalendarConstants._daylightSpecifier => new(PartType.PartsArray, CalendarPartsArrayEnum.Daylight, PartCardinality.MayBeOne, (ver) => ver.Major == 1 && TypeMatch(componentType, typeof(Parts.Calendar)), typeof(DaylightInfo), DaylightInfo.FromStringStatic, "", "", "text", [], []),
                VCalendarConstants._aAlarmSpecifier => new(PartType.PartsArray, CalendarPartsArrayEnum.AudioAlarm, PartCardinality.Any, (ver) => ver.Major == 1 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo)), typeof(AudioAlarmInfo), AudioAlarmInfo.FromStringStatic, "PCM", "", "text", ["PCM", "WAVE", "AIFF"], []),
                VCalendarConstants._dAlarmSpecifier => new(PartType.PartsArray, CalendarPartsArrayEnum.DisplayAlarm, PartCardinality.Any, (ver) => ver.Major == 1 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo)), typeof(DisplayAlarmInfo), DisplayAlarmInfo.FromStringStatic, "", "", "text", [], []),
                VCalendarConstants._mAlarmSpecifier => new(PartType.PartsArray, CalendarPartsArrayEnum.MailAlarm, PartCardinality.Any, (ver) => ver.Major == 1 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo)), typeof(MailAlarmInfo), MailAlarmInfo.FromStringStatic, "", "", "text", [], []),
                VCalendarConstants._pAlarmSpecifier => new(PartType.PartsArray, CalendarPartsArrayEnum.ProcedureAlarm, PartCardinality.Any, (ver) => ver.Major == 1 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo)), typeof(ProcedureAlarmInfo), ProcedureAlarmInfo.FromStringStatic, "", "", "text", [], []),
                VCalendarConstants._lastModSpecifier => new(PartType.PartsArray, CalendarPartsArrayEnum.LastModified, PartCardinality.MayBeOne, (_) => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarTimeZone)), typeof(LastModifiedInfo), LastModifiedInfo.FromStringStatic, "", "", "date-time", [], []),
                VCalendarConstants._freeBusySpecifier => new(PartType.PartsArray, CalendarPartsArrayEnum.FreeBusy, PartCardinality.Any, (ver) => ver.Major == 2 && TypeMatch(componentType, typeof(CalendarFreeBusy)), typeof(CalendarFreeBusyInfo), CalendarFreeBusyInfo.FromStringStatic, "", "", "period", [], []),
                VCalendarConstants._requestStatusSpecifier => new(PartType.PartsArray, CalendarPartsArrayEnum.RequestStatus, PartCardinality.Any, (ver) => ver.Major == 2 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarFreeBusy)), typeof(RequestStatusInfo), RequestStatusInfo.FromStringStatic, "", "", "text", [], []),
                VCalendarConstants._productIdSpecifier => new(PartType.Strings, CalendarStringsEnum.ProductId, PartCardinality.ShouldBeOne, null, null, null, "", "", "text", [], []),
                VCalendarConstants._calScaleSpecifier => new(PartType.Strings, CalendarStringsEnum.CalScale, PartCardinality.MayBeOne, (ver) => ver.Major == 2, null, null, "", "", "text", [], []),
                VCalendarConstants._methodSpecifier => new(PartType.Strings, CalendarStringsEnum.Method, PartCardinality.MayBeOne, (ver) => ver.Major == 2, null, null, "", "", "text", [], []),
                VCalendarConstants._classSpecifier => new(PartType.Strings, CalendarStringsEnum.Class, PartCardinality.MayBeOne, (_) => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal)), null, null, "", "", "text", [], []),
                VCalendarConstants._uidSpecifier => new(PartType.Strings, CalendarStringsEnum.Uid, PartCardinality.MayBeOne, (_) => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarFreeBusy)), null, null, "", "", "text", [], []),
                VCalendarConstants._organizerSpecifier => new(PartType.Strings, CalendarStringsEnum.Organizer, PartCardinality.MayBeOne, (_) => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarFreeBusy)), null, null, "", "", "cal-address", [], []),
                VCalendarConstants._statusSpecifier => new(PartType.Strings, CalendarStringsEnum.Status, PartCardinality.MayBeOne, (_) => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal)), null, null, "", "", "text", [], allowedStatuses),
                VCalendarConstants._summarySpecifier => new(PartType.Strings, CalendarStringsEnum.Summary, PartCardinality.MayBeOne, (_) => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarAlarm)), null, null, "", "", "text", [], []),
                VCalendarConstants._descriptionSpecifier => new(PartType.Strings, CalendarStringsEnum.Description, PartCardinality.MayBeOne, (_) => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarAlarm)), null, null, "", "", "text", [], []),
                VCalendarConstants._transparencySpecifier => new(PartType.Strings, CalendarStringsEnum.Transparency, PartCardinality.MayBeOne, (_) => TypeMatch(componentType, typeof(CalendarEvent)), null, null, "", "", calendarVersion.Major == 2 ? "text" : "integer", [], calendarVersion.Major == 2 ? ["TRANSPARENT", "OPAQUE"] : []),
                VCalendarConstants._actionSpecifier => new(PartType.Strings, CalendarStringsEnum.Action, PartCardinality.ShouldBeOne, (ver) => calendarVersion.Major == 2 && TypeMatch(componentType, typeof(CalendarAlarm)), null, null, "", "", "text", [], []),
                VCalendarConstants._triggerSpecifier => new(PartType.Strings, CalendarStringsEnum.Trigger, PartCardinality.ShouldBeOne, (ver) => calendarVersion.Major == 2 && TypeMatch(componentType, typeof(CalendarAlarm)), null, null, "", "", "duration", [], []),
                VCalendarConstants._tzidSpecifier => new(PartType.Strings, CalendarStringsEnum.TimeZoneId, PartCardinality.ShouldBeOne, (ver) => calendarVersion.Major == 2 && TypeMatch(componentType, typeof(CalendarTimeZone)), null, null, "", "", "text", [], []),
                VCalendarConstants._tzUrlSpecifier => new(PartType.Strings, CalendarStringsEnum.TimeZoneUrl, PartCardinality.MayBeOne, (ver) => calendarVersion.Major == 2 && TypeMatch(componentType, typeof(CalendarTimeZone)), null, null, "", "", "uri", [], []),
                VCalendarConstants._recurseSpecifier => new(PartType.Strings, CalendarStringsEnum.Recursion, PartCardinality.MayBeOne, (_) => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarStandard), typeof(CalendarDaylight)), null, null, "", "", "recur", [], []),
                VCalendarConstants._exRuleSpecifier => new(PartType.Strings, CalendarStringsEnum.ExRule, PartCardinality.MayBeOne, (ver) => ver.Major == 1 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo)), null, null, "", "", "recur", [], []),
                VCalendarConstants._recurIdSpecifier => new(PartType.Strings, CalendarStringsEnum.RecursionId, PartCardinality.MayBeOne, (ver) => ver.Major == 2 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal)), null, null, "", "", "date-time", [], []),
                VCalendarConstants._urlSpecifier => new(PartType.Strings, CalendarStringsEnum.Url, PartCardinality.MayBeOne, (_) => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarFreeBusy)), null, null, "", "", "uri", [], []),
                VCalendarConstants._tzSpecifier => new(PartType.Strings, CalendarStringsEnum.TimeZone, PartCardinality.MayBeOne, (_) => TypeMatch(componentType, typeof(Parts.Calendar)), null, null, "", "", "tz-offset", [], []),
                VCalendarConstants._commentSpecifier => new(PartType.Strings, CalendarStringsEnum.Comment, PartCardinality.Any, (ver) => ver.Major == 2 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarFreeBusy), typeof(CalendarStandard), typeof(CalendarDaylight)), null, null, "", "", "text", [], []),
                VCalendarConstants._locationSpecifier => new(PartType.Strings, CalendarStringsEnum.Location, PartCardinality.Any, (ver) => ver.Major == 2 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo)), null, null, "", "", "text", [], []),
                VCalendarConstants._attendeeSpecifier => new(PartType.Strings, CalendarStringsEnum.Attendee, PartCardinality.Any, (_) => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarFreeBusy), typeof(CalendarAlarm)), null, null, "", "", "cal-address", ["VCARD"], []),
                VCalendarConstants._durationSpecifier => new(PartType.Strings, CalendarStringsEnum.Duration, PartCardinality.MayBeOne, (ver) => ver.Major == 2 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarAlarm)), null, null, "", "", "duration", [], []),
                VCalendarConstants._tzNameSpecifier => new(PartType.Strings, CalendarStringsEnum.TimeZoneName, PartCardinality.Any, (ver) => ver.Major == 2 && TypeMatch(componentType, typeof(CalendarTimeZone)), null, null, "", "", "text", [], []),
                VCalendarConstants._relationshipSpecifier => new(PartType.Strings, CalendarStringsEnum.RelatedTo, PartCardinality.Any, (_) => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal)), null, null, "", "", "text", [], []),
                VCalendarConstants._contactSpecifier => new(PartType.Strings, CalendarStringsEnum.Contact, PartCardinality.Any, (ver) => ver.Major == 2 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal), typeof(CalendarFreeBusy)), null, null, "", "", "text", [], []),
                VCalendarConstants._prioritySpecifier => new(PartType.Integers, CalendarIntegersEnum.Priority, PartCardinality.MayBeOne, (_) => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo)), null, null, "", "", "integer", [], []),
                VCalendarConstants._sequenceSpecifier => new(PartType.Integers, CalendarIntegersEnum.Sequence, PartCardinality.MayBeOne, (_) => TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo), typeof(CalendarJournal)), null, null, "", "", "integer", [], []),
                VCalendarConstants._percentCompletionSpecifier => new(PartType.Integers, CalendarIntegersEnum.PercentComplete, PartCardinality.MayBeOne, (_) => TypeMatch(componentType, typeof(CalendarTodo)), null, null, "", "", "integer", [], []),
                VCalendarConstants._repeatSpecifier => new(PartType.Integers, CalendarIntegersEnum.Repeat, PartCardinality.MayBeOne, (_) => TypeMatch(componentType, typeof(CalendarAlarm)), null, null, "", "", "integer", [], []),
                VCalendarConstants._rNumSpecifier => new(PartType.Integers, CalendarIntegersEnum.RecurrTimes, PartCardinality.MayBeOne, (ver) => ver.Major == 1 && TypeMatch(componentType, typeof(CalendarEvent), typeof(CalendarTodo)), null, null, "", "", "integer", [], []),

                // Extensions are allowed
                CommonConstants._xSpecifier => new(PartType.PartsArray, CalendarPartsArrayEnum.NonstandardNames, PartCardinality.Any, null, typeof(XNameInfo), null, "", "", "", [], []),
                _ => new(PartType.PartsArray, CalendarPartsArrayEnum.IanaNames, PartCardinality.Any, null, typeof(ExtraInfo), null, "", "", "", [], []),
            };
        }

        private static bool TypeMatch(Type componentType, params Type[] expectedTypes) =>
            expectedTypes.Contains(componentType);
    }
}
