namespace et3_challenge.models;

public class TripSummary
{
    public int TripNumber { get; set; }
    public int DeliveryCount { get; set; }
    public double TotalWeight { get; set; }
    public double UtilizationPercent { get; set; }
    public List<string> Areas { get; set; }
}