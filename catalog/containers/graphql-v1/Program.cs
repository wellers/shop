using Catalog.Database;
using Catalog.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile($"appsettings.json").Build();

builder.Services
	.AddDbContext<MongoContext>();

builder.Services
	.AddGraphQLServer()
	.AddQueryType<Query>()
	.AddMutationType<Mutation>();

var app = builder.Build();

app.MapGraphQL();

app.MapGet("/status", () => Results.Json(new { start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() }));

app.MapGet("/", () => "🚀 Server ready");

app.Run();