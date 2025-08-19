using MILANO.Server.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureMilanoHost();

builder.ConfigureMilanoServices();

var app = builder.Build();

app.ConfigureMilanoApp();

app.Run();