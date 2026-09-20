using System.Text.Json;
using et3_challenge.Custom_Exceptions;
using et3_challenge.models;

string jsonPath = Path.Combine(AppContext.BaseDirectory, "sample1.json");
string json = File.ReadAllText(jsonPath);

var deliveries =
    JsonSerializer.Deserialize<List<Delivery>>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

List<Delivery> rejected = new List<Delivery>();
List<Trip> trips = new List<Trip>();

deliveries = deliveries
    .OrderBy(d => d.Priority)
    .ThenBy(d => d.Area)
    .ToList();
foreach (Delivery delivery in deliveries)
{
    try
    {
        if (delivery.Weight > 10.0)
            throw new TooHeavyPackageException(delivery);
        var bestTrip = trips
            .Where(t => t.CanFit(delivery))
            .OrderByDescending(t => t.HasArea(delivery.Area))
            .ThenBy(t => t.RemainingWeight)
            .FirstOrDefault();
        if (bestTrip != null)
        {
            bestTrip.Add(delivery);
        }
        else
        {
            Trip newTrip = new Trip();
            newTrip.Add(delivery);
            trips.Add(newTrip);
        }
    }
    catch (TooHeavyPackageException ex)
    {
        Console.WriteLine($"Regected: {ex.Message}");
        rejected.Add(delivery);
    }
}
var summaries = trips
    .Select((trip, index) => trip.Summarize(index + 1))
    .ToList();

Console.WriteLine("=== Delivery Route Planner Report ===");
Console.WriteLine($"Total deliveries: {deliveries.Count}");
Console.WriteLine($"Scheduled: {deliveries.Count - rejected.Count}");
Console.WriteLine($"Rejected: {rejected.Count}");
Console.WriteLine();


if (rejected.Count > 0)
{
    Console.WriteLine("--- Rejected Deliveries ---");
    foreach (var d in rejected)
    {
        Console.WriteLine($"  ID {d.Id}: {d.Area}, {d.Weight}kg (exceeds 10kg capacity)");
    }
    Console.WriteLine();
}

Console.WriteLine("--- Trip Details ---");
if (trips.Count > 0)
{
    for (int i = 0; i < trips.Count; i++)
    {
        Console.WriteLine($"Trip {i + 1}:");
        int stopNumber = 1;
        var tripDeliveries = trips[i].Deliveries
            .OrderBy(d => d.Area);
        foreach (var d in tripDeliveries)
        {
            Console.WriteLine($"  Stop {stopNumber}: Delivery ID {d.Id} - {d.Area}, " +
                              $"priority {d.Priority}, {d.Weight}kg");
            stopNumber++;
        }

        Console.WriteLine();
    }
}
else
{
    Console.WriteLine("There are no Trips");
}

Console.WriteLine("--- Trip Summaries ---");
if (summaries.Count > 0)
{
    foreach (var summary in summaries)
    {
        Console.WriteLine($"Trip {summary.TripNumber}: {summary.DeliveryCount} deliveries, " +
                          $"{summary.TotalWeight}kg / 10kg " +
                          $"({summary.UtilizationPercent}% utilized), " +
                          $"areas: {string.Join(", ", summary.Areas)}");
    }
}
else
{
    Console.WriteLine("There are no Trips");
}
