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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Textify.General;
using VisualCard.Common.Diagnostics;
using VisualCard.Common.Parsers;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts;
using VisualCard.Common.Parts.Enums;
using VisualCard.Common.Parts.Implementations;
using VisualCard.Exceptions;
using VisualCard.Languages;
using VisualCard.Parts;
using VisualCard.Parts.Enums;

namespace VisualCard.Parsers
{
    /// <summary>
    /// The base vCard parser
    /// </summary>
    [DebuggerDisplay("vCard contact, version {CardVersion.ToString()}, {CardContent.Length} lines")]
    internal class VcardParser
    {
        internal List<Card> nestedCards = [];
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
            LoggingTools.Debug("Content lines is {0}...", CardContent.Length);
            if (CardContent.Length == 0)
                throw new InvalidDataException(LanguageTools.GetLocalized("VISUALCARD_PARSER_EXCEPTION_CARDCONTENTEMPTY"));

            // Make a new vCard
            var card = new Card(CardVersion);
            LoggingTools.Info("Made a new card instance with version {0}...", CardVersion.ToString());

            // Move kind to the top
            if (CardVersion.Major >= 4)
            {
                var kindLine = CardContent.SingleOrDefault((line) => line.Item2.ToUpper().StartsWith(VcardConstants._kindSpecifier));
                LoggingTools.Info("Found kind line {0} with content {1}...", kindLine.Item1, kindLine.Item2);
                if (!string.IsNullOrEmpty(kindLine.Item2))
                {
                    LoggingTools.Debug("Moving kind line {0} to the top...", kindLine.Item1);
                    cardContent = [kindLine, .. cardContent.Where((line) => line.Item2 != kindLine.Item2).ToArray()];
                }
            }

            // Iterate through all the lines
            StringBuilder valueBuilder = new();
            for (int i = 0; i < CardContent.Length; i++)
            {
                // Get line
                var contentLines = CardContent.Select((tuple) => tuple.Item2).ToArray();
                var content = CardContent[i];
                string _value = CommonTools.ConstructBlocks(contentLines, ref i);
                int lineNumber = content.Item1;
                LoggingTools.Debug("Content number {0} [idx: {1}] constructed to {2}...", lineNumber, i, _value);
                if (string.IsNullOrEmpty(_value))
                    continue;

                // Process the line
                try
                {
                    Process(_value, card, CardVersion);
                }
                catch (Exception ex)
                {
                    throw new VCardParseException(ex.Message, _value, lineNumber, ex);
                }
            }

            // Add any possible nested cards
            card.nestedCards = nestedCards;
            LoggingTools.Debug("Added {0} nested cards", nestedCards.Count);

            // Validate this card before returning it.
            card.Validate();
            LoggingTools.Info("Returning valid card...");
            return card;
        }

