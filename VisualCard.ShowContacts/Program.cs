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

using System.Diagnostics;
using Terminaux.Colors;
using Terminaux.Colors.Data;
using Terminaux.Writer.ConsoleWriters;
using VisualCard.Converters;
using VisualCard.Parsers;
using VisualCard.Parts;

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

                // Get parsers
                List<BaseVcardParser> ContactParsers = 
                    android ? AndroidContactsDb.GetContactsFromDb(args[0]) : 
                    mecard ? MeCard.GetContactsFromMeCardString(meCardString) :
                    CardTools.GetCardParsers(args[0]);
                List<Card> Contacts = [];

                // Parse all contacts
                foreach (BaseVcardParser ContactParser in ContactParsers)
                {
                    Card Contact = ContactParser.Parse();
                    Contacts.Add(Contact);
                    if (save)
                        Contact.SaveTo($"contact_{DateTime.Now:dd-MM-yyyy_HH-mm-ss_ffffff}.vcf");
                }

                // If not printing, exit
                elapsed.Stop();
                if (!print)
                {
                    TextWriterColor.Write("Elapsed time: {0}", elapsed.Elapsed.ToString());
                    return;
                }

                // Show contact information
                bool showVcard5Disclaimer = Contacts.Any((card) => card.CardVersion == "5.0");
                foreach (Card Contact in Contacts)
                {
                    TextWriterColor.WriteColor("----------------------------", ConsoleColors.Green);
                    TextWriterColor.WriteColor("Name:                    {0}", ConsoleColors.Green, Contact.ContactFullName);
                    TextWriterColor.WriteColor("Revision:                {0}", ConsoleColors.Green, Contact.CardRevision);

                    // List names
                    foreach (NameInfo name in Contact.ContactNames)
                    {
                        TextWriterColor.Write("First name:              {0}", name.ContactFirstName);
                        TextWriterColor.Write("Last name:               {0}", name.ContactLastName);
                        TextWriterColor.Write("ALTID:                   {0}", name.AltId);
                        if (name.AltArguments?.Length > 0)
                            TextWriterColor.Write("Reason for ALTID:        {0}", name.AltArguments);
                    }

                    // List titles
                    foreach (TitleInfo title in Contact.ContactTitles)
                    { 
                        TextWriterColor.Write("Title or Job:            {0}", title.ContactTitle);
                        TextWriterColor.Write("ALTID:                   {0}", title.AltId);
                        if (title.AltArguments?.Length > 0)
                            TextWriterColor.Write("Reason for ALTID:        {0}", title.AltArguments);
                    }

                    // List addresses
                    foreach (AddressInfo Address in Contact.ContactAddresses)
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
                    foreach (EmailInfo Email in Contact.ContactMails)
                    {
                        TextWriterColor.Write("Email types:             {0}", Email.ContactEmailTypes);
                        TextWriterColor.Write("Email address:           {0}", Email.ContactEmailAddress);
                    }

                    // List organizations
                    foreach (OrganizationInfo Organization in Contact.ContactOrganizations)
                    {
                        TextWriterColor.Write("Organization Name:       {0}", Organization.Name);
                        TextWriterColor.Write("Organization Unit:       {0}", Organization.Unit);
                        TextWriterColor.Write("Organization Unit Role:  {0}", Organization.Role);
                    }

                    // List telephones
                    foreach (TelephoneInfo Telephone in Contact.ContactTelephones)
                    {
                        TextWriterColor.Write("Phone types:             {0}", Telephone.ContactPhoneTypes);
                        TextWriterColor.Write("Phone number:            {0}", Telephone.ContactPhoneNumber);
                    }

                    // List photos
                    foreach (PhotoInfo Photo in Contact.ContactPhotos)
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
                    foreach (RoleInfo Role in Contact.ContactRoles)
                    {
                        TextWriterColor.Write("Role:                    {0}", Role.ContactRole);
                        TextWriterColor.Write("ALTID:                   {0}", Role.AltId);
                        if (Role.AltArguments?.Length > 0)
                            TextWriterColor.Write("Reason for ALTID:        {0}", Role.AltArguments);
                    }

                    // List remaining
                    TextWriterColor.Write("Contact birthdate:       {0}", Contact.ContactBirthdate);
                    TextWriterColor.Write("Contact mailer:          {0}", Contact.ContactMailer);
                    TextWriterColor.Write("Contact URL:             {0}", Contact.ContactURL);
                    TextWriterColor.Write("Contact Note:            {0}", Contact.ContactNotes);

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
                    TextWriterColor.WriteColor("This application uses vCard 5.0, a revised version of vCard 4.0, made by Aptivi.", ConsoleColors.Gray);
            }
        }
    }
}
