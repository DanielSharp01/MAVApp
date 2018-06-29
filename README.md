# MÁV APP Backend

This project is an ASP.net core 2.0 based API for the Hungarian railway company (MÁV).

## MySQL table structure and static Station data

Is contained in the `tables.sql` and `data.sql` file in the root of the repository.

## Tasks

- [x] Rewrite projection and GPS math
- [x] Data Access Layer
- [ ] MAVAPIParser - parsing tabular HTML data
- [ ] Line mapping trains to the Station Graph
- [ ] Selective API update (update whatever we can from any MÁV API)
- [ ] RequestScheduler (to avoid requesting too much at the same time)
- [ ] Discovery algorithm (mostly experimentation)