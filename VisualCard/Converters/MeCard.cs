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

using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using VisualCard.Parsers;
using VisualCard.Parts;

namespace VisualCard.Converters
{
    /// <summary>
    /// MeCard string generator and converter
    /// </summary>
    public static class MeCard
    {
        /// <summary>
        /// Gets all contacts from a MeCard string
        /// </summary>
        /// <param name="meCardString">MeCard string</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        public static Card[] GetContactsFromMeCardString(string meCardString)
        {
            // Check to see if the MeCard string is valid
            if (string.IsNullOrWhiteSpace(meCardString))
                throw new InvalidDataException("MeCard string should not be empty.");
            if (!meCardString.StartsWith("MECARD:") && !meCardString.EndsWith(";;"))
                throw new InvalidDataException("This string doesn't represent a valid MeCard contact.");

            // Now, parse it.
            try
            {
                // Split the meCard string from the beginning and the ending
                meCardString = meCardString.Substring(meCardString.IndexOf(":") + 1, meCardString.IndexOf(";;") - (meCardString.IndexOf(":") + 1));

                // Split the values from the semicolons
                var values = meCardString.Split(';');
                string fullName = "";

                // Replace all the commas found with semicolons if possible
                for (int i = 0; i < values.Length; i++)
                {
                    string value = values[i];

                    // "SOUND:" here is actually just a Kana name, so blacklist it.
                    if (value.StartsWith($"{VcardConstants._soundSpecifier}:"))
                        continue;

                    // Now, replace all the commas in Name and Address with the semicolons.
                    if (value.StartsWith($"{VcardConstants._nameSpecifier}:") || value.StartsWith($"{VcardConstants._addressSpecifier}:"))
                        values[i] = value.Replace(",", ";");

                    // Build a full name
                    if (value.StartsWith($"{VcardConstants._nameSpecifier}:"))
                    {
                        var nameSplits = value.Substring(2).Split(',');
                        fullName = $"{nameSplits[1]} {nameSplits[0]}";
                    }
                }

                // Install the values!
                var masterContactBuilder = new StringBuilder(
                   $"""
                    {VcardConstants._beginText}
                    {VcardConstants._versionSpecifier}:3.0

                    """
                );
                foreach (var value in values)
                    masterContactBuilder.AppendLine(value);
                masterContactBuilder.AppendLine(
                   $"""
                    {VcardConstants._fullNameSpecifier}:{fullName}
                    {VcardConstants._endText}
                    """
                );

                // Now, invoke VisualCard to give us the card parsers
                return CardTools.GetCardsFromString(masterContactBuilder.ToString());
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("The MeCard contact string is not valid.", ex);
            }
        }

    }
}
