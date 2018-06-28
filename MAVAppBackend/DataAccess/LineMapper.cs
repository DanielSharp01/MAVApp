using MAVAppBackend.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.DataAccess
{
    public class LineMapper : EntityMapper<Line>
    {
        public LineMapper(MySqlConnection connection)
            : base(connection, QueryBuilder.SelectEveryColumn("line_points").OrderByAsc("id").OrderByAsc("number"))
        { }

        protected override Line createEntity(int id)
        {
            return new Line(id);
        }

        protected override bool fillEntity(Line entity, MySqlDataReader reader)
        {
            List<Vector2> lineBuffer = new List<Vector2>();
            lineBuffer.Add(reader.GetVector2OrNull("lat", "lon"));
            bool lastRead;
            while (lastRead = reader.Read())
            {
                if (entity.ID != reader.GetInt32("id")) break;

                lineBuffer.Add(reader.GetVector2OrNull("lat", "lon"));
            }

            entity.Fill(new Polyline(lineBuffer));

            return lastRead;
        }

        public override List<Line> GetAll()
        {
            List<Line> entities = new List<Line>();

            if (getAllCmd == null)
                getAllCmd = baseSelectQuery.Where("id > 0").ToCommand(connection);

            MySqlDataReader reader = getAllCmd.ExecuteReader();
            if (reader.Read())
            {
                Line entity = createEntityInternal(reader.GetInt32(idColumn));
                while (fillEntity(entity, reader))
                {
                    entities.Add(entity);
                    entity = createEntityInternal(reader.GetInt32(idColumn));
                }
                entities.Add(entity);
            }

            reader.Close();

            return entities;
        }
    }
}
