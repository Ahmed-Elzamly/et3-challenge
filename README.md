# Delivery Route Planner

A small C# console program that groups a list of delivery requests into vehicle trips, respecting a 10kg per-trip capacity, prioritizing urgent deliveries, and grouping deliveries from the same area together where reasonably possible.

## How to Run

**Requirements:** .NET 9.0 SDK (or the SDK version this project targets)

```bash
git clone <your-repo-url>
cd "et3 challenge"
dotnet run
```

The program reads `sample1.json` from the output directory (copied there automatically on build) and prints a full report to the console: total counts, rejected deliveries, per-trip stop details, and per-trip summaries.

To test with a different dataset, replace the contents of `sample1.json` with your own delivery list in the same format (see below), rebuild, and run again.

## Input Format

A JSON array of delivery objects:

```json
[
  {
    "id": 1,
    "area": "Nasr City",
    "priority": 2,
    "weight_kg": 4.5
  }
]
```

- `id`: unique integer identifier
- `area`: delivery area/neighborhood (string)
- `priority`: integer; lower numbers are more urgent
- `weight_kg`: package weight in kilograms (decimal)

## README and Reasoning

### 1. Solution approach

Deliveries are first sorted by priority ascending, with area as a secondary sort key — this puts equally-urgent, same-area deliveries next to each other before any assignment happens.

Each sorted delivery is then processed in order:

- If its weight alone exceeds the 10kg vehicle capacity, it's rejected immediately (see edge cases below) and processing continues with the next delivery.
- Otherwise, every currently open trip is checked for whether the delivery would fit without exceeding capacity. Among the trips that qualify, the delivery is placed in the trip using this preference order:
    1. A trip that already contains a delivery from the same area, if one exists.
    2. Among remaining candidates (or if no area match exists), the trip with the least remaining capacity — a "best-fit" heuristic that keeps trips tightly packed.
- If no open trip can take the delivery, a new trip is created for it.

Each `Trip` tracks its own deliveries, running weight, and a set of areas it contains, so area-membership and remaining-capacity checks are fast and self-contained. Once all deliveries are assigned, each trip produces its own summary (delivery count, total weight, capacity utilization %, and areas covered).

### 2. Most difficult part

Balancing the area-grouping rule against the priority-ordering rule, since they can pull in different directions — deciding on a single, consistent tie-break rule (same-area first, then least remaining capacity) that satisfies both without needing separate branching logic for each case. Getting the exact LINQ pipeline right for that tie-break. 

### 3. Situations where the grouping may not be optimal

This algorithm is a greedy heuristic, not an optimal solver — true optimal grouping across trips is a bin-packing problem (NP-hard), which is out of scope for an assignment of this size. Because each delivery is assigned as it's encountered (in priority order) rather than by looking ahead, two deliveries from the same area can still end up in different trips if, by the time the second one is processed, every open trip that could hold it is already full — even if a different assignment earlier on would have kept them together. The algorithm never revisits or reshuffles earlier trips once they're assigned, so a locally reasonable choice can occasionally cost a better grouping later.

### 4. What would slow down at 1,000,000 deliveries

Two things: for every delivery, the current logic scans every currently open trip to find a candidate (`O(n × open trips)`), which grows expensive as the number of open trips grows. The whole delivery list, and all trips, are also held fully in memory as in-memory lists rather than streamed.

At that scale, I'd index open trips by area (e.g. a dictionary of area → list of trip references) so only relevant trips are scanned instead of all of them, and consider closing/archiving trips once they're near-full so they drop out of the search space entirely. I'd also look at streaming/batch-processing the input rather than deserializing the entire JSON array into memory at once.

### 5. What I'd improve with another day

- Accept the input file path as a command-line argument (with a sensible default), so a reviewer can point the program at a different file without editing the source.
- Add the area-indexing optimization described above for scalability.
- Add automated unit tests for the edge cases, rather than relying on manual verification.
- Make area matching case-insensitive to guard against inconsistent input casing (e.g. "Maadi" vs. "maadi").
- Consider whether trips should be re-optimized after initial assignment (a light local-search pass) to reduce the same-area-split issue described in Q3.

## Extension: Trip Summary Report

Each trip is summarized with its delivery count, total weight, capacity utilization percentage, and the list of areas it covers. This was chosen because it's directly useful to a dispatcher managing a fleet — it answers "how full is this trip?" and "which neighborhoods does it cover?" at a glance, without needing to read through every individual stop. The utilization percentage also doubles as a simple, self-reported quality signal for the packing algorithm itself: trips with low utilization highlight where the greedy assignment left capacity on the table.

## Edge Cases

- **No deliveries:** the program reports zero totals and prints "There are no Trips" for both the trip details and summary sections, rather than erroring.
- **Package heavier than 10kg:** rejected immediately via a custom `TooHeavyPackageException`, logged in a separate "Rejected Deliveries" section with its ID, area, and weight, and excluded from all trips. Processing continues for the remaining deliveries.
- **Multiple deliveries with the same priority:** ties are broken deterministically by area name, so equally-urgent, same-area deliveries are grouped together in the base sort before assignment even begins.
- **Adding the next package would exceed capacity:** the trip-fit check (`CanFit`) excludes that trip from consideration for that delivery; a different open trip is used, or a new trip is created if none qualify.