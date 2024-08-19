using System.Text;
using RabbitMQ.Stream.Client;
using RabbitMQ.Stream.Client.Reliable;

var streamSystem = await StreamSystem.Create(new StreamSystemConfig());
await streamSystem.CreateStream(new StreamSpec("hello-stream")
{
    MaxLengthBytes = 5_000_000_000
});

var producer = await Producer.Create(new ProducerConfig(streamSystem, "hello-stream")
{
    ConfirmationHandler = confirmation =>
    {
        var messageEnumerable = confirmation.Messages.Select(m => Encoding.UTF8.GetString(m.Data.Contents));
        string messages = string.Join(", ", messageEnumerable);
        
        switch (confirmation.Status)
        {
            case ConfirmationStatus.Confirmed:
                Console.WriteLine($"Messages confirmed: '{messages}' (ID: {confirmation.PublishingId}, status: {confirmation.Status})");
                break;

            case ConfirmationStatus.StreamNotAvailable:
            case ConfirmationStatus.InternalError:
            case ConfirmationStatus.AccessRefused:
            case ConfirmationStatus.PreconditionFailed:
            case ConfirmationStatus.PublisherDoesNotExist:
            case ConfirmationStatus.UndefinedError:
            case ConfirmationStatus.ClientTimeoutError:
                Console.WriteLine(
                    $"Message {confirmation.PublishingId} failed with {confirmation.Status}");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return Task.FromResult(Task.CompletedTask);
    }
});

string message = "Hello world";
byte[] body = Encoding.UTF8.GetBytes(message);
await producer.Send(new Message(body));

Console.WriteLine($" Sent message '{message}'");

Console.WriteLine(" Press any key to exit");
Console.ReadKey();
await producer.Close();
await streamSystem.Close();