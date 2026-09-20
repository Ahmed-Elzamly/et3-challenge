namespace et3_challenge.models;

public class Trip
{
    public double CurWeight { get; set; }
    public double RemainingWeight => 10 - CurWeight;
    
    private readonly List<Delivery> _deliveries  = new ();
    private readonly HashSet<string> _areas = new ();

    public bool CanFit(Delivery delivery)
    {
        if (delivery.Weight <= (10 - CurWeight))
            return true;
        return false;
    }

    public bool HasArea(string area)
    {
        if (_areas.Contains(area)) return true;
        return false;
    }

    public void Add(Delivery delivery)
    {
        _deliveries.Add(delivery);
        _areas.Add(delivery.Area);
        CurWeight += delivery.Weight;
    }
    public TripSummary Summarize(int tripNumber)
    {
        return new TripSummary
        {
            TripNumber = tripNumber,
            DeliveryCount = _deliveries.Count,
            TotalWeight = CurWeight,
            UtilizationPercent = Math.Round(CurWeight / 10 * 100, 1),
            Areas = _areas.ToList()
        };
    }
    public IReadOnlyList<Delivery> Deliveries => _deliveries;
}