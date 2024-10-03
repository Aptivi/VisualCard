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
        private (int, string)[] cardContent = [];

        /// <summary>
        /// VCard card content
        /// </summary>
        public (int, string)[] CardContent =>
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

            // Move kind to the top
            if (CardVersion.Major >= 4)
            {
                var kindLine = CardContent.SingleOrDefault((line) => line.Item2.ToUpper().StartsWith(VcardConstants._kindSpecifier));
                if (!string.IsNullOrEmpty(kindLine.Item2))
                    cardContent = [kindLine, .. cardContent.Where((line) => line.Item2 != kindLine.Item2).ToArray()];
            }

            // Iterate through all the lines
            StringBuilder valueBuilder = new();
            string[] allowedTypes = ["HOME", "WORK", "PREF"];
            string kind = "individual";
            for (int i = 0; i < CardContent.Length; i++)
            {
                // Get line
                var content = CardContent[i];
                string _value = VcardCommonTools.ConstructBlocks(CardContent, ref i);
                int lineNumber = content.Item1;
                if (string.IsNullOrEmpty(_value))
                    continue;

                try
                {
                    // Now, parse a property
                    var info = new PropertyInfo(_value, CardVersion);
                    var (type, enumeration, classType, fromString, defaultType, defaultValue, defaultValueType, extraAllowedTypes, allowedValues) = VcardParserTools.GetPartType(info.Prefix);
                    
                    // Handle arguments
                    int altId = -1;
                    if (info.Arguments.Length > 0)
                    {
                        // If we have more than one argument, check for ALTID
                        if (CardVersion.Major >= 4 && type == PartType.PartsArray)
                        {
                            var tuple = VcardParserTools.GetPartsArrayEnumFromType(classType, CardVersion);
                            var cardinality = tuple.Item2;
                            bool supportsAltId =
                                cardinality != PartCardinality.MayBeOneNoAltId && cardinality != PartCardinality.ShouldBeOneNoAltId &&
                                cardinality != PartCardinality.AtLeastOneNoAltId && cardinality != PartCardinality.AnyNoAltId;
                            var altIdArg = info.Arguments.SingleOrDefault((arg) => arg.Key == VcardConstants._altIdArgumentSpecifier);
                            if (supportsAltId)
                            {
                                // The type supports ALTID.
                                if (info.Arguments[0].Key == VcardConstants._altIdArgumentSpecifier)
                                {
                                    // We need ALTID to be numeric
                                    if (!int.TryParse(altIdArg.Values[0].value, out altId))
                                        throw new InvalidDataException("ALTID must be numeric");

                                    // We need ALTID to be positive
                                    if (altId < 0)
                                        throw new InvalidDataException("ALTID must be positive");

                                    // Here, we require arguments for ALTID
                                    if (info.Arguments.Length <= 1)
                                        throw new InvalidDataException("ALTID must have one or more arguments to specify why this instance is an alternative");
                                }
                                else if (altIdArg is not null)
                                    throw new InvalidDataException("ALTID must be exactly in the first position of the argument, because arguments that follow it are required to be specified");
                            }
                            else if (altIdArg is not null)
                                throw new InvalidDataException($"ALTID must not be specified in the {tuple.Item1} type that expects a cardinality of {cardinality}.");
                        }
                    }

                    // Check the type for allowed types
                    bool specifierRequired = CardVersion.Major >= 3;
                    string[] elementTypes = VcardCommonTools.GetTypes(info.Arguments, defaultType, specifierRequired);
                    foreach (string elementType in elementTypes)
                    {
                        string elementTypeUpper = elementType.ToUpper();
                        if (!allowedTypes.Contains(elementTypeUpper) && !extraAllowedTypes.Contains(elementTypeUpper) && !elementTypeUpper.StartsWith("X-"))
                            throw new InvalidDataException($"Part info type {classType?.Name ?? "<null>"} doesn't support property type {elementTypeUpper} because the following base types are supported: [{string.Join(", ", allowedTypes)}] and the extra types are supported: [{string.Join(", ", extraAllowedTypes)}]");
                    }

                    // Handle the part type
                    string valueType = VcardCommonTools.GetFirstValue(info.Arguments, defaultValueType, VcardConstants._valueArgumentSpecifier);
                    string finalValue = VcardCommonTools.ProcessStringValue(info.Value, valueType);

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
                                bool supported = VcardParserTools.StringSupported(stringType, CardVersion, kind);
                                if (!supported)
                                    continue;

                                // Let VisualCard know that we've explicitly specified a kind.
                                if (stringType == StringsEnum.Kind)
                                {
                                    kind = string.IsNullOrEmpty(finalValue) ? "individual" : finalValue;
                                    card.kindExplicitlySpecified = true;
                                }
                                else if (stringType == StringsEnum.Profile && !finalValue.Equals("vcard", StringComparison.OrdinalIgnoreCase))
                                    throw new InvalidDataException("Profile must be \"vCard\"");

                                // Set the string for real
                                var stringValueInfo = new CardValueInfo<string>(info.Arguments, altId, elementTypes, valueType, info.Group, finalValue);
                                card.AddString(stringType, stringValueInfo);
                            }
                            break;
                        case PartType.PartsArray:
                            {
                                PartsArrayEnum partsArrayType = (PartsArrayEnum)enumeration;
                                bool supported = VcardParserTools.EnumArrayTypeSupported(partsArrayType, CardVersion);
                                if (!supported)
                                    continue;
                                if (fromString is null)
                                    continue;

                                // Now, get the part info
                                finalValue = partsArrayType is PartsArrayEnum.NonstandardNames or PartsArrayEnum.IanaNames ? _value : info.Value;
                                var partInfo = fromString(finalValue, info.Arguments, altId, elementTypes, info.Group, valueType, CardVersion);

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
            if (nameCardinality == PartCardinality.ShouldBeOne)
                expectedFieldList.Add(VcardConstants._nameSpecifier);
            if (CardVersion.Major >= 3)
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
                            var values = component.GetString((StringsEnum)enumeration);
                            bool exists = values.Length > 0;
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

        internal VcardParser((int, string)[] cardContent, Version cardVersion)
        {
            this.cardContent = cardContent;
            this.cardVersion = cardVersion;
        }
    }
}
