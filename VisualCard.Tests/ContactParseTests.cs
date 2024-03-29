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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Collections.Generic;
using System.IO;
using VisualCard.Converters;
using VisualCard.Parsers;
using VisualCard.Parts;

namespace VisualCard.Tests
{
    [TestClass]
    public class ContactParseTests
    {
        [TestMethod]
        [DynamicData(nameof(ContactData.singleVcardContactShorts), typeof(ContactData))]
        public void ParseDifferentContactsShorts(string cardText) =>
            ParseDifferentContactsInternal(cardText);

        [TestMethod]
        [DynamicData(nameof(ContactData.singleVcardContacts), typeof(ContactData))]
        public void ParseDifferentContacts(string cardText) =>
            ParseDifferentContactsInternal(cardText);

        [TestMethod]
        [DynamicData(nameof(ContactData.multipleVcardContacts), typeof(ContactData))]
        public void ParseDifferentContactsMultiple(string cardText) =>
            ParseDifferentContactsInternal(cardText);

        [TestMethod]
        [DynamicData(nameof(ContactData.remainingContacts), typeof(ContactData))]
        public void ParseDifferentContactsRemaining(string cardText) =>
            ParseDifferentContactsInternal(cardText);

        internal void ParseDifferentContactsInternal(string cardText)
        {
            Card[] cards;
            Should.NotThrow(() => cards = CardTools.GetCardsFromString(cardText));
        }

        [TestMethod]
        [DynamicData(nameof(ContactData.singleVcardContactShorts), typeof(ContactData))]
        public void ParseDifferentContactsAndTestEqualityShorts(string cardText) =>
            ParseDifferentContactsAndTestEqualityInternal(cardText);

        [TestMethod]
        [DynamicData(nameof(ContactData.singleVcardContacts), typeof(ContactData))]
        public void ParseDifferentContactsAndTestEquality(string cardText) =>
            ParseDifferentContactsAndTestEqualityInternal(cardText);

        [TestMethod]
        [DynamicData(nameof(ContactData.multipleVcardContacts), typeof(ContactData))]
        public void ParseDifferentContactsAndTestEqualityMultiple(string cardText) =>
            ParseDifferentContactsAndTestEqualityInternal(cardText);

        [TestMethod]
        [DynamicData(nameof(ContactData.remainingContacts), typeof(ContactData))]
        public void ParseDifferentContactsAndTestEqualityRemaining(string cardText) =>
            ParseDifferentContactsAndTestEqualityInternal(cardText);

        internal void ParseDifferentContactsAndTestEqualityInternal(string cardText)
        {
            Card[] cards = [];
            Card[] secondCards = [];

            // Parse the cards
            Should.NotThrow(() => cards = CardTools.GetCardsFromString(cardText));
            Should.NotThrow(() => secondCards = CardTools.GetCardsFromString(cardText));

            // Test equality with available data
            List<bool> foundCards = [];
            foreach (Card card in cards)
            {
                bool found = false;
                foreach (Card second in secondCards)
                    if (second == card)
                    {
                        found = true;
                        break;
                    }
                foundCards.Add(found);
            }
            foundCards.ShouldAllBe((b) => b);
        }

        [TestMethod]
        [DynamicData(nameof(ContactData.singleVcardContactShorts), typeof(ContactData))]
        public void ParseDifferentContactsSaveToStringAndTestEqualityShorts(string cardText) =>
            ParseDifferentContactsSaveToStringAndTestEqualityInternal(cardText);

        [TestMethod]
        [DynamicData(nameof(ContactData.singleVcardContacts), typeof(ContactData))]
        public void ParseDifferentContactsSaveToStringAndTestEquality(string cardText) =>
            ParseDifferentContactsSaveToStringAndTestEqualityInternal(cardText);

        [TestMethod]
        [DynamicData(nameof(ContactData.multipleVcardContacts), typeof(ContactData))]
        public void ParseDifferentContactsSaveToStringAndTestEqualityMultiple(string cardText) =>
            ParseDifferentContactsSaveToStringAndTestEqualityInternal(cardText);

        [TestMethod]
        [DynamicData(nameof(ContactData.remainingContacts), typeof(ContactData))]
        public void ParseDifferentContactsSaveToStringAndTestEqualityRemaining(string cardText) =>
            ParseDifferentContactsSaveToStringAndTestEqualityInternal(cardText);

