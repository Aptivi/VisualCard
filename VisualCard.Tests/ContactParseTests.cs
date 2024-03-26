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

global using NUnit.Framework;
using Shouldly;
using System.Collections.Generic;
using System.IO;
using VisualCard.Converters;
using VisualCard.Parsers;
using VisualCard.Parts;

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
            List<BaseVcardParser> parsers = [];
            Should.NotThrow(() => parsers = CardTools.GetCardParsersFromString(cardText));
            foreach (BaseVcardParser parser in parsers)
                Should.NotThrow(parser.Parse);
        }

        [Test]
        [TestCaseSource(typeof(ContactData), nameof(ContactData.singleVcardContactShorts))]
        [TestCaseSource(typeof(ContactData), nameof(ContactData.singleVcardContacts))]
        [TestCaseSource(typeof(ContactData), nameof(ContactData.multipleVcardContacts))]
        [TestCaseSource(typeof(ContactData), nameof(ContactData.remainingContacts))]
        public void ParseDifferentContactsAndTestEquality(string cardText)
        {
            List<BaseVcardParser> parsers = [];
            List<Card> cards = [];

            // Parse the cards
            Should.NotThrow(() => parsers = CardTools.GetCardParsersFromString(cardText));
            foreach (BaseVcardParser parser in parsers)
                cards.Add(Should.NotThrow(parser.Parse));

            // Test equality with available data
            List<bool> foundCards = [];
            foreach (Card card in cards)
            {
                bool found = false;
                foreach (Card expectedCard in ContactData.vCardContactsInstances)
                    if (expectedCard == card)
                    {
                        found = true;
                        break;
                    }
                foundCards.Add(found);
            }
            foundCards.ShouldAllBe((b) => b);
        }

        [Test]
        [TestCaseSource(typeof(ContactData), nameof(ContactData.singleVcardContactShorts))]
        [TestCaseSource(typeof(ContactData), nameof(ContactData.singleVcardContacts))]
        [TestCaseSource(typeof(ContactData), nameof(ContactData.multipleVcardContacts))]
        [TestCaseSource(typeof(ContactData), nameof(ContactData.remainingContacts))]
        public void ParseDifferentContactsSaveToStringAndTestEquality(string cardText)
        {
            List<BaseVcardParser> parsers = [];
            List<Card> cards = [];
            List<Card> savedCards = [];

            // Parse the cards
            Should.NotThrow(() => parsers = CardTools.GetCardParsersFromString(cardText));
            foreach (BaseVcardParser parser in parsers)
                cards.Add(Should.NotThrow(parser.Parse));

            // Save all the cards to strings and re-parse
            foreach (Card card in cards)
            {
                string saved = Should.NotThrow(card.SaveToString);
                Should.NotThrow(() => parsers = CardTools.GetCardParsersFromString(saved));
                foreach (BaseVcardParser parser in parsers)
                    savedCards.Add(Should.NotThrow(parser.Parse));
            }

            // Test equality with available data
            List<bool> foundCards = [];
            foreach (Card card in savedCards)
            {
                bool found = false;
                foreach (Card expectedCard in ContactData.vCardContactsInstances)
                    if (expectedCard == card)
                    {
                        found = true;
                        break;
                    }
                foundCards.Add(found);
            }
            foundCards.ShouldAllBe((b) => b);
        }

        [Test]
        [TestCaseSource(typeof(ContactData), nameof(ContactData.vCardFromMeCardContacts))]
        public void ParseAndCheckDifferentMeCardContacts((string, string) cardText)
        {
            List<BaseVcardParser> parsers = [];
            Should.NotThrow(() => parsers = MeCard.GetContactsFromMeCardString(cardText.Item1));
            Card card = default;
            foreach (BaseVcardParser parser in parsers)
                Should.NotThrow(() => card = parser.Parse());
            card.SaveToString().ShouldBe(cardText.Item2);
        }

        [Test]
        [TestCaseSource(typeof(ContactData), nameof(ContactData.meCardContacts))]
        public void ParseDifferentMeCardContacts(string cardText)
        {
            List<BaseVcardParser> parsers = [];
            Should.NotThrow(() => parsers = MeCard.GetContactsFromMeCardString(cardText));
            foreach (BaseVcardParser parser in parsers)
                Should.NotThrow(parser.Parse);
        }

        [Test]
        [TestCaseSource(typeof(ContactData), nameof(ContactData.meCardContacts))]
        public void ParseDifferentMeCardContactsAndTestEquality(string cardText)
        {
            List<BaseVcardParser> parsers = [];
            List<Card> cards = [];

            // Parse the cards
            Should.NotThrow(() => parsers = MeCard.GetContactsFromMeCardString(cardText));
            foreach (BaseVcardParser parser in parsers)
                cards.Add(Should.NotThrow(parser.Parse));

            // Test equality with available data
            List<bool> foundCards = [];
            foreach (Card card in cards)
            {
                bool found = false;
                foreach (Card expectedCard in ContactData.vCardContactsInstances)
                    if (expectedCard == card)
                    {
                        found = true;
                        break;
                    }
                foundCards.Add(found);
            }
            foundCards.ShouldAllBe((b) => b);
        }

        [Test]
        [TestCaseSource(typeof(ContactData), nameof(ContactData.meCardContacts))]
        public void ParseDifferentMeCardContactsSaveToStringAndTestEquality(string cardText)
        {
            List<BaseVcardParser> parsers = [];
            List<Card> cards = [];
            List<Card> savedCards = [];

            // Parse the cards
            Should.NotThrow(() => parsers = MeCard.GetContactsFromMeCardString(cardText));
            foreach (BaseVcardParser parser in parsers)
                cards.Add(Should.NotThrow(parser.Parse));

            // Save all the cards to strings and re-parse
            foreach (Card card in cards)
            {
                string saved = Should.NotThrow(card.SaveToString);
                Should.NotThrow(() => parsers = CardTools.GetCardParsersFromString(saved));
                foreach (BaseVcardParser parser in parsers)
                    savedCards.Add(Should.NotThrow(parser.Parse));
            }

            // Test equality with available data
            List<bool> foundCards = [];
            foreach (Card card in savedCards)
            {
                bool found = false;
                foreach (Card expectedCard in ContactData.vCardContactsInstances)
                    if (expectedCard == card)
                    {
                        found = true;
                        break;
                    }
                foundCards.Add(found);
            }
            foundCards.ShouldAllBe((b) => b);
        }

        [Test]
        [TestCaseSource(typeof(ContactDataBogus), nameof(ContactDataBogus.invalidContacts))]
        public void InvalidContactShouldThrowWhenGettingCardParsers(string cardText)
            => Should.Throw<InvalidDataException>(() => CardTools.GetCardParsersFromString(cardText));

        [Test]
        [TestCaseSource(typeof(ContactDataBogus), nameof(ContactDataBogus.invalidContactsParser))]
        public void InvalidContactShouldThrowWhenParsing(string cardText)
        {
            List<BaseVcardParser> parsers = [];
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
            List<BaseVcardParser> parsers = [];
            Should.NotThrow(() => parsers = CardTools.GetCardParsersFromString(cardText));
            foreach (BaseVcardParser parser in parsers)
                Should.NotThrow(parser.Parse);
        }
    }
}
