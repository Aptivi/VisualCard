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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using VisualCard.Parsers;

namespace VisualCard.Parts.Implementations
{
    /// <summary>
    /// Contact note info
    /// </summary>
    [DebuggerDisplay("Note = {Note}")]
    public class NoteInfo : BaseCardPartInfo, IEquatable<NoteInfo>
    {
        /// <summary>
        /// The contact's note
        /// </summary>
        public string Note { get; }

        internal static BaseCardPartInfo FromStringVcardStatic(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion) =>
            new NoteInfo().FromStringVcardInternal(value, finalArgs, altId, elementTypes, valueType, cardVersion);

        internal override string ToStringVcardInternal(Version cardVersion) =>
            Note;

        internal override BaseCardPartInfo FromStringVcardInternal(string value, string[] finalArgs, int altId, string[] elementTypes, string valueType, Version cardVersion)
        {
            // Get the value
            string _noteStr = Regex.Unescape(value);

            // Populate the fields
            NoteInfo _note = new(altId, finalArgs, elementTypes, valueType, _noteStr);
            return _note;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((NoteInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="NoteInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(NoteInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="NoteInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="NoteInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(NoteInfo source, NoteInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.Note == target.Note
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 963810291;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Note);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(NoteInfo left, NoteInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(NoteInfo left, NoteInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BaseCardPartInfo source, BaseCardPartInfo target) =>
            ((NoteInfo)source) == ((NoteInfo)target);

        internal NoteInfo() { }

        internal NoteInfo(int altId, string[] arguments, string[] elementTypes, string valueType, string note) :
            base(arguments, altId, elementTypes, valueType)
        {
            Note = note;
        }
    }
}
