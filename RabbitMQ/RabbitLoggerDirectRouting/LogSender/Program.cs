using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory
{
    HostName = "localhost",
};

using IConnection connection = factory.CreateConnection();
using IModel channel = connection.CreateModel();

string exchangeName = "logger_direct";
channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);

string[] severities = { "info", "error", "warning" };
Random rnd = new();

Console.WriteLine(" Sending messages...");

for (int i = 0; i < 40; i++)
{
    string severity = severities[rnd.Next(0, severities.Length)];
    string message = $"[{severity}] Hello #{i}";
    byte[] body = Encoding.UTF8.GetBytes(message);
    channel.BasicPublish(exchangeName, routingKey: severity, null, body);
    
    Console.WriteLine(message);

    Thread.Sleep(1000);
}


Console.WriteLine(" Finished. Press key to exit.");
Console.ReadKey();