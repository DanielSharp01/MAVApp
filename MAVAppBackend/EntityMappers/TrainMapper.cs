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
            : base(connection, QueryBuilder.SelectEveryColumn("trains"), "trains", new string[] { "id", "name", "from", "to", "type", "expiry_date", "enc_polyline" })
        { }

        protected override Train createEntity(int id)
        {
            return new Train(id);
        }

        protected override bool fillEntity(Train entity, MySqlDataReader reader)
        {
            string name = DataReaderExtensions.GetString(reader, "name");
            string from = DataReaderExtensions.GetString(reader, "from");
            string to = DataReaderExtensions.GetString(reader, "to");
            string type = DataReaderExtensions.GetString(reader, "type");
            DateTime? expiryDate = reader.GetDateTimeOrNull("expiry_date");
            Polyline polyline = reader.GetPolylineOrNull("enc_polyline");

            entity.Fill(name, from, to, type, expiryDate, polyline);

            return reader.Read();
        }

        protected override void updateCachedEntry(Train cachedEntity, Train updatedEntity)
        {
            cachedEntity.Fill(updatedEntity.Name, updatedEntity.From, updatedEntity.To, updatedEntity.Type, updatedEntity.ExpiryDate, updatedEntity.Polyline);
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
            base.fillByKeySingle(entity);
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
                    parameters.AddWithValue($"{column}_{columnIndex}", entity.Key);
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
                case "enc_polyline":    
                    parameters.AddPolylineWithValue($"{column}_{columnIndex}", entity.Polyline);
                    break;
            }
        }
    }
}
