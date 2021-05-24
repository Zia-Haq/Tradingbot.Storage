using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tradingbot.Storage.MessageQueue;
using Tradingbot.Storage.MessageQueue.Core;

namespace Tradingbot.StorageTests.MessageQueue
{
    class ATestMessage : IQueueMessage
    {
        public string MessageId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? PublishTime { get; set; }
    }

    public class AppSettings
    {
        public string RabbitMQHost { get; set; }
        public string RabbitMQUserId { get; set; }
        public string RabbitMQPassword { get; set; }
        public string RabbitMQVirtualHost { get; set; }

    }

    //MessageQueueClient tests
    //Tests connectivity ie pub/sub by performig a simple publish and subscribe
    [TestClass()]
    public class MessageQueueClientTests
    {
        private static IMessagePublisher _messagePublisher;
        private static IMessageListener _messageListener;
        private static IModel _channel;
        private static string _testQueueName;

        public MessageQueueClientTests()
        {
            var config = ConfigHelper.InitConfiguration("appsettings.test.json");
            var appSettings = config.GetSection(nameof(AppSettings)).Get<AppSettings>();

            var connectionFactory = new ConnectionFactory()
            {
                DispatchConsumersAsync = true,
                HostName = appSettings.RabbitMQHost,
                UserName = appSettings.RabbitMQUserId,
                Password = appSettings.RabbitMQPassword,
                VirtualHost = appSettings.RabbitMQVirtualHost

            };
            _channel = connectionFactory.CreateConnection().CreateModel();

            _testQueueName = $"testQueue_ForUnitTest";
            _channel.QueueDelete(_testQueueName);//Delete if it already exist 
            _channel.QueueDeclare(_testQueueName, durable: true, exclusive: false, autoDelete: true);

            var loggerFactory = Substitute.For<ILoggerFactory>();

            _messageListener = new MessageListener(loggerFactory, connectionFactory);
            _messagePublisher = new MessagePublisher(loggerFactory, connectionFactory);
        }


        [TestInitialize()]
        public void TestInit()
        {
            _channel.QueuePurge(_testQueueName);

        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            _channel.QueueDelete(_testQueueName);
            _messageListener?.Dispose();
            _messagePublisher?.Dispose();
        }

        [TestMethod()]
        public async Task Subscribe_Should_Throw_Exception_If_Queue_DoesNot_Exist()
        {
            Func<QueueMessage<ATestMessage>, Task> processMessage = async (message) =>
            {
            };

            try
            {
                await _messageListener.Subscribe<ATestMessage>(_testQueueName + "_QueueDoesNotExist", processMessage);
                Assert.Fail("An exception must have been thrown");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("NOT_FOUND - no queue"));
            }
        }


        [TestMethod()]
        public async Task Published_Subscribe_Test()
        {
            var publishTime = DateTime.UtcNow;
            _messagePublisher.Publish(new ATestMessage()
            {
                Id = 1,
                Name = "Test Name",
                PublishTime = publishTime,
                MessageId = Guid.NewGuid().ToString()
            }, _testQueueName);


            ATestMessage aTestMessageReceived = null;
            Func<QueueMessage<ATestMessage>, Task> processMessage = async (message) =>
            {
                aTestMessageReceived = message.MessagePayload;
            };

            await _messageListener.Subscribe<ATestMessage>(_testQueueName, processMessage);


            var threeSecondsLater = DateTime.UtcNow.AddSeconds(3);

            while (aTestMessageReceived == null && DateTime.UtcNow < threeSecondsLater)
                await Task.Delay(10);

            aTestMessageReceived.Id.Should().Be(1);
            aTestMessageReceived.Name.Should().Be("Test Name");
            aTestMessageReceived.PublishTime.Value.Should().Be(publishTime);

        }

        [TestMethod()]
        public void Published_Ack_Test()
        {
            var publishTime = DateTime.UtcNow;
            _messagePublisher.Publish(new ATestMessage()
            {
                Id = 3,
                Name = "Test Name",
                MessageId = Guid.NewGuid().ToString(),
                PublishTime = publishTime
            }, _testQueueName);

            System.Threading.Thread.Sleep(10);
            var qMessage = _messageListener.Get<ATestMessage>(_testQueueName);
            var aTestMessageReceived = qMessage.MessagePayload;

            aTestMessageReceived.Id.Should().Be(3);
            aTestMessageReceived.Name.Should().Be("Test Name");
            aTestMessageReceived.PublishTime.Value.Should().Be(publishTime);

            //Ack the message
            _messageListener.AckMessage(qMessage.DeliveryTag);

            //As message is acked, we should not be able to get that message, rather null 
            qMessage = _messageListener.Get<ATestMessage>(_testQueueName);

            qMessage.Should().BeNull("As message is acknowledged");

        }


        [TestMethod()]
        public void Published_Nack_Test()
        {
            var publishTime = DateTime.UtcNow;
            _messagePublisher.Publish(new ATestMessage()
            {
                Id = 4,
                Name = "Test Name",
                MessageId = Guid.NewGuid().ToString(),
                PublishTime = publishTime
            }, _testQueueName);

            System.Threading.Thread.Sleep(10);
            var qMessage = _messageListener.Get<ATestMessage>(_testQueueName);
            var aTestMessageReceived = qMessage.MessagePayload;

            aTestMessageReceived.Id.Should().Be(4);
            aTestMessageReceived.Name.Should().Be("Test Name");
            aTestMessageReceived.PublishTime.Value.Should().Be(publishTime);

            //Nack the message
            _messageListener.NackMessage(qMessage.DeliveryTag);

            //As message is nacked, we should be able to recieve it agains 
            aTestMessageReceived = _messageListener.Get<ATestMessage>(_testQueueName).MessagePayload;

            aTestMessageReceived.Id.Should().Be(4);
            aTestMessageReceived.Name.Should().Be("Test Name");
            aTestMessageReceived.PublishTime.Value.Should().Be(publishTime);

        }

        [TestMethod()]
        public void Published_Get_Test()
        {
            var publishTime = DateTime.UtcNow;
            _messagePublisher.Publish(new ATestMessage()
            {
                Id = 2,
                Name = "Test Name",
                MessageId = Guid.NewGuid().ToString(),
                PublishTime = publishTime
            }, _testQueueName);
            System.Threading.Thread.Sleep(10);

            var aTestMessageReceived = _messageListener.Get<ATestMessage>(_testQueueName).MessagePayload;

            aTestMessageReceived.Id.Should().Be(2);
            aTestMessageReceived.Name.Should().Be("Test Name");
            aTestMessageReceived.PublishTime.Value.Should().Be(publishTime);

        }
    }
}
