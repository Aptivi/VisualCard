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
        public double Latitude { get; }

        /// <summary>
        /// Longitude of a geographical area
        /// </summary>
        public double Longitude { get; }

        internal static BaseCalendarPartInfo FromStringVcalendarStatic(string value, string[] finalArgs, string[] elementTypes, string valueType, Version calendarVersion) =>
            new GeoInfo().FromStringVcalendarInternal(value, finalArgs, elementTypes, valueType, calendarVersion);

        internal override string ToStringVcalendarInternal(Version calendarVersion) =>
            $"{Latitude}{(calendarVersion.Major == 1 ? ';' : ',')}{Longitude}";

        internal override BaseCalendarPartInfo FromStringVcalendarInternal(string value, string[] finalArgs, string[] elementTypes, string valueType, Version calendarVersion)
        {
            // Get the value
            string[] _geoSplit = value.Split(calendarVersion.Major == 1 ? ';' : ',');
            if (_geoSplit.Length != 2)
                throw new ArgumentException($"When splitting geography, the split value is {_geoSplit.Length} instead of 2.");
            if (!double.TryParse(_geoSplit[0], out double lat))
                throw new ArgumentException($"Invalid latitude {_geoSplit[0]}");
            if (!double.TryParse(_geoSplit[1], out double lon))
                throw new ArgumentException($"Invalid longitude {_geoSplit[1]}");

            // Populate the fields
            GeoInfo _geo = new(finalArgs, elementTypes, valueType, lat, lon);
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

        internal override bool EqualsInternal(BaseCalendarPartInfo source, BaseCalendarPartInfo target) =>
            ((GeoInfo)source) == ((GeoInfo)target);

        internal GeoInfo() { }

        internal GeoInfo(string[] arguments, string[] elementTypes, string valueType, double lat, double lon) :
            base(arguments, elementTypes, valueType)
        {
            Latitude = lat;
            Longitude = lon;
        }
    }
}
