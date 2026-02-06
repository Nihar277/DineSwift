using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;

namespace API.Services
{
    public record RabbitMqOptions
    {
        public string Host { get; init; } = "localhost";
        public int Port { get; init; } = 5672;
        public string Username { get; init; } = "guest";
        public string Password { get; init; } = "guest";
        public string VirtualHost { get; init; } = "/";
    }

    public class RabbitMqService : IDisposable
    {
        private readonly RabbitMqOptions _opts;
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private readonly object _connLock = new();

        public RabbitMqService(IOptions<RabbitMqOptions> opts)
        {
            _opts = opts.Value;
            _factory = new ConnectionFactory
            {
                HostName = _opts.Host,
                Port = _opts.Port,
                UserName = _opts.Username,
                Password = _opts.Password,
                VirtualHost = _opts.VirtualHost,
                DispatchConsumersAsync = false
            };
            _connection = CreateConnection();
        }

        private IConnection CreateConnection()
        {
            lock (_connLock)
            {
                if (_connection != null && _connection.IsOpen) return _connection;
                try { _connection?.Dispose(); } catch { }
                _connection = _factory.CreateConnection();
                return _connection;
            }
        }

        private IModel CreateModel()
        {
            var conn = CreateConnection();
            return conn.CreateModel();
        }

        public void EnsureQueue(string username)
        {
            using var channel = CreateModel();
            channel.QueueDeclare(queue: username, durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        public void SendToUser(string fromUser, string toUser, string text)
        {
            using var channel = CreateModel();
            channel.QueueDeclare(queue: toUser, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var payload = $"{fromUser}|{DateTime.UtcNow:o}|{text}";
            var body = Encoding.UTF8.GetBytes(payload);

            var props = channel.CreateBasicProperties();
            props.Persistent = false;


            channel.BasicPublish(exchange: "", routingKey: toUser, basicProperties: props, body: body);
        }

        public string? ReceiveOneForUser(string username)
        {
            using var channel = CreateModel();
            channel.QueueDeclare(queue: username, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var result = channel.BasicGet(queue: username, autoAck: true);
            if (result == null) return null;
            var msg = Encoding.UTF8.GetString(result.Body.ToArray());
            return msg;
        }

        public void Dispose()
        {
            try
            {
                _connection?.Close();
                _connection?.Dispose();
            }
            catch { }
        }
    }
}
