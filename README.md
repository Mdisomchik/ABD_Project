# ZipsAnalyticsApp

ZipsAnalyticsApp is an ASP.NET Core MVC project that connects to a MongoDB dataset of U.S. ZIP code documents and presents a data analytics dashboard.

## Overview

The application performs a series of analytical queries against a `zips` MongoDB collection and displays results in a dashboard view.

Key analysis features:
- Requirement A: States with total population greater than 10 million.
- Requirement B: Average city population for each state.
- Requirement C: Smallest and largest city by population in each state.
- Requirement D: Smallest and largest county by population in each state.
- Requirement E: Closest 10 ZIP codes to Willis Tower in Chicago using geospatial proximity.
- Requirement F: Total population within a 50–200 km spherical radius of the Statue of Liberty.

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
4. Run the application from the project root:

```bash
dotnet run
```

5. Open the browser and navigate to `https://localhost:5073` or the URL printed in the console.

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


