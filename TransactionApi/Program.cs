#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace TransactionApi;

public class Program
{
    private static IConfiguration _configuration;
    private static IModel _channel;
    private static IConnection _connection;

    public static void Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Program>();
        _configuration = builder.Build();

        var rabbitMqUrl = _configuration["RabbitMQ:RMQ_URL"];
        var factory = new ConnectionFactory { Uri = new Uri(rabbitMqUrl) };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: "transactionApiQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        Console.WriteLine(" [*] Waiting for messages from CinemaAPI.");

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += Consumer_Received;

        _channel.BasicConsume(queue: "transactionApiQueue", autoAck: true, consumer: consumer);

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }

    private static void Consumer_Received(object model, BasicDeliverEventArgs ea)
    {
        try
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            using var doc = JsonDocument.Parse(message);
            Console.WriteLine($" [x] Received {message}");
            ProcessData(doc.RootElement);
        }
        catch (Exception ex)
        {
            Console.WriteLine($" [!] Error processing message: {ex.Message}");
            NotifyEmailApiAboutError(ex.Message);
        }
    }

    private static void ProcessData(JsonElement data)
    {
        Console.WriteLine("Processing transaction...");
        var random = new Random();
        var chance = random.Next(100);

        // Simulate a decision based on data processing
        if (chance < 70)
        {
            ForwardToAdminApi(data);
        }
        else
        {
            NotifyEmailApiAboutError("Simulated message based on data processing decision.");
        }
    }

    private static void ForwardToAdminApi(JsonElement data)
    {
        Console.WriteLine("Forwarding data to AdminAPI...");
        var message = JsonSerializer.Serialize(data);
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: "", routingKey: "successQueue", basicProperties: null, body: body);
        Console.WriteLine(" [x] Data sent to AdminAPI");
    }

    private static void NotifyEmailApiAboutError(string errorMessage)
    {
        Console.WriteLine("Notifying error...");
        var body = Encoding.UTF8.GetBytes(errorMessage);
        _channel.BasicPublish(exchange: "", routingKey: "errorQueue", basicProperties: null, body: body);
        Console.WriteLine(" [x] Sent {0}", errorMessage);
    }
}
