# Geocoding API
## Overview
A RESTful ASP.NET Core Web API that converts Canadian street addresses into geographic coordinates (latitude and longitude) using the Nominatim Geocoding API.

The API stores geocoding results locally to avoid unnecessary external API calls and demonstrates modern .NET backend development practices such as Dependency Injection, Middleware Pipeline, Entity Framework Core, asynchronous programming, and REST API integration.
Geocoding API accepts list of structured and non-structured address (Canadian address), each address must be enclosed in {} and separated by comma. Controller receives the list of address for further processing, it uses services for further processing of query. 
During processing of the query, following are the steps performed:
1. Checks for basic validation like is the query empty or contains just digits, if so it returns 400, BadRequest.
2. Once query is valid, saves the same in DB for future analyses.
3. Then service separates the address present in query and forms the list of string of addresses and checks for this data in DB, if the same is present 
in DB and it's Geocoding data also present then stores that data for retuning to users.
4. For those addresses whose corresponding geocoding data is not found in DB then calls Nominatim API for geographic coordinates and returns all addresses with corresponding responses to users.


## How to run
Open the project in Visual Studio 2022, build the project and run it. 
As mentioned Swagger UI is used to test Geocoding API.


## Features
- RESTful ASP.NET Core Web API
- Address validation
- Address normalization
- External API integration (Nominatim)
- Entity Framework Core
- SQLite database for Response caching data in file
- Dependency Injection
- Asynchronous programming with async/await
- Error handling middleware
- Duplicate request prevention (Handled using "ConcurrentDictionary" and "Lazy Loading")
- Used Serilog for logging (Path for Log files "<Project folder>\Logs\GeoCodingWebapi-<YYYYMMDD>.txt


## Architecture
Main components and 
1. Middleware
2. Controller
3. Service (Calles Nominatim APIs)
4. DataBase for storing normalize requests and it's responses


## Technologies
- ASP.NET Core 8 Web API
- C#
- Entity Framework Core
- SQLite DataBase
- Dependency Injection
- LINQ
- REST APIs
- Nominatim OpenStreetMap API
- ConcurrentDictionary with Lazy loading to handle duplicate requests


## Configuration
Configured ConnectionString and application details in appsettings.json 


## Database
Tables
1. UserRequest -- For storing incoming query, for future analyses
2. UserRequestEachAddress -- For storing each address in the query. If contains the reference to GeoCodeRequest. There is Many -to- One relation between UserRequestEachAddress -- GeoCodeRequest, that is multiple UserRequestEachAddress can refer to single GeoCodeRequest. As user can send same address in many forms as it is just a string not a proper structure address.
3. GeoCodeRequest -- Which is act as a request to External API, it contains reference to CanadianAddress. Relation between these tables is one -to- one. The other reference it stores is GeoCodeResponse.
4. CanadianAddress -- Stores structure address after normalization of query.
5. GeoCodeResponse -- Stores the geocoding data that returns by Nominatim API.


## API Endpoints
POST/api/GeocodingAPI


## Sample Requests and Sample Responses
A. Non-Structure Requests:
1. 898 carnarvon st, New Westminster BC V3M  0C3
   
Response Body:
{
  "place_id": 375280204,
  "licence": "Data © OpenStreetMap contributors, ODbL 1.0. http://osm.org/copyright",
  "osm_type": "way",
  "osm_id": 1413322056,
  "lat": "49.2018065",
  "lon": "-122.9131764",
  "class": "highway",
  "type": "tertiary",
  "place_rank": 26,
  "importance": 0.05339193484397647,
  "addresstype": "road",
  "name": "Carnarvon Street",
  "display_name": "Carnarvon Street, Downtown, New Westminster, Metro Vancouver Regional District, British Columbia, V3M 0C6, Canada",
  "address": {
    "road": "Carnarvon Street",
    "neighbourhood": "Downtown",
    "city_district": "Downtown",
    "city": "New Westminster",
    "county": "Metro Vancouver Regional District",
    "state": "British Columbia",
    "ISO3166-2-lvl4": "CA-BC",
    "postcode": "V3M 0C6",
    "country": "Canada",
    "country_code": "ca"
  },
  "boundingbox": [
    "49.2017651",
    "49.2018479",
    "-122.9132536",
    "-122.9130993"
  ]
}

3. 898 carnarvon rd, New Westminster BC V3M  0C3
   
Response Body: //With error
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500,
  "detail": "Location not found for the given address."
}

B. Structure Requests:
1. street="898 carnarvon st", city="New Westminster", province="BC", postalcode="V3M  0C3"
   
Response Body:
{
  "place_id": 375280204,
  "licence": "Data © OpenStreetMap contributors, ODbL 1.0. http://osm.org/copyright",
  "osm_type": "way",
  "osm_id": 1413322056,
  "lat": "49.2018065",
  "lon": "-122.9131764",
  "class": "highway",
  "type": "tertiary",
  "place_rank": 26,
  "importance": 0.05339193484397647,
  "addresstype": "road",
  "name": "Carnarvon Street",
  "display_name": "Carnarvon Street, Downtown, New Westminster, Metro Vancouver Regional District, British Columbia, V3M 0C6, Canada",
  "address": {
    "road": "Carnarvon Street",
    "neighbourhood": "Downtown",
    "city_district": "Downtown",
    "city": "New Westminster",
    "county": "Metro Vancouver Regional District",
    "state": "British Columbia",
    "ISO3166-2-lvl4": "CA-BC",
    "postcode": "V3M 0C6",
    "country": "Canada",
    "country_code": "ca"
  },
  "boundingbox": [
    "49.2017651",
    "49.2018479",
    "-122.9132536",
    "-122.9130993"
  ]
}


## Future Improvements
1. Redis Cache
2. Azure SQL
3. Authentication
4. Unit tests
5. SingleFlight pattern for handling duplicate addresses
6. Performance improvement 


