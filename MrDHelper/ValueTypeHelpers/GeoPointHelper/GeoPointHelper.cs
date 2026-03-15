using MrDHelper.Models;
using System;

namespace MrDHelper.ValueTypeHelpers.GeoPointHelper
{
    public static class GeoPointHelper
    {
        // Average Earth radius in meters.
        private const double _earthRadiusMeters = 6_371_008.8;


        /// <summary>
        /// Converts a distance in meters into a human-friendly string such as "xxx m" or "x.x km".
        /// </summary>
        /// <param name="meters">Distance value in meters.</param>
        /// <param name="round">If true, rounds the value in a readable way for meters or kilometers.</param>
        /// <param name="integerOnly">If true, returns only the integer part without decimals.</param>
        public static string ToReadableDistance(this double meters, bool round = true, bool integerOnly = false)
        {
            if (double.IsNaN(meters) || double.IsInfinity(meters))
                return "–";

            if (meters < 0)
                meters = 0;

            if (meters < 1000)
            {
                // Below 1 km, display meters.
                double value = round ? Math.Round(meters) : meters;
                if (integerOnly)
                    value = Math.Floor(value);
                return $"{value:0} m";
            }
            else
            {
                // At or above 1 km, display kilometers.
                double km = meters / 1000.0;
                double value = round ? Math.Round(km, km >= 10 ? 0 : 1) : km;
                if (integerOnly)
                    value = Math.Floor(value);
                return $"{value:0.#} km";
            }
        }

        /// <summary>
        /// Returns a compact UI-friendly distance such as "300m" or "2.1km".
        /// </summary>
        public static string ToCompactDistance(this double meters)
        {
            if (meters < 1000)
                return $"{Math.Round(meters):0}m";
            else
                return $"{Math.Round(meters / 1000.0, 1):0.#}km";
        }

        /// <summary>
        /// Returns the integer portion of the distance, dropping the decimal part.
        /// </summary>
        public static int ToMetersInt(this double meters)
        {
            return (int)Math.Floor(meters);
        }

        /// <summary>
        /// Calculates an approximate, fast distance between two points in meters.
        /// Recommended for short ranges to optimize performance.
        /// </summary>
        public static double ApproxDistanceMetersTo(this GeoPoint from, GeoPoint to)
        {
            if (from == null || to == null) throw new ArgumentNullException();

            // Equirectangular approximation
            double lat1 = ToRad(from.Latitude);
            double lat2 = ToRad(to.Latitude);
            double dLat = lat2 - lat1;
            double dLon = ToRad(to.Longitude - from.Longitude);

            double x = dLon * Math.Cos((lat1 + lat2) * 0.5);
            double y = dLat;
            double d = Math.Sqrt(x * x + y * y) * _earthRadiusMeters;
            return d;
        }

        /// <summary>
        /// Calculates a more accurate Haversine distance in meters.
        /// Use this when higher accuracy is required for longer distances, reporting, or storage.
        /// </summary>
        public static double HaversineDistanceMetersTo(this GeoPoint from, GeoPoint to)
        {
            if (from == null || to == null) throw new ArgumentNullException();

            double lat1 = ToRad(from.Latitude);
            double lat2 = ToRad(to.Latitude);
            double dLat = lat2 - lat1;
            double dLon = ToRad(to.Longitude - from.Longitude);

            double sinDLat = Math.Sin(dLat / 2);
            double sinDLon = Math.Sin(dLon / 2);

            double a = sinDLat * sinDLat
                       + Math.Cos(lat1) * Math.Cos(lat2) * sinDLon * sinDLon;

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return _earthRadiusMeters * c;
        }

        /// <summary>
        /// Calculates the bearing from `from` to `to` in degrees, where 0 is North and values increase clockwise.
        /// </summary>
        public static double BearingDegreesTo(this GeoPoint from, GeoPoint to)
        {
            if (from == null || to == null) throw new ArgumentNullException();

            double lat1 = ToRad(from.Latitude);
            double lat2 = ToRad(to.Latitude);
            double dLon = ToRad(to.Longitude - from.Longitude);

            double y = Math.Sin(dLon) * Math.Cos(lat2);
            double x = Math.Cos(lat1) * Math.Sin(lat2) -
                       Math.Sin(lat1) * Math.Cos(lat2) * Math.Sin(dLon);

            double brng = Math.Atan2(y, x);               // rad
            double deg = (ToDeg(brng) + 360.0) % 360.0;   // 0..360
            return deg;
        }

        private static double ToRad(double deg) => deg * Math.PI / 180.0;
        private static double ToDeg(double rad) => rad * 180.0 / Math.PI;
    }
}

