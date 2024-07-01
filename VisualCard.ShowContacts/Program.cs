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
using System.Diagnostics;
using System.Linq;
using Terminaux.Colors.Data;
using Terminaux.Writer.ConsoleWriters;
using VisualCard.Converters;
using VisualCard.Extras;
using VisualCard.Parts;
using VisualCard.Parts.Enums;
using VisualCard.Parts.Implementations;

namespace VisualCard.ShowContacts
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                TextWriterColor.WriteColor("Path to contact file is required.", ConsoleColors.Red);
            }
            else
            {
                // If one of the arguments is a switch to trigger printing, set it
                bool print = !args.Contains("-noprint");
                bool save = args.Contains("-save");
                bool dbg = args.Contains("-debug");
                bool android = args.Contains("-android");
                bool mecard = args.Contains("-mecard");
                bool gen = args.Contains("-gen");
                args = args.Except(["-noprint", "-save", "-debug", "-android", "-mecard", "-gen"]).ToArray();

                // If debug, wait for debugger
                if (dbg)
                    Debugger.Launch();

                // If mecard, get a MeCard string
                string meCardString = "";
                if (mecard)
                    meCardString = args[0];

                // Initialize stopwatch
                Stopwatch elapsed = new();
                elapsed.Start();

                // Parse all contacts
                Card[] contacts =
                    gen ? CardGenerator.GenerateCards() :
                    android ? (args.Length > 0 ? AndroidContactsDb.GetContactsFromDb(args[0]) : AndroidContactsDb.GetContactsFromDb()) :
                    mecard ? MeCard.GetContactsFromMeCardString(meCardString) :
                    CardTools.GetCards(args[0]);

                // If told to save them, do it
                foreach (var contact in contacts)
                {
                    if (save)
                        contact.SaveTo($"contact_{DateTime.Now:dd-MM-yyyy_HH-mm-ss_ffffff}.vcf");
                }

                // If not printing, exit
                elapsed.Stop();
                if (!print)
                {
                    TextWriterColor.Write("Elapsed time: {0}", elapsed.Elapsed.ToString());
                    return;
                }

                // Show contact information
                bool showVcard5Disclaimer = contacts.Any((card) => card.CardVersion.ToString(2) == "5.0");
                foreach (Card Contact in contacts)
                {
                    TextWriterColor.WriteColor("----------------------------", ConsoleColors.Green);

                    // List names
                    foreach (var fullName in Contact.GetPartsArray<FullNameInfo>())
                    {
                        TextWriterColor.Write("Name:                    {0}", fullName.FullName);
                        TextWriterColor.Write("ALTID:                   {0}", fullName.AltId);
                    }

                    // List names
                    foreach (var name in Contact.GetPartsArray<NameInfo>())
                    {
                        TextWriterColor.Write("First name:              {0}", name.ContactFirstName);
                        TextWriterColor.Write("Last name:               {0}", name.ContactLastName);
                        TextWriterColor.Write("ALTID:                   {0}", name.AltId);
                    }

                    // List titles
                    foreach (var title in Contact.GetPartsArray<TitleInfo>())
                    { 
                        TextWriterColor.Write("Title or Job:            {0}", title.ContactTitle);
                        TextWriterColor.Write("ALTID:                   {0}", title.AltId);
                    }

                    // List addresses
                    foreach (var Address in Contact.GetPartsArray<AddressInfo>())
                    {
                        TextWriterColor.Write("P.O. Box:                {0}", Address.PostOfficeBox);
                        TextWriterColor.Write("Extended Address:        {0}", Address.ExtendedAddress);
                        TextWriterColor.Write("Street Address:          {0}", Address.StreetAddress);
                        TextWriterColor.Write("Region:                  {0}", Address.Region);
                        TextWriterColor.Write("Locality:                {0}", Address.Locality);
                        TextWriterColor.Write("Postal Code:             {0}", Address.PostalCode);
                        TextWriterColor.Write("Country:                 {0}", Address.Country);
                    }

                    // List e-mails
                    foreach (var Email in Contact.GetPartsArray<EmailInfo>())
                    {
                        TextWriterColor.Write("Email address:           {0}", Email.ContactEmailAddress);
                    }

                    // List organizations
                    foreach (var Organization in Contact.GetPartsArray<OrganizationInfo>())
                    {
                        TextWriterColor.Write("Organization Name:       {0}", Organization.Name);
                        TextWriterColor.Write("Organization Unit:       {0}", Organization.Unit);
                        TextWriterColor.Write("Organization Unit Role:  {0}", Organization.Role);
                    }

                    // List telephones
                    foreach (var Telephone in Contact.GetPartsArray<TelephoneInfo>())
                    {
                        TextWriterColor.Write("Phone number:            {0}", Telephone.ContactPhoneNumber);
                    }

                    // List photos
                    foreach (var Photo in Contact.GetPartsArray<PhotoInfo>())
                    {
                        TextWriterColor.Write("Photo encoding:          {0}", Photo.Encoding);
                        TextWriterColor.Write("Photo value type:        {0}", string.Join(",", Photo.ElementTypes));
                        TextWriterColor.Write("ALTID:                   {0}", Photo.AltId);
                        TextWriterColor.Write("Photo data [blob: {0}]: \n{1}", true, Photo.IsBlob, Photo.PhotoEncoded);
                    }

                    // List roles
                    foreach (var Role in Contact.GetPartsArray<RoleInfo>())
                    {
                        TextWriterColor.Write("Role:                    {0}", Role.ContactRole);
                        TextWriterColor.Write("ALTID:                   {0}", Role.AltId);
                    }

                    // List remaining
                    var birth = Contact.GetPartsArray<BirthDateInfo>();
                    var wedding = Contact.GetPartsArray<AnniversaryInfo>();
                    var gender = Contact.GetPartsArray<GenderInfo>();
                    var url = Contact.GetPartsArray<UrlInfo>();
                    var note = Contact.GetPartsArray<NoteInfo>();
                    if (birth.Length > 0)
                        TextWriterColor.Write("Contact birthdate:       {0}", birth[0].BirthDate);
                    if (wedding.Length > 0)
                        TextWriterColor.Write("Contact wedding date:    {0}", wedding[0].Anniversary);
                    if (gender.Length > 0)
                        TextWriterColor.Write("Contact gender           {0} [{1}]", gender[0].Gender.ToString(), gender[0].GenderDescription);
                    if (url.Length > 0)
                        TextWriterColor.Write("Contact URL:             {0}", url[0].Url);
                    if (note.Length > 0)
                        TextWriterColor.Write("Contact Note:            {0}", note[0].Note);
                    TextWriterColor.Write("Contact mailer:          {0}", Contact.GetString(StringsEnum.Mailer));

                    // Print VCard
                    string raw = Contact.SaveToString();
                    TextWriterColor.WriteColor(
                        "\nRaw VCard\n" +
                        "---------\n"
                        , ConsoleColors.Green
                    );
                    TextWriterColor.Write(raw);
                }
                if (showVcard5Disclaimer)
                    TextWriterColor.WriteColor("This application uses vCard 5.0, a revised version of vCard 4.0, made by Aptivi.", ConsoleColors.Silver);
                TextWriterColor.Write("Elapsed time: {0}", elapsed.Elapsed.ToString());
            }
        }
    }
}
