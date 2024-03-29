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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using VisualCard.Exceptions;
using VisualCard.Parts;
using VisualCard.Parts.Enums;
using VisualCard.Parts.Implementations;

namespace VisualCard.Parsers
{
    /// <summary>
    /// The base vCard parser
    /// </summary>
    [DebuggerDisplay("vCard contact, version {CardVersion.ToString()}, expected {ExpectedCardVersion.ToString()}, {CardContent.Length} bytes")]
    internal abstract class BaseVcardParser : IVcardParser
    {
        /// <summary>
        /// VCard card content
        /// </summary>
        public virtual string CardContent { get; internal set; } = "";
        /// <summary>
        /// VCard card version
        /// </summary>
        public virtual Version CardVersion { get; internal set; } = new();
        /// <summary>
        /// VCard expected card version
        /// </summary>
        public virtual Version ExpectedCardVersion => new();

        /// <summary>
        /// Parses a VCard contact
        /// </summary>
        /// <returns>A strongly-typed <see cref="Card"/> instance holding information about the card</returns>
        public virtual Card Parse()
        {
            // Verify the card data
            VerifyCardData();

            // Now, make a stream out of card content
            byte[] CardContentData = Encoding.Default.GetBytes(CardContent);
            MemoryStream CardContentStream = new(CardContentData, false);
            StreamReader CardContentReader = new(CardContentStream);

            // Make a new vCard
            var card = new Card(CardVersion);

            // Iterate through all the lines
            int lineNumber = 0;
            while (!CardContentReader.EndOfStream)
            {
                // Get line
                string _value = CardContentReader.ReadLine();
                lineNumber += 1;

                // Variables
                string value = _value.Substring(_value.IndexOf(VcardConstants._argumentDelimiter) + 1);
                string prefixWithArgs = _value.Substring(0, _value.IndexOf(VcardConstants._argumentDelimiter));
                string prefix = prefixWithArgs.Contains(';') ? prefixWithArgs.Substring(0, prefixWithArgs.IndexOf(';')) : prefixWithArgs;
                string args = prefixWithArgs.Contains(';') ? prefixWithArgs.Substring(prefix.Length + 1) : "";
                string[] splitArgs = args.Split([VcardConstants._fieldDelimiter], StringSplitOptions.RemoveEmptyEntries);
                string[] splitValues = value.Split([VcardConstants._fieldDelimiter], StringSplitOptions.RemoveEmptyEntries);
                bool isWithType = splitArgs.Length > 0;
                List<string> finalArgs = [];
                int altId = 0;

                // Now, parse a line
                try
                {
                    if (isWithType)
                    {
                        // If we have more than one argument, check for ALTID
                        if (CardVersion.Major >= 4)
                        {
                            if (splitArgs[0].StartsWith(VcardConstants._altIdArgumentSpecifier))
                            {
                                if (!int.TryParse(splitArgs[0].Substring(VcardConstants._altIdArgumentSpecifier.Length), out altId))
                                    throw new InvalidDataException("ALTID must be numeric");

                                // Here, we require arguments for ALTID
                                if (splitArgs.Length <= 1)
                                    throw new InvalidDataException("ALTID must have one or more arguments to specify why this instance is an alternative");
                            }
                            else if (splitArgs.Any((arg) => arg.StartsWith(VcardConstants._altIdArgumentSpecifier)))
                                throw new InvalidDataException("ALTID must be exactly in the first position of the argument, because arguments that follow it are required to be specified");
                        }

                        // Finalize the arguments
                        finalArgs.AddRange(splitArgs.Except(
                            splitArgs.Where((arg) =>
                                arg.StartsWith(VcardConstants._altIdArgumentSpecifier) ||
                                arg.StartsWith(VcardConstants._valueArgumentSpecifier) ||
                                arg.StartsWith(VcardConstants._typeArgumentSpecifier)
                            )
                        ));
                    }

                    // TEMPORARY: Filter out BEGIN, VERSION, and END
                    if (prefix == "BEGIN" || prefix == "VERSION" || prefix == "END")
                        continue;

                    // Get the part type and handle it
                    bool xNonstandard = prefix.StartsWith(VcardConstants._xSpecifier);
                    var (type, enumeration, classType) = VcardParserTools.GetPartType(xNonstandard ? VcardConstants._xSpecifier : prefix);
                    string fromStringMethodName = isWithType ? nameof(NameInfo.FromStringVcardWithTypeStatic) : nameof(NameInfo.FromStringVcardStatic);
                    switch (type)
                    {
                        case PartType.Strings:
                            {
                                StringsEnum stringType = (StringsEnum)enumeration;
                                string finalValue = value;

                                // Now, handle each type individually
                                switch (stringType)
                                {
                                    case StringsEnum.FullName:
                                    case StringsEnum.Notes:
                                    case StringsEnum.Mailer:
                                    case StringsEnum.ProductId:
                                    case StringsEnum.SortString:
                                    case StringsEnum.AccessClassification:
                                    case StringsEnum.Xml:
                                        // Unescape the value
                                        finalValue = Regex.Unescape(value);
                                        break;
                                    case StringsEnum.Url:
                                    case StringsEnum.Source:
                                    case StringsEnum.FreeBusyUrl:
                                    case StringsEnum.CalendarUrl:
                                    case StringsEnum.CalendarSchedulingRequestUrl:
                                        // Try to parse the URL to ensure that it conforms to IETF RFC 1738: Uniform Resource Locators
                                        if (!Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
                                            throw new InvalidDataException($"URL {value} is invalid");
                                        finalValue = uri.ToString();
                                        break;
                                    case StringsEnum.Kind:
                                        // Get the kind
                                        if (!string.IsNullOrEmpty(value))
                                            finalValue = Regex.Unescape(value);
                                        else
                                            finalValue = "individual";

                                        // Let VisualCard know that we've explicitly specified a kind.
                                        card.kindExplicitlySpecified = true;
                                        break;
                                    default:
                                        throw new InvalidDataException($"The string enum type {stringType} is invalid. Are you sure that you've specified the correct type in your vCard representation?");
                                }

                                // Set the string for real
                                card.SetString(stringType, finalValue);
                            }
                            break;
                        case PartType.Parts:
                            {
                                PartsEnum partsType = (PartsEnum)enumeration;
                                Type partsClass = classType;
                                bool supported = VcardParserTools.EnumTypeSupported(partsType, CardVersion);
                                if (!supported)
                                    continue;

                                // Handle parsing parts
                                var fromStringMethod = partsClass.GetMethod(fromStringMethodName, BindingFlags.Static | BindingFlags.NonPublic);

                                // Now, get the part info
                                var partInfo =
                                    isWithType ?
                                    fromStringMethod.Invoke(null, [_value, finalArgs.ToArray(), altId, CardVersion, CardContentReader]) :
                                    fromStringMethod.Invoke(null, [_value, altId, CardVersion, CardContentReader]);
                                card.SetPart(partsType, (BaseCardPartInfo)partInfo);
                            }
                            break;
                        case PartType.PartsArray:
                            {
                                PartsArrayEnum partsArrayType = (PartsArrayEnum)enumeration;
                                Type partsArrayClass = classType;
                                bool supported = VcardParserTools.EnumArrayTypeSupported(partsArrayType, CardVersion);
                                if (!supported)
                                    continue;

                                // Handle parsing part arrays
                                var fromStringMethod = partsArrayClass.GetMethod(fromStringMethodName, BindingFlags.Static | BindingFlags.NonPublic);

                                // Now, get the part info
                                var partInfo =
                                    isWithType ?
                                    fromStringMethod.Invoke(null, [_value, finalArgs.ToArray(), altId, CardVersion, CardContentReader]) :
                                    fromStringMethod.Invoke(null, [_value, altId, CardVersion, CardContentReader]);
                                card.AddPartToArray(partsArrayType, (BaseCardPartInfo)partInfo);
                            }
                            break;
                        default:
                            throw new InvalidDataException($"The type {type} is invalid. Are you sure that you've specified the correct type in your vCard representation?");
                    }
                }
                catch (Exception ex)
                {
                    throw new VCardParseException(ex.Message, _value, lineNumber, ex);
                }
            }

            // Validate this card before returning it.
            ValidateCard(card);
            return card;
        }

        internal void VerifyCardData()
        {
            // Check the version to ensure that we're really dealing with VCard 2.1 contact
            if (CardVersion != ExpectedCardVersion)
                throw new InvalidDataException($"Card version {CardVersion} doesn't match expected {ExpectedCardVersion}.");

            // Check the content to ensure that we really have data
            if (string.IsNullOrEmpty(CardContent))
                throw new InvalidDataException($"Card content is empty.");
        }

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

        internal void ValidateCard(Card card)
        {
            // Track the required fields
            List<string> expectedFields = [];
            List<string> actualFields = [];
            switch (CardVersion.ToString(2))
            {
                case "2.1":
                    expectedFields.Add(VcardConstants._nameSpecifier);
                    break;
                case "4.0":
                    expectedFields.Add(VcardConstants._fullNameSpecifier);
                    break;
                case "3.0":
                case "5.0":
                    expectedFields.Add(VcardConstants._nameSpecifier);
                    expectedFields.Add(VcardConstants._fullNameSpecifier);
                    break;
            }

            // Requirement checks
            if (expectedFields.Contains(VcardConstants._nameSpecifier))
            {
                var names = card.GetPartsArray(PartsArrayEnum.Names);
                bool exists = names is not null && names.Length > 0;
                if (exists)
                    actualFields.Add(VcardConstants._nameSpecifier);
            }
            if (expectedFields.Contains(VcardConstants._fullNameSpecifier))
            {
                string fullName = card.GetString(StringsEnum.FullName);
                bool exists = !string.IsNullOrEmpty(fullName);
                if (exists)
                    actualFields.Add(VcardConstants._fullNameSpecifier);
            }
            expectedFields.Sort();
            actualFields.Sort();
            if (!actualFields.SequenceEqual(expectedFields))
                throw new InvalidDataException($"The following keys [{string.Join(", ", expectedFields)}] are required. Got [{string.Join(", ", actualFields)}].");
        }
    }
}
