using Dapper;
using Dapper.FastCrud;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tradingbot.Storage.SQLServer.Core;

namespace Tradingbot.Storage.SQLServer
{
    public class SQLServerStore : ISQLServerStore
    {
        private readonly string _connectionString;

        private IDbConnectionFactory _dbConnectionFactory;

        public SQLServerStore(string connectionString, IDbConnectionFactory dbConnectionFactory)
        {
            _connectionString = connectionString;
            _dbConnectionFactory = dbConnectionFactory;
            if (dbConnectionFactory.DatabaseType == DatabaseType.MySQLServer)
                OrmConfiguration.DefaultDialect = SqlDialect.MySql;
        }

        public async Task ExecuteQuery(string query)
        {
            using var dbConnection = _dbConnectionFactory.CreateDbConnection(_connectionString);
            await dbConnection.ExecuteAsync(query);
        }

        public async Task<T> Get<T>(T item)
        {
            using var dbConnection = _dbConnectionFactory.CreateDbConnection(_connectionString);
            return await dbConnection.GetAsync<T>(item);
        }

        public async Task<IEnumerable<T>> GetAll<T>(T item)
        {
            using var dbConnection = _dbConnectionFactory.CreateDbConnection(_connectionString);
            return await dbConnection.FindAsync<T>();
        }

        public async Task Insert<T>(T item)
        {
            using var dbConnection = _dbConnectionFactory.CreateDbConnection(_connectionString);
            await dbConnection.InsertAsync(item);
        }

        public async Task<bool> Update<T>(T item)
        {
            using var dbConnection = _dbConnectionFactory.CreateDbConnection(_connectionString);
            return await dbConnection.UpdateAsync(item);

        }

        public async Task<bool> Delete<T>(T item)
        {
            using var dbConnection = _dbConnectionFactory.CreateDbConnection(_connectionString);
            return await dbConnection.DeleteAsync(item);

        }
        public async Task Upsert<T>(T item)
        {
            using var dbConnection = _dbConnectionFactory.CreateDbConnection(_connectionString);

            if (!await dbConnection.UpdateAsync(item))
                await dbConnection.InsertAsync(item);
        }
    }
}
