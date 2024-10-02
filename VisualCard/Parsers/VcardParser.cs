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
using System.Text;
using Textify.General;
using VisualCard.Exceptions;
using VisualCard.Parsers.Arguments;
using VisualCard.Parts;
using VisualCard.Parts.Enums;
using VisualCard.Parts.Implementations;

namespace VisualCard.Parsers
{
    /// <summary>
    /// The base vCard parser
    /// </summary>
    [DebuggerDisplay("vCard contact, version {CardVersion.ToString()}, {CardContent.Length} lines")]
    internal class VcardParser
    {
        internal Card[] nestedCards = [];
        private readonly Version cardVersion = new();
        private readonly string[] cardContent = [];

        /// <summary>
        /// VCard card content
        /// </summary>
        public string[] CardContent =>
            cardContent;
        /// <summary>
        /// VCard card version
        /// </summary>
        public Version CardVersion =>
            cardVersion;

        /// <summary>
        /// Parses a VCard contact
        /// </summary>
        /// <returns>A strongly-typed <see cref="Card"/> instance holding information about the card</returns>
        public Card Parse()
        {
            // Check the content to ensure that we really have data
            if (CardContent.Length == 0)
                throw new InvalidDataException($"Card content is empty.");

            // Make a new vCard
            var card = new Card(CardVersion);

            // Iterate through all the lines
            bool constructing = false;
            StringBuilder valueBuilder = new();
            string[] allowedTypes = ["HOME", "WORK", "PREF"];
            string kind = "individual";
            for (int i = 0; i < CardContent.Length; i++)
            {
                // Get line
                string _value = CardContent[i];
                int lineNumber = i + 1;
                if (string.IsNullOrEmpty(_value))
                    continue;

                // First, check to see if we need to construct blocks
                string secondLine = i + 1 < CardContent.Length ? CardContent[i + 1] : "";
                bool firstConstructedLine = !_value.StartsWith(VcardConstants._spaceBreak) && !_value.StartsWith(VcardConstants._tabBreak);
                constructing = secondLine.StartsWithAnyOf([VcardConstants._spaceBreak, VcardConstants._tabBreak]);
                secondLine = secondLine.Length > 1 ? secondLine.Substring(1) : "";
                if (constructing)
                {
                    if (firstConstructedLine)
                        valueBuilder.Append(_value);
                    valueBuilder.Append(secondLine);
                    continue;
                }
                else if (!firstConstructedLine)
                {
                    _value = valueBuilder.ToString();
                    valueBuilder.Clear();
                }

                try
                {
                    // Now, parse a line
                    if (!_value.Contains(VcardConstants._argumentDelimiter))
                        throw new ArgumentException("The line must contain an argument delimiter.");
                    string value = _value.Substring(_value.IndexOf(VcardConstants._argumentDelimiter) + 1);
                    string prefixWithArgs = _value.Substring(0, _value.IndexOf(VcardConstants._argumentDelimiter));
                    string prefix = prefixWithArgs.Contains(';') ? prefixWithArgs.Substring(0, prefixWithArgs.IndexOf(';')) : prefixWithArgs;
                    string args = prefixWithArgs.Contains(';') ? prefixWithArgs.Substring(prefix.Length + 1) : "";
                    string[] splitArgs = args.Split([VcardConstants._fieldDelimiter], StringSplitOptions.RemoveEmptyEntries);
                    string[] splitValues = value.Split([VcardConstants._fieldDelimiter], StringSplitOptions.RemoveEmptyEntries);
                    bool isWithType = splitArgs.Length > 0;
                    List<ArgumentInfo> finalArgs = [];
                    int altId = -1;

                    // Extract the group name
                    string group = prefix.Contains(".") ? prefix.Substring(0, prefix.IndexOf(".")) : "";
                    prefix = prefix.RemovePrefix($"{group}.");

                    // Get the part type
                    bool xNonstandard = prefix.StartsWith(VcardConstants._xSpecifier);
                    bool specifierRequired = CardVersion.Major >= 3;
                    var (type, enumeration, classType, fromString, defaultType, defaultValue, defaultValueType, extraAllowedTypes, allowedValues) = VcardParserTools.GetPartType(xNonstandard ? VcardConstants._xSpecifier : prefix);
                    
                    // Handle arguments
                    if (isWithType)
                    {
                        // If we have more than one argument, check for ALTID
                        if (CardVersion.Major >= 4 && type == PartType.PartsArray)
                        {
                            var tuple = VcardParserTools.GetPartsArrayEnumFromType(classType, CardVersion);
                            var cardinality = tuple.Item2;
                            bool supportsAltId =
                                cardinality != PartCardinality.MayBeOneNoAltId && cardinality != PartCardinality.ShouldBeOneNoAltId &&
                                cardinality != PartCardinality.AtLeastOneNoAltId && cardinality != PartCardinality.AnyNoAltId;
                            bool altIdSpotted = splitArgs.Any((arg) => arg.StartsWith(VcardConstants._altIdArgumentSpecifier));
                            if (supportsAltId)
                            {
                                // The type supports ALTID.
                                if (splitArgs[0].StartsWith(VcardConstants._altIdArgumentSpecifier))
                                {
                                    // We need ALTID to be numeric
                                    if (!int.TryParse(splitArgs[0].Substring(VcardConstants._altIdArgumentSpecifier.Length), out altId))
                                        throw new InvalidDataException("ALTID must be numeric");

                                    // We need ALTID to be positive
                                    if (altId < 0)
                                        throw new InvalidDataException("ALTID must be positive");

                                    // Here, we require arguments for ALTID
                                    if (splitArgs.Length <= 1)
                                        throw new InvalidDataException("ALTID must have one or more arguments to specify why this instance is an alternative");
                                }
                                else if (altIdSpotted)
                                    throw new InvalidDataException("ALTID must be exactly in the first position of the argument, because arguments that follow it are required to be specified");
                            }
                            else if (altIdSpotted)
                                throw new InvalidDataException($"ALTID must not be specified in the {tuple.Item1} type that expects a cardinality of {cardinality}.");
                        }

                        // Finalize the arguments
                        var argsStr = splitArgs.Except(
                            splitArgs.Where((arg) =>
                                arg.StartsWith(VcardConstants._altIdArgumentSpecifier) ||
                                arg.StartsWith(VcardConstants._valueArgumentSpecifier) ||
                                arg.StartsWith(VcardConstants._typeArgumentSpecifier) ||
                                (CardVersion.Major == 2 && !arg.Contains(VcardConstants._argumentValueDelimiter))
                            )
                        );
                        foreach (string arg in argsStr)
                        {
                            string keyStr = arg.Substring(0, arg.IndexOf(VcardConstants._argumentValueDelimiter));
                            string valueStr = arg.RemovePrefix($"{keyStr}{VcardConstants._argumentValueDelimiter}");
                            finalArgs.Add(new(keyStr, valueStr));
                        }
                    }

                    // Check the type for allowed types
                    string[] elementTypes = VcardCommonTools.GetTypes(splitArgs, defaultType, specifierRequired);
                    foreach (string elementType in elementTypes)
                    {
                        string elementTypeUpper = elementType.ToUpper();
                        if (!allowedTypes.Contains(elementTypeUpper) && !extraAllowedTypes.Contains(elementTypeUpper) && !elementTypeUpper.StartsWith("X-"))
                            throw new InvalidDataException($"Part info type {classType?.Name ?? "<null>"} doesn't support property type {elementTypeUpper} because the following base types are supported: [{string.Join(", ", allowedTypes)}] and the extra types are supported: [{string.Join(", ", extraAllowedTypes)}]");
                    }

                    // Handle the part type
                    string valueType = VcardCommonTools.GetFirstValue(splitArgs, defaultValueType, VcardConstants._valueArgumentSpecifier);
                    string finalValue = VcardParserTools.ProcessStringValue(value, valueType);

                    // Check for allowed values
                    if (allowedValues.Length != 0)
                    {
                        bool found = false;
                        foreach (string allowedValue in allowedValues)
                        {
                            if (finalValue == allowedValue)
                                found = true;
                        }
                        if (!found)
                            throw new InvalidDataException($"Value {finalValue} not in the list of allowed values [{string.Join(", ", allowedValues)}]");
                    }

                    // Process the value
                    switch (type)
                    {
                        case PartType.Strings:
                            {
                                StringsEnum stringType = (StringsEnum)enumeration;

                                // Let VisualCard know that we've explicitly specified a kind.
                                if (stringType == StringsEnum.Kind)
                                {
                                    kind = string.IsNullOrEmpty(finalValue) ? "individual" : finalValue;
                                    card.kindExplicitlySpecified = true;
                                }
                                else if (stringType == StringsEnum.Profile && !finalValue.Equals("vcard", StringComparison.OrdinalIgnoreCase))
                                    throw new InvalidDataException("Profile must be \"vCard\"");

                                // Set the string for real
                                card.SetString(stringType, finalValue, group);
                            }
                            break;
                        case PartType.PartsArray:
                            {
                                PartsArrayEnum partsArrayType = (PartsArrayEnum)enumeration;
                                Type? partsArrayClass = classType;
                                bool supported = VcardParserTools.EnumArrayTypeSupported(partsArrayType, CardVersion, kind);
                                if (!supported)
                                    continue;
                                if (fromString is null)
                                    continue;

                                // Now, get the part info
                                finalValue = partsArrayType is PartsArrayEnum.NonstandardNames or PartsArrayEnum.IanaNames ? _value : value;
                                var partInfo = fromString(finalValue, [.. finalArgs], altId, elementTypes, group, valueType, CardVersion);

                                // Set the array for real
                                card.AddPartToArray(partsArrayType, partInfo);
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

            // Add any possible nested cards
            card.nestedCards = nestedCards;

            // Validate this card before returning it.
            ValidateCard(card);
            return card;
        }

        internal void ValidateCard(Card card)
        {
            // Track the required fields
            List<string> expectedFieldList = [];
            var nameCardinality = VcardParserTools.GetPartsArrayEnumFromType(typeof(NameInfo), CardVersion).Item2;
            var fullNameCardinality = VcardParserTools.GetPartsArrayEnumFromType(typeof(FullNameInfo), CardVersion).Item2;
            if (nameCardinality == PartCardinality.ShouldBeOne)
                expectedFieldList.Add(VcardConstants._nameSpecifier);
            if (fullNameCardinality == PartCardinality.AtLeastOne)
                expectedFieldList.Add(VcardConstants._fullNameSpecifier);

            // Now, check for requirements
            string[] expectedFields = [.. expectedFieldList];
            if (!ValidateComponent(ref expectedFields, out string[] actualFields, card))
                throw new InvalidDataException($"The following keys [{string.Join(", ", expectedFields)}] are required. Got [{string.Join(", ", actualFields)}].");
        }

        private bool ValidateComponent<TComponent>(ref string[] expectedFields, out string[] actualFields, TComponent component)
            where TComponent : Card
        {
            // Track the required fields
            List<string> actualFieldList = [];

            // Requirement checks
            foreach (string expectedFieldName in expectedFields)
            {
                var (type, enumeration, enumType, _, _, _, _, _, _) = VcardParserTools.GetPartType(expectedFieldName);
                switch (type)
                {
                    case PartType.Strings:
                        {
                            string value = component.GetString((StringsEnum)enumeration).value;
                            bool exists = !string.IsNullOrEmpty(value);
                            if (exists)
                                actualFieldList.Add(expectedFieldName);
                        }
                        break;
                    case PartType.PartsArray:
                        {
                            if (enumType is null)
                                continue;
                            var values = component.GetPartsArray(enumType, (PartsArrayEnum)enumeration);
                            bool exists = values.Length > 0;
                            if (exists)
                                actualFieldList.Add(expectedFieldName);
                        }
                        break;
                }
            }
            Array.Sort(expectedFields);
            actualFieldList.Sort();
            actualFields = [.. actualFieldList];
            return actualFields.SequenceEqual(expectedFields);
        }

        internal VcardParser(string[] cardContent, Version cardVersion)
        {
            this.cardContent = cardContent;
            this.cardVersion = cardVersion;
        }
    }
}
