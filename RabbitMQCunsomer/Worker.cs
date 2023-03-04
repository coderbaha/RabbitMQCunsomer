using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Services;

namespace Consumer
{
    public class Worker : BackgroundService
    {
        private readonly RabbitMQClientService _rabbitMQClientService;
        private IModel _channel;
        public Worker(RabbitMQClientService rabbitMQClientService)
        {
            _rabbitMQClientService = rabbitMQClientService;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitMQClientService.Connect();
            _channel.BasicQos(0, 1, false);

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            _channel.BasicConsume("UsersCreaterQueue", false, consumer);
            consumer.Received += Consumer_Recived;

            return Task.CompletedTask;
        }
        private Task Consumer_Recived(object sender, BasicDeliverEventArgs @event)
        {
            Task.Delay(5000).Wait();

            try
            {
                var response = JsonSerializer.Deserialize<string>(Encoding.UTF8.GetString(@event.Body.ToArray()));
                foreach (var item in response)
                {
                    ServiceRequest(response, "apiRequestUrl");
                }

                _channel.BasicAck(@event.DeliveryTag, false);
            }
            catch (Exception ex)
            {
            }

            return Task.CompletedTask;
        }
        public void ServiceRequest(string userName, string url)
        {

            StringContent httpContent = new StringContent(userName, Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {
                var result = client.PostAsync(url, httpContent).Result;
            }
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
