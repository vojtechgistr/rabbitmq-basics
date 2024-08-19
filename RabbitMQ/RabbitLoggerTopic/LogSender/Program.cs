using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory
{
    HostName = "localhost",
};

using IConnection connection = factory.CreateConnection();
using IModel channel = connection.CreateModel();

string exchangeName = "logger_topic";
channel.ExchangeDeclare(exchangeName, ExchangeType.Topic);

string[] routingKeys = { "kern.critical", "kern.warning", "kern.info", "auth.info", "auth.critical", "auth.warning", "cron.critical", "cron.warning", "cron.info" };

Console.WriteLine("Sending messsages...");

Random rnd = new();
for (int i = 0; i < 100; i++)
{
    string key = routingKeys[rnd.Next(routingKeys.Length)];
    string message = $"Hello ${i}";
    byte[] body = Encoding.UTF8.GetBytes(message);
    channel.BasicPublish(exchangeName, key, null, body);

    Console.WriteLine($"Message '{message}' sent with facility.severity '{key}'");

    Thread.Sleep(1000);
}

Console.WriteLine(" Finished. Press any key to exit.");
Console.ReadKey();