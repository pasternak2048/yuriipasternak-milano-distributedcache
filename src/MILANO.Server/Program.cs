using MILANO.Server.Web.Extensions;
using MILANO.Server.Web.Grpc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMilanoDistributedCache(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseRouting();
app.UseMilanoMiddleware();

app.MapMilanoEndpoints();

app.MapGrpcService<CacheGrpcService>();

app.Run();