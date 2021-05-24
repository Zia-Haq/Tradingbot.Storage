using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Tradingbot.Storage.SQLServer.Core
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateDbConnection(string connectionString);

        DatabaseType DatabaseType { get; }
    }
}
