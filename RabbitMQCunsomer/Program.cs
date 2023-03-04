using Consumer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using Services;

namespace RabbitMQCunsomer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration Configuration = hostContext.Configuration;
                    services.AddHostedService<Worker>();
                    services.AddSingleton<RabbitMQClientService>();
                    services.AddSingleton(sp => new ConnectionFactory()
                    {
                        HostName = Configuration.GetConnectionString("localhost"),
                        VirtualHost = "/"
                        ,
                        UserName = "guest",
                        Password = "guest"
                        ,
                        DispatchConsumersAsync = true
                    });
                });
    }
}
