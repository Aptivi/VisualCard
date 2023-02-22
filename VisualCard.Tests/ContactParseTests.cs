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

global using NUnit.Framework;
using Shouldly;
using System.Collections.Generic;
using System.IO;
using VisualCard.Parsers;

namespace VisualCard.Tests
{
    public class ContactParseTests
    {
        [Test]
        [TestCaseSource(typeof(ContactData), nameof(ContactData.singleVcardContactShorts))]
        [TestCaseSource(typeof(ContactData), nameof(ContactData.singleVcardContacts))]
        [TestCaseSource(typeof(ContactData), nameof(ContactData.multipleVcardContacts))]
        [TestCaseSource(typeof(ContactData), nameof(ContactData.remainingContacts))]
        public void GetCardParsersFromDifferentContacts(string cardText)
            => Should.NotThrow(() => CardTools.GetCardParsersFromString(cardText));

        [Test]
        [TestCaseSource(typeof(ContactData), nameof(ContactData.singleVcardContactShorts))]
        [TestCaseSource(typeof(ContactData), nameof(ContactData.singleVcardContacts))]
        [TestCaseSource(typeof(ContactData), nameof(ContactData.multipleVcardContacts))]
        [TestCaseSource(typeof(ContactData), nameof(ContactData.remainingContacts))]
        public void ParseDifferentContacts(string cardText)
        {
            List<BaseVcardParser> parsers = new();
            Should.NotThrow(() => parsers = CardTools.GetCardParsersFromString(cardText));
            foreach (BaseVcardParser parser in parsers)
                Should.NotThrow(parser.Parse);
        }

        [Test]
        [TestCaseSource(typeof(ContactDataBogus), nameof(ContactDataBogus.invalidContacts))]
        public void InvalidContactShouldThrowWhenGettingCardParsers(string cardText)
            => Should.Throw<InvalidDataException>(() => CardTools.GetCardParsersFromString(cardText));

        [Test]
        [TestCaseSource(typeof(ContactDataBogus), nameof(ContactDataBogus.invalidContactsParser))]
        public void InvalidContactShouldThrowWhenParsing(string cardText)
        {
            List<BaseVcardParser> parsers = new();
            Should.NotThrow(() => parsers = CardTools.GetCardParsersFromString(cardText));
            foreach (BaseVcardParser parser in parsers)
                Should.Throw<InvalidDataException>(parser.Parse);
        }

        [Test]
        [TestCaseSource(typeof(ContactDataBogus), nameof(ContactDataBogus.seemsValidContacts))]
        public void BogusButSeemsValidShouldNotThrowWhenGettingCardParsers(string cardText)
            => Should.NotThrow(() => CardTools.GetCardParsersFromString(cardText));

        [Test]
        [TestCaseSource(typeof(ContactDataBogus), nameof(ContactDataBogus.seemsValidContacts))]
        public void BogusButSeemsValidShouldNotThrowWhenParsing(string cardText)
        {
            List<BaseVcardParser> parsers = new();
            Should.NotThrow(() => parsers = CardTools.GetCardParsersFromString(cardText));
            foreach (BaseVcardParser parser in parsers)
                Should.NotThrow(parser.Parse);
        }
    }
}