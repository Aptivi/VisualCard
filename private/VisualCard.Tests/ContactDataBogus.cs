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

using System.Collections.Generic;
using VisualCard.Parsers;

namespace VisualCard.Tests
{
    public static class ContactDataBogus
    {
        private static readonly string vcardTwoNoFullName =
            """
            BEGIN:VCARD
            VERSION:2.1
            N:Hood;Rick;;;
            END:VCARD
            """
        ;

        private static readonly string vcardTwoWithUnsupportedParts =
            """
            BEGIN:VCARD
            VERSION:2.1
            N:Hood;Rick;;;
            CALURI:https://www.rickhood.com/events/calendar?type=MusicHoodFestival2024
            NICKNAME:R.H.
            END:VCARD
            """
        ;

        private static readonly string vcardTwoWithIndirectVersionSpecify =
            """
            BEGIN:VCARD
            N:Hood;Rick;;;
            CALURI:https://www.rickhood.com/events/calendar?type=MusicHoodFestival2024
            NICKNAME:R.H.
            VERSION:2.1
            END:VCARD
            """
        ;

        private static readonly string vcardThreeNoFullName =
            """
            BEGIN:VCARD
            VERSION:3.0
            N:Hood;Rick;;;
            END:VCARD
            """
        ;

        private static readonly string vcardZeroByte =
            """

            """
        ;

        private static readonly string vcardNonexistentVersion =
            """
            BEGIN:VCARD
            VERSION:1.0
            N:Hood;Rick;;;
            FN:Rick Hood
            END:VCARD
            """
        ;

        private static readonly string vcardTwoBarren =
            """
            BEGIN:VCARD
            VERSION:2.1
            END:VCARD
            """
        ;

        private static readonly string vcardThreeBarren =
            """
            BEGIN:VCARD
            VERSION:3.0
            END:VCARD
            """
        ;

        private static readonly string vcardFourBarren =
            """
            BEGIN:VCARD
            VERSION:4.0
            END:VCARD
            """
        ;

        private static readonly string vcardFiveBarren =
            """
            BEGIN:VCARD
            VERSION:5.0
            END:VCARD
            """
        ;

        private static readonly string vcardInvalidAltIdUsage =
            """
            BEGIN:VCARD
            VERSION:5.0
            N:Hood;Rick;;;
            FN:Rick Hood
            REV;ALTID=0;LANGUAGE=en:20240403
            REV;ALTID=0;LANGUAGE=de:20240404
            END:VCARD
            """
        ;

        private static readonly string vcardInvalidType =
            """
            BEGIN:VCARD
            VERSION:5.0
            N:Hood;Rick;;;
            FN:Rick Hood
            ADR;TYPE=warehouse:;;Los Angeles, USA;;;;
            END:VCARD
            """
        ;

        private static readonly string vcardInvalidVersion =
            """
            BEGIN:VCARD
            N:Hood;Rick;;;
            FN:Rick Hood
            ADR;TYPE=warehouse:;;Los Angeles, USA;;;;
            VERSION:5.0
            END:VCARD
            """
        ;

        /// <summary>
        /// All of the contacts in this field should fail immediately upon processing the test contacts in the
        /// <see cref="VcardParser.Parse()"/> function. This throws VCardParseException.
        /// </summary>
        public static IEnumerable<object[]> invalidContacts =>
        [
            [
                vcardInvalidAltIdUsage,
            ],
            [
                vcardInvalidType,
            ],
        ];

        /// <summary>
        /// All of the contacts in this field with invalid syntax or omitted requirements may be accepted by the
        /// <see cref="VcardParser.Parse()"/> function.
        /// </summary>
        public static IEnumerable<object[]> seemsValidContacts =>
        [
            [
                vcardTwoNoFullName,
            ],
            [
                vcardTwoWithUnsupportedParts,
            ],
            [
                vcardTwoWithIndirectVersionSpecify,
            ],
        ];

        /// <summary>
        /// All of the contacts in this field should fail immediately upon calling <see cref="VcardParser.Parse()"/>.
        /// These usually resemble contacts with invalid syntax. This throws InvalidDataException.
        /// </summary>
        public static IEnumerable<object[]> invalidContactsParser =>
        [
            [
                vcardThreeNoFullName,
            ],
            [
                vcardTwoBarren,
            ],
            [
                vcardThreeBarren,
            ],
            [
                vcardFourBarren,
            ],
            [
                vcardFiveBarren,
            ],
            [
                vcardNonexistentVersion,
            ],
            [
                vcardZeroByte,
            ],
            [
                vcardInvalidVersion,
            ],
        ];
    }
}
