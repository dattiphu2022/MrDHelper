using MrDHelper.Models;
using System;

namespace MrDHelper.ValueTypeHelpers.GeoPointHelper
{
    public static class GeoPointHelper
    {
        // Bán kính Trái Đất trung bình (m)
        private const double _earthRadiusMeters = 6_371_008.8;


        /// <summary>
        /// Chuyển đổi khoảng cách (mét) thành chuỗi thân thiện: "xxx m" hoặc "x.x km".
        /// </summary>
        /// <param name="meters">Giá trị khoảng cách theo mét.</param>
        /// <param name="round">Nếu true, làm tròn (mét hoặc km) theo cách dễ đọc.</param>
        /// <param name="integerOnly">Nếu true, chỉ lấy phần nguyên (bỏ phần thập phân).</param>
        public static string ToReadableDistance(this double meters, bool round = true, bool integerOnly = false)
        {
            if (double.IsNaN(meters) || double.IsInfinity(meters))
                return "–";

            if (meters < 0)
                meters = 0;

            if (meters < 1000)
            {
                // Dưới 1 km => hiển thị m
                double value = round ? Math.Round(meters) : meters;
                if (integerOnly)
                    value = Math.Floor(value);
                return $"{value:0} m";
            }
            else
            {
                // Trên 1 km => hiển thị km
                double km = meters / 1000.0;
                double value = round ? Math.Round(km, km >= 10 ? 0 : 1) : km;
                if (integerOnly)
                    value = Math.Floor(value);
                return $"{value:0.#} km";
            }
        }

        /// <summary>
        /// Rút gọn hiển thị cho UI: "300m", "2.1km"
        /// </summary>
        public static string ToCompactDistance(this double meters)
        {
            if (meters < 1000)
                return $"{Math.Round(meters):0}m";
            else
                return $"{Math.Round(meters / 1000.0, 1):0.#}km";
        }

        /// <summary>
        /// Trả về phần nguyên của khoảng cách (bỏ phần thập phân)
        /// </summary>
        public static int ToMetersInt(this double meters)
        {
            return (int)Math.Floor(meters);
        }

        /// <summary>
        /// Khoảng cách "tương đối" (xấp xỉ, nhanh) giữa 2 điểm (mét).
        /// Khuyến nghị dùng cho cự ly ngắn (vài chục km đổ lại) để tối ưu hiệu năng.
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
        /// Khoảng cách chính xác hơn bằng Haversine (mét).
        /// Dùng khi cần độ chính xác cao (cự ly xa, báo cáo, lưu trữ).
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
        /// Góc phương vị (bearing) từ from -> to (độ, 0° = Bắc, tăng chiều kim đồng hồ).
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

