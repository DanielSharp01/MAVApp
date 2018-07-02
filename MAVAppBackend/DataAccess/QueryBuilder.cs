using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAVAppBackend.DataAccess
{
    public static class QueryBuilder
    {
        public static SelectQuery Select(IEnumerable<string> columns, string from)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("SELECT ");
            string prefix = "";
            foreach (string column in columns)
            {
                builder.Append($"{prefix}`{column}`");
                prefix = ", ";
            }
            builder.Append($" FROM `{from}`");
            return new SelectQuery(builder);
        }

        public static SelectQuery SelectEveryColumn(string from)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"SELECT * FROM `{from}`");
            return new SelectQuery(builder);
        }

        public static UpdateQuery Update(string table)
        {
            return new UpdateQuery(table);
        }

        public static InsertQuery InsertInto(string table, IEnumerable<string> columns)
        {
            return new InsertQuery(table, columns);
        }
    }

    public abstract class Query
    {
        public abstract string ToSql();

        public MySqlCommand ToCommand(MySqlConnection connection)
        {
            return new MySqlCommand(ToSql(), connection);
        }

        public MySqlCommand ToPreparedCommand(MySqlConnection connection)
        {
            MySqlCommand cmd = new MySqlCommand(ToSql(), connection);
            cmd.Prepare();
            return cmd;
        }
    }

    public class SelectQuery : Query
    {
        private StringBuilder baseQuery = new StringBuilder();
        private StringBuilder whereClause = new StringBuilder();
        private StringBuilder groupByClause = new StringBuilder();
        private StringBuilder orderByClause = new StringBuilder();

        public SelectQuery(StringBuilder baseQuery)
        {
            this.baseQuery.Append(baseQuery);
        }

        private SelectQuery(StringBuilder baseQuery, StringBuilder whereClause, StringBuilder groupByClause, StringBuilder orderByClause)
        {
            this.baseQuery.Append(baseQuery);
            this.whereClause.Append(whereClause);
            this.groupByClause.Append(groupByClause);
            this.orderByClause.Append(orderByClause);
        }

        public SelectQuery Join(string table, string on)
        {
            StringBuilder newBaseQuery = new StringBuilder();
            newBaseQuery.Append(baseQuery);
            newBaseQuery.Append($" JOIN `{table}` ON {on}");
            return new SelectQuery(newBaseQuery, whereClause, groupByClause, orderByClause);
        }

        // Where clause
        public SelectQuery Where(string sql, string combineOperation = "AND")
        {
            StringBuilder newWhereClause = new StringBuilder();
            newWhereClause.Append(whereClause);
            if (newWhereClause.Length == 0) newWhereClause.Append(sql);
            else newWhereClause.Append($" {combineOperation} {sql}");

            return new SelectQuery(baseQuery, newWhereClause, groupByClause, orderByClause);
        }

        public SelectQuery WhereIn(string column, string baseParam, int paramCount, string combineOperation = "AND")
        {
            StringBuilder paramBuilder = new StringBuilder();
            for (int i = 0; i < paramCount - 1; i++)
            {
                paramBuilder.Append(baseParam + $"_{i}, ");
            }
            paramBuilder.Append(baseParam + $"_{paramCount - 1}");
            return Where($"`{column}` IN({paramBuilder.ToString()})", combineOperation);
        }

        // Group by clause
        public SelectQuery GroupBy(string column)
        {
            StringBuilder newGroupByClause = new StringBuilder();
            newGroupByClause.Append(groupByClause);
            if (newGroupByClause.Length == 0) newGroupByClause.Append($"`{column}`");
            else newGroupByClause.Append($", `{column}`");

            return new SelectQuery(baseQuery, whereClause, newGroupByClause, orderByClause);
        }

        // Order by clause
        public SelectQuery OrderByAsc(string column)
        {
            StringBuilder newOrderByClause = new StringBuilder();
            newOrderByClause.Append(orderByClause);
            if (newOrderByClause.Length == 0) newOrderByClause.Append($"`{column}` ASC");
            else newOrderByClause.Append($", `{column}` ASC");

            return new SelectQuery(baseQuery, whereClause, groupByClause, newOrderByClause);
        }
        public SelectQuery OrderByDesc(string column)
        {
            StringBuilder newOrderByClause = new StringBuilder();
            newOrderByClause.Append(orderByClause);
            if (newOrderByClause.Length == 0) newOrderByClause.Append($"`{column}` DESC");
            else newOrderByClause.Append($", `{column}` DESC");

            return new SelectQuery(baseQuery, whereClause, groupByClause, newOrderByClause);
        }

        public override string ToSql()
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(baseQuery.ToString());
            if (whereClause.Length > 0) sql.Append(" WHERE " + whereClause.ToString());
            if (groupByClause.Length > 0) sql.Append(" GROUP BY " + groupByClause.ToString());
            if (orderByClause.Length > 0) sql.Append(" ORDER BY " + orderByClause.ToString());
            return sql.ToString();
        }
    }

    public class UpdateQuery : Query
    {
        private StringBuilder setClause = new StringBuilder();
        private StringBuilder whereClause = new StringBuilder();
        private string table;

        public UpdateQuery(string table)
        {
            this.table = table;
        }

        private UpdateQuery(StringBuilder setClause, StringBuilder whereClause)
        {
            this.setClause.Append(setClause);
            this.whereClause.Append(whereClause);
        }

        // Set clause
        public UpdateQuery Set(string sql)
        {
            StringBuilder newSetClause = new StringBuilder();
            newSetClause.Append(setClause);
            if (newSetClause.Length == 0) newSetClause.Append(sql);
            else newSetClause.Append($", {sql}");
            return new UpdateQuery(newSetClause, whereClause);
        }

        public UpdateQuery SetColumns(IEnumerable<string> columns)
        {
            StringBuilder newSetClause = new StringBuilder();
            newSetClause.Append(setClause);
            foreach (string column in columns)
            {
                if (newSetClause.Length == 0) newSetClause.Append($"`{column}` = @{column}");
                else newSetClause.Append($", `{column}` = @{column}");
            }
            return new UpdateQuery(newSetClause, whereClause);
        }

        // Where clause
        public UpdateQuery Where(string sql, string combineOperation = "AND")
        {
            StringBuilder newWhereClause = new StringBuilder();
            newWhereClause.Append(whereClause);
            if (newWhereClause.Length == 0) newWhereClause.Append(sql);
            else newWhereClause.Append($" {combineOperation} {sql}");

            return new UpdateQuery(setClause, newWhereClause);
        }

        public UpdateQuery WhereIn(string column, string baseParam, int paramCount, string combineOperation = "AND")
        {
            StringBuilder paramBuilder = new StringBuilder();
            for (int i = 0; i < paramCount - 1; i++)
            {
                paramBuilder.Append(baseParam + $"_{i}, ");
            }
            paramBuilder.Append(baseParam + $"_{paramCount - 1}");
            return Where($"`{column}` IN({paramBuilder.ToString()})", combineOperation);
        }

        public override string ToSql()
        {
            if (setClause.Length == 0) throw new InvalidOperationException("No set clause is set on this query.");
            StringBuilder sql = new StringBuilder();
            sql.Append($"UPDATE `{table}` SET " + setClause.ToString());
            if (whereClause.Length > 0) sql.Append(" WHERE " + whereClause.ToString());
            return sql.ToString();
        }
    }

    public class InsertQuery : Query
    {
        private string table;
        private List<string> columns = new List<string>();
        private int valueCount = 1;
        private bool onDuplicateKey = false;
        private string keyColumn;

        public InsertQuery(string table, IEnumerable<string> columns)
        {
            this.table = table;
            this.columns.AddRange(columns);
        }

        public InsertQuery Values(int count = 1)
        {
            return new InsertQuery(table, columns) { valueCount = count, onDuplicateKey = onDuplicateKey, keyColumn = keyColumn };
        }

        public InsertQuery OnDuplicateKeyUpdate(string keyColumn)
        {
            return new InsertQuery(table, columns) { valueCount = valueCount, onDuplicateKey = true, keyColumn = keyColumn };
        }

        public override string ToSql()
        {
            StringBuilder sql = new StringBuilder();
            sql.Append($"INSERT INTO `{table}` (");
            string prefix = "";
            foreach (string column in columns)
            {
                sql.Append($"{prefix}`{column}`");
                prefix = ", ";
            }
            sql.Append(")");
            sql.Append("VALUES");
            for (int i = 0; i < valueCount; i++)
            {
                sql.Append("(");
                prefix = "";
                foreach (string column in columns)
                {
                    sql.Append($"{prefix}@{column}_{i}");
                    prefix = ", ";
                }
                if (i < valueCount -1) sql.Append("), ");
                else sql.Append(")");
            }
            if (onDuplicateKey)
            {
                sql.Append(" ON DUPLICATE KEY UPDATE ");
                prefix = "";
                foreach (string column in columns)
                {
                    if (keyColumn == column) continue;
                    sql.Append($"{prefix}`{column}` = VALUES(`{column}`)");
                    prefix = ", ";
                }
            }

            return sql.ToString();
        }
    }
}
