using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;

namespace ElasticsearchIntegration;

public class Program
{
    public static void Main()
    {
        IHost host = Host.CreateApplicationBuilder().Build();
        //IServiceCollection services;
        Console.WriteLine("Starting the application");

        while (true)
        {
            DataHub serverObject = new DataHub(host);
            Thread instanceCaller = new Thread(serverObject.ProducerHandler);
            Thread staticCaller = new Thread(serverObject.ConsumerHandler);
            instanceCaller.Start();
            staticCaller.Start();
            instanceCaller.Join();
            staticCaller.Join();
            
        }
    }
}
