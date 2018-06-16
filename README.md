# MÁV APP Backend

This project is an ASP.net core 2.0 based API for the Hungarian railway company (MÁV).

## MySQL table structure and static Station data

Is contained in the tables.sql file in the root of the repository.

## MÁV API Endpoints

### Generally

Requests can be obtained from (Body as json, Query, etc.).

Response is json only.

### Train object

Depending on how much data is requested of a train (dynamic only/static only/both):

#### Static only

```js
{
    id: 415
    elvira_id: "454845_180616"
    number: "441"
    name: "Sgsg. IC"
    type: "InterCity"
    number-type: null
    delay-reason: "Delayed because ..."
    misc-info: ["Misc #1", "Misc #2"]
    enc_polyline: "long ass string"
}
```

#### Dynamic only

```js
{
    id: 415
    elvira_id: null
    delay: 4
    gps_coord {lat: 47.09868, lon: 18.37985}
    last_gps_coord: {lat: 47.15413667, lon: 18.40711667}
}
```

Both is obviously the combination of the two.

### /dbtrain/[id : int]/

#### Request

Get train by MySQL ID. Preferred when ID is known.

```js
update: false/true
data: "dynamic only"/"static only"/"both"
```

#### Response

```js
{
    status: 200
    train: { ... TRAIN ... }
}
```

### /dbtrain/

Updates from MÁV are not requestable. Can be filtered down to specific MySQL IDs.

#### Request

```js
data: "dynamic only"/"static only"/"both"
filter: [1, 2]
```

#### Response

```js
{
    status: 200
    trains: {
        1: { ...TRAIN... },
        2: null
    }
}
```

### /train/[elvira-id]/

Get train by MySQL ID. Preferred when ID is known.

#### Request

```js
update: false/true
data: "dynamic only"/"static only"/"both"
```

#### Response

```js
{
    status: 200
    train: { ... TRAIN ... }
}
```

### /train/

Updates from MÁV are not requestable. Can be filtered down to specific ElviraIDs.

#### Request

```js
data: "dynamic only"/"static only"/"both"
filter: ["544584-180616", "1231231-180616"]
```

#### Response

```js
{
    status: 200
    trains: {
        "544584-180616": { ...TRAIN... },
        "1231231-180616": null
    }
}
```

### /close-stations/

Get the close stations in a given a radius to a latitude longitude

#### Request

```js
lat: 49.54
lon: 40.25
radius: 0.5
```

#### Response

```js
{
    status: 200
    stations: { 
        name: "Budapest-nyugati"
        gps_coord: {lat: 49.53, lon: 40.24} 
        distance: 0.12,
    }
}
```

### /station-trains/

Get all trains (Elvira IDs) stopping between `time-from` and `time-to` at `station`.

#### Request

```js
time-from: 1529170525
time-to: 1529190525
station: "Budapest-nyugati"
```

#### Response

```js
{
    status: 200
    trains: {
        1529170625: "541545-180616",
        1529180621: "145474-180616"
    }
}
```

### /route-trains/

Get all train journeys (Elvira IDs per A -> B route) stopping between `time-from` and `time-to` at `from-station` and going to `to-station`.

#### Request

```js
time-from: 1529170525
time-to: 1529171525
from-station: "Budapest-nyugati"
to-station: "Cegléd"
```

#### Response

```js
{
    status: 200
    journeys: {
        [ {from: "Budapest-nyugati", to: "Monor", train: "541545-180616" }, {from: "Monor", to: "Cegléd", train: "458445-180616"} ],
        [ {from: "Budapest-nyugati", to: "Cegléd", train: "447484-180616" } ],
        ...
    }
}
```