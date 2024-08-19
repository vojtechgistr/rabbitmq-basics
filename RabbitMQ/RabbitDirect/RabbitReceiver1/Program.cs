using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

ConnectionFactory connectionFactory = new()
{
    Uri = new Uri("amqp://guest:guest@localhost:5672"),
    ClientProvidedName = "Rabbit Reciever1 App"
};

using IConnection connection = connectionFactory.CreateConnection();
using IModel channel = connection.CreateModel();


string exchangeName = "DemoExchange";
string routingKey = "demo-routing-key";
string queueName = "DemoQueue";

channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, true);
channel.QueueDeclare(queueName, true, false, false, null);
channel.QueueBind(queueName, exchangeName, routingKey);
channel.BasicQos(0, 1, false);


var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, args) =>
{
    byte[] body = args.Body.ToArray();
    string message = Encoding.UTF8.GetString(body);
    Task.Delay(TimeSpan.FromSeconds(5)).Wait();
    
    Console.WriteLine(message);
    channel.BasicAck(args.DeliveryTag, false);
};

string consumerTag = channel.BasicConsume(queueName, false, consumer);

Console.ReadKey();

channel.BasicCancel(consumerTag);
channel.Close();
connection.Close();