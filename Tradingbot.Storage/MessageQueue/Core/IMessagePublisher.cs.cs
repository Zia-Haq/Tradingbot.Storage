using System;
using System.Collections.Generic;
using System.Text;

namespace Tradingbot.Storage.MessageQueue.Core
{
    public interface IMessagePublisher : IDisposable
    {
        void Publish<T>(T message, string queueName, string exchange = "") where T : IQueueMessage;

        bool IsConnected { get; }
    }
}
