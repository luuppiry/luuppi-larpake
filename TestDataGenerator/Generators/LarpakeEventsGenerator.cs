namespace TestDataGenerator.Generators;
internal class LarpakeEventsGenerator : IRunAll
{
    private readonly ILarpakeEventDatabase _db;

    public LarpakeEventsGenerator(ILarpakeEventDatabase db)
    {
        _db = db;
    }

    public async Task CreateEvents()
    {
        var records = await _db.GetEvents(new QueryOptions { PageOffset = 0, PageSize = 1 });
        if (records.Length is not 0)
        {
            Console.WriteLine("Larpake events already exist.");
            return;
        }

        Console.WriteLine("Generating larpake events.");

        int[] validSections = Enumerable.Range(1, 4).ToArray();

        List<LarpakeEvent> events = new Faker<LarpakeEvent>()
            .UseSeed(App.Seed)
            .RuleFor(x => x.Title, f => f.Music.Random.Word())
            .RuleFor(x => x.Body, f => f.Lorem.Paragraph())
            .RuleFor(x => x.LarpakeSectionId, f => f.PickRandom(validSections))
            .RuleFor(x => x.Points, f => f.PickRandom(validSections))
            .Generate(30);

        events[4].CancelledAt = new DateTime(2025, 1, 3);
        events[21].CancelledAt = new DateTime(2025, 1, 10);

        foreach (var e in events)
        {
            await _db.Insert(e);
        }

        Console.WriteLine($"Created {events.Count} larpake events.");
    }


    public async Task Generate()
    {
        await CreateEvents();
    }
}
