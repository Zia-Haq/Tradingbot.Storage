using System;
using System.Collections.Generic;
using System.Text;

namespace Tradingbot.Storage.MessageQueue.Core
{
    //Defined this interface so every message published should have messageId which we can use in distributed tracing
    //to diagnose issues across different services
    public interface IQueueMessage
    {
        string MessageId { get; set; }
    }
}
