# API Documentation

## Objects

### `Vector2` ![IMPLEMENTED](https://place-hold.it/120x18/33aa33/eeeeee?text=IMPLEMENTED&bold)

```
{
  x: double
  y: double
}
```

### `TrainInstance`![IMPLEMENTED](https://place-hold.it/120x18/33aa33/eeeeee?text=IMPLEMENTED&bold)

```
{
  "elvira-id": string (000000_000000)
  "number": int
  "name": string
  "type": string
  "gps-coord": Vector2
  "delay": int
  "encoded-polyline" : string
  "stations": [<TrainStation>] - in order, optionally hideable
}
```
### `Train` ![IMPLEMENTED](https://place-hold.it/120x18/33aa33/eeeeee?text=IMPLEMENTED&bold)

```
{
  "number": int
  "name": string
  "type": string
  "enc-polyline": string
  "encoded-polyline" : string
  "stations": [<TrainStation>] - in order, optionally hideable
}
```

### `Station` ![IMPLEMENTED](https://place-hold.it/120x18/33aa33/eeeeee?text=IMPLEMENTED&bold)

```
{
  "id": int
  "name": string
  "normalized-name": string
  "gps-coord": Vector2
}
```

### `TrainStation` ![IMPLEMENTED](https://place-hold.it/120x18/33aa33/eeeeee?text=IMPLEMENTED&bold)

In case of "user-data-allowed" = true MAV data based platform will be replaced by the most popular platform candidate between users.

```
{
  "station": Station
  "arrival": time?
  "actual-arrival": time? - when not <Train>
  "departure": time?
  "actual-departure": time? - when not <Train>
  "platform": string
  "relative-distance": double?
}
```

### `Journey`

```
{
  "trains": [<Train:instance:no-stations>] - sorted by departure
  "stops": [<TrainStation extended with train-id>]
}
```

## Endpoints:

#### Default parameters (which all requests include):

```
device-guid: guid - identifies the user (generated at first use of application)
user-data-allowed: bool - whether user data is allowed to be sent and recieved (currently only platform information is supported)
```

#### Default returns:

```
{
  "result": <API_RESULT>
  "status": int - status code
}
```

#### Default error template:
```
{
  "error": JToken (can be an array of errors)
  "status": int - status code
}
```

### /train/ ![IMPLEMENTED](https://place-hold.it/120x18/33aa33/eeeeee?text=IMPLEMENTED&bold)

#### Params

```
ids: string[] - elvira-id
include-stations: bool - whether to include stations list and the encoded polyline
```

#### Returns

Dictionary of train-number as key and `TrainInstance` as value

```
{
  "234142_180506": <Train>,
  ...
}
```

### /train-static/ ![IMPLEMENTED](https://place-hold.it/120x18/33aa33/eeeeee?text=IMPLEMENTED&bold)

#### Params

```
ids: int[] - train-number
include-stations: bool - whether to include stations list and the encoded polyline
```

#### Returns

Dictionary of train-number as key and `Train` as value

```
{
  "2341": <Train>,
  ...
}
```

### /station/ ![IMPLEMENTED](https://place-hold.it/120x18/33aa33/eeeeee?text=IMPLEMENTED&bold)

#### Params

```
id: string[] - id if integer, name (which will be normalized) otherwise
```

#### Returns

Dictionary of id as key and `Station` as value

```
{
  "234": <Station>,
  ...
}
```

### /all-stations/ ![IMPLEMENTED](https://place-hold.it/120x18/33aa33/eeeeee?text=IMPLEMENTED&bold)

#### Params

```
filter : string
order-by: string - id / name / distance(only when filter distance is used)
name-only : bool
```

#### Filters:

```
distance(km, lat, lon)
```

#### Returns

All stations in dictionary of id as key and `Station` as value. Optionally filtered and ordered.

```
{
  "234": <Station> or string - if name-only is set to true,
  ...
}
```

### /station-trains/

#### Params

int is station-id, string is any name (either normalized or not)

```
from: : string or int
to-stations : List<string or int>
min-time: datetime
max-time: datetime
all-stations: bool - if true it also includes an "all" destinations listing
by: "departure"/"arrival" - departure from source/arrival to destination, used for sorting and min/max-time range limiting
```

#### Returns

All stations in dictionary of `<station-key>` to List<`Stations`> that go there from "from" station.

`<station-key>` can be string, int of the specified to-stations or "all" for all-stations

```
{
  "<station-key>": [<Train>] - list sorted by "by" parameter (departure from source/arrival to destination),
  ...
}
```

### /route/

#### Params

int is station-id, string is any name (either normalized or not)

```
from: : string or int
to: string or int
min-time: datetime
max-time: datetime
by: "departure"/"arrival" - departure from source/arrival to destination, used for sorting and min/max-time range limiting
```

#### Returns

All journeys in order 

```
{
  journeys: [<Journey>]
}
```

### /listener-get/

With this you can track which train you are on. If you are already tracking you can update the position the server should see.


#### Params

To track each user the "device-guid" is used.

```
lat: double
lon: double
```

#### Returns

A candidate train if found or null if not found.

```
{
  candidate: <Train>?
}
```

### /listener-unset/

If the tracking does not come up with a single candidate you want to timeout but ideally still show the user a list of all the candidate trains.

#### Params

Only the "device-guid" is used. No additional parameters.

#### Returns

A list of candidate trains in order of relevance (or something like that)

```
{
  candidates: [<Train>]
}
```

### /set-user-train-data/

#### Params

To track each user the "device-guid" is used. If user-data-allowed = false this throws an error.

#### Returns

No additional returned values. The status-code is already enough.