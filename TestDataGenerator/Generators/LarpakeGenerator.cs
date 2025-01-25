using LarpakeServer.Models.QueryOptions;

namespace TestDataGenerator.Generators;
internal class LarpakeGenerator : IRunAll
{
    private readonly ILarpakeDatabase _db;

    public LarpakeGenerator(ILarpakeDatabase db)
    {
        _db = db;
    }

    public async Task CreateLarpakkeet()
    {
        var records = await _db.GetLarpakkeet(new QueryOptions() { PageOffset = 0, PageSize = 1 });
        if (records.Length is not 0)
        {
            Console.WriteLine("Larpakkeet already generated.");
            return;
        }

        Console.WriteLine("Create Larpakkeet.");

        Larpake[] larpakes = [
            new Larpake
            {
                Id = -1,
                Title="Kiasan seikkailu lärpäke",
                Year = 2024,
                Description = "Larpake 1 Description"
            },
            new Larpake
            {
                Id = -1,
                Title="Lärpäke 2025",
                Year = 2025,
                Description = "Everything fun for fuksis."
            }];

        foreach (var larpake in larpakes)
        {
            await _db.InsertLarpake(larpake);
        }

        Console.WriteLine($"Created {larpakes.Length} Lärpäkettä!!!");
    }

    public async Task CreateSections()
    {
        var records = await _db.GetSections(new QueryOptions() { PageOffset = 0, PageSize = 1 });
        if (records.Length is not 0)
        {
            Console.WriteLine("Sections already generated.");
            return;
        }

        Console.WriteLine("Create sections");

        LarpakeSection[] sections =
        [
            new() {
                Id = -1,
                LarpakeId = 1,
                Title = "Fuksi touhuilee",
                OrderingWeightNumber = 1,
            },
            new() {
                Id = -1,
                LarpakeId = 1,
                Title = "Tanpereella",
                OrderingWeightNumber = 2,
            },
            new() {
                Id = -1,
                LarpakeId = 2,
                Title = "Section 1",
                OrderingWeightNumber = 1,
            },
            new() {
                Id = -1,
                LarpakeId = 2,
                Title = "Section 2",
                OrderingWeightNumber = 2,
            }
        ];

        foreach (var section in sections)
        {
            await _db.InsertSection(section);
        }

        Console.WriteLine($"Created {sections.Length} sections.");
    }


    public async Task Generate()
    {
        await CreateLarpakkeet();
        await CreateSections();
    }
}
