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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Collections.Generic;
using System.IO;
using VisualCard.Extras.Converters;
using VisualCard.Exceptions;
using VisualCard.Parts;
using VisualCard.Extras.Misc;
using VisualCard.Tests.Contacts.Data;

namespace VisualCard.Tests.Contacts
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
        public void ReparseDifferentContactsShorts(string cardText) =>
            ReparseDifferentContactsInternal(cardText, false);

        [TestMethod]
        [DynamicData(nameof(ContactData.singleVcardContacts), typeof(ContactData))]
        public void ReparseDifferentContacts(string cardText) =>
            ReparseDifferentContactsInternal(cardText, false);

        [TestMethod]
        [DynamicData(nameof(ContactData.multipleVcardContacts), typeof(ContactData))]
        public void ReparseDifferentContactsMultiple(string cardText) =>
            ReparseDifferentContactsInternal(cardText, false);

        [TestMethod]
        [DynamicData(nameof(ContactData.remainingContacts), typeof(ContactData))]
        public void ReparseDifferentContactsRemaining(string cardText) =>
            ReparseDifferentContactsInternal(cardText, false);

        [TestMethod]
        [DynamicData(nameof(ContactData.singleVcardContactShorts), typeof(ContactData))]
        public void ReparseDifferentContactsShortsWithVerify(string cardText) =>
            ReparseDifferentContactsInternal(cardText, true);

        [TestMethod]
        [DynamicData(nameof(ContactData.singleVcardContacts), typeof(ContactData))]
        public void ReparseDifferentContactsWithVerify(string cardText) =>
            ReparseDifferentContactsInternal(cardText, true);

        [TestMethod]
        [DynamicData(nameof(ContactData.multipleVcardContacts), typeof(ContactData))]
        public void ReparseDifferentContactsMultipleWithVerify(string cardText) =>
            ReparseDifferentContactsInternal(cardText, true);

        [TestMethod]
        [DynamicData(nameof(ContactData.remainingContacts), typeof(ContactData))]
        public void ReparseDifferentContactsRemainingWithVerify(string cardText) =>
            ReparseDifferentContactsInternal(cardText, true);

        internal void ReparseDifferentContactsInternal(string cardText, bool verify)
        {
            Card[] cards = [];
            Card[] secondCards;
            Should.NotThrow(() => cards = CardTools.GetCardsFromString(cardText));

            // Save all the cards to strings and re-parse
            foreach (Card card in cards)
            {
                string saved = Should.NotThrow(() => card.SaveToString(verify));
                Should.NotThrow(() => secondCards = CardTools.GetCardsFromString(saved));
            }
        }

        [TestMethod]
        public void ParseGeneratedContacts()
        {
            Card[] cards;
            Should.NotThrow(() => cards = CardGenerator.GenerateCards());
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void ReparseGeneratedContacts(bool verify)
        {
            Card[] cards = [];
            Should.NotThrow(() => cards = CardGenerator.GenerateCards());

            // Save all the cards to strings and re-parse
            foreach (Card card in cards)
            {
                string saved = Should.NotThrow(() => card.SaveToString(verify));
                Should.NotThrow(() => CardTools.GetCardsFromString(saved));
            }
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
            ParseDifferentContactsSaveToStringAndTestEqualityInternal(cardText, false);

        [TestMethod]
        [DynamicData(nameof(ContactData.singleVcardContacts), typeof(ContactData))]
        public void ParseDifferentContactsSaveToStringAndTestEquality(string cardText) =>
            ParseDifferentContactsSaveToStringAndTestEqualityInternal(cardText, false);

        [TestMethod]
        [DynamicData(nameof(ContactData.multipleVcardContacts), typeof(ContactData))]
        public void ParseDifferentContactsSaveToStringAndTestEqualityMultiple(string cardText) =>
            ParseDifferentContactsSaveToStringAndTestEqualityInternal(cardText, false);

        [TestMethod]
        [DynamicData(nameof(ContactData.remainingContacts), typeof(ContactData))]
        public void ParseDifferentContactsSaveToStringAndTestEqualityRemaining(string cardText) =>
            ParseDifferentContactsSaveToStringAndTestEqualityInternal(cardText, false);

        [TestMethod]
        [DynamicData(nameof(ContactData.singleVcardContactShorts), typeof(ContactData))]
        public void ParseDifferentContactsSaveToStringAndTestEqualityShortsWithVerify(string cardText) =>
            ParseDifferentContactsSaveToStringAndTestEqualityInternal(cardText, true);

        [TestMethod]
        [DynamicData(nameof(ContactData.singleVcardContacts), typeof(ContactData))]
        public void ParseDifferentContactsSaveToStringAndTestEqualityWithVerify(string cardText) =>
            ParseDifferentContactsSaveToStringAndTestEqualityInternal(cardText, true);

        [TestMethod]
        [DynamicData(nameof(ContactData.multipleVcardContacts), typeof(ContactData))]
        public void ParseDifferentContactsSaveToStringAndTestEqualityMultipleWithVerify(string cardText) =>
            ParseDifferentContactsSaveToStringAndTestEqualityInternal(cardText, true);

        [TestMethod]
        [DynamicData(nameof(ContactData.remainingContacts), typeof(ContactData))]
        public void ParseDifferentContactsSaveToStringAndTestEqualityRemainingWithVerify(string cardText) =>
            ParseDifferentContactsSaveToStringAndTestEqualityInternal(cardText, true);

        public void ParseDifferentContactsSaveToStringAndTestEqualityInternal(string cardText, bool verify)
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
                string saved = Should.NotThrow(() => card.SaveToString(verify));
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
        [DynamicData(nameof(ContactData.meCardContactsReparsed), typeof(ContactData))]
        public void ParseDifferentMeCardContactsAndReparse((string, string) cardText)
        {
            Card[] cards = [];
            Card[] secondCards = [];
            Should.NotThrow(() => cards = MeCard.GetContactsFromMeCardString(cardText.Item1));
            foreach (var card in cards)
            {
                string meCardSaved = MeCard.SaveCardToMeCardString(card);
                meCardSaved.ShouldBe(cardText.Item2);
                Should.NotThrow(() => secondCards = MeCard.GetContactsFromMeCardString(meCardSaved));
                secondCards[0].ShouldBe(card);
            }
        }

        [TestMethod]
        [DynamicData(nameof(ContactData.meCardContactsReparsedCompatibility), typeof(ContactData))]
        public void ParseDifferentMeCardContactsAndReparseCompatibility((string, string) cardText)
        {
            Card[] cards = [];
            Card[] secondCards = [];
            Should.NotThrow(() => cards = MeCard.GetContactsFromMeCardString(cardText.Item1));
            foreach (var card in cards)
            {
                string meCardSaved = MeCard.SaveCardToMeCardString(card, true);
                meCardSaved.ShouldBe(cardText.Item2);
                Should.NotThrow(() => secondCards = MeCard.GetContactsFromMeCardString(meCardSaved));
            }
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
        [DynamicData(nameof(ContactData.meCardContactsWithVerify), typeof(ContactData))]
        public void ParseDifferentMeCardContactsSaveToStringAndTestEquality(string cardText, bool verify)
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
                string saved = Should.NotThrow(() => card.SaveToString(verify));
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
        public void InvalidContactShouldThrowWhenParsingData(string cardText) =>
            Should.Throw<InvalidDataException>(() => CardTools.GetCardsFromString(cardText));

        [TestMethod]
        [DynamicData(nameof(ContactDataBogus.invalidContacts), typeof(ContactDataBogus))]
        public void InvalidContactShouldThrowWhenParsingVcard(string cardText) =>
            Should.Throw<VCardParseException>(() => CardTools.GetCardsFromString(cardText));

        [TestMethod]
        [DynamicData(nameof(ContactDataBogus.seemsValidContacts), typeof(ContactDataBogus))]
        public void BogusButSeemsValidShouldNotThrowWhenParsing(string cardText)
        {
            Card[] cards = [];
            Should.NotThrow(() => cards = CardTools.GetCardsFromString(cardText));
            foreach (Card card in cards)
                card.ShouldNotBeNull();
        }

        [TestMethod]
        [DynamicData(nameof(ContactDataBogus.seemsValidContacts), typeof(ContactDataBogus))]
        public void BogusButSeemsValidShouldNotThrowWhenParsingAndReparsing(string cardText)
        {
            Card[] cards = [];
            Card[] secondCards = [];
            Should.NotThrow(() => cards = CardTools.GetCardsFromString(cardText));
            foreach (Card card in cards)
                card.ShouldNotBeNull();

            // Save all the cards to strings and re-parse
            foreach (Card card in cards)
            {
                string saved = Should.NotThrow(() => card.SaveToString());
                Should.NotThrow(() => secondCards = CardTools.GetCardsFromString(saved));
            }
            foreach (Card card in secondCards)
                card.ShouldNotBeNull();
        }
    }
}
