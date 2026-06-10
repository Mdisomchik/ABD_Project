# ZipsAnalyticsApp

ZipsAnalyticsApp is an ASP.NET Core MVC project that connects to a MongoDB dataset of U.S. ZIP code documents and presents a data analytics dashboard.

# Student Comments

With a clear problem statement from Campus Virtual, AI can easily propose the query logic and help verify the results, specifically using Model Context Protocol, while as a student in learing process I was more like "validator" of data. Given structured instructions, such as those used in Virtual Campus, it becomes straightforward to ask AI to generate, refine, and validate the pipeline implementation. 

At the beginning from instructions I installed all necessary components and tried some queries in database directly, when after it was taken decision to work on visuals and make project more complex. Since it was not a requirement right now, but next stup would be developing CRUD part of the application and giving user the possibility to interact with the database.
As a note, MongoDB has its own integrated AI: 
![alt text](image-7.png)

![alt text](image-6.png)

Another note, intergrated AI was able to fulfill requirements from a to d, for e and f further reasearch was needed:
![alt text](image-8.png)

![alt text](image-9.png)

![alt text](image-10.png)

As a result in the end VS Code copilot, MongoDB AI, Calude Code and Gemini AI showed same answer, what can be seen from previous and next screenshots in the Overview part. For the future od ABD it is must to use AI sinse technologies are developing and engineers now will be the ones makign sure output is valid as this project showed us.


## Overview

The application performs a series of analytical queries against a `zips` MongoDB collection and displays results in a dashboard view.

![alt text](image.png)

Key analysis features:

- Requirement A: States with total population greater than 10 million.
![alt text](image-1.png)

- Requirement B: Average city population for each state.
![alt text](image-2.png)

- Requirement C: Smallest and largest city by population in each state.
- Requirement D: Smallest and largest county by population in each state.
![alt text](image-3.png)

- Requirement E: Closest 10 ZIP codes to Willis Tower in Chicago using geospatial proximity.
![alt text](image-4.png)

- Requirement F: Total population within a 50–200 km spherical radius of the Statue of Liberty.
![alt text](image-5.png)


## Technology stack

- .NET 10.0
- ASP.NET Core MVC
- MongoDB.Driver
- MongoDB geospatial aggregation (`$geoNear`)
- Bootstrap 5 for dashboard styling

## Project structure

- `Program.cs` - application startup and dependency injection.
- `Controllers/HomeController.cs` - main controller that executes MongoDB aggregation pipelines and prepares view data.
- `Views/Home/Index.cshtml` - dashboard page that renders analysis results.
- `Models/ErrorViewModel.cs` - model definitions and DTOs used by the application.
- `appsettings.json` - MongoDB connection settings.

## How it works with MongoDB

- `Program.cs` registers `IMongoClient` as a singleton service using the `MongoDbSettings:ConnectionString` value.
- `HomeController` obtains the MongoDB database name from `MongoDbSettings:DatabaseName` and opens the `zips` collection.
- The dashboard is populated by executing aggregation pipelines in `HomeController.Index()`:
  - Requirement A uses `$group` and `$match` to aggregate state populations and filter states above 10 million.
  - Requirement B groups by state and city, computes city population totals, then averages those city populations per state.
  - Requirement C and D sort grouped city/county populations and select the smallest and largest entry per state.
  - Requirement E uses `$geoNear` with Chicago coordinates to find the 10 nearest ZIP code documents by distance.
  - Requirement F uses `$geoNear` with Statue of Liberty coordinates and distance bounds to sum population inside a 50–200 km ring.
- The controller stores results in `ViewBag` objects and passes them to `Views/Home/Index.cshtml` for rendering.

## MongoDB requirements

The application expects the following:
- A MongoDB server reachable by the configured connection string.
- A database named by `MongoDbSettings:DatabaseName` in `appsettings.json`.
- A collection named `zips` containing documents with fields such as:
  - `zip`
  - `city`
  - `state_name`
  - `county_name`
  - `population`
  - `lat`
  - `lng`
  - a valid GeoJSON field for the geospatial queries used by `$geoNear`

Example `appsettings.json` configuration:

```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://admin:secret@localhost:27017/",
    "DatabaseName": "ABD_project"
  }
}
```

## Setup and run

1. Install .NET 10 SDK.
2. Ensure MongoDB is running and accessible.
3. Update `appsettings.json` with the correct MongoDB connection string and database name.
4. From the project root, run the app:

```bash
dotnet run
```

- The default URL is displayed in the console after the app starts.
- If you want to listen on a specific port, use:

```bash
dotnet run --urls "https://localhost:5001"
```

- For a Development environment run:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run
```

5. Open the browser and navigate to the URL printed in the output.

## API endpoint

The app exposes one extra JSON endpoint for programmatic use:

- `GET /Home/GetSlideStats`
  - Returns a JSON object containing:
    - `topState` — highest-population state from Requirement A.
    - `topStatePop` — population of that state.
    - `radiusPop` — total population within the Statue of Liberty radius from Requirement F.

## Notes

- The dashboard uses server-side aggregation, so MongoDB must support the aggregation stages used in the controller.
- Geospatial queries assume location coordinates use longitude/latitude order in GeoJSON.
- If your `zips` collection does not contain a geospatial index, create one for the field used by `$geoNear`.

## Customization

- To change the database name or connection string, edit `appsettings.json` or use environment-specific configuration.
- The visual dashboard can be enhanced by modifying `Views/Home/Index.cshtml` and the CSS definitions there.


