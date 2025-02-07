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

namespace VisualCard.Tests
{
    [TestClass]
    public class ContactPropertyTests
    {
        [TestMethod]
        public void TestContactPropertySet()
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
    }
}
