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

using System.Collections.Generic;
using System.IO;
using System.Text;
using VisualCard.Parsers;
using System;
using VisualCard.Parts;
using Textify.General;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parsers;

namespace VisualCard
{
    /// <summary>
    /// Module for VCard management
    /// </summary>
    public static class CardTools
    {
        /// <summary>
        /// ISO 9070 Formal Public Identifier (FPI) for clipboard format type
        /// </summary>
        public const string FPI = "+//ISBN 1-887687-00-9::versit::PDI//vCard";

        /// <summary>
        /// Gets the list of parsers for single/multiple contacts from the string
        /// </summary>
        /// <param name="cardText">Contacts text</param>
        /// <returns>List of contact parsers for single or multiple contacts</returns>
        public static Card[] GetCardsFromString(string cardText)
        {
            // Open the stream to parse multiple contact versions (required to parse more than one contact)
            MemoryStream CardFs = new(Encoding.Default.GetBytes(cardText));
            StreamReader CardReader = new(CardFs);
            return GetCards(CardReader);
        }

        /// <summary>
        /// Gets the list of parsers for single/multiple contacts from the path
        /// </summary>
        /// <param name="Path">Path to the contacts file</param>
        /// <returns>List of contact parsers for single or multiple contacts</returns>
        public static Card[] GetCards(string Path)
        {
            // Open the stream to parse multiple contact versions (required to parse more than one contact)
            FileStream CardFs = new(Path, FileMode.Open, FileAccess.Read);
            StreamReader CardReader = new(CardFs);
            return GetCards(CardReader);
        }

