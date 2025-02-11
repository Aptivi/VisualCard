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

using System.IO;
using System.Text;
using System;
using VisualCard.Parsers;
using VisualCard.Parts;
using VisualCard.Parts.Implementations;
using System.Linq;
using System.Collections.Generic;
using VisualCard.Parts.Enums;
using VisualCard.Common.Parts.Implementations;
using VisualCard.Common.Parsers;
using VisualCard.Common.Diagnostics;

namespace VisualCard.Extras.Converters
{
    /// <summary>
    /// MeCard string generator and converter
    /// </summary>
    public static class MeCard
    {
        private const string _meCardBegin = "MECARD:";
        private const string _meCardEnd = ";;";
        private const char _meCardArgumentDelimiter = ':';
        private const char _meCardFieldDelimiter = ';';
        private const char _meCardValueDelimiter = ',';
        private const string _meCardNameSpecifier = "N";
        private const string _meCardSoundSpecifier = "SOUND";
        private const string _meCardTelephoneSpecifier = "TEL";
        private const string _meCardVideophoneSpecifier = "TEL-AV";
        private const string _meCardEmailSpecifier = "EMAIL";
        private const string _meCardNoteSpecifier = "NOTE";
        private const string _meCardBirthdaySpecifier = "BDAY";
        private const string _meCardAddressSpecifier = "ADR";
        private const string _meCardUrlSpecifier = "URL";
        private const string _meCardNicknameSpecifier = "NICKNAME";
        private const string _meCardXNameKanaSpecifier = "VISUALCARD-KANA";

