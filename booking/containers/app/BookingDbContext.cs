using Microsoft.EntityFrameworkCore;

namespace Booking
{
	public class BookingDbContext : DbContext
	{
		public DbSet<Booking> Bookings { get; set; }
		public DbSet<Movie> Movies { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
			=> optionsBuilder.UseNpgsql("Host=192.168.50.100;Database=postgres;Username=postgres;Password=password");
	}

	public class Booking
	{
		public int BookingId { get; set; }
		public Guid BasketId { get; set; }
		public DateTime BookingDate { get; set; }

		public List<Movie> Movies { get; set; }
	}

	public class Movie
	{
		public int MovieId { get; set; }
		public string Title { get; set; }
		public decimal Price { get; set; }
	}
}
