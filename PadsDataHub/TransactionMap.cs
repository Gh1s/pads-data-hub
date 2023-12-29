using CsvHelper.Configuration;

namespace ElasticsearchIntegration;

public class TransactionMap: ClassMap<Transaction>
{
    public TransactionMap()
    {
        Map(t => t.Server).Index(0);
        Map(t => t.TransactionDate).Index(1);
        Map(t => t.Logs).Index(2);
        Map(t => t.TpeVersion).Index(3);
        Map(t => t.TpeSite).Index(4);
        Map(t => t.DestinationServer).Index(5);
        Map(t => t.Contract).Index(6);
        Map(t => t.TransactionType).Index(10);
        Map(t => t.Protocol).Index(13);
        Map(t => t.Status).Index(22);
        Map(t => t.Idsa).Index(25);
    }
}