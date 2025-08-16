using MILANO.Server.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureMilanoServices();

var app = builder.Build();
app.ConfigureMilanoApp();

app.Run();