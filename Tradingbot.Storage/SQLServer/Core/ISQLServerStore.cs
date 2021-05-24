using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tradingbot.Storage.SQLServer.Core
{
    public interface ISQLServerStore
    {
        Task<bool> Delete<T>(T item);
        Task ExecuteQuery(string query);
        Task<T> Get<T>(T item);
        Task<IEnumerable<T>> GetAll<T>(T item);
        Task Insert<T>(T item);
        Task<bool> Update<T>(T item);
        Task Upsert<T>(T item);
    }
}
