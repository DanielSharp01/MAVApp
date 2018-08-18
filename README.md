# MÁV APP Backend

This project is an ASP.net core 2.0 based API for the Hungarian railway company (MÁV).

MySQL table structure is in `tables.sql`

Static Station data is in `data.sql`

API Documentation is in `APIDocs.md`

## Tasks

- [x] Data Access Layer 
  - [x] Stations
  - [x] Trains
  - [x] TrainInstances
  - [x] TrainStations
  - [x] TrainInstanceStations
  - [x] Trace

- [x] API Handlers 
  - [x] TRAIN
  - [x] STATION
  - [x] TRAINS
  - [x] ROUTE
  - [x] Make the APIHandlers more robust
  
- [ ] API Endpoints 
  - [x] API objects
  - [x] /train/
  - [x] /train-static/
  - [x] /station/
  - [x] /all-stations/
  - [ ] /station-trains/
  - [ ] /route/
  - [ ] /listener-get/, /listener-unset/
  - [ ] /set-user-train-data/