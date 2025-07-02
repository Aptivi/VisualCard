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

using System;
using VisualCard.Common.Diagnostics;
using VisualCard.Languages;

namespace VisualCard.Exceptions
{
    /// <summary>
    /// Exception of VCard parsing
    /// </summary>
    public class VCardParseException : Exception
    {
        /// <inheritdoc/>
        internal VCardParseException()
            : base(LanguageTools.GetLocalized("VISUALCARD_EXCEPTION_GENERALCONTACTPARSEERROR"))
        {
            LoggingTools.Error("Throwing vCard parse exception...");
        }

        /// <summary>
        /// Indicates that there was something wrong with parsing
        /// </summary>
        /// <param name="message">The message to clarify the reasoning for the error</param>
        /// <param name="line">Line in which it caused the error</param>
        /// <param name="linenumber">Line number in which it caused the error</param>
        /// <param name="innerException">Inner exception (if any)</param>
        internal VCardParseException(string message, string line, int linenumber, Exception innerException)
            : base(LanguageTools.GetLocalized("VISUALCARD_EXCEPTION_CONTACTPARSEERROR") + "\n" +
                   LanguageTools.GetLocalized("VISUALCARD_EXCEPTION_CONTACTPARSEERROR_ERROR") + $" {message}\n" +
                   LanguageTools.GetLocalized("VISUALCARD_EXCEPTION_CONTACTPARSEERROR_LINE") + $" {line}\n" + 
                   LanguageTools.GetLocalized("VISUALCARD_EXCEPTION_CONTACTPARSEERROR_LINENUM") + $" {linenumber}", innerException)
        {
            LoggingTools.Error(this, "Throwing vCard parse exception...");
        }
    }
}
