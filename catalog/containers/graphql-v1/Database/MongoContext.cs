using Catalog.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Catalog.Database
{
	public class MongoContext(IConfiguration configuration) : DbContext
	{
		public DbSet<Movie> Movies { get; init; }

		private readonly string? _connectionString = configuration.GetValue<string>("MongoConnection");
		private readonly string? _databaseName = configuration.GetValue<string>("DatabaseName");

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseMongoDB(_connectionString, _databaseName);

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.Entity<Movie>().ToCollection("movies");
		}
	}
}
