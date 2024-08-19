using RabbitMQ.Client;
using System.Text;

ConnectionFactory factory = new()
{
    Uri = new Uri("amqp://guest:guest@localhost:5672"),
    ClientProvidedName = "Rabbit Sender App",
};

using IConnection connection = factory.CreateConnection();
using IModel channel =  connection.CreateModel();

string exchangeName = "DemoExchange";
string routingKey = "demo-routing-key";
string queueName = "DemoQueue";

channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, true);
channel.QueueDeclare(queueName, true, false, false, null);
channel.QueueBind(queueName, exchangeName, routingKey, null);

for (int i = 0; i < 60;i++)
{
    byte[] msg = Encoding.UTF8.GetBytes($"Hello #{i}");

    var properties = channel.CreateBasicProperties();
    properties.Persistent = true;

    channel.BasicPublish(exchangeName, routingKey, properties, msg);
    Console.WriteLine($"Sent message #{i}");
    Thread.Sleep(1000);
}