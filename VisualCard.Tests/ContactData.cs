//
// MIT License
//
// Copyright (c) 2021-2024 Aptivi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

using System;
using VisualCard.Parts;

namespace VisualCard.Tests
{
    public static class ContactData
    {
        #region singleVcardTwoContactShort
        private static readonly string singleMeCardContactShort =
            """
            MECARD:N:Hood,Rick,,,;;
            """
        ;

        private static readonly string singleVcardContactShortFromMeCard =
            """
            BEGIN:VCARD
            VERSION:3.0
            FN:Rick Hood
            N:Hood;Rick;;;
            END:VCARD

            """
        ;

        private static readonly string singleVcardTwoContactShort =
            """
            BEGIN:VCARD
            VERSION:2.1
            N:Hood;Rick;;;
            FN:Rick Hood
            END:VCARD
            """
        ;

        private static readonly Card singleVcardContactShortFromMeCardInstance = new
        (
            null,
            "3.0"
        )
        {
            ContactNames =
            [
                new NameInfo(0, [], "Rick", "Hood", [], [], [])
            ],
            ContactFullName = "Rick Hood"
        };

        private static readonly Card singleVcardTwoContactShortInstance = new
        (
            null,
            "2.1"
        )
        {
            ContactNames =
            [
                new NameInfo(0, [], "Rick", "Hood", [], [], [])
            ],
            ContactFullName = "Rick Hood"
        };
        #endregion

        #region singleVcardThreeContactShort
        private static readonly string singleVcardThreeContactShort =
            """
            BEGIN:VCARD
            VERSION:3.0
            N:Hood;Rick;;;
            FN:Rick Hood
            END:VCARD
            """
        ;

        private static readonly Card singleVcardThreeContactShortInstance = new
        (
            null,
            "3.0"
        )
        {
            ContactNames =
            [
                new NameInfo(0, [], "Rick", "Hood", [], [], [])
            ],
            ContactFullName = "Rick Hood"
        };
        #endregion

        #region singleVcardFourContactShort
        private static readonly string singleVcardFourContactShort =
            """
            BEGIN:VCARD
            VERSION:4.0
            N:Hood;Rick;;;
            FN:Rick Hood
            END:VCARD
            """
        ;

        private static readonly Card singleVcardFourContactShortInstance = new
        (
            null,
            "4.0"
        )
        {
            ContactNames =
            [
                new NameInfo(0, [], "Rick", "Hood", [], [], [])
            ],
            ContactFullName = "Rick Hood"
        };
        #endregion

        #region singleVcardFiveContactShort
        private static readonly string singleVcardFiveContactShort =
            """
            BEGIN:VCARD
            VERSION:5.0
            N:Hood;Rick;;;
            FN:Rick Hood
            END:VCARD
            """
        ;

        private static readonly Card singleVcardFiveContactShortInstance = new
        (
            null,
            "5.0"
        )
        {
            ContactNames =
            [
                new NameInfo(0, [], "Rick", "Hood", [], [], [])
            ],
            ContactFullName = "Rick Hood"
        };
        #endregion

        #region singleVcardTwoContact
        private static readonly string singleMeCardContact =
            """
            MECARD:N:Sanders,John,,,;TEL:495-522-3560;EMAIL:john.s@acme.co;ADR:,,Los Angeles,,,,USA;NOTE:Note test for VisualCard;;
            """
        ;

        private static readonly string singleVcardContactFromMeCard =
            """
            BEGIN:VCARD
            VERSION:3.0
            FN:John Sanders
            N:Sanders;John;;;
            TEL;TYPE=CELL:495-522-3560
            ADR;TYPE=HOME:;;Los Angeles;;;;USA
            EMAIL;TYPE=HOME:john.s@acme.co
            NOTE:Note test for VisualCard
            END:VCARD

            """
        ;

        private static readonly Card singleVcardContactInstanceFromMeCard = new
        (
            null,
            "3.0"
        )
        {
            ContactNames =
            [
                new NameInfo(0, [], "John", "Sanders", [], [], [])
            ],
            ContactFullName = "John Sanders",
            ContactTelephones =
            [
                new TelephoneInfo(0, [], ["CELL"], "495-522-3560")
            ],
            ContactAddresses =
            [
                new AddressInfo(0, [], ["HOME"], "", "", "Los Angeles", "", "", "", "USA")
            ],
            ContactNotes = "Note test for VisualCard",
            ContactMails =
            [
                new EmailInfo(0, [], ["HOME"], "john.s@acme.co")
            ]
        };

