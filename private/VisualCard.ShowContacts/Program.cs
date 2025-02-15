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
using System.Diagnostics;
using System.Linq;
using Terminaux.Colors.Data;
using Terminaux.Writer.ConsoleWriters;
using VisualCard.Extras.Converters;
using VisualCard.Parts;
using VisualCard.Parts.Implementations;
using VisualCard.Extras.Misc;
using VisualCard.Parts.Enums;
using VisualCard.Common.Diagnostics;
using Aptivestigate.Serilog;
using Serilog;
using Aptivestigate.Logging;

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
                // Enable logging
                LoggingTools.EnableLogging = true;

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
                        contact.SaveTo($"contact_{DateTimeOffset.Now:dd-MM-yyyy_HH-mm-ss_ffffff}.vcf");
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
                    foreach (var fullName in Contact.GetString(CardStringsEnum.FullName))
                    {
                        TextWriterColor.Write("Name:                    {0} [G: {1}]", fullName.Value, fullName.Group);
                        TextWriterColor.Write("ALTID:                   {0} [G: {1}]", fullName.AltId, fullName.Group);
                    }

                    // List names
                    foreach (var name in Contact.GetPartsArray<NameInfo>())
                    {
                        TextWriterColor.Write("First name:              {0} [G: {1}]", name.ContactFirstName ?? "", name.Group);
                        TextWriterColor.Write("Last name:               {0} [G: {1}]", name.ContactLastName ?? "", name.Group);
                        TextWriterColor.Write("ALTID:                   {0} [G: {1}]", name.AltId, name.Group);
                    }

                    // List titles
                    foreach (var title in Contact.GetString(CardStringsEnum.Titles))
                    { 
                        TextWriterColor.Write("Title or Job:            {0} [G: {1}]", title.Value, title.Group);
                        TextWriterColor.Write("ALTID:                   {0} [G: {1}]", title.AltId, title.Group);
                    }

                    // List addresses
                    foreach (var Address in Contact.GetPartsArray<AddressInfo>())
                    {
                        TextWriterColor.Write("P.O. Box:                {0} [G: {1}]", Address.PostOfficeBox ?? "", Address.Group);
                        TextWriterColor.Write("Extended Address:        {0} [G: {1}]", Address.ExtendedAddress ?? "", Address.Group);
                        TextWriterColor.Write("Street Address:          {0} [G: {1}]", Address.StreetAddress ?? "", Address.Group);
                        TextWriterColor.Write("Region:                  {0} [G: {1}]", Address.Region ?? "", Address.Group);
                        TextWriterColor.Write("Locality:                {0} [G: {1}]", Address.Locality ?? "", Address.Group);
                        TextWriterColor.Write("Postal Code:             {0} [G: {1}]", Address.PostalCode ?? "", Address.Group);
                        TextWriterColor.Write("Country:                 {0} [G: {1}]", Address.Country ?? "", Address.Group);
                    }

                    // List e-mails
                    foreach (var Email in Contact.GetString(CardStringsEnum.Mails))
                    {
                        TextWriterColor.Write("Email address:           {0} [G: {1}]", Email.Value, Email.Group);
                    }

                    // List organizations
                    foreach (var Organization in Contact.GetPartsArray<OrganizationInfo>())
                    {
                        TextWriterColor.Write("Organization Name:       {0} [G: {1}]", Organization.Name ?? "", Organization.Group);
                        TextWriterColor.Write("Organization Unit:       {0} [G: {1}]", Organization.Unit ?? "", Organization.Group);
                        TextWriterColor.Write("Organization Unit Role:  {0} [G: {1}]", Organization.Role ?? "", Organization.Group);
                    }

                    // List telephones
                    foreach (var Telephone in Contact.GetString(CardStringsEnum.Telephones))
                    {
                        TextWriterColor.Write("Phone number:            {0} [G: {1}]", Telephone.Value, Telephone.Group);
                    }

                    // List photos
                    foreach (var Photo in Contact.GetPartsArray<PhotoInfo>())
                    {
                        TextWriterColor.Write("Photo encoding:          {0} [G: {1}]", Photo.Encoding ?? "", Photo.Group);
                        TextWriterColor.Write("Photo value type:        {0} [G: {1}]", string.Join(",", Photo.ElementTypes), Photo.Group);
                        TextWriterColor.Write("ALTID:                   {0} [G: {1}]", Photo.AltId, Photo.Group);
                        TextWriterColor.Write("Photo data [blob: {0}, group: {1}]: \n{2}", true, Photo.IsBlob, Photo.Group, Photo.PhotoEncoded ?? "");
                    }

                    // List roles
                    foreach (var Role in Contact.GetString(CardStringsEnum.Roles))
                    {
                        TextWriterColor.Write("Role:                    {0} [G: {1}]", Role.Value, Role.Group);
                        TextWriterColor.Write("ALTID:                   {0} [G: {1}]", Role.AltId, Role.Group);
                    }

                    // List remaining
                    var birth = Contact.GetPartsArray<BirthDateInfo>();
                    var wedding = Contact.GetPartsArray<AnniversaryInfo>();
                    var gender = Contact.GetPartsArray<GenderInfo>();
                    var url = Contact.GetString(CardStringsEnum.Url);
                    var note = Contact.GetString(CardStringsEnum.Notes);
                    if (birth.Length > 0)
                        TextWriterColor.Write("Contact birthdate:       {0} [G: {1}]", birth[0].BirthDate, birth[0].Group);
                    if (wedding.Length > 0)
                        TextWriterColor.Write("Contact wedding date:    {0} [G: {1}]", wedding[0].Anniversary, wedding[0].Group);
                    if (gender.Length > 0)
                        TextWriterColor.Write("Contact gender           {0} [{1}] [G: {2}]", gender[0].Gender.ToString(), gender[0].GenderDescription ?? "", gender[0].Group);
                    if (url.Length > 0)
                        TextWriterColor.Write("Contact URL:             {0} [G: {1}]", url[0].Value, url[0].Group);
                    if (note.Length > 0)
                        TextWriterColor.Write("Contact Note:            {0} [G: {1}]", note[0].Value, note[0].Group);
                    TextWriterColor.Write("Card kind:               {0} [{1}]", Contact.CardKind, Contact.CardKindStr);

                    // Print VCard
                    string raw = Contact.SaveToString();
                    TextWriterColor.WriteColor(
                        "\nRaw vCard representation\n" +
                          "------------------------\n"
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
