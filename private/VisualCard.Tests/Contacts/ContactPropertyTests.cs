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
using VisualCard.Common.Parts.Enums;
using VisualCard.Common.Parts.Implementations;
using VisualCard.Parts.Enums;
using VisualCard.Parts.Implementations;
using VisualCard.Tests.Contacts.Data;

namespace VisualCard.Tests.Contacts
{
    [TestClass]
    public class ContactPropertyTests
    {
        [TestMethod]
        public void TestContactPropertyAddString()
        {
            var cards = CardTools.GetCardsFromString(ContactData.singleVcardTwoContactShort);
            var card = cards[0];
            card.AddString(CardStringsEnum.Notes, "Note test");
            card.GetString(CardStringsEnum.Notes).ShouldNotBeEmpty();
            var note = card.GetString(CardStringsEnum.Notes)[0];
            note.Value.ShouldBe("Note test");
            string cardStr = card.SaveToString();
            cardStr.ShouldContain("Note test");
        }
        
        [TestMethod]
        public void TestContactPropertyAddStringWithGroup()
        {
            var cards = CardTools.GetCardsFromString(ContactData.singleVcardTwoContactShort);
            var card = cards[0];
            card.AddString(CardStringsEnum.Notes, "Note test", "GROUP");
            card.GetString(CardStringsEnum.Notes).ShouldNotBeEmpty();
            var note = card.GetString(CardStringsEnum.Notes)[0];
            note.Value.ShouldBe("Note test");
            string cardStr = card.SaveToString();
            cardStr.ShouldContain("GROUP.NOTE");
            cardStr.ShouldContain("Note test");
        }
        
        [TestMethod]
        public void TestContactPropertyAddNonstandard()
        {
            var cards = CardTools.GetCardsFromString(ContactData.singleVcardTwoContactShort);
            var card = cards[0];
            card.AddPartToArray(CardPartsArrayEnum.NonstandardNames, "T;E;S;T", "", "CHARACTERS");
            card.GetExtraPartsArray<XNameInfo>().ShouldNotBeEmpty();
            var name = card.GetExtraPartsArray<XNameInfo>()[0];
            name.XKeyName.ShouldBe("CHARACTERS");
            name.XValues.ShouldNotBeNull();
            name.XValues.Length.ShouldBe(4);
            name.XValues[0].ShouldBe("T");
            name.XValues[1].ShouldBe("E");
            name.XValues[2].ShouldBe("S");
            name.XValues[3].ShouldBe("T");
            string cardStr = card.SaveToString();
            cardStr.ShouldContain("X-CHARACTERS");
            cardStr.ShouldContain("T;E;S;T");
        }
        
        [TestMethod]
        public void TestContactPropertyAddNonstandardWithGroup()
        {
            var cards = CardTools.GetCardsFromString(ContactData.singleVcardTwoContactShort);
            var card = cards[0];
            card.AddPartToArray(CardPartsArrayEnum.NonstandardNames, "T;E;S;T", "GROUP", "CHARACTERS");
            card.GetExtraPartsArray<XNameInfo>().ShouldNotBeEmpty();
            var name = card.GetExtraPartsArray<XNameInfo>()[0];
            name.XKeyName.ShouldBe("CHARACTERS");
            name.XValues.ShouldNotBeNull();
            name.XValues.Length.ShouldBe(4);
            name.XValues[0].ShouldBe("T");
            name.XValues[1].ShouldBe("E");
            name.XValues[2].ShouldBe("S");
            name.XValues[3].ShouldBe("T");
            string cardStr = card.SaveToString();
            cardStr.ShouldContain("GROUP.X-CHARACTERS");
            cardStr.ShouldContain("T;E;S;T");
        }
        
        [TestMethod]
        public void TestContactPropertyFindNonstandard()
        {
            var cards = CardTools.GetCardsFromString(ContactData.singleVcardTwoContactShort);
            var card = cards[0];
            card.AddPartToArray(CardPartsArrayEnum.NonstandardNames, "T;E;S;T", "", "CHARACTERS");
            card.FindExtraPartsArray<XNameInfo>("CHAR").ShouldNotBeEmpty();
            var name = card.FindExtraPartsArray<XNameInfo>("CHAR")[0];
            name.XKeyName.ShouldBe("CHARACTERS");
            name.XValues.ShouldNotBeNull();
            name.XValues.Length.ShouldBe(4);
            name.XValues[0].ShouldBe("T");
            name.XValues[1].ShouldBe("E");
            name.XValues[2].ShouldBe("S");
            name.XValues[3].ShouldBe("T");
            string cardStr = card.SaveToString();
            cardStr.ShouldContain("X-CHARACTERS");
            cardStr.ShouldContain("T;E;S;T");
        }
        
        [TestMethod]
        public void TestContactPropertyFindNonstandardWithGroup()
        {
            var cards = CardTools.GetCardsFromString(ContactData.singleVcardTwoContactShort);
            var card = cards[0];
            card.AddPartToArray(CardPartsArrayEnum.NonstandardNames, "T;E;S;T", "GROUP", "CHARACTERS");
            card.FindExtraPartsArray<XNameInfo>("CHAR").ShouldNotBeEmpty();
            var name = card.FindExtraPartsArray<XNameInfo>("CHAR")[0];
            name.XKeyName.ShouldBe("CHARACTERS");
            name.XValues.ShouldNotBeNull();
            name.XValues.Length.ShouldBe(4);
            name.XValues[0].ShouldBe("T");
            name.XValues[1].ShouldBe("E");
            name.XValues[2].ShouldBe("S");
            name.XValues[3].ShouldBe("T");
            string cardStr = card.SaveToString();
            cardStr.ShouldContain("GROUP.X-CHARACTERS");
            cardStr.ShouldContain("T;E;S;T");
        }
        
