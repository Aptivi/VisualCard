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

namespace VisualCard.Calendar.Parsers
{
    internal static class VCalendarConstants
    {
        // Mandatory for each vCalendar
        internal const string _beginText = _beginSpecifier + ":" + _objectVCalendarSpecifier;
        internal const string _endText = _endSpecifier + ":" + _objectVCalendarSpecifier;
        internal const string _beginSpecifier = "BEGIN";
        internal const string _endSpecifier = "END";
        internal const string _versionSpecifier = "VERSION";

        // Object specifiers
        internal const string _objectVCalendarSpecifier = "VCALENDAR";
        internal const string _objectVEventSpecifier = "VEVENT";
        internal const string _objectVTodoSpecifier = "VTODO";

        // Object specifiers (vCalendar 2.0)
        internal const string _objectVJournalSpecifier = "VJOURNAL";
        internal const string _objectVFreeBusySpecifier = "VFREEBUSY";
        internal const string _objectVTimeZoneSpecifier = "VTIMEZONE";
        internal const string _objectVStandardSpecifier = "STANDARD";
        internal const string _objectVDaylightSpecifier = "DAYLIGHT";
        internal const string _objectVAlarmSpecifier = "VALARM";

        // Misc vCalendar constants
        internal const string _spaceBreak = " ";
        internal const string _tabBreak = "\x0009";

        // Available in vCalendar 1.0 and 2.0
        internal const char _fieldDelimiter = ';';
        internal const char _valueDelimiter = ',';
        internal const char _argumentDelimiter = ':';
        internal const char _argumentValueDelimiter = '=';
        internal const string _productIdSpecifier = "PRODID";
        internal const string _uidSpecifier = "UID";
        internal const string _dateStartSpecifier = "DTSTART";
        internal const string _dateEndSpecifier = "DTEND";
        internal const string _organizerSpecifier = "ORGANIZER";
        internal const string _statusSpecifier = "STATUS";
        internal const string _categoriesSpecifier = "CATEGORIES";
        internal const string _summarySpecifier = "SUMMARY";
        internal const string _descriptionSpecifier = "DESCRIPTION";
        internal const string _attachSpecifier = "ATTACH";
        internal const string _classSpecifier = "CLASS";
        internal const string _geoSpecifier = "GEO";
        internal const string _resourcesSpecifier = "RESOURCES";
        internal const string _sequenceSpecifier = "SEQUENCE";
        internal const string _attendeeSpecifier = "ATTENDEE";
        internal const string _transparencySpecifier = "TRANSP";
        internal const string _createdSpecifier = "CREATED";
        internal const string _created1Specifier = "DCREATED";
        internal const string _actionSpecifier = "ACTION";
        internal const string _triggerSpecifier = "TRIGGER";
        internal const string _tzidSpecifier = "TZID";
        internal const string _tzOffsetFromSpecifier = "TZOFFSETFROM";
        internal const string _tzOffsetToSpecifier = "TZOFFSETTO";
        internal const string _tzUrlSpecifier = "TZURL";
        internal const string _recurseSpecifier = "RRULE";
        internal const string _recDateSpecifier = "RDATE";
        internal const string _exDateSpecifier = "EXDATE";
        internal const string _dateCompletedSpecifier = "COMPLETED";
        internal const string _dueDateSpecifier = "DUE";
        internal const string _relationshipSpecifier = "RELATED-TO";
        internal const string _lastModSpecifier = "LAST-MODIFIED";
        internal const string _prioritySpecifier = "PRIORITY";
        internal const string _xSpecifier = "X-";
        internal const string _typeArgumentSpecifier = "TYPE=";
        internal const string _valueArgumentSpecifier = "VALUE=";
        internal const string _encodingArgumentSpecifier = "ENCODING=";

        // Available in vCalendar 1.0
        internal const string _daylightSpecifier = "DAYLIGHT";
        internal const string _aAlarmSpecifier = "AALARM";
        internal const string _dAlarmSpecifier = "DALARM";
        internal const string _mAlarmSpecifier = "MALARM";
        internal const string _pAlarmSpecifier = "PALARM";
        internal const string _exRuleSpecifier = "EXRULE";

        // Available in vCalendar 2.0
        internal const string _dateStampSpecifier = "DTSTAMP";
        internal const string _calScaleSpecifier = "CALSCALE";
        internal const string _methodSpecifier = "METHOD";
        internal const string _locationSpecifier = "LOCATION";
        internal const string _commentSpecifier = "COMMENT";
        internal const string _tzNameSpecifier = "TZNAME";
        internal const string _percentCompletionSpecifier = "PERCENT-COMPLETION";
        internal const string _freeBusySpecifier = "FREEBUSY";
        internal const string _recurIdSpecifier = "RECURRENCE-ID";
    }
}
