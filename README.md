# MÁV APP Backend

This project is an ASP.net core 2.0 based API for the Hungarian railway company (MÁV).

## MySQL table structure and static Station data

Is contained in the tables.sql file in the root of the repository.

## About the rewrite

It has become apparent that new conditions in the development of the application warrant a somewhat of a rewrite/code review phase. These are the tasks to do:

- [x] Rewrite SVG Library
- [x] Code review on projections (previously known as Map)
- [x] Get lines from .kmz file into the database
- [x] Line - Station graph as static data
- [ ] Data Access Layer rewrite
- [ ] Database designed around partial information
- [ ] MAVAPIParser - parsing tabular data
- [ ] MAVAPIParser - line mapping
- [ ] Selective API update (update whatever we can from any API)

## Discovery favor own data

In the new version of the API I also want to favor storing instead of querying at the cost of querying more in down time. For this we will need some sort of RequestScheduler.

- [ ] RequestScheduler
- [ ] Discovery algorithm (mostly experimentation)

## **Current Task:** Data Access Layer rewrite:

- [ ] Database.{Table/Object name}Mapping classes to actually do the mapping
- [ ] For more complex mappings DTOs to Domain objects