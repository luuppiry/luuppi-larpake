using Microsoft.Extensions.Hosting;

namespace TestDataGenerator;
internal class App : IHostedService
{
    public const int Seed = 69;
    public static readonly DateTime DateTimeReference = new(2025, 1, 1);


    private readonly IEnumerable<IRunAll> _generators;

    public App(IEnumerable<IRunAll> generators)
    {
        _generators = generators;   
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var gen in _generators)
        {
            await gen.Generate();
        }
    }


   


    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
