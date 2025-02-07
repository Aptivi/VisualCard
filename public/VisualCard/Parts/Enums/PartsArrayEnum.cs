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

namespace VisualCard.Parts.Enums
{
    /// <summary>
    /// Enumeration for available parts that are not strings but are arrays
    /// </summary>
    public enum PartsArrayEnum
    {
        /// <summary>
        /// The contact's names
        /// </summary>
        Names,
        /// <summary>
        /// The contact's addresses
        /// </summary>
        Addresses,
        /// <summary>
        /// The contact's agents
        /// </summary>
        Agents,
        /// <summary>
        /// The contact's organizations
        /// </summary>
        Organizations,
        /// <summary>
        /// The contact's photos
        /// </summary>
        Photos,
        /// <summary>
        /// The contact's categories
        /// </summary>
        Categories,
        /// <summary>
        /// The contact's logos
        /// </summary>
        Logos,
        /// <summary>
        /// The contact's sounds
        /// </summary>
        Sounds,
        /// <summary>
        /// The contact's XML code
        /// </summary>
        Xml,
        /// <summary>
        /// The contact's key URL or embedded PGP key
        /// </summary>
        Key,
        /// <summary>
        /// The card revision
        /// </summary>
        Revision,
        /// <summary>
        /// The contact's birthdate
        /// </summary>
        Birthdate,
        /// <summary>
        /// The contact's wedding anniversary date (that is, the day that this contact is married)
        /// </summary>
        Anniversary,
        /// <summary>
        /// The contact's gender
        /// </summary>
        Gender,
        /// <summary>
        /// Client PID map
        /// </summary>
        ClientPidMap,
        /// <summary>
        /// The contact's extended IANA options (usually starts with SOMETHING:Value1;Value2...)
        /// </summary>
        IanaNames = int.MaxValue - 2,
        /// <summary>
        /// The contact's extended options (usually starts with X-SOMETHING:Value1;Value2...)
        /// </summary>
        NonstandardNames = int.MaxValue - 1,
    }
}
