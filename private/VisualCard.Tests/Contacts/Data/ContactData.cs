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

namespace VisualCard.Tests.Contacts.Data
{
    public static class ContactData
    {
        #region singleVcardTwoContactShort
        internal static readonly string singleMeCardContactShort =
            """
            MECARD:N:Hood,Rick,,,;;
            """
        ;

        internal static readonly string singleMeCardContactShortReparsed =
            """
            MECARD:N:Hood,Rick;;
            """
        ;

        internal static readonly string singleMeCardContactShortReparsedCompatibility =
            """
            MECARD:N:Hood,Rick;;
            """
        ;

        internal static readonly string singleVcardContactShortFromMeCard =
            """
            BEGIN:VCARD
            VERSION:3.0
            FN:Rick Hood
            N:Hood;Rick;;;
            END:VCARD

            """
        ;

        internal static readonly string singleVcardTwoContactShort =
            """
            BEGIN:VCARD
            VERSION:2.1
            N:Hood;Rick;;;
            FN:Rick Hood
            END:VCARD
            """
        ;
        #endregion

        #region singleVcardThreeContactShort
        internal static readonly string singleVcardThreeContactShort =
            """
            BEGIN:VCARD
            VERSION:3.0
            N:Hood;Rick;;;
            FN:Rick Hood
            END:VCARD
            """
        ;
        #endregion

        #region singleVcardFourContactShort
        internal static readonly string singleVcardFourContactShort =
            """
            BEGIN:VCARD
            VERSION:4.0
            N:Hood;Rick;;;
            FN:Rick Hood
            END:VCARD
            """
        ;
        #endregion

        #region singleVcardFiveContactShort
        internal static readonly string singleVcardFiveContactShort =
            """
            BEGIN:VCARD
            VERSION:5.0
            N:Hood;Rick;;;
            FN:Rick Hood
            END:VCARD
            """
        ;
        #endregion

        #region singleVcardTwoContact
        internal static readonly string singleMeCardContact =
            """
            MECARD:N:Sanders,John,,,;TEL:495-522-3560;EMAIL:john.s@acme.co;ADR:,,Los Angeles,,,,USA;NOTE:Note test for VisualCard;;
            """
        ;

        internal static readonly string singleMeCardContactFull =
            """
            MECARD:N:Sanders,John,,,;SOUND:Saunders,John;TEL:495-522-3560;TEL-AV:495-522-3550;EMAIL:john.s@acme.co;ADR:,,Los Angeles,,,,USA;NOTE:Note test for VisualCard;;
            """
        ;

        internal static readonly string singleMeCardContactReparsed =
            """
            MECARD:N:Sanders,John;TEL:495-522-3560;EMAIL:john.s@acme.co;NOTE:Note test for VisualCard;ADR:,,Los Angeles,,,,USA;;
            """
        ;

        internal static readonly string singleMeCardContactFullReparsed =
            """
            MECARD:N:Sanders,John;SOUND:Saunders,John;TEL:495-522-3560;TEL-AV:495-522-3550;EMAIL:john.s@acme.co;NOTE:Note test for VisualCard;ADR:,,Los Angeles,,,,USA;;
            """
        ;

        internal static readonly string singleMeCardContactReparsedCompatibility =
            """
            MECARD:N:Sanders,John;TEL:495-522-3560;EMAIL:john.s@acme.co;ADR:,,Los Angeles,,,,USA;;
            """
        ;

        internal static readonly string singleMeCardContactFullReparsedCompatibility =
            """
            MECARD:N:Sanders,John;SOUND:Saunders,John;TEL:495-522-3560;EMAIL:john.s@acme.co;ADR:,,Los Angeles,,,,USA;;
            """
        ;

        internal static readonly string singleVcardContactFromMeCard =
            """
            BEGIN:VCARD
            VERSION:3.0
            TEL:495-522-3560
            EMAIL:john.s@acme.co
            NOTE:Note test for VisualCard
            FN:John Sanders
            N:Sanders;John;;;
            ADR:;;Los Angeles;;;;USA
            END:VCARD

            """
        ;

        internal static readonly string singleVcardContactFullFromMeCard =
            """
            BEGIN:VCARD
            VERSION:3.0
            TEL:495-522-3560
            TEL;TYPE=VIDEO:495-522-3550
            EMAIL:john.s@acme.co
            NOTE:Note test for VisualCard
            FN:John Sanders
            N:Sanders;John;;;
            X-VISUALCARD-KANA:Saunders;John
            ADR:;;Los Angeles;;;;USA
            END:VCARD

            """
        ;

