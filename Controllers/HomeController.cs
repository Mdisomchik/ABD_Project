using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ZipsAnalyticsApp.Models;

namespace ZipsAnalyticsApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMongoCollection<BsonDocument> _collection;

        public HomeController(IMongoClient mongoClient, IConfiguration config)
        {
            var dbName = config["MongoDbSettings:DatabaseName"] ?? "ABD_project";
            var database = mongoClient.GetDatabase(dbName);
            _collection = database.GetCollection<BsonDocument>("zips");
        }

        public async Task<IActionResult> Index()
        {
            // Requirement a) States with total population > 10 million
            var pipelineA = new BsonDocument[] {
                new BsonDocument("$group", new BsonDocument { { "_id", "$state_name" }, { "totalPop", new BsonDocument("$sum", "$population") } }),
                new BsonDocument("$match", new BsonDocument("totalPop", new BsonDocument("$gt", 10000000))),
                new BsonDocument("$sort", new BsonDocument("totalPop", -1))
            };
            var resA = await _collection.Aggregate<BsonDocument>(pipelineA).ToListAsync();
            ViewBag.States10M = resA.Select(d => new StatePopulationDto { 
                StateName = d["_id"].IsBsonNull ? "Unknown" : d["_id"].AsString, 
                TotalPopulation = d["totalPop"].ToInt64()
            }).ToList();

            // Requirement b) Average city population by state
            var pipelineB = new BsonDocument[] {
                new BsonDocument("$group", new BsonDocument { { "_id", new BsonDocument { { "state", "$state_name" }, { "city", "$city" } } }, { "cityPop", new BsonDocument("$sum", "$population") } }),
                new BsonDocument("$group", new BsonDocument { { "_id", "$_id.state" }, { "avgCityPop", new BsonDocument("$avg", "$cityPop") } }),
                new BsonDocument("$sort", new BsonDocument("avgCityPop", -1))
            };
            var resB = await _collection.Aggregate<BsonDocument>(pipelineB).ToListAsync();
            ViewBag.AvgCityPop = resB.Select(d => new StateAvgCityPopDto { 
                StateName = d["_id"].IsBsonNull ? "Unknown" : d["_id"].AsString, 
                AvgCityPopulation = d["avgCityPop"].AsDouble 
            }).ToList();

            // Requirement c) Largest and smallest city in each state
            var pipelineC = new BsonDocument[] {
                new BsonDocument("$group", new BsonDocument { { "_id", new BsonDocument { { "state", "$state_name" }, { "city", "$city" } } }, { "cityPop", new BsonDocument("$sum", "$population") } }),
                new BsonDocument("$sort", new BsonDocument("cityPop", 1)),
                new BsonDocument("$group", new BsonDocument {
                    { "_id", "$_id.state" },
                    { "smallest_city", new BsonDocument("$first", "$_id.city") }, { "smallest_pop", new BsonDocument("$first", "$cityPop") },
                    { "largest_city", new BsonDocument("$last", "$_id.city") }, { "largest_pop", new BsonDocument("$last", "$cityPop") }
                }),
                new BsonDocument("$sort", new BsonDocument("_id", 1))
            };
            var resC = await _collection.Aggregate<BsonDocument>(pipelineC).ToListAsync();
            ViewBag.ExtremeCities = resC.Select(d => new ExtremeGeoDto {
                StateName = d["_id"].IsBsonNull ? "Unknown" : d["_id"].AsString, 
                SmallestName = d["smallest_city"].IsBsonNull ? "N/A" : d["smallest_city"].AsString, 
                SmallestPopulation = d["smallest_pop"].ToInt64(),
                LargestName = d["largest_city"].IsBsonNull ? "N/A" : d["largest_city"].AsString, 
                LargestPopulation = d["largest_pop"].ToInt64()
            }).ToList();

            // Requirement d) Largest and smallest counties in each state
            var pipelineD = new BsonDocument[] {
                new BsonDocument("$group", new BsonDocument { { "_id", new BsonDocument { { "state", "$state_name" }, { "county", "$county_name" } } }, { "countyPop", new BsonDocument("$sum", "$population") } }),
                new BsonDocument("$sort", new BsonDocument("countyPop", 1)),
                new BsonDocument("$group", new BsonDocument {
                    { "_id", "$_id.state" },
                    { "smallest_county", new BsonDocument("$first", "$_id.county") }, { "smallest_pop", new BsonDocument("$first", "$countyPop") },
                    { "largest_county", new BsonDocument("$last", "$_id.county") }, { "largest_pop", new BsonDocument("$last", "$countyPop") }
                }),
                new BsonDocument("$sort", new BsonDocument("_id", 1))
            };
            var resD = await _collection.Aggregate<BsonDocument>(pipelineD).ToListAsync();
            ViewBag.ExtremeCounties = resD.Select(d => new ExtremeGeoDto {
                StateName = d["_id"].IsBsonNull ? "Unknown" : d["_id"].AsString, 
                SmallestName = d["smallest_county"].IsBsonNull ? "N/A" : d["smallest_county"].AsString, 
                SmallestPopulation = d["smallest_pop"].ToInt64(),
                LargestName = d["largest_county"].IsBsonNull ? "N/A" : d["largest_county"].AsString, 
                LargestPopulation = d["largest_pop"].ToInt64()
            }).ToList();

            // Requirement e) Nearest 10 zips from Willis Tower (41.878876, -87.635918)
            var pipelineE = new BsonDocument[] {
                new BsonDocument("$geoNear", new BsonDocument {
                    { "near", new BsonDocument { { "type", "Point" }, { "coordinates", new BsonArray { -87.635918, 41.878876 } } } },
                    { "distanceField", "distance_meters" }, { "spherical", true }
                }),
                new BsonDocument("$limit", 10)
            };
            var resE = await _collection.Aggregate<BsonDocument>(pipelineE).ToListAsync();
            ViewBag.NearestZips = resE.Select(d => new GeoProximityDto {
                Zip = d.Contains("zip") ? d["zip"].ToString() : "N/A", 
                City = d.Contains("city") && !d["city"].IsBsonNull ? d["city"].AsString : "N/A", 
                DistanceKm = d["distance_meters"].AsDouble / 1000.0
            }).ToList();

            // Requirement f) Total population situated between 50 and 200 kms around Statue of Liberty
            var pipelineF = new BsonDocument[] {
                new BsonDocument("$geoNear", new BsonDocument {
                    { "near", new BsonDocument { { "type", "Point" }, { "coordinates", new BsonArray { -74.044502, 40.689247 } } } },
                    { "distanceField", "distance_meters" }, { "minDistance", 50000 }, { "maxDistance", 200000 }, { "spherical", true }
                }),
                new BsonDocument("$group", new BsonDocument { { "_id", BsonNull.Value }, { "total_population", new BsonDocument("$sum", "$population") } })
            };
            var resF = await _collection.Aggregate<BsonDocument>(pipelineF).ToListAsync();
            ViewBag.StatueRadiusPop = resF.Count > 0 && resF[0].Contains("total_population") ? resF[0]["total_population"].ToInt64() : 0;

            return View();
        }

        // Headless programmatic endpoint sending data properties straight to Google Slides presentation scripts
        [HttpGet]
        public async Task<IActionResult> GetSlideStats()
        {
            var pipelineA = new BsonDocument[] {
                new BsonDocument("$group", new BsonDocument { { "_id", "$state_name" }, { "totalPop", new BsonDocument("$sum", "$population") } }),
                new BsonDocument("$match", new BsonDocument("totalPop", new BsonDocument("$gt", 10000000))),
                new BsonDocument("$sort", new BsonDocument("totalPop", -1))
            };
            var resA = await _collection.Aggregate<BsonDocument>(pipelineA).ToListAsync();
            var highestStateName = resA.FirstOrDefault() != null ? (resA.First()["_id"].IsBsonNull ? "Unknown" : resA.First()["_id"].AsString) : "N/A";
            var highestStatePopulation = resA.FirstOrDefault() != null ? resA.First()["totalPop"].ToInt64() : 0;

            var pipelineF = new BsonDocument[] {
                new BsonDocument("$geoNear", new BsonDocument {
                    { "near", new BsonDocument { { "type", "Point" }, { "coordinates", new BsonArray { -74.044502, 40.689247 } } } },
                    { "distanceField", "distance_meters" }, { "minDistance", 50000 }, { "maxDistance", 200000 }, { "spherical", true }
                }),
                new BsonDocument("$group", new BsonDocument { { "_id", BsonNull.Value }, { "total_population", new BsonDocument("$sum", "$population") } })
            };
            var resF = await _collection.Aggregate<BsonDocument>(pipelineF).ToListAsync();
            var statueRadiusPop = resF.Count > 0 && resF[0].Contains("total_population") ? resF[0]["total_population"].ToInt64() : 0;

            return Json(new {
                topState = highestStateName,
                topStatePop = highestStatePopulation,
                radiusPop = statueRadiusPop
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}