using FoodFast.API.Domain.Entities;
using FoodFast.API.Domain.Models;
using FoodFast.API.Hubs;
using FoodFast.API.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace FoodFast.API.Background;

public class RabbitMqAnnouncementConsumer : BackgroundService
{
    private readonly IHubContext<AnnouncementsHub> _hub;
    private readonly RabbitMqSetting _settings;
    private readonly IEmailService _emailService;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqAnnouncementConsumer(
        IHubContext<AnnouncementsHub> hub,
        IOptions<RabbitMqSetting> options, 
        IEmailService emailService)
    {
        _hub = hub;
        _settings = options.Value;
        _emailService = emailService;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _settings.HostName,
            UserName = _settings.UserName,
            Password = _settings.Password,
            // Uri = new Uri(_settings.ConnectionString)
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(exchange: _settings.ExchangeName,
                                            ExchangeType.Direct,
                                            cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(queue: _settings.QueueName,
                                        durable: true,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null,
                                        cancellationToken: cancellationToken);

        await _channel.QueueBindAsync(queue: _settings.QueueName,
                                     exchange: _settings.ExchangeName,
                                     routingKey: _settings.RoutingKey,
                                     arguments: null,
                                     cancellationToken: cancellationToken);

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel is null)
            return;

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (sender, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var announcement = JsonSerializer.Deserialize<Announcement>(json)!;

                var announcementId = announcement.Id;

                // send Announcement for online users

                _ = _hub.Clients.Group($"announcement_{announcementId}")
                                  .SendAsync("NewAnnouncement", json, stoppingToken);

                // send Announcement emails to all users
                // didn't have time to write it :)
            }
            catch (Exception)
            {
                await _channel.BasicRejectAsync(deliveryTag: ea.DeliveryTag,
                                                requeue: true,
                                                cancellationToken: stoppingToken);

                throw;
            }
            finally
            {
                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag,
                                             multiple: false,
                                             cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(_settings.QueueName,
                                   autoAck: false,
                                   consumer,
                                   cancellationToken: stoppingToken);

        await Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await DisposeAsync();
        await base.StopAsync(cancellationToken);
    }

    public async Task DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync();
            await _channel.DisposeAsync();
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
    }

    public override void Dispose()
    {
        DisposeAsync().GetAwaiter().GetResult();
    }
}

