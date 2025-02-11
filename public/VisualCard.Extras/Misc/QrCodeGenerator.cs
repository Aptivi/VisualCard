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
using VisualCard.Parts;

namespace VisualCard.Extras.Misc
{
    /// <summary>
    /// QR code generator for cards and calendars
    /// </summary>
    public static class QrCodeGenerator
    {
        public static void SaveQrCode(Card card, bool validate = false)
        {
            string savedCard = card.ToString(validate);
            using var generator = new QRCodeGenerator();
            using var qrData = generator.CreateQrCode(savedCard, QRCodeGenerator.ECCLevel.Q);
            using var qrBitmap = new PngByteQRCode(qrData);
            var graphicsByte = qrBitmap.GetGraphic(20);

            // TODO: UNFINISHED
        }
    }
}
