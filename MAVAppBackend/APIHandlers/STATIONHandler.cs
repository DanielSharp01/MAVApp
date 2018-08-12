using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MAVAppBackend.DataAccess;
using MAVAppBackend.Entities;
using MAVAppBackend.EntityMappers;
using Newtonsoft.Json.Linq;

namespace MAVAppBackend.APIHandlers
{
    public class STATIONHandler
    {
        private readonly string stationName;
        private readonly MAVTable table;

        public STATIONHandler(JObject apiResponse)
        {;
            if (apiResponse == null) return;

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(WebUtility.HtmlDecode((string)apiResponse["d"]["result"]));
            table = new MAVTable(html.DocumentNode.Descendants("table").FirstOrDefault(tb => tb.HasClass("af")));
            stationName = apiResponse["d"]["param"]["a"].ToString();
        }

        public void UpdateDatabase()
        {
            if (table == null) throw new MAVParseException("No station table.");

            Station station = Database.Instance.StationMapper.ByNormName.GetByKey(Database.StationNormalizeName(stationName));
            if (!station.Filled)
            {
                station.Key = -1;
                station.Name = stationName;
                Database.Instance.StationMapper.Update(station);
            }

            List<Train> trains = new List<Train>();
            List<TrainInstance> trainInstances = new List<TrainInstance>();
            Database.Instance.TrainMapper.BeginSelect();
            List<MAVTableRow> rows = table.GetRows().ToList();
            foreach (var row in rows)
            {
                (int? trainID, string trainType, string name, string elviraID) = row.GetCellStationTrain(row.CellCount == 4 ? 3 : 2);

                if (trainID.HasValue)
                {
                    Train train = Database.Instance.TrainMapper.GetByKey(trainID.Value);
                    train.Name = name;
                    train.Type = trainType;
                    trains.Add(train);

                    TrainInstance trainInstance = new TrainInstance
                    {
                        Key = TrainInstance.GetInstanceID(elviraID),
                        TrainID = trainID.Value
                    };
                    trainInstances.Add(trainInstance);
                }
                else throw new MAVParseException("Could not resolve train.");
            }

            Database.Instance.TrainMapper.EndSelect();
            Database.Instance.TrainMapper.Update(trains.Where(t => !t.Filled).ToList());
            Database.Instance.TrainInstanceMapper.Update(trainInstances);

            Database.Instance.TrainStationMapper.UniqueSelector.BeginSelect();
            List<TrainStation> trainStations = new List<TrainStation>();
            for (int i = 0; i < rows.Count; i++)
            {
                string platform = rows[i].CellCount == 4 ? rows[i].GetCellString(2) : null;

                if (trains[i].Polyline == null)
                {
                    TrainStation trainStation = new TrainStation
                    {
                        TrainID = trains[i].Key,
                        Ordinal = -1,
                        StationID = station.Key,
                        Arrival = rows[i].GetCellTimes(0).first,
                        Departure = rows[i].GetCellTimes(1).first,
                        Platform = platform
                    };

                    Database.Instance.TrainStationMapper.UniqueSelector.FillByKey(trainStation);
                    trainStations.Add(trainStation);
                }
            }
            Database.Instance.TrainStationMapper.UniqueSelector.EndSelect();

            Database.Instance.TrainStationMapper.Update(trainStations.Where(st => !st.Filled).ToList());

            Database.Instance.TrainInstanceStationMapper.BeginUpdate();
            for (int i = 0; i < rows.Count; i++)
            {
                Database.Instance.TrainInstanceStationMapper.Update(new TrainInstanceStation
                {
                    TrainInstanceID = trainInstances[i].Key,
                    TrainStationID = trainStations[i].Key,
                    ActualArrival = (((TimeSpan? first, TimeSpan? second))rows[i].GetCellObject(0)).second,
                    ActualDeparture = (((TimeSpan? first, TimeSpan? second))rows[i].GetCellObject(1)).second
                });
            }
            Database.Instance.TrainInstanceStationMapper.EndUpdate();
        }
    }
}
