using LarpakeServer.Models.Localizations;
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
        var records = await _db.GetLarpakkeet(new LarpakeQueryOptions() { PageOffset = 0, PageSize = 1 });
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
                Year = 2024,
                TextData = [
                    new LarpakeLocalization
                    {
                        LanguageCode = "fi",
                        Title = "Kiasan seikkailu lärpäke",
                        Description = "Larpake 1 Description"
                    },
                    new LarpakeLocalization
                    {
                        LanguageCode = "en",
                        Title = "Kiasan adventure lärpäke",
                        Description = "Larpake 1 Description"
                    }
            ]
            },
            new Larpake
            {
                Id = -1,
                Year = 2025,
                TextData = [
                    new LarpakeLocalization
                    {
                        LanguageCode = "fi",
                        Title = "Fuksien lärpäke 2025",
                        Description = "Kaikki hauska fuksiksille."
                    },
                    new LarpakeLocalization
                    {
                        LanguageCode = "en",
                        Title = "Fuksi lärpäke 2025",
                        Description = "Everything fun for fuksis."
                    }
                ]
            }];

        foreach (var larpake in larpakes)
        {
            await _db.InsertLarpake(larpake);
        }

        Console.WriteLine($"Created {larpakes.Length} Lärpäkettä!!!");
    }

    public async Task CreateSections()
    {
        var records = await _db.GetLarpakkeet(new LarpakeQueryOptions() { PageOffset = 0, PageSize = 1, DoMinimize = false });
        if (records.FirstOrDefault()?.Sections?.Count > 0)
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
                OrderingWeightNumber = 1,
                TextData = [
                    new LarpakeSectionLocalization
                    {
                        LanguageCode = "fi",
                        Title = "Fuksi touhuilee",
                    },
                    new LarpakeSectionLocalization
                    {
                        LanguageCode = "en",
                        Title = "Fuksi fucking around",
                    }
                ]
            },
            new() {
                Id = -1,
                LarpakeId = 1,
                OrderingWeightNumber = 2,
                TextData = [
                    new LarpakeSectionLocalization
                    {
                        LanguageCode = "fi",
                        Title = "Tampereella",
                    },
                    new LarpakeSectionLocalization
                    {
                        LanguageCode = "en",
                        Title = "In Tammerfors",
                    }
                ]
            },
            new() {
                Id = -1,
                LarpakeId = 2,
                OrderingWeightNumber = 1,
                TextData = [
                    new LarpakeSectionLocalization
                    {
                        LanguageCode = "fi",
                        Title = "Fuksi touhuilee",
                    }]
            },
            new() {
                Id = -1,
                LarpakeId = 2,
                OrderingWeightNumber = 2,
                TextData = [
                    new LarpakeSectionLocalization
                    {
                        LanguageCode = "en",
                        Title = "Hello fresh!"
                    }]
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
