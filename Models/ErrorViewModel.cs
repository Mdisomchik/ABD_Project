using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ZipsAnalyticsApp.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    [BsonIgnoreExtraElements]
    public class ZipDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("zip")]
        public string? Zip { get; set; }

        [BsonElement("city")]
        public string? City { get; set; }

        [BsonElement("state_name")]
        public string? StateName { get; set; }

        [BsonElement("county_name")]
        public string? CountyName { get; set; }

        [BsonElement("population")]
        public int Population { get; set; }

        [BsonElement("lat")]
        public double Lat { get; set; }

        [BsonElement("lng")]
        public double Lng { get; set; }
    }

    public class StatePopulationDto
    {
        public string? StateName { get; set; }
        public long TotalPopulation { get; set; }
    }

    public class StateAvgCityPopDto
    {
        public string? StateName { get; set; }
        public double AvgCityPopulation { get; set; }
    }

    public class ExtremeGeoDto
    {
        public string? StateName { get; set; }
        public string? SmallestName { get; set; }
        public long SmallestPopulation { get; set; }
        public string? LargestName { get; set; }
        public long LargestPopulation { get; set; }
    }

    public class GeoProximityDto
    {
        public string? Zip { get; set; }
        public string? City { get; set; }
        public double DistanceKm { get; set; }
    }
}