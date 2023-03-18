﻿/*
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

namespace VisualCard.Tests
{
    public static class ContactData
    {
        private static readonly string singleVcardTwoContactShort =
            """
            BEGIN:VCARD
            VERSION:2.1
            N:Hood;Rick;;;
            FN:Rick Hood
            END:VCARD
            """
        ;

        private static readonly string singleVcardThreeContactShort =
            """
            BEGIN:VCARD
            VERSION:3.0
            N:Hood;Rick;;;
            FN:Rick Hood
            END:VCARD
            """
        ;

        private static readonly string singleVcardFourContactShort =
            """
            BEGIN:VCARD
            VERSION:4.0
            N:Hood;Rick;;;
            FN:Rick Hood
            END:VCARD
            """
        ;

        private static readonly string singleVcardTwoContact =
            """
            BEGIN:VCARD
            VERSION:2.1
            N:Sanders;John;;;
            FN:John Sanders
            X-PHONETIC-FIRST-NAME;TYPE:Saunders
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

        private static readonly string singleVcardThreeContact =
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

        private static readonly string singleVcardFourContact =
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

        private static readonly string multipleVcardTwoContacts =
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

        private static readonly string multipleVcardThreeContacts =
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
            X-AIM;HOME:IM
            X-MSN;HOME:Windows LIVE
            X-YAHOO;HOME:Yahoo
            END:VCARD

            BEGIN:VCARD
            VERSION:3.0
            N:Hood;Rick;;;
            FN:Rick Hood
            END:VCARD
            """
        ;

        private static readonly string multipleVcardFourContacts =
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
            N:Navasquillo;Neville;Neville\,Nevile;Mr.;Jr.
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

        private static readonly string vcardThreeOldSample =
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

        /// <summary>
        /// Test VCard single contact contents (shorts)
        /// </summary>
        public static readonly string[] singleVcardContactShorts =
        {
            singleVcardTwoContactShort,
            singleVcardThreeContactShort,
            singleVcardFourContactShort,
        };

        /// <summary>
        /// Test VCard single contact contents
        /// </summary>
        public static readonly string[] singleVcardContacts =
        {
            singleVcardTwoContact,
            singleVcardThreeContact,
            singleVcardFourContact,
        };

        /// <summary>
        /// Test VCard multiple contact contents
        /// </summary>
        public static readonly string[] multipleVcardContacts =
        {
            multipleVcardTwoContacts,
            multipleVcardThreeContacts,
            multipleVcardFourContacts,
        };

        /// <summary>
        /// All the remaining valid contacts
        /// </summary>
        public static readonly string[] remainingContacts =
        {
            vcardThreeOldSample,
        };
    }
}