        private static readonly string singleVcardTwoContact =
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

        private static readonly Card singleVcardTwoContactInstance = new
        (
            null,
            "2.1"
        )
        {
            ContactNames =
            [
                new NameInfo(0, [], "John", "Sanders", [], [], [])
            ],
            ContactFullName = "John Sanders",
            ContactTelephones =
            [
                new TelephoneInfo(0, [], ["CELL"], "495-522-3560")
            ],
            ContactAddresses =
            [
                new AddressInfo(0, [], ["HOME"], "", "", "Los Angeles, USA", "", "", "", "")
            ],
            ContactOrganizations =
            [
                new OrganizationInfo(0, [], "Acme Co.", "", "", ["WORK"])
            ],
            ContactTitles =
            [
                new TitleInfo(0, [], "Product Manager")
            ],
            ContactNotes = "Note test for VisualCard",
            ContactMails =
            [
                new EmailInfo(0, [], ["HOME"], "john.s@acme.co")
            ],
            ContactXNames =
            [
                new XNameInfo(0, [], "PHONETIC-FIRST-NAME", ["Saunders"], []),
                new XNameInfo(0, [], "PHONETIC-LAST-NAME",  ["John"], []),
                new XNameInfo(0, [], "ANDROID-CUSTOM", ["vnd.android.cursor.item/nickname", "JS", "1", "", "", "", "", "", "", "", "", "", "", "", "", ""], []),
                new XNameInfo(0, [], "AIM", ["john.s"], []),
            ]
        };
        #endregion

        #region singleVcardThreeContact
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

        private static readonly Card singleVcardThreeContactInstance = new
        (
            null,
            "3.0"
        )
        {
            ContactNames =
            [
                new NameInfo(0, [], "John", "Sanders", [], [], [])
            ],
            ContactFullName = "John Sanders",
            ContactTelephones =
            [
                new TelephoneInfo(0, [], ["cell"], "495-522-3560")
            ],
            ContactAddresses =
            [
                new AddressInfo(0, [], ["home"], "", "", "Los Angeles, USA", "", "", "", "")
            ],
            ContactOrganizations =
            [
                new OrganizationInfo(0, [], "Acme Co.", "", "", ["WORK"])
            ],
            ContactTitles =
            [
                new TitleInfo(0, [], "Product Manager")
            ],
            ContactNotes = "Note test for VisualCard",
            ContactMails =
            [
                new EmailInfo(0, [], ["home"], "john.s@acme.co")
            ],
            ContactXNames =
            [
                new XNameInfo(0, [], "PHONETIC-FIRST-NAME", ["Saunders"], []),
                new XNameInfo(0, [], "PHONETIC-LAST-NAME",  ["John"], []),
                new XNameInfo(0, [], "AIM", ["john.s"], []),
            ],
            ContactNicknames =
            [
                new NicknameInfo(0, [], "JS", ["HOME"])
            ]
        };
        #endregion

        #region singleVcardFourContact
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

        private static readonly Card singleVcardFourContactInstance = new
        (
            null,
            "4.0"
        )
        {
            ContactNames =
            [
                new NameInfo(0, [], "John", "Sanders", [], [], [])
            ],
            ContactFullName = "John Sanders",
            ContactTelephones =
            [
                new TelephoneInfo(0, [], ["cell"], "495-522-3560")
            ],
            ContactAddresses =
            [
                new AddressInfo(0, [], ["home"], "", "", "Los Angeles, USA", "", "", "", "")
            ],
            ContactOrganizations =
            [
                new OrganizationInfo(0, [], "Acme Co.", "", "", ["WORK"])
            ],
            ContactTitles =
            [
                new TitleInfo(0, [], "Product Manager")
            ],
            ContactNotes = "Note test for VisualCard",
            ContactMails =
            [
                new EmailInfo(0, [], ["HOME"], "john.s@acme.co")
            ],
            ContactXNames =
            [
                new XNameInfo(0, [], "ANDROID-CUSTOM", ["vnd.android.cursor.item/nickname", "JS", "1", "", "", "", "", "", "", "", "", "", "", "", "", ""], []),
                new XNameInfo(0, [], "PHONETIC-FIRST-NAME", ["Saunders"], []),
                new XNameInfo(0, [], "PHONETIC-LAST-NAME",  ["John"], [])
            ],
            ContactImpps =
            [
                new ImppInfo(0, [], "aim:john.s", ["HOME"])
            ]
        };
        #endregion

