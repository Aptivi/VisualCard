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

namespace VisualCard.Common.Parsers
{
    internal static class CommonConstants
    {
        // Mandatory for each vCard and vCalendar
        internal const string _beginSpecifier = "BEGIN";
        internal const string _endSpecifier = "END";
        internal const string _versionSpecifier = "VERSION";

        // Misc constants
        internal const string _spaceBreak = " ";
        internal const string _tabBreak = "\x0009";

        // Encodings
        internal const string _quotedPrintable = "QUOTED-PRINTABLE";

        // Available in all implementations
        internal const char _fieldDelimiter = ';';
        internal const char _valueDelimiter = ',';
        internal const char _argumentDelimiter = ':';
        internal const char _argumentValueDelimiter = '=';
        internal const string _xSpecifier = "X-";
        internal const string _typeArgumentSpecifier = "TYPE";
        internal const string _valueArgumentSpecifier = "VALUE";
        internal const string _encodingArgumentSpecifier = "ENCODING";
    }
}