        /// <summary>
        /// Gets all contacts from a MeCard string
        /// </summary>
        /// <param name="meCardString">MeCard string</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        public static Card[] GetContactsFromMeCardString(string meCardString)
        {
            // Check to see if the MeCard string is valid
            if (string.IsNullOrWhiteSpace(meCardString))
                throw new InvalidDataException("MeCard string should not be empty.");
            LoggingTools.Info("Checking for prefix and suffix...");
            if (!meCardString.StartsWith(_meCardBegin) && !meCardString.EndsWith(_meCardEnd))
            {
                LoggingTools.Error("MeCard didn't start with {0} and end with {1}", _meCardBegin, _meCardEnd);
                throw new InvalidDataException("This string doesn't represent a valid MeCard contact.");
            }

            // Now, parse it.
            LoggingTools.Info("Parsing MeCard...");
            try
            {
                // Split the meCard string from the beginning and the ending
                int beginningIdx = meCardString.IndexOf(_meCardArgumentDelimiter) + 1;
                int endingIdx = meCardString.IndexOf(_meCardEnd) - beginningIdx;
                LoggingTools.Debug("Stripping {0} with indexes [{1} to {2}].", meCardString, beginningIdx, endingIdx);
                meCardString = meCardString.Substring(beginningIdx, endingIdx);
                LoggingTools.Debug("Final MeCard string: {0}", meCardString);

                // Split the values from the semicolons
                var values = meCardString.Split(_meCardFieldDelimiter);
                LoggingTools.Debug("Values: {0}", values.Length);
                string fullName = "";

                // Replace all the commas found with semicolons if possible
                for (int i = 0; i < values.Length; i++)
                {
                    string value = values[i];
                    LoggingTools.Debug("Parsing value {0}", value);

                    // "SOUND:" here is actually just a Kana name, so demote it to X-nonstandard
                    LoggingTools.Debug("Checking for Kana sound (SOUND:)");
                    if (value.StartsWith($"{_meCardSoundSpecifier}{_meCardArgumentDelimiter}"))
                    {
                        LoggingTools.Info("Kana sound (SOUND:) found!");
                        string xNonstandard = $"{CommonConstants._xSpecifier}{_meCardXNameKanaSpecifier}{_meCardArgumentDelimiter}";
                        values[i] = value.Replace(",", ";");
                        values[i] = xNonstandard + values[i].Substring(6);
                        LoggingTools.Debug("Populated X-nonstandard value: {0}", values[i]);
                    }

                    // Now, replace all the commas in Name and Address with the semicolons.
                    LoggingTools.Debug("Checking for name (N:) or address (ADR:)");
                    if (value.StartsWith($"{_meCardNameSpecifier}{_meCardArgumentDelimiter}") || value.StartsWith($"{_meCardAddressSpecifier}{_meCardArgumentDelimiter}"))
                    {
                        LoggingTools.Info("Name (N:) or address (ADR:) found!");
                        values[i] = value.Replace(",", ";");
                        LoggingTools.Debug("Replaced ',' with ';' in value {0}", values[i]);
                    }

                    // Build a full name
                    LoggingTools.Debug("Checking for name (N:)");
                    if (value.StartsWith($"{_meCardNameSpecifier}{_meCardArgumentDelimiter}"))
                    {
                        LoggingTools.Info("Name (N:) found!");
                        var nameSplits = value.Substring(2).Split(',');
                        LoggingTools.Debug("Name split to {0} pieces", nameSplits.Length);
                        fullName = $"{nameSplits[1]} {nameSplits[0]}";
                        LoggingTools.Debug("Full name built: {0}", fullName);
                    }

                    // "TEL-AV:" here is actually just "TEL;TYPE=VIDEO:[...]"
                    LoggingTools.Debug("Checking for videophone (TEL-AV:)");
                    if (value.StartsWith($"{_meCardVideophoneSpecifier}{_meCardArgumentDelimiter}"))
                    {
                        LoggingTools.Info("Videophone (TEL-AV:) found!");
                        string prefix =
                            $"{VcardConstants._telephoneSpecifier}" +
                            $"{CommonConstants._fieldDelimiter}" +
                            $"{CommonConstants._typeArgumentSpecifier}=VIDEO{CommonConstants._argumentDelimiter}";
                        LoggingTools.Debug("Populated prefix: {0}", prefix);
                        values[i] = prefix + values[i].Substring(7);
                        LoggingTools.Debug("Resulting value: {0}", values[i]);
                    }
                }
                LoggingTools.Info("All values parsed!");

                // Install the values!
                LoggingTools.Debug("Appending vCard 3.0 header...");
                var masterContactBuilder = new StringBuilder(
                   $"""
                    {VcardConstants._beginText}
                    {CommonConstants._versionSpecifier}{CommonConstants._argumentDelimiter}3.0

                    """
                );
                LoggingTools.Info("Installing values...");
                foreach (var value in values)
                {
                    LoggingTools.Debug("Installing {0}...", value);
                    masterContactBuilder.AppendLine(value);
                }
                LoggingTools.Debug("Appending vCard 3.0 footer...");
                masterContactBuilder.AppendLine(
                   $"""
                    {VcardConstants._fullNameSpecifier}{CommonConstants._argumentDelimiter}{fullName}
                    {VcardConstants._endText}
                    """
                );

                // Now, invoke VisualCard to give us the card parsers
                LoggingTools.Info("Parsing cards...");
                return CardTools.GetCardsFromString(masterContactBuilder.ToString());
            }
            catch (Exception ex)
            {
                LoggingTools.Error(ex, "Can't parse the MeCard string {0}", meCardString);
                throw new InvalidDataException("The MeCard contact string is not valid.", ex);
            }
        }

