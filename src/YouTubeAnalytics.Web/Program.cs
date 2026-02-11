var builder = WebApplication.CreateBuilder(args);

// TODO: Add Serilog configuration
// TODO: Add DI registration via DependencyInjection.AddInfrastructure()
// TODO: Add Application layer service registrations

var app = builder.Build();

app.MapGet("/", () => "YouTube Analytics Tool");

app.Run();
