using HtmlAgilityPack;
using MAVAppBackend.DataAccess;
using MAVAppBackend.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.APIHandlers
{
    public class MAVTableRow
    {
        private List<HtmlNode> cells = new List<HtmlNode>();

        public int CellCount { get => cells.Count; }

        public MAVTableRow(IEnumerable<HtmlNode> cells)
        {
            this.cells.AddRange(cells);
        }

        public HtmlNode GetCell(int i)
        {
            return cells[i];
        }

        public string GetCellString(int i)
        {
            if (CellCount <= i) return null;
            return cells[i].InnerText;
        }

        public (Train train, TimeSpan? relFromTime, TimeSpan? relToTime) GetSTATIONSCellTrain(int i, Station station)
        {
            if (CellCount <= i) return (null, null, null);

            TimeSpan? relFromTime = null, relToTime = null;
            IEnumerator<HtmlNode> cellNodes = cells[i].Descendants().GetEnumerator();
            if (!cellNodes.MoveNext()) throw new MAVParseException("No train link.");

            HtmlNode link = cellNodes.Current;

            Train train = null;
            if (int.TryParse(link.InnerText, out int trainNumber))
            {
                train = Database.Instance.TrainMapper.GetByID(trainNumber);
            }
            else throw new MAVParseException("Train link does not contain a number.");

            if (!cellNodes.MoveNext()) throw new MAVParseException("No train type.");
            
            string[] nameType = cellNodes.Current.InnerText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            string name = nameType.Length == 2 ? nameType[0] : null;
            string type = nameType.Length == 2 ? nameType[1] : nameType[0];

            if (!cellNodes.MoveNext() || !cellNodes.MoveNext()) throw new MAVParseException("No train relation.");

            string[] relationSpl = cellNodes.Current.InnerText.Split("--");
            if (relationSpl.Length != 2) throw new MAVParseException("Train relation is not in the correct format.");
            relationSpl[0] = relationSpl[0].Trim();
            relationSpl[1] = relationSpl[1].Trim();

            string from = null;
            string to = null;

            if (relationSpl[0] != "")
            {
                string[] spl = relationSpl[0].Split(' ');
                if (spl.Length != 2) throw new MAVParseException("Train relation is not in the correct format.");
                relFromTime = timeFromHM(spl[0]);
                from = spl[1];
            }

            if (relationSpl[1] != "")
            {
                string[] spl = relationSpl[1].Split(' ');
                if (spl.Length != 2) throw new MAVParseException("Train relation is not in the correct format.");
                relToTime = timeFromHM(spl[0]);
                to = spl[1];
            }

            if (from == null && to == null) throw new MAVParseException("Train relation is not in the correct format.");

            from = from ?? station.Name;
            to = to ?? station.Name;

            train.APIFill(name, from, to, type);

            return (train, relFromTime, relToTime);
        }

        public Train GetROUTECellTrain(int i, Station station)
        {
            if (CellCount <= i) return null;

            IEnumerator<HtmlNode> cellNodes = cells[i].Descendants().GetEnumerator();
            if (!cellNodes.MoveNext()) throw new MAVParseException("No train link.");

            HtmlNode link = cellNodes.Current;

            Train train = null;
            if (int.TryParse(link.InnerText, out int trainNumber))
            {
                train = Database.Instance.TrainMapper.GetByID(trainNumber);
            }
            else throw new MAVParseException("Train link does not contain a number.");

            if (!cellNodes.MoveNext()) throw new MAVParseException("No train type.");

            string[] nameType = cellNodes.Current.InnerText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            string name = nameType.Length == 2 ? nameType[0] : null;
            string type = nameType.Length == 2 ? nameType[1] : nameType[0];

            if (!cellNodes.MoveNext() && cellNodes.Current.Name == "span")
            {
                name = cellNodes.Current.Name;
            }

            if ((cellNodes.Current.Name == "span" || !cellNodes.MoveNext()) || !cellNodes.MoveNext()) throw new MAVParseException("No train relation.");

            string[] relationSpl = cellNodes.Current.InnerText.Split(" - ");
            if (relationSpl.Length != 2) throw new MAVParseException("Train relation is not in the correct format.");
            relationSpl[0] = relationSpl[0].Trim();

            string from = null, to = null;
            if (relationSpl[0] != "")
                from = relationSpl[0];

            if (relationSpl[1] != "")
                to = relationSpl[1];

            if (from == null && to == null) throw new MAVParseException("Train relation is not in the correct format.");

            from = from ?? station.Name;
            to = to ?? station.Name;

            train.APIFill(name, from, to, type);

            return train;
        }

        public (TimeSpan? first, TimeSpan? second) GetCellTimes(int i)
        {
            if (CellCount <= i) return (null, null);

            TimeSpan? first = null;
            TimeSpan? second = null;
            IEnumerator<HtmlNode> cellNodes = cells[i].Descendants().GetEnumerator();

            if (cellNodes.MoveNext() && cellNodes.Current.InnerText.Trim() != "")
                first = timeFromHM(cellNodes.Current.InnerText);

            if (cellNodes.MoveNext() && cellNodes.MoveNext() && cellNodes.Current.InnerText.Trim() != "")
                second = timeFromHM(cellNodes.Current.InnerText);

            return (first, second);
        }

        private TimeSpan timeFromHM(string hmString)
        {
            string[] spl = hmString.Split(':');
            if (spl.Length == 2 && int.TryParse(spl[0], out int hours) && int.TryParse(spl[1], out int minutes))
            {
                return new TimeSpan(hours, minutes, 0);
            }
            else throw new MAVAPIException("Cell times are not in the correct format.");
        }
    }

    public class MAVTable
    {
        private HtmlNode table;

        public MAVTable(HtmlNode table)
        {
            this.table = table;
        }

        public HtmlNode GetTopHeader()
        {
            HtmlNode tr = table.Descendants("tr").FirstOrDefault();
            if (tr == null) return null;

            IEnumerable<HtmlNode> ths = tr.Descendants("th");
            if (ths.Count() == 1) return ths.First();
            else return null;
        }

        public IEnumerable<MAVTableRow> GetRows()
        {
            foreach (HtmlNode tr in table.Descendants("tr"))
            {
                if (tr.Descendants("th").Any()) continue;

                yield return new MAVTableRow(tr.Descendants("td"));
            }

            yield break;
        }

        public IEnumerable<MAVTable> GetRouteRows()
        {
            foreach (HtmlNode tr in table.Descendants("tr"))
            {
                if (tr.Descendants("th").Any()) continue;

                IEnumerable<HtmlNode> table = tr.Descendants("table");
                if (table.Any())
                    yield return new MAVTable(table.First());
            }

            yield break;
        }
    }
}
