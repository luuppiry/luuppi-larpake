using LarpakeServer.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestDataGenerator;
using TestDataGenerator.Generators;


HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration["ConnectionStrings:Sqlite"] = @"Data Source='C:\Users\henri\Documents\SqliteDatabases\Testing\Larpake.db';";
builder.Configuration["Signature:PointLimit"] = "1000";

builder.Services.AddSingleton(builder.Configuration);
builder.Services.AddServices(builder.Configuration);
builder.Services.AddSqliteDatabases(builder.Configuration);
builder.Services.AddLogging();
builder.Services.AddHostedService<App>();

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


