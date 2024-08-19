using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RPCServer;

var factory = new ConnectionFactory
{
    HostName = "localhost",
};

IConnection connection = factory.CreateConnection();
IModel channel = connection.CreateModel();

const string queueName = "rpc_queue";
channel.QueueDeclare(queueName, false, false, false, null);
channel.BasicQos(0, 1, false);

var consumer = new EventingBasicConsumer(channel);

consumer.Received += (sender, args) =>
{
    var body = args.Body.ToArray();

    var props = args.BasicProperties;
    var replyProps = channel.CreateBasicProperties();
    replyProps.CorrelationId = props.CorrelationId;

    string response = "";
    try
    {
        string message = Encoding.UTF8.GetString(body);
        int n = int.Parse(message);
        Console.WriteLine($" [.] Fib({message})");
        response = RPCMethods.Fib(n).ToString();
    }
    catch (Exception ex)
    {
        Console.WriteLine($" [.] {ex.Message}");
    }
    finally
    {
        var responseBytes = Encoding.UTF8.GetBytes(response);
        channel.BasicPublish("", props.ReplyTo, replyProps, responseBytes);
        channel.BasicAck(args.DeliveryTag, false);
    }
};

channel.BasicConsume(queueName, false, consumer);

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

connection.Close();