
namespace TestDataGenerator.Generators;
internal class FreshmanGroupGenerator : IRunAll
{
    private readonly IFreshmanGroupDatabase _db;
    private readonly IUserDatabase _userDb;
    private readonly ILarpakeDatabase _larpakeDb;

    public FreshmanGroupGenerator(IFreshmanGroupDatabase db, IUserDatabase userDb, ILarpakeDatabase larpakeDb)
    {
        _db = db;
        _userDb = userDb;
        _larpakeDb = larpakeDb;
    }

    public async Task GenerateGroups()
    {
        var larpakkeet = await _larpakeDb.GetLarpakkeet(new LarpakeQueryOptions { PageOffset = 0, PageSize = 10 });
        var larpakeIds = larpakkeet.Select(x => x.Id).ToArray();

        var records = await _db.GetGroupsMinimized(new FreshmanGroupQueryOptions { PageOffset = 0, PageSize = 1 });
        if (records.Length is not 0)
        {
            Console.WriteLine("Groups already exist."); 
            return;
        }

        Console.WriteLine("Generating groups.");

        var groups = new Faker<FreshmanGroup>()
            .UseSeed(App.Seed)
            .RuleFor(x => x.Name, f => f.Random.Word())
            .RuleFor(x => x.GroupNumber, f => f.Random.Int(0, 15))
            .RuleFor(x => x.LarpakeId, f => f.PickRandom(larpakeIds))
            .Generate(5);

        foreach (var group in groups)
        {
            await _db.Insert(group);
        }

        Console.WriteLine($"Generated {groups.Count} groups.");

    }

    private async Task AddMembers()
    {
        var groups = await _db.GetGroups(new FreshmanGroupQueryOptions { PageOffset = 0, PageSize = 10 });
        if (groups.FirstOrDefault()?.Members?.Count > 0)
        {
            Console.WriteLine("Members already added.");
            return;
        }

        Console.WriteLine("Adding members.");

        var users = await _userDb.Get(new UserQueryOptions { PageOffset = 0, PageSize = 40 });
        
        int usersPerGroup = users.Length / groups.Length;
        for (int i = 0; i < groups.Length; i++)
        {
            Guid[] members = users
                .Skip(i * usersPerGroup)
                .Take(usersPerGroup)
                .Select(x => x.Id)
                .ToArray();

            await _db.InsertMembers(groups[i].Id, members);
        }

        Console.WriteLine("Added members.");
    }

    public async Task Generate()
    {
        await GenerateGroups();
        await AddMembers();
    }
}
