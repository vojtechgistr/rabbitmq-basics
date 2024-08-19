using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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

string queueName = channel.QueueDeclare().QueueName;
channel.QueueBind(queueName, exchangeName, routingKey);

Console.WriteLine("Waiting for logs to come in...");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, args) =>
{
    string message = Encoding.UTF8.GetString(args.Body.ToArray());
    Console.WriteLine(message);
};

channel.BasicConsume(queueName, true, consumer);

Console.WriteLine("Press key to exit");
Console.ReadKey();