using MySql.Data.MySqlClient;

namespace SharpEntities.MySql
{
    public class MySqlDatabaseConnection : DatabaseConnection
    {
        private readonly MySqlConnection connection;
        public MySqlDatabaseConnection(string connectionString)
            : base(connectionString)
        {
            connection = new MySqlConnection(connectionString);
            connection.Open();
        }

        public override DatabaseCommand CreateCommand(string sql, bool prepared)
        {
            return new MySqlDatabaseCommand(connection, sql, prepared);
        }

        public override void Dispose()
        {
            connection?.Dispose();
        }
    }
}
