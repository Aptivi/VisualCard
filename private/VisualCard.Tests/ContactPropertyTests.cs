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
using VisualCard.Parts.Enums;
using VisualCard.Parts.Implementations;

namespace VisualCard.Tests
{
    [TestClass]
    public class ContactPropertyTests
    {
        [TestMethod]
        public void TestContactPropertySetString()
        {
            var cards = CardTools.GetCardsFromString(ContactData.singleVcardTwoContactShort);
            var card = cards[0];
            var fullName = card.GetString(StringsEnum.FullName)[0];
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
            var note = card.GetString(StringsEnum.Notes)[0];
            note.Value.ShouldBe("Note test for VisualCard");
            card.DeleteString(StringsEnum.Notes, 0);
            card.GetString(StringsEnum.Notes).ShouldBeEmpty();
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
