using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Tradingbot.Storage.SQLServer.Core;

namespace Tradingbot.Storage.SQLServer
{
    public enum DatabaseType
    {
        NotSupported,
        MSSQLServer,
        MySQLServer
    }

    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly DatabaseType _databaseType = DatabaseType.NotSupported;
        public DbConnectionFactory(string datbaseType)
        {
            _databaseType = Enum.Parse<DatabaseType>(datbaseType, true);
        }

        public DatabaseType DatabaseType => _databaseType;

        public IDbConnection CreateDbConnection(string connectionString)
        {
            return _databaseType switch
            {
                DatabaseType.MSSQLServer => new SqlConnection(connectionString),
                DatabaseType.MySQLServer => new MySqlConnection(connectionString),
                _ => throw new NotSupportedException($"{_databaseType.ToString()} is not supported."),
            };
        }


    }
}