        #region singleVcardFiveContact
        private static readonly string singleVcardFiveContact =
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

        private static readonly Card singleVcardFiveContactInstance = new
        (
            null,
            "5.0"
        )
        {
            ContactNames =
            [
                new NameInfo(0, [], "John", "Sanders", [], [], [])
            ],
            ContactFullName = "John Sanders",
            ContactTelephones =
            [
                new TelephoneInfo(0, [], ["cell"], "495-522-3560")
            ],
            ContactAddresses =
            [
                new AddressInfo(0, [], ["home"], "", "", "Los Angeles, USA", "", "", "", "")
            ],
            ContactOrganizations =
            [
                new OrganizationInfo(0, [], "Acme Co.", "", "", ["WORK"])
            ],
            ContactTitles =
            [
                new TitleInfo(0, [], "Product Manager")
            ],
            ContactNotes = "Note test for VisualCard",
            ContactMails =
            [
                new EmailInfo(0, [], ["HOME"], "john.s@acme.co")
            ],
            ContactXNames =
            [
                new XNameInfo(0, [], "ANDROID-CUSTOM", ["vnd.android.cursor.item/nickname", "JS", "1", "", "", "", "", "", "", "", "", "", "", "", "", ""], []),
                new XNameInfo(0, [], "PHONETIC-FIRST-NAME", ["Saunders"], []),
                new XNameInfo(0, [], "PHONETIC-LAST-NAME",  ["John"], [])
            ],
            ContactImpps =
            [
                new ImppInfo(0, [], "aim:john.s", ["HOME"])
            ],
            ContactSortString = "johnsanders"
        };
        #endregion

        #region multipleVcardTwoContacts
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

        private static readonly Card multipleVcardTwoContactsInstanceOne = singleVcardTwoContactInstance;
        private static readonly Card multipleVcardTwoContactsInstanceTwo = new
        (
            null,
            "2.1"
        )
        {
            ContactNames =
            [
                new NameInfo(0, [], "Sarah", "Santos", [], [], [])
            ],
            ContactFullName = "Sarah Santos",
            ContactTelephones =
            [
                new TelephoneInfo(0, [], ["CELL"], "589-210-1059")
            ],
            ContactAddresses =
            [
                new AddressInfo(0, [], ["HOME"], "", "", "New York, USA", "", "", "", "")
            ],
            ContactOrganizations =
            [
                new OrganizationInfo(0, [], "Support Scammer Outcry Organization", "", "", ["WORK"])
            ],
            ContactTitles =
            [
                new TitleInfo(0, [], "Chief Executive Officer")
            ],
            ContactURL = "https://sso.org/",
            ContactMails =
            [
                new EmailInfo(0, [], ["HOME"], "sarah.s@gmail.com"),
                new EmailInfo(0, [], ["WORK"], "sarah.s@sso.org"),
            ],
            ContactXNames =
            [
                new XNameInfo(0, [], "SIP", ["sip test"], []),
            ]
        };
        private static readonly Card multipleVcardTwoContactsInstanceThree = new
        (
            null,
            "2.1"
        )
        {
            ContactNames =
            [
                new NameInfo(0, [], "Neville", "Navasquillo", ["Neville", "Nevile"], ["Mr."], ["Jr."])
            ],
            ContactFullName = "Neville Navasquillo",
            ContactTelephones =
            [
                new TelephoneInfo(0, [], ["CELL"], "1-234-567-890"),
                new TelephoneInfo(0, [], ["WORK"], "098-765-4321"),
                new TelephoneInfo(0, [], ["VOICE"], "078-494-6434"),
                new TelephoneInfo(0, [], ["HOME"], "348-404-8404"),
            ],
            ContactAddresses =
            [
                new AddressInfo(0, [], ["WORK"], "POBOX", "", "Street Address ExtAddress", "Reg", "Loc", "Postal", "Country"),
                new AddressInfo(0, [], ["HOME"], "", "", "Street Address", "", "", "", ""),
            ],
            ContactOrganizations =
            [
                new OrganizationInfo(0, [], "Organization", "", "", ["WORK"])
            ],
            ContactTitles =
            [
                new TitleInfo(0, [], "Title")
            ],
            ContactNotes = "Notes",
            ContactMails =
            [
                new EmailInfo(0, [], ["HOME"], "neville.nvs@gmail.com"),
                new EmailInfo(0, [], ["WORK"], "neville.nvs@nvsc.com"),
            ],
            ContactXNames =
            [
                new XNameInfo(0, [], "ANDROID-CUSTOM", ["vnd.android.cursor.item/nickname", "NVL.N", "1", "", "", "", "", "", "", "", "", "", "", "", "", ""], []),
                new XNameInfo(0, [], "AIM", ["IM"], ["HOME"]),
                new XNameInfo(0, [], "MSN", ["Windows LIVE"], ["HOME"]),
                new XNameInfo(0, [], "YAHOO", ["Yahoo"], ["HOME"]),
            ]
        };
        private static readonly Card multipleVcardTwoContactsInstanceFour = singleVcardTwoContactShortInstance;
        #endregion

