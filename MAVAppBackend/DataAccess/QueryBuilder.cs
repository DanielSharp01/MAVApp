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
                builder.Append(prefix + column);
                prefix = ", ";
            }
            builder.Append(" FROM { from}");
            return new SelectQuery(builder);
        }

        public static SelectQuery SelectEveryColumn(string from)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"SELECT * FROM {from}");
            return new SelectQuery(builder);
        }
    }

    public class SelectQuery
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
            this.baseQuery = baseQuery;
            this.whereClause = whereClause;
            this.groupByClause = groupByClause;
            this.orderByClause = orderByClause;
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
            return Where($"{column} IN({paramBuilder.ToString()})", combineOperation);
        }

        // Group by clause
        public SelectQuery GroupBy(string column)
        {
            StringBuilder newGroupByClause = new StringBuilder();
            newGroupByClause.Append(groupByClause);
            if (newGroupByClause.Length == 0) newGroupByClause.Append(column);
            else newGroupByClause.Append($", {column}");

            return new SelectQuery(baseQuery, whereClause, newGroupByClause, orderByClause);
        }

        // Order by clause
        public SelectQuery OrderByAsc(string column)
        {
            StringBuilder newOrderByClause = new StringBuilder();
            newOrderByClause.Append(orderByClause);
            if (newOrderByClause.Length == 0) newOrderByClause.Append($"{column} ASC");
            else newOrderByClause.Append($", {column} ASC");

            return new SelectQuery(baseQuery, whereClause, groupByClause, newOrderByClause);
        }
        public SelectQuery OrderByDesc(string column)
        {
            StringBuilder newOrderByClause = new StringBuilder();
            newOrderByClause.Append(orderByClause);
            if (newOrderByClause.Length == 0) newOrderByClause.Append($"{column} DESC");
            else newOrderByClause.Append($", {column} DESC");

            return new SelectQuery(baseQuery, whereClause, groupByClause, newOrderByClause);
        }

        public string ToSql()
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(baseQuery.ToString());
            if (whereClause.Length > 0) sql.Append(" WHERE " + whereClause.ToString());
            if (groupByClause.Length > 0) sql.Append(" GROUP BY " + groupByClause.ToString());
            if (orderByClause.Length > 0) sql.Append(" ORDER BY " + orderByClause.ToString());
            return sql.ToString();
        }

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
}
