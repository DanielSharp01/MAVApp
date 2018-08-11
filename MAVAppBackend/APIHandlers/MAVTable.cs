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
                IEnumerator<HtmlNode> cellNodes = cells[i].Descendants().GetEnumerator();

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

                cellObjects[i] = (first, second);
            }

            return ((TimeSpan?, TimeSpan?))cellObjects[i];
        }

        public (int? trainID, string type, string elviraID) GetCellTrain(int i)
        {
            if (CellCount <= i) cellObjects[i] = ((int?, string, string))(null, null, null);
            else
            {
                int? trainID = int.Parse(cells[i].ChildNodes[0].InnerText);
                string type = cells[i].ChildNodes[1].InnerText.Trim();
                string elviraID = Parsing.OnClickToJOBject(cells[i].ChildNodes[0].Attributes["onclick"].Value)["v"].ToString();

                cellObjects[i] = (trainID, type, elviraID);
            }

            return ((int?, string, string)) cellObjects[i];
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

        public IEnumerable<MAVTableRow> GetRows()
        {
            return table.Descendants("tr").Where(tr => !tr.Descendants("th").Any() && tr.Attributes.Contains("onmouseover")).Select(tr => new MAVTableRow(tr.Descendants("td")));
        }

        public IEnumerable<MAVTable> GetRouteRows()
        {
            return table.Descendants("tr").Where(tr => !tr.Descendants("th").Any()).Select(tr => tr.Descendants("table")).Where(t => t.Any()).Select(t => new MAVTable(t.First()));
        }
    }
}
