/*
 * MIT License
 *
 * Copyright (c) 2021-2022 Aptivi
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 */

namespace VisualCard.Parsers
{
    internal static class VcardConstants
    {
        // Available in vCard 2.1, vCard 3.0, and vCard 4.0
        internal const char _fieldDelimiter = ';';
        internal const char _valueDelimiter = ',';
        internal const char _argumentDelimiter = ':';
        internal const string _nameSpecifier = "N";
        internal const string _fullNameSpecifier = "FN";
        internal const string _telephoneSpecifier = "TEL";
        internal const string _addressSpecifier = "ADR";
        internal const string _labelSpecifier = "LABEL";
        internal const string _emailSpecifier = "EMAIL";
        internal const string _orgSpecifier = "ORG";
        internal const string _titleSpecifier = "TITLE";
        internal const string _urlSpecifier = "URL";
        internal const string _noteSpecifier = "NOTE";
        internal const string _photoSpecifier = "PHOTO";
        internal const string _logoSpecifier = "LOGO";
        internal const string _soundSpecifier = "SOUND";
        internal const string _revSpecifier = "REV";
        internal const string _birthSpecifier = "BDAY";
        internal const string _mailerSpecifier = "MAILER";
        internal const string _roleSpecifier = "ROLE";
        internal const string _timeZoneSpecifier = "TZ";
        internal const string _geoSpecifier = "GEO";
        internal const string _imppSpecifier = "IMPP";
        internal const string _sourceSpecifier = "SOURCE";
        internal const string _xmlSpecifier = "XML";
        internal const string _fbUrlSpecifier = "FBURL";
        internal const string _calUriSpecifier = "CALURI";
        internal const string _caladrUriSpecifier = "CALADRURI";
        internal const string _xSpecifier = "X-";
        internal const string _typeArgumentSpecifier = "TYPE=";
        internal const string _valueArgumentSpecifier = "VALUE=";
        internal const string _encodingArgumentSpecifier = "ENCODING=";

        // Available in vCard 3.0 and vCard 4.0
        internal const string _nicknameSpecifier = "NICKNAME";
        internal const string _categoriesSpecifier = "CATEGORIES";
        internal const string _productIdSpecifier = "PRODID";
        internal const string _sortStringSpecifier = "SORT-STRING";

        // Available in vCard 4.0
        internal const string _kindSpecifier = "KIND";
        internal const string _altIdArgumentSpecifier = "ALTID=";
    }
}
