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

namespace VisualCard.Parts.Enums
{
    /// <summary>
    /// Enumeration for available parts that are strings
    /// </summary>
    public enum StringsEnum
    {
        /// <summary>
        /// The VCard kind (individual is the default)
        /// </summary>
        Kind,
        /// <summary>
        /// The contact's mailing software
        /// </summary>
        Mailer,
        /// <summary>
        /// The contact's product ID
        /// </summary>
        ProductId,
        /// <summary>
        /// The contact's sort string
        /// </summary>
        SortString,
        /// <summary>
        /// The contact's access classification
        /// </summary>
        AccessClassification,
        /// <summary>
        /// The contact's unique ID
        /// </summary>
        Uid,
        /// <summary>
        /// Name of the vCard corresponding to the source
        /// </summary>
        SourceName,
        /// <summary>
        /// Profile. Either empty or, if specified, vCard
        /// </summary>
        Profile,
    }
}