        internal static readonly string singleVcardTwoContact =
            """
            BEGIN:VCARD
            VERSION:2.1
            N:Sanders;John;;;
            FN:John Sanders
            X-PHONETIC-FIRST-NAME:Saunders
            X-PHONETIC-LAST-NAME:John
            X-ANDROID-CUSTOM:vnd.android.cursor.item/nickname;JS;1;;;;;;;;;;;;;
            TEL;TYPE=CELL:495-522-3560
            EMAIL;HOME:john.s@acme.co
            ADR;HOME:;;Los Angeles, USA;;;;
            ORG:Acme Co.
            TITLE:Product Manager
            NOTE:Note test for VisualCard
            X-AIM:john.s
            END:VCARD
            """
        ;
        #endregion

        #region singleVcardThreeContact
        internal static readonly string singleVcardThreeContact =
            """
            BEGIN:VCARD
            VERSION:3.0
            N:Sanders;John;;;
            FN:John Sanders
            X-PHONETIC-FIRST-NAME:Saunders
            X-PHONETIC-LAST-NAME:John
            NICKNAME:JS
            TEL;TYPE=cell:495-522-3560
            EMAIL;TYPE=home:john.s@acme.co
            ADR;TYPE=home:;;Los Angeles, USA;;;;
            ORG:Acme Co.
            TITLE:Product Manager
            NOTE:Note test for VisualCard
            X-AIM:john.s
            END:VCARD
            """
        ;
        #endregion

        #region singleVcardFourContact
        internal static readonly string singleVcardFourContact =
            """
            BEGIN:VCARD
            VERSION:4.0
            ADR;TYPE=home:;;Los Angeles\, USA;;;;
            EMAIL;TYPE=HOME:john.s@acme.co
            FN:John Sanders
            IMPP:aim:john.s
            N:Sanders;John;;;
            NOTE:Note test for VisualCard
            ORG:Acme Co.
            TEL;TYPE=cell:495-522-3560
            TITLE:Product Manager
            X-ANDROID-CUSTOM:vnd.android.cursor.item/nickname;JS;1;;;;;;;;;;;;;
            X-PHONETIC-FIRST-NAME:Saunders
            X-PHONETIC-LAST-NAME:John
            END:VCARD
            """
        ;
        #endregion

        #region singleVcardFiveContact
        internal static readonly string singleVcardFiveContact =
            """
            BEGIN:VCARD
            VERSION:5.0
            ADR;TYPE=home:;;Los Angeles\, USA;;;;
            EMAIL;TYPE=HOME:john.s@acme.co
            FN:John Sanders
            IMPP:aim:john.s
            N:Sanders;John;;;
            NOTE:Note test for VisualCard
            ORG:Acme Co.
            TEL;TYPE=cell:495-522-3560
            TITLE:Product Manager
            X-ANDROID-CUSTOM:vnd.android.cursor.item/nickname;JS;1;;;;;;;;;;;;;
            X-PHONETIC-FIRST-NAME:Saunders
            X-PHONETIC-LAST-NAME:John
            SORT-STRING:johnsanders
            END:VCARD
            """
        ;
        #endregion

        #region multipleVcardTwoContacts
        internal static readonly string multipleVcardTwoContacts =
            """
            BEGIN:VCARD
            VERSION:2.1
            N:Sanders;John;;;
            FN:John Sanders
            X-PHONETIC-FIRST-NAME:Saunders
            X-PHONETIC-LAST-NAME:John
            X-ANDROID-CUSTOM:vnd.android.cursor.item/nickname;JS;1;;;;;;;;;;;;;
            TEL;CELL:495-522-3560
            EMAIL;HOME:john.s@acme.co
            ADR;HOME:;;Los Angeles, USA;;;;
            ORG:Acme Co.
            TITLE:Product Manager
            NOTE:Note test for VisualCard
            X-AIM:john.s
            END:VCARD

            BEGIN:VCARD
            VERSION:2.1
            N:Santos;Sarah;;;
            FN:Sarah Santos
            TEL;CELL:589-210-1059
            EMAIL;HOME:sarah.s@gmail.com
            EMAIL;WORK:sarah.s@sso.org
            ADR;HOME:;;New York, USA;;;;
            ORG:Support Scammer Outcry Organization
            TITLE:Chief Executive Officer
            URL:https://sso.org/
            X-SIP:sip test
            END:VCARD

            BEGIN:VCARD
            VERSION:2.1
            N:Navasquillo;Neville;Neville,Nevile;Mr.;Jr.
            FN:Neville Navasquillo
            X-ANDROID-CUSTOM:vnd.android.cursor.item/nickname;NVL.N;1;;;;;;;;;;;;;
            TEL;CELL:1-234-567-890
            TEL;WORK:098-765-4321
            TEL;VOICE:078-494-6434
            TEL;HOME:348-404-8404
            EMAIL;HOME:neville.nvs@gmail.com
            EMAIL;WORK:neville.nvs@nvsc.com
            ADR;WORK:POBOX;;Street Address ExtAddress;Reg;Loc;Postal;Country
            ADR;HOME:;;Street Address;;;;
            ORG:Organization
            TITLE:Title
            NOTE:Notes
            X-AIM;HOME:IM
            X-MSN;HOME:Windows LIVE
            X-YAHOO;HOME:Yahoo
            END:VCARD

            BEGIN:VCARD
            VERSION:2.1
            N:Hood;Rick;;;
            FN:Rick Hood
            END:VCARD
            """
        ;
        #endregion

