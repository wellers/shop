using Catalog.Database;
using Catalog.Jobs;
using Catalog.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile($"appsettings.json").Build();

builder.Services
	.AddDbContext<MongoContext>()
	.AddGraphQLServer()
	.AddQueryType<Query>()
	.AddMutationType<Mutation>();

builder.Services.AddScheduler(config =>
{
	var jobOptions = new BackgroundJobOptions();
	builder.Configuration.GetSection("SyncBookableMoviesJobOptions").Bind(jobOptions);

	config.AddJob(
		provider => new SyncBookableMovies
		(
			provider,
			builder.Configuration,
			jobOptions
		),
		configure: options =>
		{
			options.CronSchedule = jobOptions.CronPattern;
		},
		jobName: jobOptions.Name);
});

var app = builder.Build();

app.MapGraphQL();

app.MapGet("/status", () => Results.Json(new { start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() }));

app.MapGet("/sync", async (IServiceProvider serviceProvider) =>
{
	using var scope = app.Services.CreateScope();

	var jobOptions = new BackgroundJobOptions();
	builder.Configuration.GetSection("SyncBookableMoviesJobOptions").Bind(jobOptions);

	var job = new SyncBookableMovies
	(
		serviceProvider,
		builder.Configuration,
		jobOptions
	);

	await job.ExecuteAsync(new CancellationToken());

	return Results.Json(new { success = true, message = "Job complete." });
});

app.MapGet("/", () => "🚀 Server ready");

app.Run();