        #region multipleVcardThreeContacts
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

        private static readonly Card multipleVcardThreeContactsInstanceOne = singleVcardThreeContactInstance;
        private static readonly Card multipleVcardThreeContactsInstanceTwo = new
        (
            null,
            "3.0"
        )
        {
            ContactNames =
            [
                new NameInfo(0, [], "Sarah", "Santos", [], [], [])
            ],
            ContactFullName = "Sarah Santos",
            ContactTelephones =
            [
                new TelephoneInfo(0, [], ["cell"], "589-210-1059")
            ],
            ContactAddresses =
            [
                new AddressInfo(0, [], ["home"], "", "", "New York, USA", "", "", "", "")
            ],
            ContactOrganizations =
            [
                new OrganizationInfo(0, [], "Support Scammer Outcry Organization", "", "", ["WORK"])
            ],
            ContactTitles =
            [
                new TitleInfo(0, [], "Chief Executive Officer")
            ],
            ContactURL = "https://sso.org/",
            ContactMails =
            [
                new EmailInfo(0, [], ["home"], "sarah.s@gmail.com"),
                new EmailInfo(0, [], ["work"], "sarah.s@sso.org"),
            ],
            ContactXNames =
            [
                new XNameInfo(0, [], "SIP", ["sip test"], []),
            ]
        };
        private static readonly Card multipleVcardThreeContactsInstanceThree = new
        (
            null,
            "3.0"
        )
        {
            ContactNames =
            [
                new NameInfo(0, [], "Neville", "Navasquillo", ["Neville", "Nevile"], ["Mr."], ["Jr."])
            ],
            ContactFullName = "Neville Navasquillo",
            ContactTelephones =
            [
                new TelephoneInfo(0, [], ["cell"], "1-234-567-890"),
                new TelephoneInfo(0, [], ["work"], "098-765-4321"),
                new TelephoneInfo(0, [], ["voice"], "078-494-6434"),
                new TelephoneInfo(0, [], ["home"], "348-404-8404"),
            ],
            ContactAddresses =
            [
                new AddressInfo(0, [], ["work"], "POBOX", "", "Street Address ExtAddress", "Reg", "Loc", "Postal", "Country"),
                new AddressInfo(0, [], ["home"], "", "", "Street Address", "", "", "", ""),
            ],
            ContactOrganizations =
            [
                new OrganizationInfo(0, [], "Organization", "", "", ["WORK"])
            ],
            ContactTitles =
            [
                new TitleInfo(0, [], "Title")
            ],
            ContactNotes = "Notes",
            ContactMails =
            [
                new EmailInfo(0, [], ["home"], "neville.nvs@gmail.com"),
                new EmailInfo(0, [], ["work"], "neville.nvs@nvsc.com"),
            ],
            ContactXNames =
            [
                new XNameInfo(0, [], "AIM", ["IM"], ["HOME"]),
                new XNameInfo(0, [], "MSN", ["Windows LIVE"], ["HOME"]),
                new XNameInfo(0, [], "YAHOO", ["Yahoo"], ["HOME"]),
            ],
            ContactNicknames =
            [
                new NicknameInfo(0, [], "NVL.N", ["HOME"])
            ]
        };
        private static readonly Card multipleVcardThreeContactsInstanceFour = singleVcardThreeContactShortInstance;
        #endregion

        #region multipleVcardFourContacts
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

