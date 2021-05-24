using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tradingbot.Storage.MessageQueue.Core;

namespace Tradingbot.Storage.MessageQueue
{
    public class MessageListener : QueueClient, IMessageListener
    {


        public MessageListener(ILoggerFactory loggerFactory, IConnectionFactory connectionFactory) : base(loggerFactory, connectionFactory)
        {
        }

        /// <summary>
        /// Ackowledge that message has been processed
        /// </summary>
        /// <param name="deliveryTag"></param>
        public void AckMessage(ulong deliveryTag)
        {
            Connect();
            _channel.BasicAck(deliveryTag, false);
        }


        /// <summary>
        /// Nack the message so it can be potentially requeued
        /// </summary>
        /// <param name="deliveryTag"></param>
        /// <param name="reQueue"></param>
        public void NackMessage(ulong deliveryTag, bool reQueue = true)
        {
            Connect();
            _channel.BasicNack(deliveryTag, false, reQueue);
        }

        public QueueMessage<T> Get<T>(string queueName) where T : IQueueMessage, new()
        {
            Connect();
            var result = _channel.BasicGet(queueName, false);

            if (result == null)
                return null;

            var body = Encoding.UTF8.GetString(result.Body.ToArray());

            return new QueueMessage<T>
            {
                DatePublished = DateTimeOffset.FromUnixTimeSeconds(result.BasicProperties.Timestamp.UnixTime).UtcDateTime,
                Exchange = result.Exchange,
                DeliveryTag = result.DeliveryTag,
                Redelivered = result.Redelivered,
                RoutingKey = result.RoutingKey,
                MessagePayload = JsonConvert.DeserializeObject<T>(body)
            };

        }


        /// <summary>
        /// This will cancel the subscription but not instantly as you would still get the messages already on the wire
        /// </summary>
        /// <param name="consumerTage"></param>
        public void UnSubscribe(string consumerTage)
        {
            Connect();
            _channel.BasicCancel(consumerTage);

        }

        /// <summary>
        /// Subscribe to the message stream on a queue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queueName"></param>
        /// <param name="processMessage"></param>
        /// <param name="includeRawMessage"></param>
        public async Task<string> Subscribe<T>(string queueName, Func<QueueMessage<T>, Task> processMessage, bool includeRawMessage = false) where T : IQueueMessage, new()
        {
            Connect();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, eventargs) =>
            {
                var body = Encoding.UTF8.GetString(eventargs.Body.ToArray());
                var t = new QueueMessage<T>
                {
                    DatePublished = DateTimeOffset.FromUnixTimeSeconds(eventargs.BasicProperties.Timestamp.UnixTime).UtcDateTime,
                    ConsumerTag = eventargs.ConsumerTag,
                    Exchange = eventargs.Exchange,
                    DeliveryTag = eventargs.DeliveryTag,
                    Redelivered = eventargs.Redelivered,
                    RoutingKey = eventargs.RoutingKey,
                    RawMessage = includeRawMessage ? body : null,
                    MessagePayload = JsonConvert.DeserializeObject<T>(body)
                };

                await processMessage?.Invoke(t);
             

            };

            return await Task.FromResult<string>(_channel.BasicConsume(queueName, false, consumer));

        }

       
    }
}