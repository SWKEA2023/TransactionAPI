#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace TransactionApi;


/// <summary>
/// Main entry point for the TransactionAPI
/// </summary>
public class Program
{
    // private static IConfiguration _configuration;
    private static IModel _channel;
    private static IConnection _connection;

    /// <summary>
    /// Main entry point for the TransactionAPI
    /// </summary>
    public static void Main(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("RMQ_URL");
        var transactionQueue = Environment.GetEnvironmentVariable("TRANSACTION_QUEUE");

        // var builder = new ConfigurationBuilder()
        //     .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        //     .AddUserSecrets<Program>();
        // _configuration = builder.Build();

        // var rabbitMqUrl = _configuration["RabbitMQ:RMQ_URL"];

        var factory = new ConnectionFactory { Uri = new Uri(env ?? throw new ArgumentNullException()) };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: transactionQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
        Console.WriteLine(" [*] Waiting for messages from CinemaAPI.");

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += Consumer_Received;

        _channel.BasicConsume(queue: transactionQueue, autoAck: true, consumer: consumer);

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();

        Console.WriteLine("Application started. Press Ctrl+C to exit.");
        ManualResetEvent resetEvent = new ManualResetEvent(false);
        resetEvent.WaitOne();
    }

    /// <summary>
    /// Event handler for received messages from CinemaAPI.
    /// </summary>
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

    /// <summary>
    /// Process data received from the CinemaAPI.
    /// </summary>
    /// <param name="data">Data being processed from CinemaAPI as a JsonElement</param>
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

    /// <summary>
    /// Forwards data to the AdminAPI.
    /// </summary>
    /// <param name="data">Data being forwarded to AdminAPI as a JsonElement</param>
    private static void ForwardToAdminApi(JsonElement data)
    {
        Console.WriteLine("Forwarding data to AdminAPI...");
        var adminQueue = Environment.GetEnvironmentVariable("ADMIN_QUEUE");
        var message = JsonSerializer.Serialize(data);
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: "", routingKey: adminQueue, basicProperties: null, body: body);
        Console.WriteLine(" [x] Data sent to AdminAPI");
    }

    /// <summary>
    /// Notify the EmailAPI about an error.
    /// </summary>
    /// <param name="errorMessage">For setting an error message</param>
    private static void NotifyEmailApiAboutError(string errorMessage)
    {
        Console.WriteLine("Notifying error...");
        var emailQueue = Environment.GetEnvironmentVariable("EMAIL_QUEUE");
        var body = Encoding.UTF8.GetBytes(errorMessage);
        _channel.BasicPublish(exchange: "", routingKey: emailQueue, basicProperties: null, body: body);
        Console.WriteLine(" [x] Sent {0}", errorMessage);
    }
}
