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

using System.IO;
using System.Text;
using System;
using VisualCard.Parsers;
using VisualCard.Parts;
using VisualCard.Parts.Implementations;
using System.Linq;
using System.Collections.Generic;
using VisualCard.Parts.Enums;

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
            if (!meCardString.StartsWith(_meCardBegin) && !meCardString.EndsWith(_meCardEnd))
                throw new InvalidDataException("This string doesn't represent a valid MeCard contact.");

            // Now, parse it.
            try
            {
                // Split the meCard string from the beginning and the ending
                int beginningIdx = meCardString.IndexOf(_meCardArgumentDelimiter) + 1;
                int endingIdx = meCardString.IndexOf(_meCardEnd) - beginningIdx;
                meCardString = meCardString.Substring(beginningIdx, endingIdx);

                // Split the values from the semicolons
                var values = meCardString.Split(_meCardFieldDelimiter);
                string fullName = "";

                // Replace all the commas found with semicolons if possible
                for (int i = 0; i < values.Length; i++)
                {
                    string value = values[i];

                    // "SOUND:" here is actually just a Kana name, so demote it to X-nonstandard
                    if (value.StartsWith($"{_meCardSoundSpecifier}{_meCardArgumentDelimiter}"))
                    {
                        string xNonstandard = $"{VcardConstants._xSpecifier}{_meCardXNameKanaSpecifier}{_meCardArgumentDelimiter}";
                        values[i] = value.Replace(",", ";");
                        values[i] = xNonstandard + values[i].Substring(6);
                    }

                    // Now, replace all the commas in Name and Address with the semicolons.
                    if (value.StartsWith($"{_meCardNameSpecifier}{_meCardArgumentDelimiter}") || value.StartsWith($"{_meCardAddressSpecifier}{_meCardArgumentDelimiter}"))
                        values[i] = value.Replace(",", ";");

                    // Build a full name
                    if (value.StartsWith($"{_meCardNameSpecifier}{_meCardArgumentDelimiter}"))
                    {
                        var nameSplits = value.Substring(2).Split(',');
                        fullName = $"{nameSplits[1]} {nameSplits[0]}";
                    }

                    // "TEL-AV:" here is actually just "TEL;TYPE=VIDEO:[...]"
                    if (value.StartsWith($"{_meCardVideophoneSpecifier}{_meCardArgumentDelimiter}"))
                    {
                        string prefix =
                            $"{VcardConstants._telephoneSpecifier}" +
                            $"{VcardConstants._fieldDelimiter}" +
                            $"{VcardConstants._typeArgumentSpecifier}VIDEO{VcardConstants._argumentDelimiter}";
                        values[i] = prefix + values[i].Substring(7);
                    }
                }

                // Install the values!
                var masterContactBuilder = new StringBuilder(
                   $"""
                    {VcardConstants._beginText}
                    {VcardConstants._versionSpecifier}{VcardConstants._argumentDelimiter}3.0

                    """
                );
                foreach (var value in values)
                    masterContactBuilder.AppendLine(value);
                masterContactBuilder.AppendLine(
                   $"""
                    {VcardConstants._fullNameSpecifier}{VcardConstants._argumentDelimiter}{fullName}
                    {VcardConstants._endText}
                    """
                );

                // Now, invoke VisualCard to give us the card parsers
                return CardTools.GetCardsFromString(masterContactBuilder.ToString());
            }
            catch (Exception ex)
            {
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
            var fullNames = card.GetString(StringsEnum.FullName);
            var xNames = card.GetPartsArray<XNameInfo>();
            var telephones = card.GetString(StringsEnum.Telephones);
            var emails = card.GetString(StringsEnum.Mails);
            var notes = card.GetString(StringsEnum.Notes);
            var birthdays = card.GetPartsArray<BirthDateInfo>();
            var addresses = card.GetPartsArray<AddressInfo>();
            var urls = card.GetString(StringsEnum.Url);
            var nicknames = card.GetString(StringsEnum.Nicknames);

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
            if (!hasNames && !hasFullName)
                throw new InvalidDataException("Can't build a MeCard string from a vCard containing an empty name or an empty full name.");

            // Add the types
            List<string> properties = [];
            if (hasNames)
            {
                StringBuilder builder = new();
                var name = names[0];
                builder.Append(_meCardNameSpecifier + _meCardArgumentDelimiter);
                builder.Append(name.ContactLastName + _meCardValueDelimiter);
                builder.Append(name.ContactFirstName);
                properties.Add(builder.ToString());
            }
            else if (hasFullName)
            {
                StringBuilder builder = new();
                var fullName = fullNames[0].Value;
                string[] splitFullName = fullName.Split([" "], StringSplitOptions.RemoveEmptyEntries);
                builder.Append(_meCardNameSpecifier + _meCardArgumentDelimiter);
                builder.Append(string.Join(_meCardValueDelimiter.ToString(), splitFullName));
                properties.Add(builder.ToString());
            }
            if (hasReading)
            {
                StringBuilder builder = new();
                var kana = xNames.First((xName) => xName.XKeyName == _meCardXNameKanaSpecifier);
                builder.Append(_meCardSoundSpecifier + _meCardArgumentDelimiter);
                builder.Append(string.Join(_meCardValueDelimiter.ToString(), kana.XValues));
                properties.Add(builder.ToString());
            }
            if (hasTelephone)
            {
                StringBuilder builder = new();
                var telephone = telephones.First((tel) => !tel.HasType("video"));
                builder.Append(_meCardTelephoneSpecifier + _meCardArgumentDelimiter);
                builder.Append(telephone.Value);
                properties.Add(builder.ToString());
            }
            if (hasVideophone)
            {
                StringBuilder builder = new();
                var videophone = telephones.First((tel) => tel.HasType("video"));
                builder.Append(_meCardVideophoneSpecifier + _meCardArgumentDelimiter);
                builder.Append(videophone.Value);
                properties.Add(builder.ToString());
            }
            if (hasEmails)
            {
                StringBuilder builder = new();
                var email = emails[0];
                builder.Append(_meCardEmailSpecifier + _meCardArgumentDelimiter);
                builder.Append(email.Value);
                properties.Add(builder.ToString());
            }
            if (hasNote)
            {
                StringBuilder builder = new();
                var note = notes[0];
                builder.Append(_meCardNoteSpecifier + _meCardArgumentDelimiter);
                builder.Append(note.Value);
                properties.Add(builder.ToString());
            }
            if (hasBirthday)
            {
                StringBuilder builder = new();
                var birthday = birthdays[0];
                builder.Append(_meCardBirthdaySpecifier + _meCardArgumentDelimiter);
                builder.Append($"{birthday.BirthDate:yyyyMMdd}");
                properties.Add(builder.ToString());
            }
            if (hasAddresses)
            {
                StringBuilder builder = new();
                var address = addresses[0];
                builder.Append(_meCardAddressSpecifier + _meCardArgumentDelimiter);
                builder.Append(address.PostOfficeBox + _meCardValueDelimiter);
                builder.Append(address.ExtendedAddress + _meCardValueDelimiter);
                builder.Append(address.StreetAddress + _meCardValueDelimiter);
                builder.Append(address.Locality + _meCardValueDelimiter);
                builder.Append(address.Region + _meCardValueDelimiter);
                builder.Append(address.PostalCode + _meCardValueDelimiter);
                builder.Append(address.Country);
                properties.Add(builder.ToString());
            }
            if (hasUrl)
            {
                StringBuilder builder = new();
                var url = urls[0];
                builder.Append(_meCardUrlSpecifier + _meCardArgumentDelimiter);
                builder.Append(url.Value);
                properties.Add(builder.ToString());
            }
            if (hasNicknames)
            {
                StringBuilder builder = new();
                var nickname = nicknames[0];
                builder.Append(_meCardNicknameSpecifier + _meCardArgumentDelimiter);
                builder.Append(nickname.Value);
                properties.Add(builder.ToString());
            }

            // Now, build the MeCard string
            StringBuilder meCard = new(_meCardBegin);
            meCard.Append(string.Join(_meCardFieldDelimiter.ToString(), properties));
            meCard.Append(_meCardEnd);
            return meCard.ToString();
        }
    }
}
