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

                // If debug, wait for debugger
                if (dbg)
                    Debugger.Launch();

                // If mecard, get a MeCard string
                string meCardString = "";
                if (mecard)
                    meCardString = args[^1];

                // Initialize stopwatch
                Stopwatch elapsed = new();
                elapsed.Start();

                // Parse all contacts
                Card[] contacts =
                    android ? AndroidContactsDb.GetContactsFromDb(args[0]) :
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
                    TextWriterColor.WriteColor("Name:                    {0}", ConsoleColors.Green, Contact.GetString(StringsEnum.FullName));
                    TextWriterColor.WriteColor("Revision:                {0}", ConsoleColors.Green, Contact.GetPart(PartsEnum.Revision));

                    // List names
                    foreach (NameInfo name in Contact.GetPartsArray(PartsArrayEnum.Names))
                    {
                        TextWriterColor.Write("First name:              {0}", name.ContactFirstName);
                        TextWriterColor.Write("Last name:               {0}", name.ContactLastName);
                        TextWriterColor.Write("ALTID:                   {0}", name.AltId);
                        if (name.AltArguments?.Length > 0)
                            TextWriterColor.Write("Reason for ALTID:        {0}", name.AltArguments);
                    }

                    // List titles
                    foreach (TitleInfo title in Contact.GetPartsArray(PartsArrayEnum.Titles))
                    { 
                        TextWriterColor.Write("Title or Job:            {0}", title.ContactTitle);
                        TextWriterColor.Write("ALTID:                   {0}", title.AltId);
                        if (title.AltArguments?.Length > 0)
                            TextWriterColor.Write("Reason for ALTID:        {0}", title.AltArguments);
                    }

                    // List addresses
                    foreach (AddressInfo Address in Contact.GetPartsArray(PartsArrayEnum.Addresses))
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
                    foreach (EmailInfo Email in Contact.GetPartsArray(PartsArrayEnum.Mails))
                    {
                        TextWriterColor.Write("Email types:             {0}", Email.ContactEmailTypes);
                        TextWriterColor.Write("Email address:           {0}", Email.ContactEmailAddress);
                    }

                    // List organizations
                    foreach (OrganizationInfo Organization in Contact.GetPartsArray(PartsArrayEnum.Organizations))
                    {
                        TextWriterColor.Write("Organization Name:       {0}", Organization.Name);
                        TextWriterColor.Write("Organization Unit:       {0}", Organization.Unit);
                        TextWriterColor.Write("Organization Unit Role:  {0}", Organization.Role);
                    }

                    // List telephones
                    foreach (TelephoneInfo Telephone in Contact.GetPartsArray(PartsArrayEnum.Telephones))
                    {
                        TextWriterColor.Write("Phone types:             {0}", Telephone.ContactPhoneTypes);
                        TextWriterColor.Write("Phone number:            {0}", Telephone.ContactPhoneNumber);
                    }

                    // List photos
                    foreach (PhotoInfo Photo in Contact.GetPartsArray(PartsArrayEnum.Photos))
                    {
                        TextWriterColor.Write("Photo encoding:          {0}", Photo.Encoding);
                        TextWriterColor.Write("Photo type:              {0}", Photo.PhotoType);
                        TextWriterColor.Write("Photo value type:        {0}", Photo.ValueType);
                        TextWriterColor.Write("ALTID:                   {0}", Photo.AltId);
                        if (Photo.AltArguments?.Length > 0)
                            TextWriterColor.Write("Reason for ALTID:        {0}", Photo.AltArguments);
                        TextWriterColor.Write("Photo data: \n{0}", Photo.PhotoEncoded);
                    }

                    // List roles
                    foreach (RoleInfo Role in Contact.GetPartsArray(PartsArrayEnum.Roles))
                    {
                        TextWriterColor.Write("Role:                    {0}", Role.ContactRole);
                        TextWriterColor.Write("ALTID:                   {0}", Role.AltId);
                        if (Role.AltArguments?.Length > 0)
                            TextWriterColor.Write("Reason for ALTID:        {0}", Role.AltArguments);
                    }

                    // List remaining
                    var birth = Contact.GetPart(PartsEnum.Birthdate);
                    var wed = Contact.GetPart(PartsEnum.Anniversary);
                    var gnd = Contact.GetPart(PartsEnum.Gender);
                    if (birth is BirthDateInfo bday)
                        TextWriterColor.Write("Contact birthdate:       {0}", bday.BirthDate);
                    if (wed is AnniversaryInfo adate)
                        TextWriterColor.Write("Contact wedding date:    {0}", adate.Anniversary);
                    if (gnd is GenderInfo gender)
                        TextWriterColor.Write("Contact gender           {0} [{1}]", gender.Gender.ToString(), gender.GenderDescription);
                    TextWriterColor.Write("Contact mailer:          {0}", Contact.GetString(StringsEnum.Mailer));
                    TextWriterColor.Write("Contact URL:             {0}", Contact.GetString(StringsEnum.Url));
                    TextWriterColor.Write("Contact Note:            {0}", Contact.GetString(StringsEnum.Notes));

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
