using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RPCClient;

public class RpcClient : IDisposable
{
    private const string QueueName = "rpc_queue";
    // private const string ExchangeName = "rpc_exchange";

    private readonly IConnection connection;
    private readonly IModel channel;
    private readonly string replyQueueName;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper = new();

    public RpcClient()
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
        };

        connection = factory.CreateConnection();
        channel = connection.CreateModel();
        replyQueueName = channel.QueueDeclare().QueueName;
        
        // channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (sender, args) =>
        {
            if (!callbackMapper.TryRemove(args.BasicProperties.CorrelationId, out var taskCompletionSource))
            {
                return;
            }
            
            var body = args.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            taskCompletionSource.TrySetResult(message);
        };

        channel.BasicConsume(replyQueueName, true, consumer);
    }
    public Task<string> CallAsync(string message, CancellationToken cancellationToken = default)
    {
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId  = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;

        var body = Encoding.UTF8.GetBytes(message);
        var taskCompletionSource = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, taskCompletionSource);
 
        channel.BasicPublish("", QueueName, props, body);

        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out _));
        return taskCompletionSource.Task;
    }

    public void Dispose()
    {
        connection.Close();
    }
}