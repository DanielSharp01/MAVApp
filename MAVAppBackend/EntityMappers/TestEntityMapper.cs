using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MAVAppBackend.DataAccess;
using MAVAppBackend.Model;
using SharpEntities;

namespace MAVAppBackend.EntityMappers
{
    public class TestEntityMapper : UpdatableEntityMapper<int, TestEntity>
    {
        private readonly SelectQuery baseQuery;
        public TestEntityMapper(DbConnection connection)
            : base(connection, new Dictionary<int, TestEntity>())
        {
            baseQuery = SqlQuery.Select().AllColumns().From("test_table");
        }

        protected override TestEntity CreateEntity(int key)
        {
            return new TestEntity(key);
        }

        protected override DbDataReader SelectByKey(int key)
        {
            DbCommand command = baseQuery.Clone().Where("`id` = @id").ToPreparedCommand(connection);
            command.Parameters.Clear();
            DbParameters.AddParameter(command.Parameters, "@id", key);
            return command.ExecuteReader();
        }

        protected override DbDataReader SelectByKeys(IEnumerable<int> keys)
        {
            DbCommand command = baseQuery.Clone().WhereIn("id", keys.Count()).ToPreparedCommand(connection);
            command.Parameters.Clear();
            DbParameters.AddParameters(command.Parameters, "@id", keys);
            return command.ExecuteReader();
        }

        protected override DbDataReader SelectAll()
        {
            return baseQuery.ToCommand(connection).ExecuteReader();
        }

        protected override int GetKey(DbDataReader reader)
        {
            return reader.GetInt32("id");
        }

        protected override void InsertEntities(IEnumerable<TestEntity> entities)
        {
            var entityArray = entities as TestEntity[] ?? entities.ToArray();
            if (entityArray.Length == 0) return;

            DbCommand cmd = SqlQuery.Insert().Into("test_table").Columns(new[] { "id", "name", "money", "lat", "lon" }).Values(entityArray.Length)
                .OnDuplicateKey(new[] { "money", "lat", "lon" }).ToPreparedCommand(connection);

            DbParameters.AddParameters(cmd.Parameters, "@id", entityArray.Select((e) => (e.Key == -1) ? null : (object)e.Key));
            DbParameters.AddParameters(cmd.Parameters, "@name", entityArray.Select((e) => e.Name));
            DbParameters.AddParameters(cmd.Parameters, "@money", entityArray.Select((e) => e.Money));
            DbParameterExtensions.AddVector2Parameters(cmd.Parameters, "@lat", "@lon", entityArray.Select((e) => e.Coord));
            cmd.ExecuteNonQuery();
        }
    }
}
