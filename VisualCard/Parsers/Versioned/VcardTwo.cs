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

using System;

namespace VisualCard.Parsers.Versioned
{
    /// <summary>
    /// Parser for VCard version 2.1. Consult the vcard-21.txt file in source for the specification.
    /// </summary>
    internal class VcardTwo : BaseVcardParser, IVcardParser
    {
        /// <inheritdoc/>
        public override Version ExpectedCardVersion =>
            new(2, 1);

        internal VcardTwo(string cardContent, Version cardVersion)
        {
            CardContent = cardContent;
            CardVersion = cardVersion;
        }
    }
}
