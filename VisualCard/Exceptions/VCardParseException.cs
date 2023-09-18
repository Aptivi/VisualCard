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

using System;

namespace VisualCard.Exceptions
{
    /// <summary>
    /// Exception of VCard parsing
    /// </summary>
    public class VCardParseException : Exception
    {
        /// <inheritdoc/>
        public VCardParseException()
            : base("General contact parsing error.")
        {
        }

        /// <summary>
        /// Indicates that there was something wrong with parsing
        /// </summary>
        /// <param name="message">The message to clarify the reasoning for the error</param>
        /// <param name="line">Line in which it caused the error</param>
        /// <param name="linenumber">Line number in which it caused the error</param>
        /// <param name="innerException">Inner exception (if any)</param>
        public VCardParseException(string message, string line, int linenumber, Exception innerException)
            : base($"An error occurred while parsing the VCard contact\n" +
                   $"Error: {message}\n" +
                   $"Line: {line}\n" + 
                   $"Line number: {linenumber}", innerException)
        {
        }
    }
}
