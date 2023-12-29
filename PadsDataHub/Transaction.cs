using System.Text.Json.Serialization;

namespace ElasticsearchIntegration;

public class Transaction
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
    
    [JsonPropertyName("server")]
    public string? Server { get; set; } 

    [JsonPropertyName("transaction_date")]
    public string? TransactionDate { get; set; } 

    [JsonPropertyName("logs")]
    public string? Logs { get; set; }

    [JsonPropertyName("tpe_version")]
    public string? TpeVersion { get; set; } 

    [JsonPropertyName("tpe_site")]
    public string? TpeSite { get; set; }

    [JsonPropertyName("destination_server")]
    public string? DestinationServer { get; set; } 

    [JsonPropertyName("contract")]
    public string? Contract { get; set; }

    [JsonPropertyName("transaction_type")]
    public string? TransactionType { get; set; }

    [JsonPropertyName("protocol")]
    public string? Protocol { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("idsa")]
    public string? Idsa { get; set; }
}