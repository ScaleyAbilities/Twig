using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Twig
{
    static class RabbitHelper
    {
        private static IConnection rabbitConnection;
        private static IModel rabbitChannel;

        private static string rabbitHost = Environment.GetEnvironmentVariable("RABBIT_HOST") ?? "localhost";
        public static string rabbitTriggerPendingQueue = "triggerPending";
        private static IBasicProperties rabbitProperties;

        static RabbitHelper()
        {
            // Ensure Rabbit Queue is set up
            var factory = new ConnectionFactory()
            {
                HostName = rabbitHost,
                UserName = "scaley",
                Password = "abilities",
                DispatchConsumersAsync = true
            };

            // Try connecting to rabbit until it works
            var connected = false;
            while (!connected)
            {
                try
                {
                    rabbitConnection = factory.CreateConnection();
                    connected = true;
                }
                catch (BrokerUnreachableException)
                {
                    Console.Error.WriteLine("Unable to connect to Rabbit, retrying...");
                    Thread.Sleep(3000);
                }
            }

            rabbitChannel = rabbitConnection.CreateModel();

            rabbitChannel.QueueDeclare(
                queue: rabbitTriggerPendingQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            // This makes Rabbit wait for an ACK before sending us the next message
            rabbitChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            rabbitProperties = rabbitChannel.CreateBasicProperties();
            rabbitProperties.Persistent = true;
        }

        public static void CreateConsumer(Func<JObject, TriggerList, Task> messageCallback, TriggerList tl)
        {
            var consumer = new AsyncEventingBasicConsumer(rabbitChannel);
            consumer.Received += async (model, eventArgs) =>
            {
                JObject message = null;
                try
                {
                    message = JObject.Parse(Encoding.UTF8.GetString(eventArgs.Body));
                }
                catch (JsonReaderException ex)
                {
                    Console.Error.WriteLine($"Unable to parse Queue message into JSON: {ex.Message}");
                }

                if (message != null)
                    await messageCallback(message, tl);

                // We will always ack even if we can't parse it otherwise queue will hang
                rabbitChannel.BasicAck(eventArgs.DeliveryTag, false);
            };

            // This will begin consuming messages asynchronously
            rabbitChannel.BasicConsume(
                queue: rabbitTriggerPendingQueue,
                autoAck: false,
                consumer: consumer
            );
        }

        public static void PushTrigger(JObject properties)
        {
            // Make sure queue exists
            var queue = properties["queue"].ToString();
            if (string.IsNullOrWhiteSpace(queue))
                throw new ArgumentException("No response queue specified");

            rabbitChannel.BasicPublish(
                exchange: "",
                routingKey: queue,
                basicProperties: rabbitProperties,
                body: Encoding.UTF8.GetBytes(properties.ToString(Formatting.None))
            );
        }
    }
}