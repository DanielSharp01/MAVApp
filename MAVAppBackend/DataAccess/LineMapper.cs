using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.DataAccess
{
    public class LineMapper
    {
        private MySqlConnection connection;

        public LineMapper(MySqlConnection connection)
        {
            this.connection = connection;
        }

        private MySqlCommand getIdCmd = null;
        public Line GetByID(int id)
        {
            if (getIdCmd == null)
            {
                getIdCmd = new MySqlCommand("SELECT lat, lon FROM line_points WHERE id = @id ORDER BY `number` ASC", connection);
                getIdCmd.Prepare();
            }
            getIdCmd.Parameters.Clear();
            getIdCmd.Parameters.AddWithValue("id", id);
            MySqlDataReader reader = getIdCmd.ExecuteReader();
            if (!reader.HasRowsOrClose()) return null;
            List<Vector2> lineBuffer = new List<Vector2>();
            while (reader.Read())
            {
                lineBuffer.Add(reader.GetVector2OrNull("lat", "lon"));
            }

            reader.Close();
            return new Line(id, new Polyline(lineBuffer));
        }

        private MySqlCommand getAllCmd = null;
        public List<Line> GetAll()
        {
            if (getAllCmd == null)
            {
                getAllCmd = new MySqlCommand("SELECT id, lat, lon FROM line_points WHERE id > 0 ORDER BY id ASC, number ASC", connection);
                getAllCmd.Prepare();
            }
            MySqlDataReader reader = getAllCmd.ExecuteReader();
            List<Line> results = new List<Line>();

            List<Vector2> lineBuffer = new List<Vector2>();
            int lastId = -1;
            while (reader.Read())
            {
                int id = reader.GetInt32("id");
                Vector2 gpsCoord = reader.GetVector2OrNull("lat", "lon");
                if (lastId != -1 && id != lastId)
                {
                    results.Add(new Line(lastId, new Polyline(lineBuffer)));
                    lineBuffer.Clear();
                }

                lineBuffer.Add(gpsCoord);

                lastId = id;
            }

            if (lineBuffer.Count > 0)
            {
                results.Add(new Line(lastId, new Polyline(lineBuffer)));
            }

            reader.Close();
            return results;
        }
    }
}
