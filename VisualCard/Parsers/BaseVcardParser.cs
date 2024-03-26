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

using System.Diagnostics;
using System.Text;
using VisualCard.Parts;

namespace VisualCard.Parsers
{
    /// <summary>
    /// The base VCard parser
    /// </summary>
    [DebuggerDisplay("VCard contact, version {CardVersion}")]
    public abstract class BaseVcardParser : IVcardParser
    {
        /// <summary>
        /// VCard card content
        /// </summary>
        public virtual string CardContent => "";
        /// <summary>
        /// VCard card version
        /// </summary>
        public virtual string CardVersion => "";

        /// <summary>
        /// Parses a VCard contact
        /// </summary>
        /// <returns>A strongly-typed <see cref="Card"/> instance holding information about the card</returns>
        public abstract Card Parse();
        internal abstract string SaveToString(Card card);
        internal abstract void SaveTo(string path, Card card);

        internal static string MakeStringBlock(string target, int firstLength)
        {
            const int maxChars = 74;
            int maxCharsFirst = maxChars - firstLength + 1;

            // Construct the block
            StringBuilder block = new();
            int selectedMax = maxCharsFirst;
            int processed = 0;
            for (int currCharNum = 0; currCharNum < target.Length; currCharNum++)
            {
                block.Append(target[currCharNum]);
                processed++;
                if (processed >= selectedMax)
                {
                    // Append a new line because we reached the maximum limit
                    selectedMax = maxChars;
                    processed = 0;
                    block.Append("\n ");
                }
            }
            return block.ToString();
        }
    }
}
