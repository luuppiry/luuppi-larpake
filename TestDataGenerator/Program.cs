using LarpakeServer.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestDataGenerator;
using TestDataGenerator.Generators;

// Pull configuration



HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);


builder.Services.AddSingleton(builder.Configuration);
builder.Services.AddServices(builder.Configuration);
builder.Services.AddPostgresDatabases(builder.Configuration);
builder.Services.AddHostedService<App>();
builder.Services.AddLogging();

// generators (order matters)
builder.Services.AddTransient<IRunAll, UserGenerator>();
builder.Services.AddTransient<IRunAll, LarpakeGenerator>();
builder.Services.AddTransient<IRunAll, LarpakeEventsGenerator>();
builder.Services.AddTransient<IRunAll, SignaturesGenerator>();
builder.Services.AddTransient<IRunAll, OrganizationEventGenerator>();
builder.Services.AddTransient<IRunAll, FreshmanGroupGenerator>();
builder.Services.AddTransient<IRunAll, AttendancesGenerator>();

using IHost host = builder.Build();
await host.RunAsync();


