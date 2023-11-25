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

        /// <summary>
        /// All of the contacts in this field should fail immediately upon processing the test contacts in the
        /// <see cref="CardTools.GetCardParsers(System.IO.StreamReader)"/> function.
        /// </summary>
        public static readonly string[] invalidContacts =
        [
            vcardZeroByte,
            vcardNonexistentVersion,
        ];

        /// <summary>
        /// All of the contacts in this field with invalid syntax or omitted requirements may be accepted by the
        /// <see cref="CardTools.GetCardParsers(System.IO.StreamReader)"/> function.
        /// </summary>
        public static readonly string[] seemsValidContacts =
        [
            vcardTwoNoFullName,
        ];

        /// <summary>
        /// All of the contacts in this field should fail immediately upon calling <see cref="BaseVcardParser.Parse()"/>.
        /// These usually resemble contacts with invalid syntax.
        /// </summary>
        public static readonly string[] invalidContactsParser =
        [
            vcardThreeNoFullName,
            vcardTwoBarren,
            vcardThreeBarren,
            vcardFourBarren,
        ];
    }
}