        public void ParseDifferentContactsSaveToStringAndTestEqualityInternal(string cardText)
        {
            List<Card> savedCards = [];
            Card[] cards = [];
            Card[] secondCards = [];

            // Parse the cards
            Should.NotThrow(() => cards = CardTools.GetCardsFromString(cardText));
            Should.NotThrow(() => secondCards = CardTools.GetCardsFromString(cardText));

            // Save all the cards to strings and re-parse
            foreach (Card card in cards)
            {
                string saved = Should.NotThrow(card.SaveToString);
                Should.NotThrow(() => secondCards = CardTools.GetCardsFromString(saved));
            }

            // Test equality with available data
            List<bool> foundCards = [];
            foreach (Card card in savedCards)
            {
                bool found = false;
                foreach (Card second in secondCards)
                    if (second == card)
                    {
                        found = true;
                        break;
                    }
                foundCards.Add(found);
            }
            foundCards.ShouldAllBe((b) => b);
        }

        [TestMethod]
        [DynamicData(nameof(ContactData.vCardFromMeCardContacts), typeof(ContactData))]
        public void ParseAndCheckDifferentMeCardContacts((string, string) cardText)
        {
            Card[] cards = [];
            Should.NotThrow(() => cards = MeCard.GetContactsFromMeCardString(cardText.Item1));
            Card card = cards[0];
            card.SaveToString().ShouldBe(cardText.Item2);
        }

        [TestMethod]
        [DynamicData(nameof(ContactData.meCardContacts), typeof(ContactData))]
        public void ParseDifferentMeCardContacts(string cardText)
        {
            Card[] cards;
            Should.NotThrow(() => cards = MeCard.GetContactsFromMeCardString(cardText));
        }

        [TestMethod]
        [DynamicData(nameof(ContactData.meCardContacts), typeof(ContactData))]
        public void ParseDifferentMeCardContactsAndTestEquality(string cardText)
        {
            Card[] cards = [];
            Card[] secondCards = [];

            // Parse the cards
            Should.NotThrow(() => cards = MeCard.GetContactsFromMeCardString(cardText));
            Should.NotThrow(() => secondCards = MeCard.GetContactsFromMeCardString(cardText));

            // Test equality with available data
            List<bool> foundCards = [];
            foreach (Card card in cards)
            {
                bool found = false;
                foreach (Card second in secondCards)
                    if (second == card)
                    {
                        found = true;
                        break;
                    }
                foundCards.Add(found);
            }
            foundCards.ShouldAllBe((b) => b);
        }

        [TestMethod]
        [DynamicData(nameof(ContactData.meCardContacts), typeof(ContactData))]
        public void ParseDifferentMeCardContactsSaveToStringAndTestEquality(string cardText)
        {
            List<Card> savedCards = [];
            Card[] cards = [];
            Card[] secondCards = [];

            // Parse the cards
            Should.NotThrow(() => cards = MeCard.GetContactsFromMeCardString(cardText));
            Should.NotThrow(() => secondCards = MeCard.GetContactsFromMeCardString(cardText));

            // Save all the cards to strings and re-parse
            foreach (Card card in cards)
            {
                string saved = Should.NotThrow(card.SaveToString);
                Should.NotThrow(() => secondCards = CardTools.GetCardsFromString(saved));
            }

            // Test equality with available data
            List<bool> foundCards = [];
            foreach (Card card in savedCards)
            {
                bool found = false;
                foreach (Card second in secondCards)
                    if (second == card)
                    {
                        found = true;
                        break;
                    }
                foundCards.Add(found);
            }
            foundCards.ShouldAllBe((b) => b);
        }

        [TestMethod]
        [DynamicData(nameof(ContactDataBogus.invalidContactsParser), typeof(ContactDataBogus))]
        public void InvalidContactShouldThrowWhenParsing(string cardText) =>
            Should.Throw<InvalidDataException>(() => CardTools.GetCardsFromString(cardText));

        [TestMethod]
        [DynamicData(nameof(ContactDataBogus.seemsValidContacts), typeof(ContactDataBogus))]
        public void BogusButSeemsValidShouldNotThrowWhenParsing(string cardText)
        {
            Card[] cards = [];
            Should.NotThrow(() => cards = CardTools.GetCardsFromString(cardText));
            foreach (Card card in cards)
                card.ShouldNotBeNull();
        }
    }
}
