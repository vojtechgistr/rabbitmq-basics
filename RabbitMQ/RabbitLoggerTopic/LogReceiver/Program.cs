
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory
{
    HostName = "localhost",
};

using IConnection connection = factory.CreateConnection();
using IModel channel = connection.CreateModel();

string exchangeName = "logger_topic";
channel.ExchangeDeclare(exchangeName, ExchangeType.Topic);

string queueName = channel.QueueDeclare().QueueName;

if (args.Length < 1)
{
    Console.Error.WriteLine("Usage: {0} [binding_key...]",
                            Environment.GetCommandLineArgs()[0]);
    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
    Environment.ExitCode = 1;
    return;
}

foreach (string routingKey in args)
{
    channel.QueueBind(queueName, exchangeName, routingKey);
}

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, args) =>
{
    var keys = args.RoutingKey.Split('.');
    string message = Encoding.UTF8.GetString(args.Body.ToArray());
    if (keys.Length == 2)
    {
        Console.WriteLine($"Received - [{keys[0]} : {keys[1]}] {message}");
    }
    else
    {
        Console.WriteLine($"Received invalid routing key for message: '{message}'");
    }
};

channel.BasicConsume(queueName, true, consumer);
Console.WriteLine($"Listening to messsages... (facility.severity: '{string.Join(", ", args)}')");

Console.WriteLine(" Press any key to exit.");
Console.ReadKey();