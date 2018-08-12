using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpEntities
{
    public abstract class SqlQuery
    {
        public static SelectQuery Select()
        {
            return new SelectQuery();
        }

        public static InsertQuery Insert()
        {
            return new InsertQuery();
        }

        public static UpdateQuery Update()
        {
            return new UpdateQuery();
        }

        public static DeleteQuery Delete()
        {
            return new DeleteQuery();
        }

        public static CustomQuery Custom()
        {
            return new CustomQuery();
        }

        public static string EscapeSqlIdentifier(string identifier)
        {
            string[] split = identifier.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < split.Length; i++)
            {
                split[i] = $"`{split[i]}`";
            }

            return string.Join(".", split);
        }

        public abstract string ToSql();

        public DatabaseCommand ToCommand(DatabaseConnection connection)
        {
            return connection.CreateCommand(ToSql(), false);
        }

        public DatabaseCommand ToPreparedCommand(DatabaseConnection connection)
        {
            return connection.CreateCommand(ToSql(), true);
        }
    }

    public class SelectQuery : SqlQuery
    {
        private enum Clause
        {
            From,
            Column,
            Join,
            Where,
            Having,
            GroupBy,
            OrderBy
        }

        private readonly List<string> tables = new List<string>();
        private readonly List<string> columns = new List<string>();
        private readonly List<string> joins = new List<string>();
        private readonly List<string> whereConditions = new List<string>();
        private readonly List<string> havingConditions = new List<string>();
        private readonly List<string> groupByColumns = new List<string>();
        private readonly List<string> orderByColumns = new List<string>();
        private uint? limit = null;
        private Clause activeClause = Clause.From;

        public SelectQuery Clone()
        {
            var cloneQuery = new SelectQuery();
            cloneQuery.activeClause = activeClause;
            cloneQuery.tables.AddRange(tables);
            cloneQuery.columns.AddRange(columns);
            cloneQuery.joins.AddRange(joins);
            cloneQuery.whereConditions.AddRange(whereConditions);
            cloneQuery.groupByColumns.AddRange(groupByColumns);
            cloneQuery.orderByColumns.AddRange(orderByColumns);
            cloneQuery.limit = limit;

            return cloneQuery;
        }

        public SelectQuery From(string table, string alias = null)
        {
            tables.Add(EscapeSqlIdentifier(table) + (alias != null ? $" AS {EscapeSqlIdentifier(alias)}" : ""));
            activeClause = Clause.From;

            return this;
        }

        public SelectQuery AllColumns()
        {
            columns.Clear();
            columns.Add("*");
            activeClause = Clause.Column;

            return this;
        }

        public SelectQuery Column(string column, bool escape = true, string alias = null)
        {
            if (escape) columns.Add(EscapeSqlIdentifier(column) + (alias != null ? $" AS {EscapeSqlIdentifier(alias)}" : ""));
            else columns.Add(column + (alias != null ? $" AS {EscapeSqlIdentifier(alias)}" : ""));
            activeClause = Clause.Column;

            return this;
        }

        public SelectQuery Column(IEnumerable<(string column, bool escape, string alias)> columns)
        {
            foreach ((string column, bool escape, string alias) in columns)
            {
                Column(column, escape, alias);
            }

            return this;
        }

        public SelectQuery Column(IEnumerable<string> columns)
        {
            foreach (string column in columns)
            {
                Column(column);
            }

            return this;
        }

        public SelectQuery Join(string table, string on)
        {
            joins.Add($"{EscapeSqlIdentifier(table)} {on}");
            activeClause = Clause.Join;

            return this;
        }

        public SelectQuery Where(string codition)
        {
            if (whereConditions.Count > 0)
                whereConditions.Add("AND");

            whereConditions.Add($"{codition}");
            activeClause = Clause.Where;

            return this;
        }

        public SelectQuery WhereIn(string column, int optionCount)
        {
            return WhereIn(new[] {column}, optionCount);
        }

        public SelectQuery WhereIn(IList<string> columns, int optionCount)
        {
            if (whereConditions.Count > 0)
                whereConditions.Add("AND");

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < optionCount; i++)
            {
                
                builder.Append($"{(i == 0 ? "" : ", ")}({ string.Join(", ", columns.Select(s => $"@{s}_{i}"))})");
            }
            whereConditions.Add($"({string.Join(", ", columns.Select(EscapeSqlIdentifier))}) IN ({builder.ToString()})");
            activeClause = Clause.Where;

            return this;
        }

        public SelectQuery And()
        {
            if (activeClause == Clause.Where) whereConditions.Add("AND");
            else if (activeClause == Clause.Having) havingConditions.Add("AND");

            return this;
        }

        public SelectQuery Or()
        {
            if (activeClause == Clause.Where) whereConditions.Add("OR");
            else if (activeClause == Clause.Having) havingConditions.Add("OR");

            return this;
        }

        public SelectQuery GroupBy(string column)
        {
            groupByColumns.Add(EscapeSqlIdentifier(column));
            activeClause = Clause.GroupBy;

            return this;
        }

        public SelectQuery Having(string codition)
        {
            havingConditions.Add($"{codition}");
            activeClause = Clause.Having;

            return this;
        }

        public SelectQuery OrderBy(string column, ColumnOrder order)
        {
            orderByColumns.Add($"{EscapeSqlIdentifier(column)} {order.GetName()}");
            activeClause = Clause.OrderBy;

            return this;
        }

        public SelectQuery Limit(uint count)
        {
            limit = count;

            return this;
        }

        public override string ToSql()
        {
            if (columns.Count == 0) throw new InvalidOperationException("You must select at least one column.");
            if (tables.Count == 0) throw new InvalidOperationException("You must select from at least one table.");

            StringBuilder builder = new StringBuilder();
            builder.Append("SELECT ");
            builder.Append(string.Join(", ", columns));
            builder.Append(" FROM ");
            builder.Append(string.Join(", ", tables));

            if (whereConditions.Count > 0)
            {
                builder.Append(" WHERE ");
                builder.Append(string.Join(" ", whereConditions));
            }

            if (groupByColumns.Count > 0)
            {
                builder.Append(" GROUP BY ");
                builder.Append(string.Join(", ", groupByColumns));
            }

            if (havingConditions.Count > 0)
            {
                builder.Append(" HAVING ");
                builder.Append(string.Join(" ", havingConditions));
            }

            if (orderByColumns.Count > 0)
            {
                builder.Append(" ORDER BY ");
                builder.Append(string.Join(", ", orderByColumns));
            }

            if (limit.HasValue)
            {
                builder.Append(" LIMIT " + limit);
            }

            return builder.ToString();
        }

        public enum ColumnOrder
        {
            [EnumNameAttribute("ASC")]
            Ascending,
            [EnumNameAttribute("DESC")]
            Descending
        }
    }

    public class InsertQuery : SqlQuery
    {
        private string table;
        private readonly List<string> columns = new List<string>();
        private int valueCount = 1;
        private readonly List<string> setColumns = new List<string>();
        private bool ignoreUpdate = false;

        public InsertQuery Clone()
        {
            var cloneQuery = new InsertQuery();
            cloneQuery.table = table;
            cloneQuery.columns.AddRange(columns);
            cloneQuery.valueCount = valueCount;
            cloneQuery.setColumns.AddRange(setColumns);
            cloneQuery.ignoreUpdate = ignoreUpdate;

            return cloneQuery;
        }

        public InsertQuery Into(string table)
        {
            this.table = EscapeSqlIdentifier(table);

            return this;
        }

        public InsertQuery Column(string column)
        {
            if (columns.Contains(column)) return this;

            columns.Add(column);

            return this;
        }

        public InsertQuery Columns(IEnumerable<string> columns)
        {
            foreach (string column in columns)
            {
                Column(column);
            }

            return this;
        }

        public InsertQuery Values(int valueCount)
        {
            this.valueCount = valueCount;

            return this;
        }

        public InsertQuery IgnoreUpdate()
        {
            ignoreUpdate = true;

            return this;
        }

        public InsertQuery OnDuplicateKey(string key)
        {
            setColumns.Add(key);

            return this;
        }

        public InsertQuery OnDuplicateKey(IEnumerable<string> keys)
        {
            setColumns.AddRange(keys);

            return this;
        }

        public override string ToSql()
        {
            if (columns.Count == 0) throw new InvalidOperationException("You must insert with at least one column defined.");
            if (table == null) throw new InvalidOperationException("You define the table to insert into.");

            StringBuilder builder = new StringBuilder();

            builder.Append($"INSERT INTO {table} (");
            builder.Append(string.Join(", ", columns));
            builder.Append(") VALUES ");
            for (int i = 0; i < valueCount; i++)
            {
                builder.Append($"{(i == 0 ? "" : ", ")}(");
                bool first = true;
                foreach (string column in columns)
                {
                    builder.Append($"{(first ? "" : ", ")}@{column}_{i}");
                    first = false;
                }
                builder.Append(")");
            }

            if (setColumns.Count > 0)
            {
                builder.Append(" ON DUPLICATE KEY UPDATE ");
                bool first = true;
                foreach (string column in setColumns)
                {
                    builder.Append($"{(first ? "" : ", ")}{EscapeSqlIdentifier(column)} = ");
                    builder.Append(ignoreUpdate ? $"{EscapeSqlIdentifier(column)}" : $"VALUES({EscapeSqlIdentifier(column)})");
                    first = false;
                }
            }

            return builder.ToString();
        }
    }

    public class UpdateQuery : SqlQuery
    {
        private enum Clause
        {
            Table,
            Set,
            Where
        }

        private string table = null;
        private readonly List<string> sets = new List<string>();
        private readonly List<string> whereConditions = new List<string>();
        private Clause activeClause = Clause.Table;

        public UpdateQuery Clone()
        {
            var cloneQuery = new UpdateQuery();
            cloneQuery.activeClause = activeClause;
            cloneQuery.table = table;
            cloneQuery.sets.AddRange(sets);
            cloneQuery.whereConditions.AddRange(whereConditions);

            return cloneQuery;
        }

        public UpdateQuery Table(string table)
        {
            this.table = EscapeSqlIdentifier(table);
            activeClause = Clause.Table;

            return this;
        }

        public UpdateQuery Set(string set)
        {
            sets.Add(set);
            activeClause = Clause.Set;

            return this;
        }

        public UpdateQuery SetColumn(string column, string to = null)
        {
            sets.Add(EscapeSqlIdentifier(column) + "=" + to ?? $"@{column}");
            activeClause = Clause.Set;

            return this;
        }

        public UpdateQuery SetColumn(IEnumerable<string> columns)
        {
            foreach (string column in columns)
            {
                SetColumn(column);
            }

            return this;
        }

        public UpdateQuery SetColumn(IEnumerable<(string column, string to)> sets)
        {
            foreach ((string column, string to) in sets)
            {
                SetColumn(column, to);
            }

            return this;
        }

        public UpdateQuery Where(string codition)
        {
            if (whereConditions.Count > 0)
                whereConditions.Add("AND");

            whereConditions.Add($"{codition}");
            activeClause = Clause.Where;

            return this;
        }

        public UpdateQuery WhereIn(string column, int optionCount)
        {
            if (whereConditions.Count > 0)
                whereConditions.Add("AND");

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < optionCount; i++)
            {
                builder.Append($"{(i == 0 ? "" : ", ")}@{column}_{i}");
            }
            whereConditions.Add($"{EscapeSqlIdentifier(column)} IN ({builder.ToString()})");
            activeClause = Clause.Where;

            return this;
        }

        public UpdateQuery And()
        {
            if (activeClause == Clause.Where) whereConditions.Add("AND");

            return this;
        }

        public UpdateQuery Or()
        {
            if (activeClause == Clause.Where) whereConditions.Add("OR");

            return this;
        }

        public override string ToSql()
        {
            if (sets.Count == 0) throw new InvalidOperationException("You must set at least one column.");
            if (table == null) throw new InvalidOperationException("You define the table to update.");

            StringBuilder builder = new StringBuilder();
            builder.Append($"UPDATE {table}");
            builder.Append(" SET ");
            builder.Append(string.Join(", ", sets));
            if (whereConditions.Count > 0)
            {
                builder.Append(" WHERE ");
                builder.Append(string.Join(" ", whereConditions));
            }

            return builder.ToString();
        }
    }

    public class DeleteQuery : SqlQuery
    {
        private enum Clause
        {
            Table,
            Where
        }

        private string table = null;
        private readonly List<string> whereConditions = new List<string>();
        private Clause activeClause = Clause.Table;

        public DeleteQuery Clone()
        {
            var cloneQuery = new DeleteQuery();
            cloneQuery.activeClause = activeClause;
            cloneQuery.table = table;
            cloneQuery.whereConditions.AddRange(whereConditions);

            return cloneQuery;
        }

        public DeleteQuery From(string table)
        {
            this.table = EscapeSqlIdentifier(table);
            activeClause = Clause.Table;

            return this;
        }

        public DeleteQuery Where(string codition)
        {
            if (whereConditions.Count > 0)
                whereConditions.Add("AND");

            whereConditions.Add($"{codition}");
            activeClause = Clause.Where;

            return this;
        }

        public DeleteQuery WhereIn(string column, int optionCount)
        {
            if (whereConditions.Count > 0)
                whereConditions.Add("AND");

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < optionCount; i++)
            {
                builder.Append($"{(i == 0 ? "" : ", ")}@{column}_{i}");
            }
            whereConditions.Add($"{EscapeSqlIdentifier(column)} IN ({builder.ToString()})");
            activeClause = Clause.Where;

            return this;
        }

        public DeleteQuery And()
        {
            if (activeClause == Clause.Where) whereConditions.Add("AND");

            return this;
        }

        public DeleteQuery Or()
        {
            if (activeClause == Clause.Where) whereConditions.Add("OR");

            return this;
        }

        public override string ToSql()
        {
            if (table == null) throw new InvalidOperationException("You define the table to delete from.");

            StringBuilder builder = new StringBuilder();
            builder.Append($"DELETE FROM {table}");
            if (whereConditions.Count > 0)
            {
                builder.Append(" WHERE ");
                builder.Append(string.Join(" ", whereConditions));
            }

            return builder.ToString();
        }
    }

    public class CustomQuery : SqlQuery
    {
        private readonly StringBuilder builder = new StringBuilder();

        public CustomQuery Clone()
        {
            var cloneQuery = new CustomQuery();
            cloneQuery.builder.Append(builder);

            return cloneQuery;
        }

        public void AppendSql(string sql)
        {
            builder.Append(sql);
        }

        public override string ToSql()
        {
            return builder.ToString();
        }
    }
}
