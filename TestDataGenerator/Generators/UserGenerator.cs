using LarpakeServer.Identity;
using LarpakeServer.Models.QueryOptions;

namespace TestDataGenerator.Generators;
internal class UserGenerator : IRunAll
{
    private readonly IUserDatabase _db;

    public UserGenerator(IUserDatabase db)
    {
        _db = db;
    }

    public int Seed { get; } = App.Seed;

    public async Task CreateUsers()
    {
        // Check if users already exist
        var existingRecords = await _db.Get(new UserQueryOptions()
        {
            PageSize = 1
        });
        if (existingRecords.Length is not 0)
        {
            Console.WriteLine("Users already exist.");
            return;
        }

        // Generate
        int count = 20;
        User[] users = new Faker<User>()
            .RuleFor(u => u.Id, f => Guid.NewGuid())
            .RuleFor(u => u.StartYear, f => f.Random.Number(2010, 2024))
            .UseSeed(Seed)
            .Generate(count)
            .ToArray();
        
        foreach (User user in users)
        {
            await _db.Insert(user);
        }
        Console.WriteLine($"Created {count} users.");
    }
    public async Task GivePermissions()
    {
        // Check if permissions already set
        var users = await _db.Get(new UserQueryOptions()
        {
            PageSize = int.MaxValue
        });
        if (users.First().Permissions is not 0)
        {
            Console.WriteLine("Permissions already set.");
            return;
        }

        var faker = new Faker<User>()
            .UseSeed(Seed)
            .RuleFor(u => u.Permissions,
                f => f.PickRandom(
                new List<Permissions>(){
                    Permissions.Freshman,
                    Permissions.Freshman,
                    Permissions.Freshman,
                    Permissions.Freshman,
                    Permissions.Freshman,
                    Permissions.Freshman,
                    Permissions.Freshman,
                    Permissions.Freshman,
                    Permissions.Tutor,
                    Permissions.Tutor,
                    Permissions.Tutor,
                    Permissions.Tutor,
                    Permissions.Tutor,
                    Permissions.Tutor,
                    Permissions.Admin,
                    Permissions.Admin,
                    Permissions.Sudo
                }));

        foreach (var user in users)
        {
            user.Permissions = faker.Generate().Permissions;
            await _db.UpdatePermissions(user.Id, user.Permissions);
        }
        Console.WriteLine("Updated permissions");
    }

    public async Task Generate()
    {
        await CreateUsers();
        await GivePermissions();
    }
}
