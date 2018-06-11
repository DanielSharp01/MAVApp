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
- [ ] Trains from ROUTE API
- [ ] Trains from STATION API
- [x] Train static data extraction
- [x] Train dynamic data extraction
- [ ] Closest station (from GPS coordinates)
- [ ] Deduce train travelled with (from stream of GPS coordinates for a couple seconds)
- [x] MySQL data storage