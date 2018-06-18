# MÁV APP Backend

This project is an ASP.net core 2.0 based API for the Hungarian railway company (MÁV).

## MySQL table structure and static Station data

Is contained in the tables.sql file in the root of the repository.

## MÁV API Endpoints

### Generally

Requests can be obtained from (Body as json, Query, etc.).

Response is json only in the following format:

```js
{
    status: 200
    result: { ... }
}
```

or in the case of errors:

```js
{
    status: 400
    error: "Parameter 'data' must be either 'static only', 'dynamic only' or 'both'."
}
```

### Train object

Depending on how much data is requested of a train (dynamic only/static only/both):

#### Static only

Stations appearing in the correct order.

```js
{
    id: 415
    elvira-id: "454845_180616"
    number: "441"
    name: "Sgsg. IC"
    type: "InterCity"
    number-type: null
    delay-reason: "Delayed because ..."
    misc-info: ["Misc #1", "Misc #2"]
    enc-polyline: "long ass string"
    stations: [
        {
            name: "Szolnok",
            int-distance: 100,
            distance: 99.99312361641964,
            position-accuracy: "precise"/"integer-precision"/"missing",
            arrival: "2018-06-17 08:15:00",
            departure: "2018-06-17 08:16:00",
            actual-arrival: "2018-06-17 08:44:00",
            actual-departure: "2018-06-17 08:45:00",
            arrived: true/false,
            platform: null/"or some string"
        },
        ...
    ]
}
```

#### Dynamic only

Where `gpscoord` being null means the train is not going, and if `last-gpscoord` is null aswell, then we never "saw" it going.

```js
{
    id: 415
    elvira-id: null
    delay: 4
    gpscoord {lat: 47.09868, lon: 18.37985}
    last-gpscoord: {lat: 47.15413667, lon: 18.40711667}
}
```

Both is obviously the combination of the two.

### /train/[id]/

#### Request

Gets a train by MySQL ID if `id` is integer or else by ElviraID

```js
update: false/true
data: "dynamic only"/"static only"/"both"
```

#### Response result

```js
{ ... TRAIN ... }
```

### /trains/

Gets all trains from the Database without updating any of them. The results can be filtered like such:

#### Request all trains

```js
data: "dynamic only"/"static only"/"both"
```

#### Response result

An endless stream of objects in particular order

```js
[
    { ...TRAIN... },
]
```

#### Request all trains with MySQL or Elvira IDs

Integer means MySQL ID else means ElviraID. You can mix and match them, but if you request the same train twice it's gonna appear twice.

```js
data: "dynamic only"/"static only"/"both"
ids: []
```

#### Response result

The response is an associative array with MySQL or ElviraID keys.

```js
{
    "1545": { ...TRAIN... },
    ...
}
```

#### Request all trains moving (or not)

In other words their GPSCoordinate not being null (or being null).

```js
data: "dynamic only"/"static only"/"both"
moving: true/false
```

#### Response result

An endless stream of objects in no particular order.

```js
[
    { ...TRAIN... },
]
```

#### Request all trains in a given distance from location (latitude, longitude)

```js
data: "dynamic only"/"static only"/"both"
lat: 49.54
lon: 40.25
radius: 0.5
```

#### Response result

The response is an ordered array by distance. The distance property will get appended to the train object.

```js
[
    { ...TRAIN..., distance: 0.12 },
    ...
]
```

### /close-stations/

Get the close stations in a given a distance to a location (latitude, longitude).

#### Request

```js
lat: 49.54
lon: 40.25
radius: 0.5
```

#### Response result

The response is an ordered array of stations by distance.

```js
[
    { 
        name: "Budapest-nyugati"
        gpscoord: {lat: 49.53, lon: 40.24} 
        distance: 0.12,
    },
    ...
]
```

### /station-trains/

Get all trains (Elvira IDs) stopping between `time-from` and `time-to` at `station`.

#### Request

The API does not support times more than one day apart.

```js
from-time: 2018-06-17 17:00:00
to-time: 2018-06-17 19:00:00
station: "Budapest-nyugati"
```

#### Response result

The response is an associative array with DateTime of arrival as index.

```js
{
    "2018-06-17 17:56:00": "541545-180616",
    "2018-06-17 18:56:00": "145474-180616"
}
```

### /route-trains/

Get all train journeys (Elvira IDs per A -> B route) stopping between `time-from` and `time-to` at `from-station` and going to `to-station`.

#### Request

The API does not support times more than one day apart.

```js
from-time: 2018-06-17 17:00:00
to-time: 2018-06-17 19:00:00
from-station: "Budapest-nyugati"
to-station: "Cegléd"
```

#### Response result

The response is an array of train journeys in no particular. A train journey is represented as an ordered array of the trains to take to complete it.

```js
[
    [ {from: "Budapest-nyugati", to: "Monor", train: "541545-180616" }, {from: "Monor", to: "Cegléd", train: "458445-180616"} ],
    [ {from: "Budapest-nyugati", to: "Cegléd", train: "447484-180616" } ],
    ...
]
```