        /// <summary>
        /// Gets the list of parsers for single/multiple contacts from the stream
        /// </summary>
        /// <param name="stream">Stream containing the contacts</param>
        /// <returns>List of contact parsers for single or multiple contacts</returns>
        public static Card[] GetCards(StreamReader stream)
        {
            // Variables and flags
            List<VcardParser> FinalParsers = [];
            List<Card> FinalCards = [];
            bool BeginSpotted = false;
            bool VersionSpotted = false;
            bool EndSpotted = false;

            // Parse the lines of the card file
            string CardLine;
            Version CardVersion = new();
            List<Card> nestedCards = [];
            List<(int, string)> lines = [];
            bool nested = false;
            bool versionDirect = false;
            bool isAgent = false;
            int lineNumber = 0;
            while (!stream.EndOfStream)
            {
                bool append = false;
                lineNumber++;

                // Read the line
                CardLine = stream.ReadLine();

                // Get the property info
                string prefix = "";
                string value;
                try
                {
                    var prop = new PropertyInfo(CardLine);
                    if (prop.CanContinueMultiline)
                        CardLine = CardLine.Remove(CardLine.Length - 1, 1);
                    while (prop.CanContinueMultiline)
                    {
                        prop.rawValue.Remove(prop.rawValue.Length - 1, 1);
                        string nextLine = stream.ReadLine();
                        prop.rawValue.Append(nextLine);

                        // Add it to the current line for later processing
                        CardLine += nextLine;
                        if (CardLine[CardLine.Length - 1] == '=')
                            CardLine = CardLine.Remove(CardLine.Length - 1, 1);
                    }
                    prefix = prop.Prefix;
                    value = prop.Value;
                }
                catch
                {
                    value = CardLine;
                }

                // Process the line for begin, version, and end specifiers
                if (string.IsNullOrEmpty(CardLine))
                    continue;
                else if ((!prefix.EqualsNoCase(CommonConstants._beginSpecifier) &&
                         !prefix.EqualsNoCase(CommonConstants._versionSpecifier) &&
                         !prefix.EqualsNoCase(CommonConstants._endSpecifier)) || isAgent)
                    append = true;
                else if (prefix.EqualsNoCase(CommonConstants._beginSpecifier) && nested && !isAgent)
                {
                    // We have a nested card!
                    StringBuilder nestedBuilder = new();
                    int nestLevel = 1;
                    nestedBuilder.AppendLine(CardLine);
                    while (!stream.EndOfStream && !prefix.EqualsNoCase(CommonConstants._endSpecifier) && !value.EqualsNoCase(VcardConstants._objectVCardSpecifier))
                    {
                        CardLine = stream.ReadLine();
                        nestedBuilder.AppendLine(CardLine);
                        if (prefix.EqualsNoCase(CommonConstants._beginSpecifier) && value.EqualsNoCase(VcardConstants._objectVCardSpecifier))
                            nestLevel++;
                        else if (prefix.EqualsNoCase(CommonConstants._endSpecifier) && value.EqualsNoCase(VcardConstants._objectVCardSpecifier) && nestLevel > 1)
                        {
                            nestLevel--;
                            continue;
                        }
                    }

                    // Get the cards from the nested card content
                    var nestedCardList = GetCardsFromString(nestedBuilder.ToString());
                    nestedCards.AddRange(nestedCardList);
                    continue;
                }
                if (append)
                    lines.Add((lineNumber, isAgent ? (CardLine.StartsWith(" ") ? CardLine : $" {CardLine}\\n") : CardLine));

                // Check for agent property
                if (prefix == VcardConstants._agentSpecifier && string.IsNullOrEmpty(value))
                    isAgent = true;
                else if (prefix == CommonConstants._endSpecifier && isAgent)
                {
                    isAgent = false;
                    continue;
                }
                else if (isAgent)
                    continue;

                // All VCards must begin with BEGIN:VCARD
                if (!prefix.EqualsNoCase(CommonConstants._beginSpecifier) && !value.EqualsNoCase(VcardConstants._objectVCardSpecifier) && !BeginSpotted)
                    throw new InvalidDataException("This is not a valid vCard contact file.");
                else if (!BeginSpotted)
                {
                    BeginSpotted = true;
                    VersionSpotted = false;
                    EndSpotted = false;
                    nested = true;
                    versionDirect = true;
                    continue;
                }

                // Now that the beginning of the card tag is spotted, parse the version as we need to know how to select the appropriate parser.
                // vCard 4.0 and 5.0 cards are required to have their own version directly after the BEGIN:VCARD tag, but vCard 3.0 and 2.1 may
                // or may not place VERSION directly after the BEGIN:VCARD tag.
                if (prefix.EqualsNoCase(CommonConstants._versionSpecifier) &&
                    !value.EqualsNoCase("2.1") && !value.EqualsNoCase("3.0") &&
                    !value.EqualsNoCase("4.0") && !value.EqualsNoCase("5.0") &&
                    !VersionSpotted)
                    throw new InvalidDataException($"This card has an invalid VCard version {CardLine}.");
                else if (!VersionSpotted && prefix.EqualsNoCase(CommonConstants._versionSpecifier))
                {
                    VersionSpotted = true;
                    CardVersion = new(value);
                    
                    // Check to see if the vCard has VERSION directly after BEGIN:VCARD for 4.0 and 5.0
                    if (!versionDirect && (CardVersion.Major == 4 || CardVersion.Major == 5))
                        throw new InvalidDataException($"vCard {CardVersion.Major}.0 requires that VERSION comes directly after {VcardConstants._beginText}.");
                    continue;
                }
                if (!VersionSpotted)
                    versionDirect = false;

                // If the ending tag is spotted, reset everything.
                if (prefix.EqualsNoCase(CommonConstants._endSpecifier) && value.EqualsNoCase(VcardConstants._objectVCardSpecifier) && !EndSpotted)
                {
                    EndSpotted = true;
                    nested = false;

                    // Make a new parser instance
                    VcardParser CardParser = new([.. lines], CardVersion)
                    {
                        nestedCards = nestedCards
                    };
                    FinalParsers.Add(CardParser);

                    // Clear the content in case we want to make a second contact
                    lines.Clear();
                    nestedCards.Clear();
                    BeginSpotted = false;
                    versionDirect = false;
                }
            }

            // Close the stream to avoid stuck file handle
            stream.Close();

            // Throw if the card ended prematurely
            if (!EndSpotted)
                throw new InvalidDataException("Card ended prematurely without the ending tag");

            // Now, assuming that all cards and their parsers are valid, parse all of them
            foreach (var parser in FinalParsers)
            {
                var card = parser.Parse();
                FinalCards.Add(card);
            }
            return [.. FinalCards];
        }
    }
}