        private static readonly Card multipleVcardFourContactsInstanceOne = singleVcardFourContactShortInstance;
        private static readonly Card multipleVcardFourContactsInstanceTwo = new
        (
            null,
            "4.0"
        )
        {
            ContactNames =
            [
                new NameInfo(0, [], "Neville", "Navasquillo", ["Neville", "Nevile"], ["Mr."], ["Jr."]),
                new NameInfo(0, ["LANGUAGE=de"], "Neville", "NAVASQUILLO", ["Neville", "Nevile"], ["Mr."], ["Jr."])
            ],
            ContactFullName = "Neville Navasquillo",
            ContactTelephones =
            [
                new TelephoneInfo(0, [], ["work"], "098-765-4321"),
                new TelephoneInfo(0, [], ["cell"], "1-234-567-890"),
                new TelephoneInfo(0, [], ["voice"], "078-494-6434"),
                new TelephoneInfo(0, [], ["home"], "348-404-8404"),
            ],
            ContactAddresses =
            [
                new AddressInfo(0, [], ["work"], "POBOX", "", "Street Address ExtAddress", "Reg", "Loc", "Postal", "Country"),
                new AddressInfo(0, [], ["home"], "", "", "Street Address", "", "", "", ""),
            ],
            ContactOrganizations =
            [
                new OrganizationInfo(0, [], "Organization", "", "", ["WORK"])
            ],
            ContactTitles =
            [
                new TitleInfo(0, [], "Title")
            ],
            ContactNotes = "Notes",
            ContactMails =
            [
                new EmailInfo(0, [], ["HOME"], "neville.nvs@gmail.com"),
                new EmailInfo(0, [], ["WORK"], "neville.nvs@nvsc.com"),
            ],
            ContactXNames =
            [
                new XNameInfo(0, [], "ANDROID-CUSTOM", ["vnd.android.cursor.item/nickname", "NVL.N", "1", "", "", "", "", "", "", "", "", "", "", "", "", ""], []),
            ],
            ContactImpps =
            [
                new ImppInfo(0, [], "aim:IM", ["HOME"]),
                new ImppInfo(0, [], "msn:Windows LIVE", ["HOME"]),
                new ImppInfo(0, [], "ymsgr:Yahoo", ["HOME"])
            ],
        };
        private static readonly Card multipleVcardFourContactsInstanceThree = new
        (
            null,
            "4.0"
        )
        {
            ContactNames =
            [
                new NameInfo(0, [], "Sarah", "Santos", [], [], [])
            ],
            ContactFullName = "Sarah Santos",
            ContactTelephones =
            [
                new TelephoneInfo(0, [], ["cell"], "589-210-1059")
            ],
            ContactAddresses =
            [
                new AddressInfo(0, [], ["home"], "", "", "New York, USA", "", "", "", "")
            ],
            ContactOrganizations =
            [
                new OrganizationInfo(0, [], "Support Scammer Outcry Organization", "", "", ["WORK"])
            ],
            ContactTitles =
            [
                new TitleInfo(0, [], "Chief Executive Officer")
            ],
            ContactURL = "https://sso.org/",
            ContactMails =
            [
                new EmailInfo(0, [], ["HOME"], "sarah.s@gmail.com"),
                new EmailInfo(0, [], ["WORK"], "sarah.s@sso.org"),
            ],
            ContactXNames =
            [
                new XNameInfo(0, [], "SIP-SIP", ["sip test"], []),
            ],
            ContactBirthdate = new DateTime(1989, 2, 22),
        };
        private static readonly Card multipleVcardFourContactsInstanceFour = singleVcardFourContactInstance;
        #endregion

        #region multipleVcardFiveContacts
        private static readonly string multipleVcardFiveContacts =
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

