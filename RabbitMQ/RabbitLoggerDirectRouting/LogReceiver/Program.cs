using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory
{
    HostName = "localhost",
};

using IConnection connection = factory.CreateConnection();
using IModel channel = connection.CreateModel();

string exchangeName = "logger_direct";

if (args.Length < 1)
{
    Console.Error.WriteLine("Usage: {0} [info] [warning] [error]",
                            Environment.GetCommandLineArgs()[0]);
    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
    Environment.ExitCode = 1;
}

channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);

string queueName = channel.QueueDeclare().QueueName;

foreach (string severity in args)
{
    channel.QueueBind(queueName, exchangeName, severity);
}

Console.WriteLine(" Waiting for messages...");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, args) =>
{
    string message = Encoding.UTF8.GetString(args.Body.ToArray());
    Console.WriteLine(message);
};

channel.BasicConsume(queueName, true, consumer);

Console.WriteLine(" Press key to exit.");
Console.ReadKey();