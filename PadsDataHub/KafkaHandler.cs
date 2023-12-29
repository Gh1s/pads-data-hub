using System;
using Confluent.Kafka;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace ElasticsearchIntegration;

public class KafkaHandler
{
    public async Task SendDataKafka(Transaction transaction, string bootstrapServers, string topic)
    {
        var producerConfig = new ProducerConfig() {BootstrapServers = bootstrapServers};
        
        using (var producer = new ProducerBuilder<Null, string>(producerConfig).Build())
        {
            var message = new Message<string, Transaction>
            {
                Key = transaction.Id,
                Value = new Transaction
                {
                    Id = transaction.Id,
                    Server = transaction.Server,
                    TransactionDate = transaction.TransactionDate,
                    Logs = transaction.Logs,
                    TpeVersion = transaction.TpeVersion,
                    TpeSite = transaction.TpeSite,
                    DestinationServer = transaction.DestinationServer,
                    Contract = transaction.Contract,
                    TransactionType = transaction.TransactionType,
                    Protocol = transaction.Protocol,
                    Status = transaction.Status,
                    Idsa = transaction.Idsa
                }
            };
            
            string messageString = JsonSerializer.Serialize(message.Value);
            Console.WriteLine(message.Value.Id);

            try
            {
                var dr = await producer.ProduceAsync(topic, new Message<Null, string> {Value = messageString});
                Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
            }
            catch (ProduceException<Null, string> e)
            {
                Console.WriteLine($"Delivery failed: {e.Error.Reason}");
            }
        }
    }

    public async Task ConsumeDataFromKafka(string node, string? indice, string? bootstrapServer,
        string? groupId, string? topic)
    {
        var elasticSettings = new ElasticsearchClientSettings(new Uri(node))
            .DefaultMappingFor<Transaction>(i =>
            {
                if (indice != null)
                    i
                        .IndexName(indice)
                        .IdProperty(t => t.Id);
            })
            //.Authentication(new BasicAuthentication(username, password))
            .EnableDebugMode()
            .PrettyJson()
            .RequestTimeout(TimeSpan.FromMinutes(2));
    
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServer,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        
        //const string topic = "transactions";
        CancellationTokenSource cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => {
            e.Cancel = true; // prevent the process from terminating.
            cts.Cancel();
        };
        
        var client = new ElasticsearchClient(elasticSettings);
        
        using (var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build())
        {
            consumer.Subscribe(topic);
            try
            {
                while (true)
                {
                    var cr = consumer.Consume(cts.Token);
                    Console.WriteLine(
                        $"Consumed event from topic {topic} with key {cr.Message.Key} and value {cr.Message.Value}");
                    var mObject = JsonSerializer.Deserialize<Transaction>(cr.Message.Value);
                    var transaction = new Transaction
                    {
                        Id   = mObject?.Id,
                        Server = mObject.Server,
                        TransactionDate = mObject.TransactionDate,
                        Logs = mObject.Logs,
                        TpeVersion = mObject.TpeVersion,
                        TpeSite = mObject.TpeSite,
                        DestinationServer = mObject.DestinationServer,
                        Contract = mObject.Contract,
                        TransactionType = mObject.TransactionType,
                        Protocol = mObject.Protocol,
                        Status = mObject.Status,
                        Idsa = mObject.Idsa
                    };
                    if (indice != null)
                    {
                        var response = await client.IndexAsync(transaction, indice);
                        Console.WriteLine(response.DebugInformation);
                        Console.WriteLine(response.ElasticsearchWarnings);
                        if (response.IsValidResponse)
                        {
                            Console.WriteLine($"Indexing line: {response.Result.ToString()}");
                        }
                        else
                        {
                            var debugInfo = response.DebugInformation;
                            var error = response.ElasticsearchServerError;
                            Console.WriteLine(debugInfo);
                            Console.WriteLine(error);
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                consumer.Close();
            }
        }
    }
    
}