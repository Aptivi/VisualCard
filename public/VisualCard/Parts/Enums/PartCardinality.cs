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
    /// Part cardinality (specifies how many times an integer, a string, or an array of parts may exist)
    /// </summary>
    internal enum PartCardinality
    {
        /// <summary>
        /// Cardinality: * (One or more instances per vCard MAY be present.) - AltID supported
        /// </summary>
        Any,
        /// <summary>
        /// Cardinality: 1* (One or more instances per vCard MUST be present.) - AltID supported
        /// </summary>
        AtLeastOne,
        /// <summary>
        /// Cardinality: *1 (Exactly one instance per vCard MAY be present.) - AltID supported
        /// </summary>
        MayBeOne,
        /// <summary>
        /// Cardinality: 1 (Exactly one instance per vCard MUST be present.) - AltID supported
        /// </summary>
        ShouldBeOne,
        /// <summary>
        /// Cardinality: * (One or more instances per vCard MAY be present.) - AltID not supported
        /// </summary>
        AnyNoAltId,
        /// <summary>
        /// Cardinality: 1* (One or more instances per vCard MUST be present.) - AltID not supported
        /// </summary>
        AtLeastOneNoAltId,
        /// <summary>
        /// Cardinality: *1 (Exactly one instance per vCard MAY be present.) - AltID not supported
        /// </summary>
        MayBeOneNoAltId,
        /// <summary>
        /// Cardinality: 1 (Exactly one instance per vCard MUST be present.) - AltID not supported
        /// </summary>
        ShouldBeOneNoAltId,
    }
}
