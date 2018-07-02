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
            : base(connection, QueryBuilder.SelectEveryColumn("trains"), "trains", new string[] { "id", "name", "from", "to", "type", "expiry_date" })
        { }

        protected override Train createEntity(int id)
        {
            return new Train(id);
        }

        protected override bool fillEntity(Train entity, MySqlDataReader reader)
        {
            string name = reader.GetStringOrNull("name");
            string from = reader.GetStringOrNull("from");
            string to = reader.GetStringOrNull("to");
            string type = reader.GetStringOrNull("type");
            DateTime? expiryDate = reader.GetDateTimeOrNull("expiry_date");

            Station fromStation = from == null ? null : Database.Instance.StationMapper.GetByName(from, false);
            if (!fromStation.Filled) fromStation = null;
            Station toStation = from == null ? null : Database.Instance.StationMapper.GetByName(to, false);
            if (!toStation.Filled) toStation = null;

            entity.Fill(name, from, fromStation, to, toStation, type, expiryDate);

            return reader.Read();
        }

        protected override void updateCachedEntry(Train cachedEntity, Train updatedEntity)
        {
            cachedEntity.Fill(updatedEntity.Name, updatedEntity.From, updatedEntity.FromStation, updatedEntity.To, updatedEntity.ToStation, updatedEntity.Type, updatedEntity.ExpiryDate);
        }

        public override List<Train> GetAll()
        {
            Database.Instance.StationMapper.BeginSelect(new AllStrategy<int, Station>());
            List<Train> trains = base.GetAll();
            Database.Instance.StationMapper.EndSelect();
            return trains;
        }

        public override void BeginSelect(BatchSelectStrategy<int, Train> sbatchStrategy)
        {
            base.BeginSelect(sbatchStrategy);
            Database.Instance.StationMapper.BeginSelect(new WhereInStrategy<int, Station>());
        }

        protected override void fillByIDSingle(Train entity)
        {
            Database.Instance.StationMapper.BeginSelect(new WhereInStrategy<int, Station>());
            base.fillByIDSingle(entity);
            Database.Instance.StationMapper.EndSelect();
        }

        public override void EndSelect()
        {
            base.EndSelect();
            Database.Instance.StationMapper.EndSelect();
        }

        protected override void addUpdateColumn(Train entity, string column, int columnIndex, MySqlParameterCollection parameters)
        {
            switch (column)
            {
                case "id":
                    parameters.AddWithValue($"{column}_{columnIndex}", null);
                    break;
                case "name":
                    parameters.AddWithValue($"{column}_{columnIndex}", entity.Name);
                    break;
                case "from":
                    parameters.AddWithValue($"{column}_{columnIndex}", entity.From);
                    break;
                case "to":
                    parameters.AddWithValue($"{column}_{columnIndex}", entity.To);
                    break;
                case "type":
                    parameters.AddWithValue($"{column}_{columnIndex}", entity.Type);
                    break;
                case "expiry_date":
                    parameters.AddWithValue($"{column}_{columnIndex}", entity.ExpiryDate);
                    break;
            }
        }
    }
}
