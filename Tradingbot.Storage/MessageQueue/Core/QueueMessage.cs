using System;
using System.Collections.Generic;
using System.Text;

namespace Tradingbot.Storage.MessageQueue.Core
{
    public class QueueMessage<T> where T : IQueueMessage, new()
    {
        public DateTime? DatePublished { get; set; }

        public ulong DeliveryTag { get; set; }


        public string ConsumerTag { get; set; }
        public string Exchange { get; set; }
        public bool Redelivered { get; set; }

        public string RoutingKey { get; set; }

        public string RawMessage { get; set; }

        public T MessagePayload { get; set; }

    }
}
