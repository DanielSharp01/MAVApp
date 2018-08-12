using HtmlAgilityPack;
using MAVAppBackend.DataAccess;
using MAVAppBackend.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            cellObjects = this.cells.Select(o => (object)o).ToArray();
        }

        public HtmlNode GetCell(int i)
        {
            return cells[i];
        }

        public object GetCellObject(int i)
        {
            return cellObjects[i];
        }

        public string GetCellString(int i)
        {
            if (CellCount <= i) cellObjects[i] = null;
            else cellObjects[i] = cells[i].InnerText;

            return (string)cellObjects[i];
        }

        public (TimeSpan? first, TimeSpan? second) GetCellTimes(int i)
        {
            if (CellCount <= i) cellObjects[i] = ((TimeSpan?, TimeSpan?))(null, null);
            else
            {
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
            }

            return ((TimeSpan?, TimeSpan?))cellObjects[i];
        }

        public TimeSpan? GetCellTime(int i)
        {
            if (CellCount <= i) cellObjects[i] = null;
            else
            {
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
            }

            return (TimeSpan?)cellObjects[i];
        }

        public string GetCellStationString(int i)
        {
            cellObjects[i] = null;
            if (CellCount > i)
                cellObjects[i] = cells[i].Descendants("a").FirstOrDefault()?.InnerText;

            return (string)cellObjects[i];
        }

        public (int? trainID, string type, string name, string elviraID) GetCellStationTrain(int i)
        {
            if (CellCount <= i) cellObjects[i] = ((int?, string, string, string))(null, null, null, null);
            else
            {
                int? trainID = int.Parse(cells[i].ChildNodes[0].InnerText.Trim());
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
            }

            return ((int?, string, string, string)) cellObjects[i];
        }

        public (int? trainID, string type, string name, string elviraID) GetCellRouteTrain(int i)
        {
            if (CellCount <= i || GetCellString(i).Trim() == "") cellObjects[i] = ((int?, string, string, string))(null, null, null, null);
            else
            {
                using (IEnumerator<HtmlNode> nodes = cells[i].ChildNodes.AsEnumerable().GetEnumerator())
                {
                    if (!nodes.MoveNext()) throw new MAVParseException("No train information.");

                    int? trainID = int.Parse(nodes.Current.InnerText.Trim());
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

                    cellObjects[i] = ((int?, string, string, string))(trainID, type, name, elviraID);
                }
            }

            return ((int?, string, string, string))cellObjects[i];
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
