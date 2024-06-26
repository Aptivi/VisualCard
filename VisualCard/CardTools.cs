﻿//
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
using VisualCard.Parsers;
using System;
using VisualCard.Parts;
using Textify.General;

namespace VisualCard
{
    /// <summary>
    /// Module for VCard management
    /// </summary>
    public static class CardTools
    {
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
            StringBuilder CardContent = new();
            Version CardVersion = new();
            while (!stream.EndOfStream)
            {
                bool append = false;

                // Skip empty lines
                CardLine = stream.ReadLine();
                if (string.IsNullOrEmpty(CardLine))
                {
                    if (!stream.EndOfStream)
                        continue;
                }
                else if (CardLine != VcardConstants._beginText &&
                         !CardLine.StartsWith(VcardConstants._versionSpecifier) &&
                         CardLine != VcardConstants._endText)
                    append = true;
                if (append)
                    CardContent.Append(CardLine);

                // All VCards must begin with BEGIN:VCARD
                if (CardLine != VcardConstants._beginText && !BeginSpotted)
                    throw new InvalidDataException($"This is not a valid VCard contact file.");
                else if (!BeginSpotted)
                {
                    BeginSpotted = true;
                    VersionSpotted = false;
                    EndSpotted = false;
                    continue;
                }

                // Now that the beginning of the card tag is spotted, parse the version as we need to know how to select the appropriate parser.
                // All VCards are required to have their own version directly after the BEGIN:VCARD tag
                if (CardLine != $"{VcardConstants._versionSpecifier}:2.1" &&
                    CardLine != $"{VcardConstants._versionSpecifier}:3.0" &&
                    CardLine != $"{VcardConstants._versionSpecifier}:4.0" &&
                    CardLine != $"{VcardConstants._versionSpecifier}:5.0" &&
                    !VersionSpotted)
                    throw new InvalidDataException($"This has an invalid VCard version {CardLine}.");
                else if (!VersionSpotted)
                {
                    VersionSpotted = true;
                    CardVersion = new(CardLine.Substring(8));
                    continue;
                }

                // If the ending tag is spotted, reset everything.
                if (CardLine == VcardConstants._endText && !EndSpotted)
                {
                    EndSpotted = true;

                    // Make a new parser instance
                    string content = CardContent.ToString();
                    string[] contentLines = content.SplitNewLines();
                    VcardParser CardParser = new(contentLines, CardVersion);
                    FinalParsers.Add(CardParser);

                    // Clear the content in case we want to make a second contact
                    CardContent.Clear();
                    BeginSpotted = false;
                }
                else if (append)
                    CardContent.AppendLine();
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
