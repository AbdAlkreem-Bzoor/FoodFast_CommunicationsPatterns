using FoodFast.API.Domain.Entities;
using FoodFast.API.Domain.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace FoodFast.API.Infrastructure;

public class RabbitMqAnnouncementPublisher : IRabbitMqAnnouncementPublisher, IDisposable
{

    private readonly ConcurrentDictionary<string, bool> _declared;
    private readonly RabbitMqSetting _settings;
    private IConnection _connection = null!;
    private IChannel _channel = null!;
    public RabbitMqAnnouncementPublisher(IOptions<RabbitMqSetting> options)
    {
        _settings = options.Value;
        _declared = new ConcurrentDictionary<string, bool>();

        StartAsync().GetAwaiter().GetResult();
    }

    private async Task StartAsync()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _settings.HostName,
            UserName = _settings.UserName,
            Password = _settings.Password,
            //Uri = new Uri(_settings.ConnectionString)
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
    }

    public void Dispose()
    {
        DisposeAsync().WaitAsync(cancellationToken: default);
    }

    private async Task DisposeAsync()
    {
        await _channel.CloseAsync();
        await _connection.CloseAsync();
        await _channel.DisposeAsync();
        await _connection.DisposeAsync();
    }

    public async Task PublishAsync(Announcement announcement)
    {
        string key = $"{_settings.ExchangeName}:{_settings.QueueName}:{_settings.RoutingKey}";

        if (!_declared.ContainsKey(key))
        {
            await _channel.ExchangeDeclareAsync(exchange: _settings.ExchangeName,
                                            ExchangeType.Direct);

            await _channel.QueueDeclareAsync(queue: _settings.QueueName,
                                            durable: true,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);

            await _channel.QueueBindAsync(queue: _settings.QueueName,
                                         exchange: _settings.ExchangeName,
                                         routingKey: _settings.RoutingKey,
                                         arguments: null);

            _declared[key] = true;
        }


        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(announcement));

        var basicProperties = new BasicProperties()
        {
            Expiration = _settings.Expiration,
            DeliveryMode = DeliveryModes.Persistent
        };

        await _channel.BasicPublishAsync(exchange: _settings.ExchangeName,
                                        routingKey: _settings.RoutingKey,
                                        mandatory: false,
                                        basicProperties: basicProperties,
                                        body: body);
    }
}
