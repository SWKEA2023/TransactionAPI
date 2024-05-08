using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Program>();

        IConfiguration configuration = builder.Build();

        var rabbitMqUrl = configuration["RabbitMQ:RMQ_URL"];

        var factory = new ConnectionFactory { Uri = new Uri(rabbitMqUrl) };

        using (IConnection connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "hello",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var message = "Hello World!";
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: string.Empty,
                routingKey: "hello",
                basicProperties: null,
                body: body);

            Console.WriteLine($" [x] Sent {message}");
        }

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}