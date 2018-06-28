using MAVAppBackend.DataAccess;
using MAVAppBackend.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.EntityMappers
{
    public class TrainMapper : SimpleUpdatableEntityMapper<Train>
    {
        public TrainMapper(MySqlConnection connection)
            : base(connection, QueryBuilder.Select(new string[] { "id", "name" }, "trains"), "trains", new string[] { "id", "name" })
        { }

        protected override Train createEntity(int id)
        {
            return new Train(id);
        }

        protected override bool fillEntity(Train entity, MySqlDataReader reader)
        {
            string name = reader.GetStringOrNull("name");
            entity.Fill(name);

            return reader.Read();
        }

        protected override void updateCachedEntry(Train cachedEntity, Train updatedEntity)
        {
            cachedEntity.Fill(updatedEntity.Name);
        }

        protected override void addUpdateColumn(Train entity, string column, int columnIndex, MySqlParameterCollection parameters)
        {
            switch (column)
            {
                case "id":
                    if (entity.ID == -1) parameters.AddWithValue($"{column}_{columnIndex}", null);
                    else parameters.AddWithValue($"{column}_{columnIndex}", entity.ID);
                    break;
                case "name":
                    parameters.AddWithValue($"{column}_{columnIndex}", entity.Name);
                    break;
            }
        }
    }
}