        private static readonly Card multipleVcardFiveContactsInstanceOne = singleVcardFiveContactShortInstance;
        private static readonly Card multipleVcardFiveContactsInstanceTwo = new
        (
            null,
            "5.0"
        )
        {
            ContactNames =
            [
                new NameInfo(0, [], "Neville", "Navasquillo", ["Neville", "Nevile"], ["Mr."], ["Jr."]),
                new NameInfo(0, ["LANGUAGE=de"], "Neville", "NAVASQUILLO", ["Neville", "Nevile"], ["Mr."], ["Jr."])
            ],
            ContactFullName = "Neville Navasquillo",
            ContactTelephones =
            [
                new TelephoneInfo(0, [], ["work"], "098-765-4321"),
                new TelephoneInfo(0, [], ["cell"], "1-234-567-890"),
                new TelephoneInfo(0, [], ["voice"], "078-494-6434"),
                new TelephoneInfo(0, [], ["home"], "348-404-8404"),
            ],
            ContactAddresses =
            [
                new AddressInfo(0, [], ["work"], "POBOX", "", "Street Address ExtAddress", "Reg", "Loc", "Postal", "Country"),
                new AddressInfo(0, [], ["home"], "", "", "Street Address", "", "", "", ""),
            ],
            ContactOrganizations =
            [
                new OrganizationInfo(0, [], "Organization", "", "", ["WORK"])
            ],
            ContactTitles =
            [
                new TitleInfo(0, [], "Title")
            ],
            ContactNotes = "Notes",
            ContactMails =
            [
                new EmailInfo(0, [], ["HOME"], "neville.nvs@gmail.com"),
                new EmailInfo(0, [], ["WORK"], "neville.nvs@nvsc.com"),
            ],
            ContactXNames =
            [
                new XNameInfo(0, [], "ANDROID-CUSTOM", ["vnd.android.cursor.item/nickname", "NVL.N", "1", "", "", "", "", "", "", "", "", "", "", "", "", ""], []),
            ],
            ContactImpps =
            [
                new ImppInfo(0, [], "aim:IM", ["HOME"]),
                new ImppInfo(0, [], "msn:Windows LIVE", ["HOME"]),
                new ImppInfo(0, [], "ymsgr:Yahoo", ["HOME"])
            ],
        };
        private static readonly Card multipleVcardFiveContactsInstanceThree = new
        (
            null,
            "5.0"
        )
        {
            ContactNames =
            [
                new NameInfo(0, [], "Sarah", "Santos", [], [], [])
            ],
            ContactFullName = "Sarah Santos",
            ContactTelephones =
            [
                new TelephoneInfo(0, [], ["cell"], "589-210-1059")
            ],
            ContactAddresses =
            [
                new AddressInfo(0, [], ["home"], "", "", "New York, USA", "", "", "", "")
            ],
            ContactOrganizations =
            [
                new OrganizationInfo(0, [], "Support Scammer Outcry Organization", "", "", ["WORK"])
            ],
            ContactTitles =
            [
                new TitleInfo(0, [], "Chief Executive Officer")
            ],
            ContactURL = "https://sso.org/",
            ContactMails =
            [
                new EmailInfo(0, [], ["HOME"], "sarah.s@gmail.com"),
                new EmailInfo(0, [], ["WORK"], "sarah.s@sso.org"),
            ],
            ContactXNames =
            [
                new XNameInfo(0, [], "SIP-SIP", ["sip test"], []),
            ],
            ContactBirthdate = new DateTime(1989, 2, 22),
            ContactSortString = "sarahsantos"
        };
        private static readonly Card multipleVcardFiveContactsInstanceFour = singleVcardFiveContactInstance;
        #endregion

        #region vcardThreeOldSample
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

        private static readonly Card vcardThreeOldSampleInstanceOne = new
        (
            null,
            "3.0"
        )
        {
            ContactNames =
            [
                new NameInfo(0, [], "Derik", "Stenerson", [], [], [])
            ],
            ContactFullName = "Derik Stenerson",
            ContactTelephones =
            [
                new TelephoneInfo(0, [], ["WORK", "MSG"], "+1-425-936-5522"),
                new TelephoneInfo(0, [], ["WORK", "FAX"], "+1-425-936-7329")
            ],
            ContactAddresses =
            [
                new AddressInfo(0, [], ["WORK", "POSTAL", "PARCEL"], "", "", "One Microsoft Way", "Redmond", "WA", "98052-6399", "USA")
            ],
            ContactOrganizations =
            [
                new OrganizationInfo(0, [], "Microsoft Corporation", "", "", ["WORK"])
            ],
            ContactMails =
            [
                new EmailInfo(0, [], ["INTERNET"], "deriks@Microsoft.com")
            ],
            ContactBirthdate = new DateTime(1963, 9, 21),
        };
        private static readonly Card vcardThreeOldSampleInstanceTwo = new
        (
            null,
            "3.0"
        )
        {
            ContactNames =
            [
                new NameInfo(0, [], "Anik", "Ganguly", [], [], [])
            ],
            ContactFullName = "Anik Ganguly",
            ContactTelephones =
            [
                new TelephoneInfo(0, [], ["WORK", "MSG"], "+1-734-542-5955")
            ],
            ContactAddresses =
            [
                new AddressInfo(0, [], ["WORK", "POSTAL", "PARCEL"], "", "Suite 101", "38777 West Six Mile Road", "Livonia", "MI", "48152", "USA")
            ],
            ContactOrganizations =
            [
                new OrganizationInfo(0, [], " Open Text Inc.", "", "", ["WORK"])
            ],
            ContactMails =
            [
                new EmailInfo(0, [], ["INTERNET"], "ganguly@acm.org")
            ]
        };
        private static readonly Card vcardThreeOldSampleInstanceThree = new
        (
            null,
            "3.0"
        )
        {
            ContactNames =
            [
                new NameInfo(0, [], "Robert", "Moskowitz", [], [], [])
            ],
            ContactFullName = "Robert Moskowitz",
            ContactMails =
            [
                new EmailInfo(0, [], ["INTERNET"], "rgm-ietf@htt-consult.com")
            ]
        };
        #endregion

