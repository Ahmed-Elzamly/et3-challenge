using System.Text.Json.Serialization;

public class Delivery
{
    public int Id { get; set; }
    public string Area { get; set; } = String.Empty;
    public int Priority { get; set; }
    [JsonPropertyName("weight_kg")]
    public double Weight { get; set; }
}