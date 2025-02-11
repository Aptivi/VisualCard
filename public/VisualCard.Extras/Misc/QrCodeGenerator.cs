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

using QRCoder;
using System;
using System.IO;
using System.Text;
using VisualCard.Calendar;
using VisualCard.Calendar.Parts;
using VisualCard.Extras.Converters;
using VisualCard.Parts;

namespace VisualCard.Extras.Misc
{
    /// <summary>
    /// QR code generator for cards and calendars
    /// </summary>
    public static class QrCodeGenerator
    {
        /// <summary>
        /// Saves the card to a QR code byte array
        /// </summary>
        /// <param name="card">Card to get its QR code</param>
        /// <param name="validate">Whether to validate before saving</param>
        /// <returns>A PNG representation of the QR code that can be saved</returns>
        public static byte[] SaveQrCode(Card card, bool validate = false)
        {
            // Save the card to a string
            string savedCard = card.ToString(validate);
            return SaveQrCodeInternal(savedCard);
        }

        /// <summary>
        /// Saves the calendar to a QR code byte array
        /// </summary>
        /// <param name="calendar">Calendar to get its QR code</param>
        /// <param name="validate">Whether to validate before saving</param>
        /// <returns>A PNG representation of the QR code that can be saved</returns>
        public static byte[] SaveQrCode(Calendar.Parts.Calendar calendar, bool validate = false)
        {
            // Save the calendar to a string
            string savedCalendar = calendar.ToString(validate);
            return SaveQrCodeInternal(savedCalendar);
        }

        /// <summary>
        /// Saves the MeCard contact to a QR code byte array
        /// </summary>
        /// <param name="meCard">MeCard representation to get its QR code</param>
        /// <returns>A PNG representation of the QR code that can be saved</returns>
        public static byte[] SaveQrCode(string meCard)
        {
            // Verify the MeCard string and return its QR code representation
            MeCard.GetContactsFromMeCardString(meCard);
            return SaveQrCodeInternal(meCard);
        }

        /// <summary>
        /// Saves the card to a QR code byte array
        /// </summary>
        /// <param name="card">Card to get its QR code</param>
        /// <param name="validate">Whether to validate before saving</param>
        /// <param name="filePath">Path to the file to export to</param>
        public static void ExportQrCode(Card card, string filePath, bool validate = false)
        {
            // Save the card to a string
            string savedCard = card.ToString(validate);
            ExportQrCodeInternal(savedCard, filePath);
        }

        /// <summary>
        /// Saves the calendar to a QR code byte array
        /// </summary>
        /// <param name="calendar">Calendar to get its QR code</param>
        /// <param name="validate">Whether to validate before saving</param>
        /// <param name="filePath">Path to the file to export to</param>
        public static void ExportQrCode(Calendar.Parts.Calendar calendar, string filePath, bool validate = false)
        {
            // Save the calendar to a string
            string savedCalendar = calendar.ToString(validate);
            ExportQrCodeInternal(savedCalendar, filePath);
        }

        /// <summary>
        /// Saves the MeCard contact to a QR code byte array
        /// </summary>
        /// <param name="meCard">MeCard representation to get its QR code</param>
        /// <param name="filePath">Path to the file to export to</param>
        public static void ExportQrCode(string meCard, string filePath)
        {
            // Verify the MeCard string and return its QR code representation
            MeCard.GetContactsFromMeCardString(meCard);
            ExportQrCodeInternal(meCard, filePath);
        }

        private static QRCodeData GetQrCodeData(string representation)
        {
            // Generate the saved payload for QR code
            var generator = new QRCodeGenerator();
            var qrData = generator.CreateQrCode(representation, QRCodeGenerator.ECCLevel.Q);

            // Return the QR data
            return qrData;
        }

        private static byte[] SaveQrCodeInternal(string representation)
        {
            // Generate the saved payload for QR code
            var qrData = GetQrCodeData(representation);
            var qrBitmap = new PngByteQRCode(qrData);
            var graphicsByte = qrBitmap.GetGraphic(20);

            // Finally, return the byte array
            return graphicsByte;
        }

        private static void ExportQrCodeInternal(string representation, string filePath)
        {
            var qrCodeData = GetQrCodeData(representation);

            // Get the file path
            qrCodeData.SaveRawData(filePath, QRCodeData.Compression.Uncompressed);
        }
    }
}