        internal static void Process(string _value, Card card, Version version)
        {
            string[] allowedTypes = ["HOME", "WORK", "PREF"];
            string kind = card.CardKindStr;
            LoggingTools.Debug("Processing value with kind {0} in version {1}: {2}", kind, version.ToString(), _value);

            // Parse a property
            var info = new PropertyInfo(_value);
            var partType = VcardParserTools.GetPartType(info.Prefix, version, kind);

            // Handle AltID
            int altId = VcardParserTools.GetAltIdFromArgs(version, info, partType);
            LoggingTools.Debug("Got altid {0}", altId);

            // Check the type for allowed types
            bool specifierRequired = version.Major >= 3;
            LoggingTools.Debug("Needs specifier: {0}", specifierRequired);
            string[] elementTypes = CommonTools.GetTypes(info.Arguments, partType.defaultType, specifierRequired);
            LoggingTools.Debug("Got {0} element types [{1}]", elementTypes.Length, string.Join(", ", elementTypes));
            foreach (string elementType in elementTypes)
            {
                string elementTypeUpper = elementType.ToUpper();
                LoggingTools.Debug("Processing element type [{0}, resolved to {1}]", elementType, elementTypeUpper);
                if (!allowedTypes.Contains(elementTypeUpper) && !partType.allowedExtraTypes.Contains(elementTypeUpper) && !elementTypeUpper.StartsWith("X-"))
                {
                    if (partType.type == PartType.PartsArray &&
                        ((CardPartsArrayEnum)partType.enumeration == CardPartsArrayEnum.IanaNames ||
                         (CardPartsArrayEnum)partType.enumeration == CardPartsArrayEnum.NonstandardNames))
                        continue;
                    LoggingTools.Error("Element type {0} is not in the list of allowed types", elementTypeUpper);
                    throw new InvalidDataException(LanguageTools.GetLocalized("VISUALCARD_PARSER_EXCEPTION_PARTTYPEUNSUPPORTED").FormatString(partType.enumType?.Name ?? "<null>", elementTypeUpper, string.Join(", ", allowedTypes), string.Join(", ", partType.allowedExtraTypes)));
                }
            }

            // Handle the part type
            string valueType = CommonTools.GetFirstValue(info.Arguments, partType.defaultValueType, CommonConstants._valueArgumentSpecifier);
            string finalValue = CommonTools.ProcessStringValue(info.Value, valueType);
            info.ValueType = valueType;
            LoggingTools.Debug("Got value [type: {0}]: {1}", valueType, finalValue);

            // Check for allowed values
            if (partType.allowedValues.Length != 0)
            {
                bool found = false;
                LoggingTools.Debug("Comparing value against {0} allowed values [{1}] [type: {2}]: {3}", partType.allowedValues.Length, string.Join(", ", partType.allowedValues), valueType, finalValue);
                foreach (string allowedValue in partType.allowedValues)
                {
                    if (finalValue == allowedValue)
                        found = true;
                }
                if (!found)
                    throw new InvalidDataException(LanguageTools.GetLocalized("VISUALCARD_PARSER_EXCEPTION_VALUEDISALLOWED").FormatString(finalValue, string.Join(", ", partType.allowedValues)));
                LoggingTools.Debug("Found allowed value [type: {0}]: {1}", valueType, finalValue);
            }

            // Check for support
            bool supported = partType.minimumVersionCondition(version);
            if (!supported)
            {
                LoggingTools.Warning("Part {0} doesn't support version {1}", partType.enumeration, version.ToString());
                return;
            }

            // Process the value
            LoggingTools.Debug("Part {0} is {1}", partType.enumeration, partType.type);
            switch (partType.type)
            {
                case PartType.Strings:
                    {
                        CardStringsEnum stringType = (CardStringsEnum)partType.enumeration;

                        // Check if the profile is vCard or not.
                        if (stringType == CardStringsEnum.Profile && !finalValue.Equals("vcard", StringComparison.OrdinalIgnoreCase))
                        {
                            LoggingTools.Error("String part is {0} and value is not vcard [{1}]", stringType, finalValue);
                            throw new InvalidDataException(LanguageTools.GetLocalized("VISUALCARD_PARSER_EXCEPTION_INVALIDPROFILE"));
                        }

                        // Set the string for real
                        var stringValueInfo = new ValueInfo<string>(info, altId, elementTypes, finalValue);
                        card.AddString(stringType, stringValueInfo);
                        LoggingTools.Debug("Added string {0} with value {1}", stringType, finalValue);
                    }
                    break;
                case PartType.PartsArray:
                    {
                        CardPartsArrayEnum partsArrayType = (CardPartsArrayEnum)partType.enumeration;
                        bool isExtra = partsArrayType is CardPartsArrayEnum.NonstandardNames or CardPartsArrayEnum.IanaNames;

                        // Now, get the part info
                        if (isExtra)
                        {
                            // Get the base part info from the nonstandard value, stripping the group part from the initial value.
                            if (info.Group.Length > 0)
                                _value = _value.Substring(info.Group.Length + 1);
                            var partInfo =
                                partsArrayType == CardPartsArrayEnum.NonstandardNames ?
                                XNameInfo.FromStringStatic(_value, info, altId, elementTypes, version) :
                                ExtraInfo.FromStringStatic(_value, info, altId, elementTypes, version);
                            card.AddExtraPartToArray((PartsArrayEnum)partsArrayType, partInfo);
                            LoggingTools.Debug("Added extra part {0} with value {1}", partsArrayType, _value);
                        }
                        else
                        {
                            if (partType.fromStringFunc is null)
                                return;

                            // Get the part info from the part type and add it to the part array
                            var partInfo = partType.fromStringFunc(finalValue, info, altId, elementTypes, version);
                            card.AddPartToArray(partsArrayType, partInfo);
                            LoggingTools.Debug("Added part {0} to array with value {1}", partsArrayType, finalValue);
                        }
                    }
                    break;
                default:
                    LoggingTools.Error("Unknown part {0}", partType.type);
                    throw new InvalidDataException(LanguageTools.GetLocalized("VISUALCARD_PARSER_EXCEPTION_INVALIDTYPE").FormatString(partType.type));
            }
        }

        internal VcardParser((int, string)[] cardContent, Version cardVersion)
        {
            this.cardContent = cardContent;
            this.cardVersion = cardVersion;
        }
    }
}
