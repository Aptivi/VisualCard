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
using System.IO;
using Textify.General;
using VisualCard.Parts;
using VisualCard.Parts.Enums;
using VisualCard.Parts.Implementations;

namespace VisualCard.Tests.Contacts
{
    [TestClass]
    public class ContactMiscTests
    {
        [TestMethod]
        public void TestCreateNewCard21()
        {
            var card = new Card(new(2, 1));
            card.NestedCards.Count.ShouldBe(0);
            card.Strings.Count.ShouldBe(0);
            card.PartsArray.Count.ShouldBe(0);
            string[] savedLines = card.SaveToString().SplitNewLines(false);
            savedLines[0].ShouldBe("BEGIN:VCARD");
            savedLines[1].ShouldBe("VERSION:2.1");
            savedLines[2].ShouldBe("END:VCARD");
        }
        
        [TestMethod]
        public void TestCreateNewMinimalCard21()
        {
            var card = new Card(new(2, 1));
            card.NestedCards.Count.ShouldBe(0);
            card.Strings.Count.ShouldBe(0);
            card.PartsArray.Count.ShouldBe(0);
            card.AddPartToArray(CardPartsArrayEnum.Names, "Doherty;Alisha;;;");
            card.NestedCards.Count.ShouldBe(0);
            card.Strings.Count.ShouldBe(0);
            card.PartsArray.Count.ShouldBe(1);
            var name = card.GetPartsArray<NameInfo>()[0];
            name.ContactFirstName.ShouldBe("Alisha");
            name.ContactLastName.ShouldBe("Doherty");
            string[] savedLines = card.SaveToString(true).SplitNewLines(false);
            savedLines[0].ShouldBe("BEGIN:VCARD");
            savedLines[1].ShouldBe("VERSION:2.1");
            savedLines[2].ShouldBe("N:Doherty;Alisha;;;");
            savedLines[3].ShouldBe("END:VCARD");
        }

        [TestMethod]
        public void TestValidateNewCard21()
        {
            var card = new Card(new(2, 1));
            card.NestedCards.Count.ShouldBe(0);
            card.Strings.Count.ShouldBe(0);
            card.PartsArray.Count.ShouldBe(0);
            Should.Throw(card.Validate, typeof(InvalidDataException));
        }

        [TestMethod]
        public void TestCreateNewCard30()
        {
            var card = new Card(new(3, 0));
            card.NestedCards.Count.ShouldBe(0);
            card.Strings.Count.ShouldBe(0);
            card.PartsArray.Count.ShouldBe(0);
            string[] savedLines = card.SaveToString().SplitNewLines(false);
            savedLines[0].ShouldBe("BEGIN:VCARD");
            savedLines[1].ShouldBe("VERSION:3.0");
            savedLines[2].ShouldBe("END:VCARD");
        }

        [TestMethod]
        public void TestCreateNewMinimalCard30()
        {
            var card = new Card(new(3, 0));
            card.NestedCards.Count.ShouldBe(0);
            card.Strings.Count.ShouldBe(0);
            card.PartsArray.Count.ShouldBe(0);
            card.AddString(CardStringsEnum.FullName, "Alisha Doherty");
            card.AddPartToArray(CardPartsArrayEnum.Names, "Doherty;Alisha;;;");
            card.NestedCards.Count.ShouldBe(0);
            card.Strings.Count.ShouldBe(1);
            card.PartsArray.Count.ShouldBe(1);
            var fullName = card.GetString(CardStringsEnum.FullName)[0];
            fullName.Value.ShouldBe("Alisha Doherty");
            var name = card.GetPartsArray<NameInfo>()[0];
            name.ContactFirstName.ShouldBe("Alisha");
            name.ContactLastName.ShouldBe("Doherty");
            string[] savedLines = card.SaveToString(true).SplitNewLines(false);
            savedLines[0].ShouldBe("BEGIN:VCARD");
            savedLines[1].ShouldBe("VERSION:3.0");
            savedLines[2].ShouldBe("FN:Alisha Doherty");
            savedLines[3].ShouldBe("N:Doherty;Alisha;;;");
            savedLines[4].ShouldBe("END:VCARD");
        }

