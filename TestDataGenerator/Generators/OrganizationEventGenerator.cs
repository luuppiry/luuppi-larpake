namespace TestDataGenerator.Generators;
internal class OrganizationEventGenerator : IRunAll
{
    private readonly IOrganizationEventDatabase _db;
    private readonly IUserDatabase _userDb;
    private readonly ILarpakeEventDatabase _larpakeEventDb;

    public OrganizationEventGenerator(
        IOrganizationEventDatabase db, 
        IUserDatabase userDb,
        ILarpakeEventDatabase larpakeEventDb)
    {
        _db = db;
        _userDb = userDb;
        _larpakeEventDb = larpakeEventDb;
    }

    public async Task GenerateOrganizationEvents()
    {
        var records = await _db.Get(new EventQueryOptions { PageOffset = 0, PageSize = 1 });
        if (records.Length is not 0)
        {
            Console.WriteLine("Organization events already exist.");
            return;
        }

        DateTime refDate = new(2025, 1, 1);

        Console.WriteLine("Generating organization events.");
        
        var users = await _userDb.Get(new UserQueryOptions { PageOffset = 0, PageSize = 20 });

        List<OrganizationEvent> events = new Faker<OrganizationEvent>()
            .UseSeed(App.Seed)
            .RuleFor(x => x.Title, f => f.Database.Random.Word())
            .RuleFor(x => x.Body, f => f.Lorem.Paragraph())
            .RuleFor(x => x.StartsAt, f => f.Date.Future(refDate: refDate))
            .RuleFor(x => x.Location, f => f.Address.StreetAddress())
            .RuleFor(x => x.WebsiteUrl, f => f.Internet.Url())
            .RuleFor(x => x.ImageUrl, f => "https://images.alko.fi/images/cs_srgb,f_auto,t_medium/cdn/319027/gambina-muovipullo.jpg")
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(users).Id)
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(users).Id)
            .Generate(30);

        events[7].CancelledAt = new DateTime(2025, 1, 3);
        events[15].CancelledAt = new DateTime(2025, 2, 7);



        foreach (var e in events)
        {
            await _db.Insert(e);
        }

        Console.WriteLine($"Created {events.Count} organization events.");
    }


    public async Task Generate()
    {
        await GenerateOrganizationEvents();
    }
}
