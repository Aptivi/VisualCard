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
using Textify.General;
using VisualCard.Calendar.Languages;
using VisualCard.Common.Parsers.Arguments;
using VisualCard.Common.Parts;

namespace VisualCard.Calendar.Parts.Implementations
{
    /// <summary>
    /// Event or to-do geographical information
    /// </summary>
    [DebuggerDisplay("Geography = {Latitude}, {Longitude}")]
    public class GeoInfo : BaseCalendarPartInfo, IEquatable<GeoInfo>
    {
        /// <summary>
        /// Latitude of a geographical area
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude of a geographical area
        /// </summary>
        public double Longitude { get; set; }

        internal static BaseCalendarPartInfo FromStringStatic(string value, PropertyInfo property, int altId, string[] elementTypes, Version calendarVersion) =>
            (BaseCalendarPartInfo)new GeoInfo().FromStringInternal(value, property, altId, elementTypes, calendarVersion);

        internal override string ToStringInternal(Version calendarVersion) =>
            $"{Latitude}{(calendarVersion.Major == 1 ? ',' : ';')}{Longitude}";

        internal override BasePartInfo FromStringInternal(string value, PropertyInfo property, int altId, string[] elementTypes, Version calendarVersion)
        {
            // Get the value
            string[] _geoSplit = value.Split(calendarVersion.Major == 1 ? ',' : ';');
            if (_geoSplit.Length != 2)
                throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_GEO_ARGMISMATCH").FormatString(_geoSplit.Length));
            if (!double.TryParse(_geoSplit[0], out double lat))
                throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_GEO_INVALIDLAT").FormatString(_geoSplit[0]));
            if (!double.TryParse(_geoSplit[1], out double lon))
                throw new ArgumentException(LanguageTools.GetLocalized("VISUALCARD_CALENDAR_PARTS_EXCEPTION_GEO_INVALIDLONG").FormatString(_geoSplit[1]));

            // Populate the fields
            GeoInfo _geo = new(property, elementTypes, lat, lon);
            return _geo;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            Equals((GeoInfo)obj);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="other">The target <see cref="GeoInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(GeoInfo other) =>
            Equals(this, other);

        /// <summary>
        /// Checks to see if both the parts are equal
        /// </summary>
        /// <param name="source">The source <see cref="GeoInfo"/> instance to check to see if they equal</param>
        /// <param name="target">The target <see cref="GeoInfo"/> instance to check to see if they equal</param>
        /// <returns>True if all the part elements are equal. Otherwise, false.</returns>
        public bool Equals(GeoInfo source, GeoInfo target)
        {
            // We can't perform this operation on null.
            if (source is null || target is null)
                return false;

            // Check all the properties
            return
                source.Latitude == target.Latitude &&
                source.Longitude == target.Longitude
            ;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -985272543;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Latitude.GetHashCode();
            hashCode = hashCode * -1521134295 + Longitude.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(GeoInfo left, GeoInfo right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(GeoInfo left, GeoInfo right) =>
            !(left == right);

        internal override bool EqualsInternal(BasePartInfo source, BasePartInfo target) =>
            ((GeoInfo)source) == ((GeoInfo)target);

        internal GeoInfo() { }

        internal GeoInfo(PropertyInfo? property, string[] elementTypes, double lat, double lon) :
            base(property, elementTypes)
        {
            Latitude = lat;
            Longitude = lon;
        }
    }
}
