# MAVAppBackend

This project is a server for accessing and processing MÁV's (Hungarian Railway Company) inconsistently (and horrendously) formatted data into something useful.

## Planned API's

- [ ] Trains going from A to B
- [ ] Trains stopping at a specific station
- [ ] Trains stopping near a GPS Coordinate (closest station)
- [ ] GPS Location and delays for moving trains

## Notice

The application uses an `all_stations.txt` which I can't share legally as it's using Google Places API data. You have to place this file next to the executable (for now, it will probably move into an SQL database).

`all_stations.txt` has this format:
```
Station name (can include spaces)|LATITUDE (double), LONGITUDE (double)
```
Examples can be found below.

The extraction code used is shared in the GoogleMapsExtract.cs source file, but you do have to get creative to actually get the full list. For that I share a comparison code with the MAV Station data in MAVAPI.cs. When you only see foreign sounding stations returned, that's when you have won.

### Additional hand-collected stations

The extraction algorithm is going to miss these for sure, this is not Google data it was just eyeballed near it. You are legally fine to use these. You will have close to no problems if you put these at the end of the extracted text file. Some typos will still happen and there are some stations named completely differently.

```
Hort-Csány|47.672220, 19.797506 <- this actually closed but still is in MÁV's list
Eternitgyár|47.755572, 18.529840
Hejce-Vilmány|48.419444, 21.245556
Korlát-Vizsoly|48.364993, 21.219168
Mecsekjánosi|46.216355, 18.242207
Nagycsécs|47.953695, 20.945105
Nyergesújfalu felső|47.762214, 18.540548
Röszke|46.194223, 20.032743
Sajószöged|47.942437, 20.989112
Süttő felső|47.756629, 18.436862
Szeged|46.239722, 20.143064
Várhegyalja|47.737770, 18.366657
Zsujta|48.492507, 21.264975
Rudnaykert|47.7175,19.2577778
```