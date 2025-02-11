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
using VisualCard.Calendar;
using VisualCard.Extras.Misc;
using VisualCard.Tests.Calendars.Data;
using VisualCard.Tests.Contacts.Data;

namespace VisualCard.Tests.QrCode
{
    [TestClass]
    public class QrCodeTests
    {
        [TestMethod]
        public void TestGenerateQrCodeFromContact()
        {
            var cards = CardTools.GetCardsFromString(ContactData.singleVcardTwoContactShort);
            var card = cards[0];
            var qrImageBytes = QrCodeGenerator.SaveQrCode(card);
            qrImageBytes.ShouldNotBeEmpty();
        }

        [TestMethod]
        public void TestGenerateQrCodeFromMeCardContact()
        {
            var qrImageBytes = QrCodeGenerator.SaveQrCode(ContactData.singleMeCardContactFull);
            qrImageBytes.ShouldNotBeEmpty();
        }

        [TestMethod]
        public void TestGenerateQrCodeFromCalendar()
        {
            var calendars = CalendarTools.GetCalendarsFromString(CalendarData.singleVCalendarOneCalendar);
            var calendar = calendars[0];
            var qrImageBytes = QrCodeGenerator.SaveQrCode(calendar);
            qrImageBytes.ShouldNotBeEmpty();
        }

        [TestMethod]
        public void TestExportQrCodeFromContact()
        {
            // Load the cards first
            var cards = CardTools.GetCardsFromString(ContactData.singleVcardTwoContactShort);
            var card = cards[0];

            // Save the code as qrCodeTest1.qrr
            QrCodeGenerator.ExportQrCode(card, "qrCodeTest1");
            File.Exists("qrCodeTest1.qrr").ShouldBeTrue();
        }

        [TestMethod]
        public void TestExportQrCodeFromMeCardContact()
        {
            // Save the MeCard contact as qrCodeTest2.qrr
            QrCodeGenerator.ExportQrCode(ContactData.singleMeCardContactFull, "qrCodeTest2");
            File.Exists("qrCodeTest2.qrr").ShouldBeTrue();
        }

        [TestMethod]
        public void TestExportQrCodeFromCalendar()
        {
            // Load the calendars first
            var calendars = CalendarTools.GetCalendarsFromString(CalendarData.singleVCalendarOneCalendar);
            var calendar = calendars[0];

            // Save the code as qrCodeTest3.qrr
            QrCodeGenerator.ExportQrCode(calendar, "qrCodeTest3");
            File.Exists("qrCodeTest3.qrr").ShouldBeTrue();
        }
    }
}