        #region multipleVcardThreeContacts
        internal static readonly string multipleVcardThreeContacts =
            """
            BEGIN:VCARD
            VERSION:3.0
            N:Sanders;John;;;
            FN:John Sanders
            X-PHONETIC-FIRST-NAME:Saunders
            X-PHONETIC-LAST-NAME:John
            NICKNAME:JS
            TEL;TYPE=cell:495-522-3560
            EMAIL;TYPE=home:john.s@acme.co
            ADR;TYPE=home:;;Los Angeles, USA;;;;
            ORG:Acme Co.
            TITLE:Product Manager
            NOTE:Note test for VisualCard
            X-AIM:john.s
            END:VCARD

            BEGIN:VCARD
            VERSION:3.0
            N:Santos;Sarah;;;
            FN:Sarah Santos
            TEL;TYPE=cell:589-210-1059
            EMAIL;TYPE=home:sarah.s@gmail.com
            EMAIL;TYPE=work:sarah.s@sso.org
            ADR;TYPE=home:;;New York, USA;;;;
            ORG:Support Scammer Outcry Organization
            TITLE:Chief Executive Officer
            URL:https://sso.org/
            X-SIP:sip test
            END:VCARD

            BEGIN:VCARD
            VERSION:3.0
            N:Navasquillo;Neville;Neville,Nevile;Mr.;Jr.
            FN:Neville Navasquillo
            NICKNAME:NVL.N
            TEL;TYPE=cell:1-234-567-890
            TEL;TYPE=work:098-765-4321
            TEL;TYPE=voice:078-494-6434
            TEL;TYPE=home:348-404-8404
            EMAIL;TYPE=home:neville.nvs@gmail.com
            EMAIL;TYPE=work:neville.nvs@nvsc.com
            ADR;TYPE=work:POBOX;;Street Address ExtAddress;Reg;Loc;Postal;Country
            ADR;TYPE=home:;;Street Address;;;;
            ORG:Organization
            TITLE:Title
            NOTE:Notes
            X-AIM;TYPE=HOME:IM
            X-MSN;TYPE=HOME:Windows LIVE
            X-YAHOO;TYPE=HOME:Yahoo
            END:VCARD

            BEGIN:VCARD
            VERSION:3.0
            N:Hood;Rick;;;
            FN:Rick Hood
            END:VCARD
            """
        ;
        #endregion

