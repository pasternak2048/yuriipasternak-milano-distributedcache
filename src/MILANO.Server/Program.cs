using MILANO.Server.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureMilanoServices();

builder.WebHost.ConfigureKestrel(options =>
{
	options.Limits.MaxConcurrentConnections = 100_000;
	options.Limits.MaxConcurrentUpgradedConnections = 10_000;
	options.Limits.MaxRequestBodySize = 1_048_576;
	options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(30);
	options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(15);
	options.AddServerHeader = false;
});

var app = builder.Build();
app.ConfigureMilanoApp();

app.Run();