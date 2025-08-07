using MILANO.DistributedCache.Server.Web.Extensions;
using MILANO.DistributedCache.Server.Web.Grpc;

var builder = WebApplication.CreateBuilder(args);

// Add core services, options, and DI bindings
builder.Services.AddMilanoDistributedCache(builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Grcp
builder.Services.AddGrpc();

var app = builder.Build();

// Swagger UI (dev only)
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// Middleware: API Key auth per route
app.UseRouting();
app.UseMilanoMiddleware();

// Endpoints: controllers + system endpoints
app.MapMilanoEndpoints();

app.MapGrpcService<CacheGrpcService>();

app.Run();