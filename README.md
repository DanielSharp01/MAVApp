# MÁV API Endpoints

## Generally

Updates directly from MÁV are gonna be limited by the server itself. TRAINS will not be callable from the API and TRAIN will only be called on an object no older than 20 seconds.

Requests can be obtained from (Body as json, Query, etc.).

Response is json only.

## Train object

Depending on how much data is requested of a train (dynamic only/static only/both):

### Static only

```json
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

### Dynamic only

```json
{
    id: 415
    elvira_id: null <- may even be null
    delay: 4
    gps_coord {lat: 47.09868, lon: 18.37985}
    last_gps_coord: {lat: 47.15413667, lon: 18.40711667}
}
```

Both is obviously the combination of the two.

## /dbtrain/[id : int]/

### Request

Get train by MySQL ID. Preferred when ID is known.

```json
update: false/true
data: "dynamic only"/"static only"/"both"
```

### Response

```json
{
    status: 200
    train: { ... TRAIN ... }
}
```

## /dbtrain/

Updates from MÁV are not requestable. Can be filtered down to specific MySQL IDs.

### Request

```json
data: "dynamic only"/"static only"/"both"
filter: [1, 2]
```

### Response

```json
{
    status: 200
    trains: {
        1: { ...TRAIN... },
        2: null
    }
}
```

## /train/[elvira-id]/

Get train by MySQL ID. Preferred when ID is known.

### Request

```json
update: false/true
data: "dynamic only"/"static only"/"both"
```

### Response

```json
{
    status: 200
    train: { ... TRAIN ... }
}
```

## /train/

Updates from MÁV are not requestable. Can be filtered down to specific ElviraIDs.

### Request

```json
data: "dynamic only"/"static only"/"both"
filter: ["544584-180616", "1231231-180616"]
```

### Response

```json
{
    status: 200
    trains: {
        "544584-180616": { ...TRAIN... },
        "1231231-180616": null
    }
}
```

## /close-stations/

Get the close stations in a given a radius to a latitude longitude

```json
lat: 49.54
lon: 40.25
radius: 0.5
```

### Response

```json
{
    status: 200
    stations: { 
        name: "Budapest-nyugati"
        gps_coord: {lat: 49.53, lon: 40.24} 
        distance: 0.12,
    }
}
```

## /station-trains/

Get all trains (Elvira IDs) stopping between `time-from` and `time-to` at `station`.

```json
time-from: 1529170525
time-to: 1529190525
station: "Budapest-nyugati"
```

### Response

```json
{
    status: 200
    trains: {
        1529170625: "541545-180616",
        1529180621: "145474-180616"
    }
}
```

## /route-trains/

Get all train journeys (Elvira IDs per A -> B route) stopping between `time-from` and `time-to` at `from-station` and going to `to-station`.

```json
time-from: 1529170525
time-to: 1529171525
from-station: "Budapest-nyugati"
to-station: "Cegléd"
```

### Response

```json
{
    status: 200
    journeys: {
        [ {from: "Budapest-nyugati", to: "Monor", train: "541545-180616" }, {from: "Monor", to: "Cegléd", train: "458445-180616"} ],
        [ {from: "Budapest-nyugati", to: "Cegléd", train: "447484-180616" } ],
        ...
    }
}
```