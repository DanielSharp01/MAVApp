using MAVAppBackend.DataAccess;
using MAVAppBackend.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.EntityMappers
{
    public class StationLineMapper : EntityMapper<StationLine>
    {
        public StationLineMapper(MySqlConnection connection)
            : base(connection, QueryBuilder.SelectEveryColumn("station_line"))
        { }

        protected override StationLine createEntity(int id)
        {
            return new StationLine(id);
        }
        
        protected override bool fillEntity(StationLine entity, MySqlDataReader reader)
        {
            int stationId = reader.GetInt32("station_id");
            int lineId = reader.GetInt32("line_id");
            double distance = reader.GetDouble("distance");
            entity.Fill(Database.Instance.StationMapper.GetByID(stationId, false), Database.Instance.LineMapper.GetByID(lineId, false), distance);

            return reader.Read();
        }
        
        public override List<StationLine> GetAll()
        {
            Database.Instance.StationMapper.BeginSelect(new AllStrategy<int, Station>());
            Database.Instance.LineMapper.BeginSelect(new AllStrategy<int, Line>());
            List<StationLine> stationLines = base.GetAll();
            Database.Instance.StationMapper.EndSelect();
            Database.Instance.LineMapper.EndSelect();

            return stationLines;
        }

        public override void BeginSelect(BatchSelectStrategy<int, StationLine> sbatchStrategy)
        {
            base.BeginSelect(sbatchStrategy);
            Database.Instance.StationMapper.BeginSelect(new WhereInStrategy<int, Station>());
            Database.Instance.LineMapper.BeginSelect(new WhereInStrategy<int, Line>());
        }

        protected override void fillByIDSingle(StationLine entity)
        {
            Database.Instance.StationMapper.BeginSelect(new WhereInStrategy<int, Station>());
            Database.Instance.LineMapper.BeginSelect(new WhereInStrategy<int, Line>());
            base.fillByIDSingle(entity);
            Database.Instance.StationMapper.EndSelect();
            Database.Instance.LineMapper.EndSelect();
        }

        public override void EndSelect()
        {
            base.EndSelect();
            Database.Instance.StationMapper.EndSelect();
            Database.Instance.LineMapper.EndSelect();
        }
    }
}
