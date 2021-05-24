using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tradingbot.Storage.MessageQueue.Core
{
    public abstract class QueueClient : IDisposable
    {
        private readonly ILogger<QueueClient> _logger;
        protected IModel _channel;
        private IConnection _connection;
        private readonly IConnectionFactory _connectionFactory;
        private bool _isDisposed;
        public QueueClient(ILoggerFactory loggerFactory, IConnectionFactory connectionFactory)
        {
            _logger = loggerFactory.CreateLogger<QueueClient>();
            _connectionFactory = connectionFactory;
            _connection = connectionFactory.CreateConnection();

            Connect();
        }

        /// <summary>
        /// Connect if its not already connected
        /// </summary>
        protected void Connect()
        {
            if (_connection == null || _connection.IsOpen == false)
                _connection = _connectionFactory.CreateConnection();

            if (_channel == null || _channel.IsOpen == false)
                _channel = _connection.CreateModel();

        }

        public bool IsConnected
        {
            get
            {
                return !(_connection == null || _connection.IsOpen == false || _channel == null || _channel.IsOpen == false);
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (disposing)
            {
                _connection?.Close();
                _connection?.Dispose();
            }
            _isDisposed = true;

        }
    }
}