        #region multipleVcardFourContacts
        internal static readonly string multipleVcardFourContacts =
            """
            BEGIN:VCARD
            VERSION:4.0
            FN:Rick Hood
            N:Hood;Rick;;;
            END:VCARD

            BEGIN:VCARD
            VERSION:4.0
            ADR;TYPE=work:POBOX;;Street Address ExtAddress;Reg;Loc;Postal;Country
            ADR;TYPE=home:;;Street Address;;;;
            EMAIL;TYPE=HOME:neville.nvs@gmail.com
            EMAIL;TYPE=WORK:neville.nvs@nvsc.com
            FN:Neville Navasquillo
            IMPP;TYPE=HOME:aim:IM
            IMPP;TYPE=HOME:msn:Windows LIVE
            IMPP;TYPE=HOME:ymsgr:Yahoo
            N;ALTID=0;LANGUAGE=en:Navasquillo;Neville;Neville\,Nevile;Mr.;Jr.
            N;ALTID=0;LANGUAGE=de:NAVASQUILLO;Neville;Neville\,Nevile;Mr.;Jr.
            NOTE:Notes
            ORG:Organization
            TEL;TYPE=work:098-765-4321
            TEL;TYPE=cell:1-234-567-890
            TEL;TYPE=voice:078-494-6434
            TEL;TYPE=home:348-404-8404
            TITLE:Title
            X-ANDROID-CUSTOM:vnd.android.cursor.item/nickname;NVL.N;1;;;;;;;;;;;;;
            END:VCARD

            BEGIN:VCARD
            VERSION:4.0
            ADR;TYPE=home:;;New York\, USA;;;;
            EMAIL;TYPE=HOME:sarah.s@gmail.com
            EMAIL;TYPE=WORK:sarah.s@sso.org
            FN:Sarah Santos
            N:Santos;Sarah;;;
            ORG:Support Scammer Outcry Organization
            TEL;TYPE=cell:589-210-1059
            TITLE:Chief Executive Officer
            URL:https://sso.org/
            BDAY:19890222
            X-SIP-SIP:sip test
            END:VCARD

            BEGIN:VCARD
            VERSION:4.0
            ADR;TYPE=home:;;Los Angeles\, USA;;;;
            EMAIL;TYPE=HOME:john.s@acme.co
            FN:John Sanders
            IMPP:aim:john.s
            N:Sanders;John;;;
            NOTE:Note test for VisualCard
            ORG:Acme Co.
            TEL;TYPE=cell:495-522-3560
            TITLE:Product Manager
            X-ANDROID-CUSTOM:vnd.android.cursor.item/nickname;JS;1;;;;;;;;;;;;;
            X-PHONETIC-FIRST-NAME:Saunders
            X-PHONETIC-LAST-NAME:John
            END:VCARD
            """
        ;
        #endregion

        #region multipleVcardFiveContacts
        internal static readonly string multipleVcardFiveContacts =
            """
            BEGIN:VCARD
            VERSION:5.0
            FN:Rick Hood
            N:Hood;Rick;;;
            END:VCARD

            BEGIN:VCARD
            VERSION:5.0
            ADR;TYPE=work:POBOX;;Street Address ExtAddress;Reg;Loc;Postal;Country
            ADR;TYPE=home:;;Street Address;;;;
            EMAIL;TYPE=HOME:neville.nvs@gmail.com
            EMAIL;TYPE=WORK:neville.nvs@nvsc.com
            FN:Neville Navasquillo
            IMPP;TYPE=HOME:aim:IM
            IMPP;TYPE=HOME:msn:Windows LIVE
            IMPP;TYPE=HOME:ymsgr:Yahoo
            N;ALTID=0;LANGUAGE=en:Navasquillo;Neville;Neville\,Nevile;Mr.;Jr.
            N;ALTID=0;LANGUAGE=de:NAVASQUILLO;Neville;Neville\,Nevile;Mr.;Jr.
            NOTE:Notes
            ORG:Organization
            TEL;TYPE=work:098-765-4321
            TEL;TYPE=cell:1-234-567-890
            TEL;TYPE=voice:078-494-6434
            TEL;TYPE=home:348-404-8404
            TITLE:Title
            X-ANDROID-CUSTOM:vnd.android.cursor.item/nickname;NVL.N;1;;;;;;;;;;;;;
            END:VCARD

            BEGIN:VCARD
            VERSION:5.0
            ADR;TYPE=home:;;New York\, USA;;;;
            EMAIL;TYPE=HOME:sarah.s@gmail.com
            EMAIL;TYPE=WORK:sarah.s@sso.org
            FN:Sarah Santos
            N:Santos;Sarah;;;
            ORG:Support Scammer Outcry Organization
            TEL;TYPE=cell:589-210-1059
            TITLE:Chief Executive Officer
            URL:https://sso.org/
            BDAY:19890222
            X-SIP-SIP:sip test
            SORT-STRING:sarahsantos
            END:VCARD

            BEGIN:VCARD
            VERSION:5.0
            ADR;TYPE=home:;;Los Angeles\, USA;;;;
            EMAIL;TYPE=HOME:john.s@acme.co
            FN:John Sanders
            IMPP:aim:john.s
            N:Sanders;John;;;
            NOTE:Note test for VisualCard
            ORG:Acme Co.
            TEL;TYPE=cell:495-522-3560
            TITLE:Product Manager
            X-ANDROID-CUSTOM:vnd.android.cursor.item/nickname;JS;1;;;;;;;;;;;;;
            X-PHONETIC-FIRST-NAME:Saunders
            X-PHONETIC-LAST-NAME:John
            SORT-STRING:johnsanders
            END:VCARD
            """
        ;
        #endregion