        [TestMethod]
        public void TestContactPropertyAddIana()
        {
            var cards = CardTools.GetCardsFromString(ContactData.singleVcardTwoContactShort);
            var card = cards[0];
            card.AddPartToArray(CardPartsArrayEnum.IanaNames, "T;E;S;T", "", "CHARACTERS");
            card.GetExtraPartsArray<ExtraInfo>().ShouldNotBeEmpty();
            var name = card.GetExtraPartsArray<ExtraInfo>()[0];
            name.KeyName.ShouldBe("CHARACTERS");
            name.Values.ShouldNotBeNull();
            name.Values.Length.ShouldBe(4);
            name.Values[0].ShouldBe("T");
            name.Values[1].ShouldBe("E");
            name.Values[2].ShouldBe("S");
            name.Values[3].ShouldBe("T");
            string cardStr = card.SaveToString();
            cardStr.ShouldContain("CHARACTERS");
            cardStr.ShouldContain("T;E;S;T");
        }
        
        [TestMethod]
        public void TestContactPropertyAddIanaWithGroup()
        {
            var cards = CardTools.GetCardsFromString(ContactData.singleVcardTwoContactShort);
            var card = cards[0];
            card.AddPartToArray(CardPartsArrayEnum.IanaNames, "T;E;S;T", "GROUP", "CHARACTERS");
            card.GetExtraPartsArray<ExtraInfo>().ShouldNotBeEmpty();
            var name = card.GetExtraPartsArray<ExtraInfo>()[0];
            name.KeyName.ShouldBe("CHARACTERS");
            name.Values.ShouldNotBeNull();
            name.Values.Length.ShouldBe(4);
            name.Values[0].ShouldBe("T");
            name.Values[1].ShouldBe("E");
            name.Values[2].ShouldBe("S");
            name.Values[3].ShouldBe("T");
            string cardStr = card.SaveToString();
            cardStr.ShouldContain("GROUP.CHARACTERS");
            cardStr.ShouldContain("T;E;S;T");
        }
        
        [TestMethod]
        public void TestContactPropertyFindIana()
        {
            var cards = CardTools.GetCardsFromString(ContactData.singleVcardTwoContactShort);
            var card = cards[0];
            card.AddPartToArray(CardPartsArrayEnum.IanaNames, "T;E;S;T", "", "CHARACTERS");
            card.FindExtraPartsArray<ExtraInfo>("CHAR").ShouldNotBeEmpty();
            var name = card.FindExtraPartsArray<ExtraInfo>("CHAR")[0];
            name.KeyName.ShouldBe("CHARACTERS");
            name.Values.ShouldNotBeNull();
            name.Values.Length.ShouldBe(4);
            name.Values[0].ShouldBe("T");
            name.Values[1].ShouldBe("E");
            name.Values[2].ShouldBe("S");
            name.Values[3].ShouldBe("T");
            string cardStr = card.SaveToString();
            cardStr.ShouldContain("CHARACTERS");
            cardStr.ShouldContain("T;E;S;T");
        }
        
        [TestMethod]
        public void TestContactPropertyFindIanaWithCharacters()
        {
            var cards = CardTools.GetCardsFromString(ContactData.singleVcardTwoContactShort);
            var card = cards[0];
            card.AddPartToArray(CardPartsArrayEnum.IanaNames, "T;E;S;T", "GROUP", "CHARACTERS");
            card.FindExtraPartsArray<ExtraInfo>("CHAR").ShouldNotBeEmpty();
            var name = card.FindExtraPartsArray<ExtraInfo>("CHAR")[0];
            name.KeyName.ShouldBe("CHARACTERS");
            name.Values.ShouldNotBeNull();
            name.Values.Length.ShouldBe(4);
            name.Values[0].ShouldBe("T");
            name.Values[1].ShouldBe("E");
            name.Values[2].ShouldBe("S");
            name.Values[3].ShouldBe("T");
            string cardStr = card.SaveToString();
            cardStr.ShouldContain("GROUP.CHARACTERS");
            cardStr.ShouldContain("T;E;S;T");
        }
        
        [TestMethod]
        public void TestContactPropertySetString()
        {
            var cards = CardTools.GetCardsFromString(ContactData.singleVcardTwoContactShort);
            var card = cards[0];
            var fullName = card.GetString(CardStringsEnum.FullName)[0];
            fullName.Value.ShouldBe("Rick Hood");
            fullName.Value = "Rick Rock";
            fullName.Value.ShouldBe("Rick Rock");
            string cardStr = card.SaveToString();
            cardStr.ShouldContain("Rick Rock");
        }

        [TestMethod]
        public void TestContactPropertyRemoveString()
        {
            var cards = CardTools.GetCardsFromString(ContactData.singleVcardTwoContact);
            var card = cards[0];
            var note = card.GetString(CardStringsEnum.Notes)[0];
            note.Value.ShouldBe("Note test for VisualCard");
            card.DeleteString(CardStringsEnum.Notes, 0);
            card.GetString(CardStringsEnum.Notes).ShouldBeEmpty();
            string cardStr = card.SaveToString();
            cardStr.ShouldNotContain("Note test for VisualCard");
        }

        [TestMethod]
        public void TestContactPropertyRemovePartsArray()
        {
            var cards = CardTools.GetCardsFromString(ContactData.singleVcardTwoContact);
            var card = cards[0];
            var note = card.GetPartsArray<AddressInfo>()[0];
            note.StreetAddress.ShouldBe("Los Angeles, USA");
            card.DeletePartsArray<AddressInfo>(0);
            card.GetPartsArray<AddressInfo>().ShouldBeEmpty();
            string cardStr = card.SaveToString();
            cardStr.ShouldNotContain("Los Angeles, USA");
        }
    }
}
