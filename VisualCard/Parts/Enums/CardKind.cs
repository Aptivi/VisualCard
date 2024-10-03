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
    /// vCard kind
    /// </summary>
    public enum CardKind
    {
        /// <summary>
        /// Card that contains information about a person
        /// </summary>
        Individual,
        /// <summary>
        /// Card that contains information about an organization
        /// </summary>
        Organization,
        /// <summary>
        /// Card that contains information about a group
        /// </summary>
        Group,
        /// <summary>
        /// Card that contains information about a location
        /// </summary>
        Location,
        /// <summary>
        /// Card that is of any other type
        /// </summary>
        Others,
    }
}