        /// <summary>
        /// Saves the vCard <see cref="Card"/> instance to a MeCard string for QR code generation
        /// </summary>
        /// <param name="card">Card instance</param>
        /// <param name="compatibility">Compatibility mode (excludes some MeCard values)</param>
        /// <returns>A MeCard string</returns>
        public static string SaveCardToMeCardString(Card? card, bool compatibility = false)
        {
            // Check the card for validity
            if (card is null)
                throw new ArgumentNullException(nameof(card), "Card is not provided.");

            // Now, get all the values in the below order
            var names = card.GetPartsArray<NameInfo>();
            var fullNames = card.GetString(CardStringsEnum.FullName);
            var xNames = card.GetExtraPartsArray<XNameInfo>();
            var telephones = card.GetString(CardStringsEnum.Telephones);
            var emails = card.GetString(CardStringsEnum.Mails);
            var notes = card.GetString(CardStringsEnum.Notes);
            var birthdays = card.GetPartsArray<BirthDateInfo>();
            var addresses = card.GetPartsArray<AddressInfo>();
            var urls = card.GetString(CardStringsEnum.Url);
            var nicknames = card.GetString(CardStringsEnum.Nicknames);

            // Check them for existence
            bool hasNames = names.Length > 0;
            bool hasFullName = fullNames.Length > 0;
            bool hasReading = xNames.Any((xName) => xName.XKeyName == _meCardXNameKanaSpecifier);
            bool hasTelephone = telephones.Length > 0 && telephones.Any((tel) => !tel.HasType("video"));
            bool hasVideophone = telephones.Length > 0 && telephones.Any((tel) => tel.HasType("video")) && !compatibility;
            bool hasEmails = emails.Length > 0;
            bool hasNote = notes.Length > 0 && !compatibility;
            bool hasBirthday = birthdays.Length > 0;
            bool hasAddresses = addresses.Length > 0;
            bool hasUrl = urls.Length > 0 && !compatibility;
            bool hasNicknames = nicknames.Length > 0 && !compatibility;
            LoggingTools.Debug("Checking for names and full name...");
            if (!hasNames && !hasFullName)
            {
                LoggingTools.Error("No name and full name to build MeCard. Throwing error...");
                throw new InvalidDataException("Can't build a MeCard string from a vCard containing an empty name or an empty full name.");
            }

            // Add the types
            List<string> properties = [];
            if (hasNames)
            {
                StringBuilder builder = new();
                var name = names[0];
                LoggingTools.Debug("Got first name {0} {1}.", name.ContactFirstName, name.ContactLastName);
                builder.Append(_meCardNameSpecifier + _meCardArgumentDelimiter);
                builder.Append(name.ContactLastName + _meCardValueDelimiter);
                builder.Append(name.ContactFirstName);
                LoggingTools.Debug("Built as {0}.", builder.ToString());
                properties.Add(builder.ToString());
                LoggingTools.Info("Added name property");
            }
            else if (hasFullName)
            {
                StringBuilder builder = new();
                var fullName = fullNames[0].Value;
                LoggingTools.Debug("Got full name {0}.", fullName);
                string[] splitFullName = fullName.Split([" "], StringSplitOptions.RemoveEmptyEntries);
                builder.Append(_meCardNameSpecifier + _meCardArgumentDelimiter);
                builder.Append(string.Join(_meCardValueDelimiter.ToString(), splitFullName));
                LoggingTools.Debug("Built as {0}.", builder.ToString());
                properties.Add(builder.ToString());
                LoggingTools.Info("Added full name property");
            }
            if (hasReading)
            {
                StringBuilder builder = new();
                var kana = xNames.First((xName) => xName.XKeyName == _meCardXNameKanaSpecifier);
                LoggingTools.Debug("Got Kana sound with {0} values.", kana.XValues?.Length ?? 0);
                builder.Append(_meCardSoundSpecifier + _meCardArgumentDelimiter);
                builder.Append(string.Join(_meCardValueDelimiter.ToString(), kana.XValues));
                LoggingTools.Debug("Built as {0}.", builder.ToString());
                properties.Add(builder.ToString());
                LoggingTools.Info("Added Kana sound property");
            }
            if (hasTelephone)
            {
                StringBuilder builder = new();
                var telephone = telephones.First((tel) => !tel.HasType("video"));
                LoggingTools.Debug("Got telephone {0}.", telephone.Value);
                builder.Append(_meCardTelephoneSpecifier + _meCardArgumentDelimiter);
                builder.Append(telephone.Value);
                LoggingTools.Debug("Built as {0}.", builder.ToString());
                properties.Add(builder.ToString());
                LoggingTools.Info("Added telephone property");
            }
            if (hasVideophone)
            {
                StringBuilder builder = new();
                var videophone = telephones.First((tel) => tel.HasType("video"));
                LoggingTools.Debug("Got videophone {0}.", videophone.Value);
                builder.Append(_meCardVideophoneSpecifier + _meCardArgumentDelimiter);
                builder.Append(videophone.Value);
                LoggingTools.Debug("Built as {0}.", builder.ToString());
                properties.Add(builder.ToString());
                LoggingTools.Info("Added videophone property");
            }
            if (hasEmails)
            {
                StringBuilder builder = new();
                var email = emails[0];
                LoggingTools.Debug("Got email {0}.", email.Value);
                builder.Append(_meCardEmailSpecifier + _meCardArgumentDelimiter);
                builder.Append(email.Value);
                LoggingTools.Debug("Built as {0}.", builder.ToString());
                properties.Add(builder.ToString());
                LoggingTools.Info("Added email property");
            }
            if (hasNote)
            {
                StringBuilder builder = new();
                var note = notes[0];
                LoggingTools.Debug("Got note {0}.", note.Value);
                builder.Append(_meCardNoteSpecifier + _meCardArgumentDelimiter);
                builder.Append(note.Value);
                LoggingTools.Debug("Built as {0}.", builder.ToString());
                properties.Add(builder.ToString());
                LoggingTools.Info("Added note property");
            }
            if (hasBirthday)
            {
                StringBuilder builder = new();
                var birthday = birthdays[0];
                LoggingTools.Debug("Got birthday {0}.", birthday.BirthDate);
                builder.Append(_meCardBirthdaySpecifier + _meCardArgumentDelimiter);
                builder.Append($"{birthday.BirthDate:yyyyMMdd}");
                LoggingTools.Debug("Built as {0}.", builder.ToString());
                properties.Add(builder.ToString());
                LoggingTools.Info("Added birthday property");
            }
            if (hasAddresses)
            {
                StringBuilder builder = new();
                var address = addresses[0];
                LoggingTools.Debug("Got address {0}, {1}, {2}, {3}, {4}, {5}, and {6}.", address.PostOfficeBox, address.ExtendedAddress, address.StreetAddress, address.Locality, address.Region, address.PostalCode, address.Country);
                builder.Append(_meCardAddressSpecifier + _meCardArgumentDelimiter);
                builder.Append(address.PostOfficeBox + _meCardValueDelimiter);
                builder.Append(address.ExtendedAddress + _meCardValueDelimiter);
                builder.Append(address.StreetAddress + _meCardValueDelimiter);
                builder.Append(address.Locality + _meCardValueDelimiter);
                builder.Append(address.Region + _meCardValueDelimiter);
                builder.Append(address.PostalCode + _meCardValueDelimiter);
                builder.Append(address.Country);
                LoggingTools.Debug("Built as {0}.", builder.ToString());
                properties.Add(builder.ToString());
                LoggingTools.Info("Added address property");
            }
            if (hasUrl)
            {
                StringBuilder builder = new();
                var url = urls[0];
                LoggingTools.Debug("Got URL {0}.", url.Value);
                builder.Append(_meCardUrlSpecifier + _meCardArgumentDelimiter);
                builder.Append(url.Value);
                LoggingTools.Debug("Built as {0}.", builder.ToString());
                properties.Add(builder.ToString());
                LoggingTools.Info("Added URL property");
            }
            if (hasNicknames)
            {
                StringBuilder builder = new();
                var nickname = nicknames[0];
                LoggingTools.Debug("Got nickname {0}.", nickname.Value);
                builder.Append(_meCardNicknameSpecifier + _meCardArgumentDelimiter);
                builder.Append(nickname.Value);
                LoggingTools.Debug("Built as {0}.", builder.ToString());
                properties.Add(builder.ToString());
                LoggingTools.Info("Added nickname property");
            }
            LoggingTools.Info("To be installed: {0} properties", properties.Count);

            // Now, build the MeCard string
            LoggingTools.Debug("Adding header {0}", _meCardBegin);
            StringBuilder meCard = new(_meCardBegin);
            LoggingTools.Debug("Adding {0} properties delimited by ;", properties.Count);
            meCard.Append(string.Join(_meCardFieldDelimiter.ToString(), properties));
            LoggingTools.Debug("Adding footer {0}", _meCardEnd);
            meCard.Append(_meCardEnd);

            // Return the result
            LoggingTools.Info("Result: {0}", meCard.ToString());
            return meCard.ToString();
        }
    }
}
