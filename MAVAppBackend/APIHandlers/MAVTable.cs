using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MAVAppBackend.APIHandlers
{
    public class MAVTableRow
    {
        private readonly HtmlNode[] cells;
        private readonly object[] cellObjects;

        public int CellCount => cells.Length;

        public MAVTableRow(IEnumerable<HtmlNode> cells)
        {
            this.cells = cells.ToArray();
            cellObjects = new object[this.cells.Length];
        }

        public HtmlNode GetCell(int i)
        {
            if (i < 0 || CellCount <= i) throw new IndexOutOfRangeException();

            return cells[i];
        }

        public object GetCellObject(int i)
        {
            if (i < 0 || CellCount <= i) throw new IndexOutOfRangeException();

            return cellObjects[i];
        }

        public string GetCellString(int i)
        {
            if (i < 0 || CellCount <= i) throw new IndexOutOfRangeException();

            cellObjects[i] = cells[i].InnerText;
            return (string)cellObjects[i];
        }

        public (TimeSpan? first, TimeSpan? second) GetCellTimes(int i)
        {
            if (i < 0 || CellCount <= i) throw new IndexOutOfRangeException();

            TimeSpan? first = null;
            TimeSpan? second = null;
            using (IEnumerator<HtmlNode> cellNodes = cells[i].Descendants().GetEnumerator())
            {

                if (cellNodes.MoveNext() && cellNodes.Current.InnerText.Trim() != "")
                {
                    first = Parsing.TimeFromHoursMinutes(cellNodes.Current.InnerText);
                    if (first == null)
                        throw new MAVAPIException("Cell times are not in the correct format.");
                }

                if (cellNodes.MoveNext() && cellNodes.MoveNext() && cellNodes.Current.InnerText.Trim() != "")
                {
                    second = Parsing.TimeFromHoursMinutes(cellNodes.Current.InnerText);
                    if (second == null)
                        throw new MAVAPIException("Cell times are not in the correct format.");
                }
            }

            cellObjects[i] = (first, second);

            return ((TimeSpan?, TimeSpan?))cellObjects[i];
        }

        public TimeSpan? GetCellTime(int i)
        {
            if (i < 0 || CellCount <= i) throw new IndexOutOfRangeException();

            TimeSpan? time = null;
            using (IEnumerator<HtmlNode> cellNodes = cells[i].Descendants().GetEnumerator())
            {
                if (cellNodes.MoveNext() && cellNodes.Current.InnerText.Trim() != "")
                {
                    time = Parsing.TimeFromHoursMinutes(cellNodes.Current.InnerText);
                    if (time == null)
                        throw new MAVAPIException("Cell time is not in the correct format.");
                }
            }

            cellObjects[i] = time;

            return (TimeSpan?)cellObjects[i];
        }

        public string GetCellStationString(int i)
        {
            if (i < 0 || CellCount <= i) throw new IndexOutOfRangeException();

            cellObjects[i] = cells[i].Descendants("a").FirstOrDefault()?.InnerText;
            return (string)cellObjects[i];
        }

        public (int trainID, string type, string name, string elviraID) GetCellStationTrain(int i)
        {
            if (i < 0 || CellCount <= i) throw new IndexOutOfRangeException();

            if (!int.TryParse(cells[i].ChildNodes[0].InnerText.Trim(), out int trainID)) throw new MAVParseException("Train number is not in the correct format.");

            string type = cells[i].ChildNodes[1].InnerText.Trim();
            string name = null;
            if (type.Contains("IC"))
            {
                name = type.Replace("IC", "").Trim();
                type = "IC";
            }
            else if (type.Contains("EC"))
            {
                name = type.Replace("EC", "").Trim();
                type = "EC";
            }
            string elviraID = Parsing.OnClickToJOBject(cells[i].ChildNodes[0].Attributes["onclick"].Value)["v"].ToString();

            cellObjects[i] = (trainID, type, name, elviraID);

            return ((int, string, string, string)) cellObjects[i];
        }

        public (int trainID, string type, string name, string elviraID) GetCellRouteTrain(int i)
        {
            if (i < 0 || CellCount <= i) throw new IndexOutOfRangeException();

            if (GetCellString(i).Trim() == "")
            {
                cellObjects[i] = ((int, string, string, string))(-1, null, null, null);
                return ((int, string, string, string))cellObjects[i];
            }

            using (IEnumerator<HtmlNode> nodes = cells[i].ChildNodes.AsEnumerable().GetEnumerator())
            {
                if (!nodes.MoveNext()) throw new MAVParseException("No train information.");

                if (!int.TryParse(nodes.Current.InnerText.Trim(), out int trainID)) throw new MAVParseException("Train number is not in the correct format.");

                string elviraID = Parsing.OnClickToJOBject(nodes.Current.Attributes["onclick"].Value)["v"].ToString();

                if (!nodes.MoveNext()) throw new MAVParseException("No train type information.");

                string type = nodes.Current.InnerText.Trim();
                string name = null;
                if (type.Contains("IC"))
                {
                    name = type.Replace("IC", "").Trim();
                    type = "IC";
                }
                else if (type.Contains("EC"))
                {
                    name = type.Replace("EC", "").Trim();
                    type = "EC";
                }

                if (nodes.MoveNext() && nodes.Current.HasClass("viszszam2"))
                    name = nodes.Current.InnerText;

                cellObjects[i] = (trainID, type, name, elviraID);
                return ((int, string, string, string))cellObjects[i];
            }

        }
    }

    public class MAVTable
    {
        private readonly HtmlNode table;

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

        public IEnumerable<MAVTableRow> GetRows(bool onMouseOverCheck = true)
        {
            return table.Descendants("tr").Where(tr => !tr.Descendants("th").Any() && (tr.Attributes.Contains("onmouseover") || !onMouseOverCheck)).Select(tr => new MAVTableRow(tr.Descendants("td")));
        }

        public IEnumerable<MAVTable> GetRouteRows()
        {
            return table.Descendants("tr").Select(tr => tr.Descendants("table")).Where(t => t.Any()).Select(t => new MAVTable(t.First()));
        }
    }
}
