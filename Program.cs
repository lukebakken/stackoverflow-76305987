using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;

namespace MQTTTest
{
    static class Program
    {
        static void Main()
        {
            RabbitMQ.Client.ConnectionFactory _factory;
            RabbitMQ.Client.IConnection _connection;
            try
            {
                _factory = new ConnectionFactory
                {
                    HostName = "localhost",
                    Port = 5672,
                    UserName = "guest",
                    Password = "guest",
                    VirtualHost = "/",
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                    ContinuationTimeout = TimeSpan.FromSeconds(10),
                    RequestedHeartbeat = TimeSpan.FromSeconds(10)
                };

                /*
                _factory.Ssl.Enabled = true;
                _factory.Ssl.Version = SslProtocols.Tls12;
                _factory.Ssl.AcceptablePolicyErrors = 
                    SslPolicyErrors.RemoteCertificateChainErrors |
                    SslPolicyErrors.RemoteCertificateNameMismatch |
                    SslPolicyErrors.RemoteCertificateNotAvailable;
                */

                _connection = _factory.CreateConnection();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error while connection to RabbitMQ Broker: " + e.Message);
                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
                return;
            }


            var _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: "exchangekey",
                            type: ExchangeType.Topic);
            var _queueName = _channel.QueueDeclare().QueueName;

            _channel.QueueBind(queue: _queueName,
                      exchange: "exchangekey",
                      routingKey: "routingkey");

            Console.WriteLine(" [*] Waiting for messages.");

            var _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += (model, ea) =>
            {
                var _body = ea.Body.ToArray();
                var _message = Encoding.UTF8.GetString(_body);
                var _routingKey = ea.RoutingKey;
                Console.WriteLine($" [x] Received '{_routingKey}':'{_message}'");
            };
            _channel.BasicConsume(queue: _queueName,
                                 autoAck: true,
                                 consumer: _consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
