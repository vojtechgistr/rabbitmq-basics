using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory
{
    HostName = "localhost"
};

using IConnection connection = factory.CreateConnection();
using IModel channel = connection.CreateModel();

string exchangeName = "logs";
string routingKey = "";

channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout);

Console.WriteLine("Sending logs...");

for (int i = 0; i < 20; i++)
{
    string message = $"Test #{i}";
    
    byte[] body = Encoding.UTF8.GetBytes(message);
    channel.BasicPublish(exchangeName, routingKey, false, null, body);

    Console.WriteLine($"Sent message - {message}");
    Task.Delay(1000).Wait();
}

Console.WriteLine("Finished. Press key to exit");
Console.ReadKey();