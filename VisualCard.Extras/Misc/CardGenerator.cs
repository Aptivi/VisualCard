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

using Nettify.MailAddress;
using System;
using System.Collections.Generic;
using System.Text;
using Textify.Data.NameGen;
using VisualCard.Parsers;
using VisualCard.Parts;
using VisualCard.Parts.Enums;
using VisualCard.Parts.Implementations;

namespace VisualCard.Extras.Misc
{
    /// <summary>
    /// A card generator
    /// </summary>
    public static class CardGenerator
    {
        private static readonly Random rng = new();

        /// <summary>
        /// Generates cards
        /// </summary>
        /// <param name="namePrefix">Prefix of the first name</param>
        /// <param name="nameSuffix">Suffix of the first name</param>
        /// <param name="surnamePrefix">Prefix of the last name</param>
        /// <param name="surnameSuffix">Suffix of the last name</param>
        /// <param name="nameGender">Name gender type</param>
        /// <param name="min">Minimum number of cards</param>
        /// <param name="max">Maximum number of cards</param>
        /// <returns>A list of generated cards (by default, it generates up to 100 cards.)</returns>
        public static Card[] GenerateCards(string namePrefix = "", string nameSuffix = "", string surnamePrefix = "", string surnameSuffix = "", NameGenderType nameGender = NameGenderType.Unified, int min = 1, int max = 100)
        {
            int cardNumbers = rng.Next(min, max + 1);
            return GenerateCards(cardNumbers, namePrefix, nameSuffix, surnamePrefix, surnameSuffix, nameGender);
        }

        /// <summary>
        /// Generates cards
        /// </summary>
        /// <param name="namePrefix">Prefix of the first name</param>
        /// <param name="nameSuffix">Suffix of the first name</param>
        /// <param name="surnamePrefix">Prefix of the last name</param>
        /// <param name="surnameSuffix">Suffix of the last name</param>
        /// <param name="nameGender">Name gender type</param>
        /// <param name="cards">Number of cards to generate</param>
        /// <returns>A list of generated cards or an empty array if <paramref name="cards"/> is less than or equal to zero.</returns>
        public static Card[] GenerateCards(int cards, string namePrefix = "", string nameSuffix = "", string surnamePrefix = "", string surnameSuffix = "", NameGenderType nameGender = NameGenderType.Unified)
        {
            if (cards <= 0)
                return [];

            // Get first and last names
            string[] firstNames = NameGenerator.GenerateFirstNames(cards, namePrefix, nameSuffix, nameGender);
            string[] lastNames = NameGenerator.GenerateLastNames(cards, surnamePrefix, surnameSuffix);
            string[] mailHosts = IspTools.KnownIspHosts;
            List<Card> cardList = [];

            // Build this number of cards
            StringBuilder builder = new();
            for (int i = 0; i < cards; i++)
            {
                // Get first and last names from the card index
                string firstName = firstNames[i];
                string lastName = lastNames[i];
                if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                    continue;

                // Add the begin header and the name indicators
                builder.AppendLine(VcardConstants._beginText);
                builder.AppendLine(VcardConstants._fullNameSpecifier + $":{firstName} {lastName}");
                builder.AppendLine(VcardConstants._nameSpecifier + $":{lastName};{firstName}");

                // Now, generate random telephone numbers and e-mail addresses
                bool generateTelephone = rng.NextDouble() < 0.3;
                bool generateEmail = rng.NextDouble() < 0.3;
                if (generateTelephone)
                {
                    bool generateWorkTelephone = rng.NextDouble() < 0.25;
                    int firstPart = rng.Next(1000);
                    int secondPart = rng.Next(1000);
                    int thirdPart = rng.Next(10000);
                    builder.AppendLine(VcardConstants._telephoneSpecifier + $";TYPE=HOME:{firstPart:D3}-{secondPart:D3}-{thirdPart:D4}");
                    if (generateWorkTelephone)
                    {
                        firstPart = rng.Next(1000);
                        secondPart = rng.Next(1000);
                        thirdPart = rng.Next(10000);
                        builder.AppendLine(VcardConstants._telephoneSpecifier + $";TYPE=WORK:{firstPart:D3}-{secondPart:D3}-{thirdPart:D4}");
                    }
                }
                if (generateEmail)
                {
                    bool generateWorkEmail = rng.NextDouble() < 0.25;
                    bool firstNameLong = rng.NextDouble() < 0.25;
                    string firstNameNormalized = firstName.ToLower().Replace(" ", "-");
                    string lastNameNormalized = lastName.ToLower().Replace(" ", "-");
                    string emailName = firstNameLong ? firstNameNormalized + "." + char.ToLower(lastName[0]) : char.ToLower(firstName[0]) + "." + lastNameNormalized;
                    string mailHost = mailHosts[rng.Next(mailHosts.Length)];
                    builder.AppendLine(VcardConstants._emailSpecifier + $";TYPE=HOME:{emailName}@{mailHost}");
                    if (generateWorkEmail)
                        builder.AppendLine(VcardConstants._emailSpecifier + $";TYPE=WORK:{emailName}@{lastNameNormalized}.com");
                }

                // Add the end header
                builder.AppendLine(VcardConstants._endText);
                string cardString = builder.ToString();
                builder.Clear();

                // Verify the generated card and add it to the list of cards
                var finalCard = CardTools.GetCardsFromString(cardString)[0];
                cardList.Add(finalCard);
            }
            return [.. cardList];
        }
    }
}
