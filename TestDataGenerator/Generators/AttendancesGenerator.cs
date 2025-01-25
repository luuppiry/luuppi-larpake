
using LarpakeServer.Extensions;
using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels.Metadata;
using LarpakeServer.Services;
using System.Text.Json;

namespace TestDataGenerator.Generators;
internal class AttendancesGenerator : IRunAll
{
    private readonly IAttendanceDatabase _db;
    private readonly IUserDatabase _userDb;
    private readonly ILarpakeEventDatabase _eventDb;
    private readonly ISignatureDatabase _signatureDb;
    private readonly AttendanceKeyService _keyService;

    public AttendancesGenerator(
        IAttendanceDatabase db,
        IUserDatabase userDb,
        ILarpakeEventDatabase eventDb,
        ISignatureDatabase signatureDb,
        AttendanceKeyService keyService)
    {
        _db = db;
        _userDb = userDb;
        _eventDb = eventDb;
        _signatureDb = signatureDb;
        _keyService = keyService;
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
        var events = await _eventDb.GetEvents(new LarpakeEventQueryOptions { PageOffset = 0, PageSize = 40 });

        var attendances = new Faker<Attendance>()
            .UseSeed(App.Seed)
            .RuleFor(a => a.UserId, f => f.PickRandom(users).Id)
            .RuleFor(a => a.LarpakeEventId, f => f.PickRandom(events).Id)
            .Generate(200)
            .DistinctBy(x => new { x.UserId, x.LarpakeEventId })
            .ToArray();

        foreach (var attendance in attendances)
        {
            var key = _keyService.GenerateKey();
            attendance.QrCodeKey = key.QrCodeKey;
            attendance.KeyInvalidAt = key.KeyInvalidAt;
            await _db.RequestAttendanceKey(attendance);
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

        
        var attendancesToComplete = new Faker().PickRandom(attendances, Math.Min(100, attendances.Length)).ToList();
        var signatures = await _signatureDb.Get(new SignatureQueryOptions { PageOffset = 0, PageSize = 10 });


        foreach (var a in attendancesToComplete)
        {
            var completion = new CompletionMetadata
            {
                Id = Guid.Empty,
                CompletedAt = faker.Date.Future(1),
                EventId = a.LarpakeEventId,
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