        [TestMethod]
        public void TestValidateNewCard30()
        {
            var card = new Card(new(3, 0));
            card.NestedCards.Count.ShouldBe(0);
            card.Strings.Count.ShouldBe(0);
            card.PartsArray.Count.ShouldBe(0);
            Should.Throw(card.Validate, typeof(InvalidDataException));
        }

        [TestMethod]
        public void TestCreateNewCard40()
        {
            var card = new Card(new(4, 0));
            card.NestedCards.Count.ShouldBe(0);
            card.Strings.Count.ShouldBe(0);
            card.PartsArray.Count.ShouldBe(0);
            string[] savedLines = card.SaveToString().SplitNewLines(false);
            savedLines[0].ShouldBe("BEGIN:VCARD");
            savedLines[1].ShouldBe("VERSION:4.0");
            savedLines[2].ShouldBe("END:VCARD");
        }

        [TestMethod]
        public void TestCreateNewMinimalCard40()
        {
            var card = new Card(new(4, 0));
            card.NestedCards.Count.ShouldBe(0);
            card.Strings.Count.ShouldBe(0);
            card.PartsArray.Count.ShouldBe(0);
            card.AddString(CardStringsEnum.FullName, "Alisha Doherty");
            card.NestedCards.Count.ShouldBe(0);
            card.Strings.Count.ShouldBe(1);
            card.PartsArray.Count.ShouldBe(0);
            var fullName = card.GetString(CardStringsEnum.FullName)[0];
            fullName.Value.ShouldBe("Alisha Doherty");
            string[] savedLines = card.SaveToString(true).SplitNewLines(false);
            savedLines[0].ShouldBe("BEGIN:VCARD");
            savedLines[1].ShouldBe("VERSION:4.0");
            savedLines[2].ShouldBe("FN:Alisha Doherty");
            savedLines[3].ShouldBe("END:VCARD");
        }

        [TestMethod]
        public void TestValidateNewCard40()
        {
            var card = new Card(new(4, 0));
            card.NestedCards.Count.ShouldBe(0);
            card.Strings.Count.ShouldBe(0);
            card.PartsArray.Count.ShouldBe(0);
            Should.Throw(card.Validate, typeof(InvalidDataException));
        }

        [TestMethod]
        public void TestCreateNewCard50()
        {
            var card = new Card(new(5, 0));
            card.NestedCards.Count.ShouldBe(0);
            card.Strings.Count.ShouldBe(0);
            card.PartsArray.Count.ShouldBe(0);
            string[] savedLines = card.SaveToString().SplitNewLines(false);
            savedLines[0].ShouldBe("BEGIN:VCARD");
            savedLines[1].ShouldBe("VERSION:5.0");
            savedLines[2].ShouldBe("END:VCARD");
        }

        [TestMethod]
        public void TestCreateNewMinimalCard50()
        {
            var card = new Card(new(5, 0));
            card.NestedCards.Count.ShouldBe(0);
            card.Strings.Count.ShouldBe(0);
            card.PartsArray.Count.ShouldBe(0);
            card.AddString(CardStringsEnum.FullName, "Alisha Doherty");
            card.AddPartToArray(CardPartsArrayEnum.Names, "Doherty;Alisha;;;");
            card.NestedCards.Count.ShouldBe(0);
            card.Strings.Count.ShouldBe(1);
            card.PartsArray.Count.ShouldBe(1);
            var fullName = card.GetString(CardStringsEnum.FullName)[0];
            fullName.Value.ShouldBe("Alisha Doherty");
            var name = card.GetPartsArray<NameInfo>()[0];
            name.ContactFirstName.ShouldBe("Alisha");
            name.ContactLastName.ShouldBe("Doherty");
            string[] savedLines = card.SaveToString(true).SplitNewLines(false);
            savedLines[0].ShouldBe("BEGIN:VCARD");
            savedLines[1].ShouldBe("VERSION:5.0");
            savedLines[2].ShouldBe("FN:Alisha Doherty");
            savedLines[3].ShouldBe("N:Doherty;Alisha;;;");
            savedLines[4].ShouldBe("END:VCARD");
        }

        [TestMethod]
        public void TestValidateNewCard50()
        {
            var card = new Card(new(5, 0));
            card.NestedCards.Count.ShouldBe(0);
            card.Strings.Count.ShouldBe(0);
            card.PartsArray.Count.ShouldBe(0);
            Should.Throw(card.Validate, typeof(InvalidDataException));
        }
    }
}
