using System;

namespace SharpEntities
{
    public abstract class DatabaseConnection : IDisposable
    {
        protected DatabaseConnection(string connectionString) { }

        public abstract DatabaseCommand CreateCommand(string sql, bool prepare);

        public void Close()
        {
            Dispose();
        }
        public abstract void Dispose();
    }
}
