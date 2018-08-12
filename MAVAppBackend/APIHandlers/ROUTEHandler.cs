using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using MAVAppBackend.DataAccess;
using MAVAppBackend.Entities;
using Newtonsoft.Json.Linq;

namespace MAVAppBackend.APIHandlers
{
    public class ROUTEHandler
    {
        private readonly MAVTable table;

        public ROUTEHandler(JObject apiResponse)
        {
            if (apiResponse == null) return;

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(WebUtility.HtmlDecode((string)apiResponse["d"]["result"]));
            table = new MAVTable(html.DocumentNode.Descendants("table").FirstOrDefault(tb => tb.HasClass("uf")));
        }

        public void UpdateDatabase()
        {
            if (table == null) throw new MAVParseException("No station table.");
            
            List<MAVTableRow> rows = table.GetRouteRows().SelectMany(tbl => tbl.GetRows(false)).ToList();
            List<Station> stations = new List<Station>();
            List<Train> trains = new List<Train>();
            
            Database.Instance.TrainInstanceMapper.BeginUpdate();
            Database.Instance.TrainMapper.BeginSelect();
            Database.Instance.StationMapper.ByNormName.BeginSelect();
            foreach (var row in rows)
            {
                string stationName = row.GetCellStationString(0);
                (int trainID, string type, string name, string elviraID) = row.GetCellRouteTrain(row.CellCount == 4 ? 3 : 2);

                Station station = Database.Instance.StationMapper.ByNormName.GetByKey(Database.StationNormalizeName(stationName));
                station.Key = -1;
                station.Name = stationName;
                stations.Add(station);

                if (trainID != -1)
                {
                    Train train = Database.Instance.TrainMapper.GetByKey(trainID);
                    train.Name = name;
                    train.Type = type;
                    trains.Add(train);

                    TrainInstance trainInstance = new TrainInstance
                    {
                        Key = TrainInstance.GetInstanceID(elviraID),
                        TrainID = trainID
                    };
                    Database.Instance.TrainInstanceMapper.Update(trainInstance);
                }
                else
                {
                    trains.Add(trains.LastOrDefault() ?? throw new MAVParseException("Could not resolve train."));
                }
            }
            Database.Instance.StationMapper.ByNormName.EndSelect();
            Database.Instance.TrainMapper.EndSelect();
            Database.Instance.StationMapper.Update(stations.Where(s => !s.Filled).ToList());
            Database.Instance.TrainMapper.Update(trains.Where(t => !t.Filled).ToList());
            Database.Instance.TrainInstanceMapper.EndUpdate();

            Database.Instance.TrainStationMapper.UniqueSelector.BeginSelect();
            List<TrainStation> trainStations = new List<TrainStation>();
            bool departure = true;
            for (int i = 0; i < rows.Count; i++)
            {
                if (trains[i].Polyline == null)
                {
                    TimeSpan time = rows[i].GetCellTime(1) ?? throw new MAVParseException("Time cannot be resolved.");
                    TrainStation trainStation = new TrainStation()
                    {
                        Key = -1,
                        TrainID = trains[i].Key,
                        Ordinal = -1,
                        Platform = rows[i].CellCount == 4 ? rows[i].GetCellString(2) : null,
                        StationID = stations[i].Key
                    };
                    if (departure)
                        trainStation.Departure = time;
                    else
                        trainStation.Arrival = time;

                    Database.Instance.TrainStationMapper.UniqueSelector.FillByKey(trainStation);
                    trainStations.Add(trainStation);
                }

                departure = !departure;
            }
            Database.Instance.TrainStationMapper.UniqueSelector.EndSelect();
            Database.Instance.TrainStationMapper.Update(trainStations.Where(s => !s.Filled).ToList());
        }
    }
}