        #region vcardThreeOldSample
        internal static readonly string vcardThreeOldSample =
            """
            BEGIN:VCARD
            VERSION:3.0
            BDAY;VALUE=DATE:1963-09-21
            N:Stenerson;Derik
            FN:Derik Stenerson
            ORG:Microsoft Corporation
            ADR;TYPE=WORK,POSTAL,PARCEL:;;One Microsoft Way;Redmond;WA;98052-6399;USA
            TEL;TYPE=WORK,MSG:+1-425-936-5522
            TEL;TYPE=WORK,FAX:+1-425-936-7329
            EMAIL;TYPE=INTERNET:deriks@Microsoft.com
            END:VCARD
            BEGIN:VCARD
            VERSION:3.0
            N:Ganguly;Anik
            FN:Anik Ganguly
            ORG: Open Text Inc.
            ADR;TYPE=WORK,POSTAL,PARCEL:;Suite 101;38777 West Six Mile Road;Livonia;MI;48152;USA
            TEL;TYPE=WORK,MSG:+1-734-542-5955
            EMAIL;TYPE=INTERNET:ganguly@acm.org
            END:VCARD
            BEGIN:VCARD
            VERSION:3.0
            N:Moskowitz;Robert
            FN:Robert Moskowitz
            EMAIL;TYPE=INTERNET:rgm-ietf@htt-consult.com
            END:VCARD
            """
        ;
        #endregion

        /// <summary>
        /// Test VCard single contact contents (shorts)
        /// </summary>
        public static IEnumerable<object[]> singleVcardContactShorts =>
        [
            [
                singleVcardTwoContactShort,
            ],
            [
                singleVcardThreeContactShort,
            ],
            [
                singleVcardFourContactShort,
            ],
            [
                singleVcardFiveContactShort,
            ],
        ];

        /// <summary>
        /// Test VCard single contact contents
        /// </summary>
        public static IEnumerable<object[]> singleVcardContacts =>
        [
            [
                singleVcardTwoContact,
            ],
            [
                singleVcardThreeContact,
            ],
            [
                singleVcardFourContact,
            ],
            [
                singleVcardFiveContact,
            ],
        ];

        /// <summary>
        /// Test VCard multiple contact contents
        /// </summary>
        public static IEnumerable<object[]> multipleVcardContacts =>
        [
            [
                multipleVcardTwoContacts,
            ],
            [
                multipleVcardThreeContacts,
            ],
            [
                multipleVcardFourContacts,
            ],
            [
                multipleVcardFiveContacts,
            ],
        ];

        /// <summary>
        /// Test MeCard contacts
        /// </summary>
        public static IEnumerable<object[]> meCardContacts =>
        [
            [
                singleMeCardContactShort,
            ],
            [
                singleMeCardContact,
            ],
            [
                singleMeCardContactFull,
            ],
        ];

        /// <summary>
        /// Test MeCard contacts
        /// </summary>
        public static IEnumerable<object[]> meCardContactsReparsed =>
        [
            [
                (singleMeCardContactShort, singleMeCardContactShortReparsed),
            ],
            [
                (singleMeCardContact, singleMeCardContactReparsed),
            ],
            [
                (singleMeCardContactFull, singleMeCardContactFullReparsed),
            ],
        ];

        /// <summary>
        /// Test MeCard contacts
        /// </summary>
        public static IEnumerable<object[]> meCardContactsReparsedCompatibility =>
        [
            [
                (singleMeCardContactShort, singleMeCardContactShortReparsedCompatibility),
            ],
            [
                (singleMeCardContact, singleMeCardContactReparsedCompatibility),
            ],
            [
                (singleMeCardContactFull, singleMeCardContactFullReparsedCompatibility),
            ],
        ];

        /// <summary>
        /// Test MeCard contacts
        /// </summary>
        public static IEnumerable<object[]> vCardFromMeCardContacts =>
        [
            [
                (singleMeCardContactShort, singleVcardContactShortFromMeCard),
            ],
            [
                (singleMeCardContact, singleVcardContactFromMeCard),
            ],
            [
                (singleMeCardContactFull, singleVcardContactFullFromMeCard),
            ],
        ];

        /// <summary>
        /// All the remaining valid contacts
        /// </summary>
        public static IEnumerable<object[]> remainingContacts =>
        [
            [
                vcardThreeOldSample,
            ]
        ];
    }
}
