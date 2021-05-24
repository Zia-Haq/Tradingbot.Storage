using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using Tradingbot.Storage.MessageQueue.Core;

namespace Tradingbot.Storage.MessageQueue
{
    public class MessagePublisher : QueueClient, IMessagePublisher
    {
        public MessagePublisher(ILoggerFactory loggerFactory, IConnectionFactory connectionFactory) : base(loggerFactory, connectionFactory)
        {
        }

        private IBasicProperties BasicProperties
        {
            get
            {
                var basicProperties = _channel.CreateBasicProperties();
                basicProperties.DeliveryMode = 2;//Persistant
                basicProperties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                return basicProperties;
            }
        }

        /// <summary>
        /// Publishes messages to the queue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="queueName"></param>
        /// <param name="exchange"></param>
        public void Publish<T>(T message, string queueName, string exchange = "") where T : IQueueMessage
        {
            Connect();
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            _channel.BasicPublish(exchange, queueName, BasicProperties, body);

        }
    }
}
