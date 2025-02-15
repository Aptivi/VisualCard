﻿//
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

using VisualCard.Common.Parsers;

namespace VisualCard.Parsers
{
    internal static class VcardConstants
    {
        // Mandatory for each vCard
        internal const string _beginText = CommonConstants._beginSpecifier + ":" + _objectVCardSpecifier;
        internal const string _endText = CommonConstants._endSpecifier + ":" + _objectVCardSpecifier;

        // Object specifiers
        internal const string _objectVCardSpecifier = "VCARD";

        // Available in vCard 2.1, 3.0, 4.0, and 5.0
        internal const string _nameSpecifier = "N";
        internal const string _fullNameSpecifier = "FN";
        internal const string _telephoneSpecifier = "TEL";
        internal const string _addressSpecifier = "ADR";
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
        internal const string _categoriesSpecifier = "CATEGORIES";
        internal const string _keySpecifier = "KEY";

        // Available in vCard 2.1, 3.0, and 4.0
        internal const string _uidSpecifier = "UID";

        // Available in vCard 2.1, 3.0, and 5.0
        internal const string _labelSpecifier = "LABEL";
        internal const string _sortStringSpecifier = "SORT-STRING";
        internal const string _agentSpecifier = "AGENT";

        // Available in vCard 3.0, 4.0, and 5.0
        internal const string _nicknameSpecifier = "NICKNAME";
        internal const string _productIdSpecifier = "PRODID";

        // Available in vCard 3.0 and 5.0
        internal const string _classSpecifier = "CLASS";

        // Available in vCard 4.0 and 5.0
        internal const string _kindSpecifier = "KIND";
        internal const string _anniversarySpecifier = "ANNIVERSARY";
        internal const string _genderSpecifier = "GENDER";
        internal const string _langSpecifier = "LANG";
        internal const string _contactUriSpecifier = "CONTACT-URI";
        internal const string _altIdArgumentSpecifier = "ALTID";

        // Available in vCard 3.0
        internal const string _srcNameSpecifier = "NAME";
        internal const string _profileSpecifier = "PROFILE";

        // Available in vCard 4.0
        internal const string _memberSpecifier = "MEMBER";
        internal const string _relatedSpecifier = "RELATED";
        internal const string _clientPidMapSpecifier = "CLIENTPIDMAP";
        internal const string _expertiseSpecifier = "EXPERTISE";
        internal const string _hobbySpecifier = "HOBBY";
        internal const string _interestSpecifier = "INTEREST";
        internal const string _orgDirectorySpecifier = "ORG-DIRECTORY";
    }
}