        /// <summary>
        /// Test VCard single contact contents (shorts)
        /// </summary>
        public static readonly string[] singleVcardContactShorts =
        [
            singleVcardTwoContactShort,
            singleVcardThreeContactShort,
            singleVcardFourContactShort,
            singleVcardFiveContactShort,
        ];

        /// <summary>
        /// Test VCard single contact contents
        /// </summary>
        public static readonly string[] singleVcardContacts =
        [
            singleVcardTwoContact,
            singleVcardThreeContact,
            singleVcardFourContact,
            singleVcardFiveContact,
        ];

        /// <summary>
        /// Test VCard multiple contact contents
        /// </summary>
        public static readonly string[] multipleVcardContacts =
        [
            multipleVcardTwoContacts,
            multipleVcardThreeContacts,
            multipleVcardFourContacts,
            multipleVcardFiveContacts,
        ];

        /// <summary>
        /// Test MeCard contacts
        /// </summary>
        public static readonly string[] meCardContacts =
        [
            singleMeCardContactShort,
            singleMeCardContact,
        ];

        /// <summary>
        /// Test MeCard contacts
        /// </summary>
        public static readonly (string, string)[] vCardFromMeCardContacts =
        [
            (singleMeCardContactShort, singleVcardContactShortFromMeCard),
            (singleMeCardContact, singleVcardContactFromMeCard),
        ];

        /// <summary>
        /// All the remaining valid contacts
        /// </summary>
        public static readonly string[] remainingContacts =
        [
            vcardThreeOldSample,
        ];

        /// <summary>
        /// VCard <see cref="Card"/> instances for equality check
        /// </summary>
        public static readonly Card[] vCardContactsInstances =
        [
            singleVcardTwoContactShortInstance,
            singleVcardThreeContactShortInstance,
            singleVcardFourContactShortInstance,
            singleVcardFiveContactShortInstance,
            singleVcardTwoContactInstance,
            singleVcardThreeContactInstance,
            singleVcardFourContactInstance,
            singleVcardFiveContactInstance,
            multipleVcardTwoContactsInstanceOne,
            multipleVcardTwoContactsInstanceTwo,
            multipleVcardTwoContactsInstanceThree,
            multipleVcardTwoContactsInstanceFour,
            multipleVcardThreeContactsInstanceOne,
            multipleVcardThreeContactsInstanceTwo,
            multipleVcardThreeContactsInstanceThree,
            multipleVcardThreeContactsInstanceFour,
            multipleVcardFourContactsInstanceOne,
            multipleVcardFourContactsInstanceTwo,
            multipleVcardFourContactsInstanceThree,
            multipleVcardFourContactsInstanceFour,
            multipleVcardFiveContactsInstanceOne,
            multipleVcardFiveContactsInstanceTwo,
            multipleVcardFiveContactsInstanceThree,
            multipleVcardFiveContactsInstanceFour,
            vcardThreeOldSampleInstanceOne,
            vcardThreeOldSampleInstanceTwo,
            vcardThreeOldSampleInstanceThree,
            singleVcardContactInstanceFromMeCard,
            singleVcardContactShortFromMeCardInstance,
        ];
    }
}
