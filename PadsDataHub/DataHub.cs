using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ElasticsearchIntegration;

public class DataHub
{
    private static IHost _host ;

    public DataHub(IHost host)
    {
        _host = host;
    }
    public async void ProducerHandler()
    {
        //Get variables from file conf appsettings.json
        string? fileName = _host.Services.GetRequiredService<IConfiguration>()
            .GetValue<string>("LogFile:Path");
        string? topic = _host.Services.GetRequiredService<IConfiguration>()
            .GetValue<string>("Kafka:Topic");
        string? bootstrapServer = _host.Services.GetRequiredService<IConfiguration>()
            .GetValue<string>("Kafka:BootstrapServers");
        int timeOut = _host.Services.GetRequiredService<IConfiguration>()
            .GetValue<int>("LogFile:TimeOut");

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = "|"
        };

        Blockchain chain = new Blockchain();

        if (fileName != null)
        {
            var reader = new StreamReader(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            var csv = new CsvReader(reader, config);
            IEnumerable<Transaction>? records;
            records = csv.GetRecords<Transaction>();
            long lastMaxOffset = reader.BaseStream.Length;
            csv.Context.RegisterClassMap<TransactionMap>();

            //start at the end of the file
            while (true)
            {
                Thread.Sleep(timeOut);

                //if the file size has not changed, idle
                if (reader.BaseStream.Length == lastMaxOffset)
                    continue;

                //seek to the last max offset
                reader.BaseStream.Seek(lastMaxOffset, SeekOrigin.Begin);

                //read out of the file until the EOF
                foreach (var line in records.ToArray())
                {
                    if (line != null)
                    {
                        var presentBlock = new Block(DateTime.Now, null, JsonSerializer.Serialize(line));
                        chain.AddBlock(presentBlock);
                        
                        Console.WriteLine(presentBlock.PreviousHash);
                        Console.WriteLine(presentBlock.Hash);
                        
                        if (presentBlock.PreviousHash != presentBlock.Hash)
                        {
                            var transactionToKafka = new Transaction
                            {
                                Id = Guid.NewGuid().ToString(),
                                Server = line.Server,
                                TransactionDate = line.TransactionDate,
                                Logs = line.Logs,
                                TpeVersion = line.TpeVersion,
                                TpeSite = line.TpeSite,
                                DestinationServer = line.DestinationServer,
                                Contract = line.Contract?[8..] ?? "",
                                TransactionType = line.TransactionType,
                                Protocol = line.Protocol,
                                Status = line.Status,
                                Idsa = line.Idsa
                            };
                            
                            await new KafkaHandler().SendDataKafka(transactionToKafka, bootstrapServer, topic);
                        }
                    }
                }
                //update the last max offset
                lastMaxOffset = reader.BaseStream.Position;
            }
        }
    }

    public async void ConsumerHandler()
    {
        string? node = _host.Services.GetRequiredService<IConfiguration>()
            .GetValue<string>("Elasticsearch:Node");
        string? indice = _host.Services.GetRequiredService<IConfiguration>()
            .GetValue<string>("Elasticsearch:Indice");
        string? bootstrapServer = _host.Services.GetRequiredService<IConfiguration>()
            .GetValue<string>("Kafka:BootstrapServers");
        string? groupId = _host.Services.GetRequiredService<IConfiguration>()
            .GetValue<string>("Kafka:GroupId");
        string? topic = _host.Services.GetRequiredService<IConfiguration>()
            .GetValue<string>("Kafka:Topic");
        /*string? elasticUser = _host.Services.GetRequiredService<IConfiguration>()
            .GetValue<string>("Elasticsearch:Username");
        string? elasticPass = _host.Services.GetRequiredService<IConfiguration>()
            .GetValue<string>("Elasticsearch:Password");*/
        
        var dateNow = DateTime.Now;
        var indiceFormatted = indice + "-" + dateNow.Year + "-" + dateNow.Month;
        /*if (node != null)
            if (elasticUser != null)
                if (elasticPass != null)
                    await new KafkaHandler().ConsumeDataFromKafka(node, indiceFormatted, bootstrapServer, groupId,
                        topic, elasticUser, elasticPass);*/
        if (node != null)
        {
            await new KafkaHandler().ConsumeDataFromKafka(node, indiceFormatted, bootstrapServer, groupId, topic);
        }
    }
}
