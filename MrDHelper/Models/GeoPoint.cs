using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MrDHelper.Models
{
    [Owned]
    public sealed class GeoPoint
    {
        public GeoPoint()
        {
            
        }
        public GeoPoint(double lat, double lng, double? accMeters = null)
        {
            Latitude = lat;
            Longitude = lng;
            AccuracyMeters = accMeters;
        }

        [Display(Name = "Vĩ độ", Order = 1)]
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }



        [Display(Name = "Kinh độ", Order = 2)]
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [Display(Name = "Độ chính xác", Order = 3)] 
        [JsonPropertyName("accuracy")]
        public double? AccuracyMeters { get; set; }

        
        [Display(Name = "Địa chỉ ước lượng", Order = 4)] 
        public string? Name { get; set; }
    }


}
