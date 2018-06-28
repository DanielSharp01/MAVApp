using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.DataAccess
{
    public abstract class SimpleUpdatableEntityMapper<E> : UpdatableEntityMapper<E> where E : Entity
    {
        protected List<string> updatedColumns = new List<string>();
        protected string table;
        public SimpleUpdatableEntityMapper(MySqlConnection connection, SelectQuery baseSelectQuery, string table, IEnumerable<string> updatedColumns)
            : base(connection, baseSelectQuery)
        {
            this.updatedColumns.AddRange(updatedColumns);
            this.table = table;
        }

        protected MySqlCommand updateSingleCmd = null;
        protected override void updateSingle(E entity)
        {
            if (updateSingleCmd == null)
                updateSingleCmd = QueryBuilder.InsertInto(table, updatedColumns).Values(1).OnDuplicateKeyUpdate(idColumn).ToPreparedCommand(connection);

            updateSingleCmd.Parameters.Clear();
            foreach (string column in updatedColumns)
            {
                addUpdateColumn(entity, column, 0, updateSingleCmd.Parameters);
            }
            updateSingleCmd.ExecuteNonQuery();
        }
        protected override void updateBatch()
        {
            List<E> values = updatedEntities.Values.Concat(insertedEntities).ToList();
            MySqlCommand cmd = QueryBuilder.InsertInto(table, updatedColumns).Values(values.Count).OnDuplicateKeyUpdate(idColumn).ToPreparedCommand(connection);
            for (int i = 0; i < values.Count; i++)
            {
                foreach (string column in updatedColumns)
                {
                    addUpdateColumn(values[i], column, i, cmd.Parameters);
                }
            }
            cmd.ExecuteNonQuery();
        }

        protected abstract void addUpdateColumn(E entity, string column, int columnIndex, MySqlParameterCollection parameters);
    }
}
