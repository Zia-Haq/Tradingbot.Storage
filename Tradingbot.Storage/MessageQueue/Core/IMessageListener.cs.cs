using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tradingbot.Storage.MessageQueue.Core
{
    public interface IMessageListener : IDisposable
    {
        void AckMessage(ulong deliveryTag);
        void NackMessage(ulong deliveryTag, bool reQueue = true);
        Task<string> Subscribe<T>(string queueName, Func<QueueMessage<T>, Task> processMessage, bool includeRawMessage = false) where T : IQueueMessage, new();
        void UnSubscribe(string consumerTage);

        bool IsConnected { get; }
        QueueMessage<T> Get<T>(string queueName) where T : IQueueMessage, new();

    }
}
