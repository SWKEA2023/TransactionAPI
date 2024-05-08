using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Receive;

public class Program
{
    public static void Main()
    {
        var factory = new ConnectionFactory { HostName = "127.0.0.1", Password = "password123", UserName = "admin", VirtualHost = "my_vhost"};
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "hello",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        Console.WriteLine(" [*] Waiting for messages from CinemaAPI.");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            try
            {
                using var doc = JsonDocument.Parse(message);
                Console.WriteLine($" [x] Received {message}");

                ProcessData(doc.RootElement);
                ForwardToAdminApi(doc.RootElement);
            }
            catch (Exception ex)
            {
                Console.WriteLine($" [!] Error processing message: {ex.Message}");
                NotifyEmailApiAboutError(ex.Message);
            }
        };

        channel.BasicConsume(queue: "hello",
            autoAck: true,
            consumer: consumer);

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }

    private static void ProcessData(JsonElement data)
    {
        try
        {
            throw new Exception("Simulated transaction failure.");

            // Code to process transaction normally
            // Console.WriteLine("Processing transaction...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred: {ex.Message}");
            NotifyEmailApiAboutError(ex.Message);
        }
    }

    private static void ForwardToAdminApi(JsonElement data)
    {
        // TODO: Create code to forward data to AdminAPI
        Console.WriteLine("Forwarding data to AdminAPI...");
    }

    private static void NotifyEmailApiAboutError(string errorMessage)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        var body = Encoding.UTF8.GetBytes(errorMessage);

        channel.BasicPublish(exchange: "",
            routingKey: "errorQueue",
            basicProperties: null,
            body: body);
        Console.WriteLine(" [x] Sent {0}", errorMessage);
    }
}