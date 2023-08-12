/*
 * MIT License
 *
 * Copyright (c) 2021-2022 Aptivi
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 */

using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using VisualCard.Parsers;
using System.Linq;

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
        public static List<BaseVcardParser> GetContactsFromMeCardString(string meCardString)
        {
            // Check to see if the MeCard string is valid
            if (string.IsNullOrWhiteSpace(meCardString))
                throw new InvalidDataException("MeCard string should not be empty.");
            if (!meCardString.StartsWith("MECARD:") && !meCardString.EndsWith(";;"))
                throw new InvalidDataException("This string doesn't represent a valid MeCard contact.");

            // Now, parse it.
            List<BaseVcardParser> cardParsers;
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
                    if (value.StartsWith("SOUND:"))
                        continue;

                    // Now, replace all the commas in Name and Address with the semicolons.
                    if (value.StartsWith("N:") || value.StartsWith("ADR:"))
                        values[i] = value.Replace(",", ";");

                    // Build a full name
                    if (value.StartsWith("N:"))
                    {
                        var nameSplits = value.Substring(2).Split(',');
                        fullName = $"{nameSplits[1]} {nameSplits[0]}";
                    }
                }

                // Install the values!
                var masterContactBuilder = new StringBuilder(
                   $"""
                    BEGIN:VCARD
                    VERSION:3.0
                    {string.Join(Environment.NewLine, values)}
                    FN:{fullName}
                    END:VCARD
                    """
                );

                // Now, invoke VisualCard to give us the card parsers
                cardParsers = CardTools.GetCardParsersFromString(masterContactBuilder.ToString());
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("The MeCard contact string is not valid.", ex);
            }

            return cardParsers;
        }

    }
}
