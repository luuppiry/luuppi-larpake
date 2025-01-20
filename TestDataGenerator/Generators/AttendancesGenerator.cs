
using LarpakeServer.Extensions;
using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels.Metadata;

namespace TestDataGenerator.Generators;
internal class AttendancesGenerator : IRunAll
{
    private readonly IAttendanceDatabase _db;
    private readonly IUserDatabase _userDb;
    private readonly ILarpakeEventDatabase _eventDb;
    private readonly ISignatureDatabase _signatureDb;

    public AttendancesGenerator(
        IAttendanceDatabase db,
        IUserDatabase userDb,
        ILarpakeEventDatabase eventDb,
        ISignatureDatabase signatureDb)
    {
        _db = db;
        _userDb = userDb;
        _eventDb = eventDb;
        _signatureDb = signatureDb;
    }


    public async Task GenerateAttendaces()
    {
        var records = await _db.Get(new AttendanceQueryOptions { PageOffset = 0, PageSize = 1 });
        if (records.Length is not 0)
        {
            Console.WriteLine("Attendances already exists.");
            return;
        }

        Console.WriteLine("Generating attendances.");

        var users = await _userDb.Get(new UserQueryOptions { PageOffset = 0, PageSize = 40 });
        var events = await _eventDb.GetEvents(new QueryOptions { PageOffset = 0, PageSize = 40 });

        var attendances = new Faker<Attendance>()
            .UseSeed(App.Seed)
            .RuleFor(a => a.UserId, f => f.PickRandom(users).Id)
            .RuleFor(a => a.EventId, f => f.PickRandom(events).Id)
            .Generate(200)
            .DistinctBy(x => new { x.UserId, x.EventId })
            .ToArray();

        foreach (var attendance in attendances)
        {
            await _db.InsertUncompleted(attendance);
        }
        Console.WriteLine($"Generated {attendances.Length} attendances.");
    }


    public async Task CompleteAttendances()
    {
        var records = await _db.Get(new AttendanceQueryOptions { PageOffset = 0, PageSize = 200 });
        if (records.Any(x => x.CompletionId is not null))
        {
            Console.WriteLine("Completed attendances already exists.");
            return;
        }

        Console.WriteLine("Complete attendances.");

        var faker = new Faker
        {
            Random = new Randomizer(App.Seed),
            DateTimeReference = App.DateTimeReference
        };

        var users = await _userDb.Get(new UserQueryOptions { PageOffset = 0, PageSize = 40 });
        var tutors = users.Where(x => x.Permissions.Has(Permissions.Tutor));
        var attendances = await _db.Get(new AttendanceQueryOptions { PageOffset = 0, PageSize = 200 });
        var attendancesToComplete = new Faker().PickRandom(attendances, 100).ToList();
        var signatures = await _signatureDb.Get(new SignatureQueryOptions { PageOffset = 0, PageSize = 10 });


        foreach (var a in attendancesToComplete)
        {
            var completion = new AttendanceCompletionMetadata
            {
                Id = Guid.Empty,
                CompletedAt = faker.Date.Future(1),
                EventId = a.EventId,
                UserId = a.UserId,
                SignerId = faker.PickRandom(tutors).Id,
                SignatureId = faker.PickRandom(signatures).Id.OrNull(faker, 0.25f),
            };

            await _db.Complete(completion);
        }

        Console.WriteLine($"Completed {attendancesToComplete.Count} attendances.");
    }



    public async Task Generate()
    {
        await GenerateAttendaces();
        await CompleteAttendances();
    }
}
