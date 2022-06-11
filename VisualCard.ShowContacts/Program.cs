﻿/*
 * MIT License
 *
 * Copyright (c) 2021-2022 EoflaOE and its companies
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 */

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
                Console.WriteLine("Path to contact file is required.");
            }
            else
            {
                // Get parsers
                List<BaseVcardParser> ContactParsers = CardTools.GetCardParsers(args[0]);
                List<Card> Contacts = new();

                // Parse all contacts
                foreach (BaseVcardParser ContactParser in ContactParsers)
                {
                    Card Contact = ContactParser.Parse();
                    Contacts.Add(Contact);
                }

                // Show contact information
                foreach (Card Contact in Contacts)
                {
                    Console.WriteLine("----------------------------");
                    Console.WriteLine("Name:                    {0}", Contact.ContactFullName);
                    Console.WriteLine("First name:              {0}", Contact.ContactFirstName);
                    Console.WriteLine("Last name:               {0}", Contact.ContactLastName);
                    Console.WriteLine("Title or Job:            {0}", Contact.ContactTitle);
                    Console.WriteLine("Contact URL:             {0}", Contact.ContactURL);
                    Console.WriteLine("Contact Note:            {0}", Contact.ContactNotes);

                    // List addresses
                    foreach (AddressInfo Address in Contact.ContactAddresses)
                    {
                        Console.WriteLine("P.O. Box:                {0}", Address.PostOfficeBox);
                        Console.WriteLine("Extended Address:        {0}", Address.ExtendedAddress);
                        Console.WriteLine("Street Address:          {0}", Address.StreetAddress);
                        Console.WriteLine("Region:                  {0}", Address.Region);
                        Console.WriteLine("Locality:                {0}", Address.Locality);
                        Console.WriteLine("Postal Code:             {0}", Address.PostalCode);
                        Console.WriteLine("Country:                 {0}", Address.Country);
                    }

                    // List organizations
                    foreach (OrganizationInfo Organization in Contact.ContactOrganizations)
                    {
                        Console.WriteLine("Organization Name:       {0}", Organization.Name);
                        Console.WriteLine("Organization Unit:       {0}", Organization.Unit);
                        Console.WriteLine("Organization Unit Role:  {0}", Organization.Role);
                    }

                    foreach (TelephoneInfo Telephone in Contact.ContactTelephones)
                    {
                        Console.WriteLine("Phone types:             {0}", Telephone.ContactPhoneTypes);
                        Console.WriteLine("Phone number:            {0}", Telephone.ContactPhoneNumber);
                    }
                }
            }
        }
    }
}