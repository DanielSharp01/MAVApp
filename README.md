# MAVAppBackend

This project is a JSON API for accessing and processing MÁV's (Hungarian Railway Company) inconsistently formatted data into something useful.

## Planned API's

- [ ] Trains going from A to B
- [ ] Trains stopping at a specific station
- [ ] Trains stopping near a GPS Coordinate (closest station)
- [ ] GPS Location and delays for moving trains

## TODO List:

- [x] Station extraction
- [x] Train station deduction when Google data is unavailable
- [ ] Legally correct approach to Google Places API data extraction (dynamic approach instead static storage)
- [ ] Trains from ROUTE API
- [ ] Trains from STATION API
- [x] Train static data extraction
- [ ] Train dynamic data extraction
- [ ] Closest station (from GPS coordinates)
- [ ] Deduce train travelled with (from stream of GPS coordinates for a couple seconds)
- [ ] MySQL data storage


## Notice

The application uses an `all_stations.txt` which I can't share legally as it's using Google Places API data. You have to place this file next to the executable (for now, it will probably move into an SQL database).

`all_stations.txt` has this format:
```
Station name (can include spaces)|LATITUDE (double), LONGITUDE (double)
eg. Szeged|46.239722, 20.143064
```

The extraction code used is shared in the GoogleMapsExtract.cs source file, but you do have to get creative to actually get the full list. For that I share a comparison code with the MAV Station data in MAVAPI.cs. When you only see foreign sounding stations returned, that's when you have won.
