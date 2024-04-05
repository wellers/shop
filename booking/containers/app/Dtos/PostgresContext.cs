using Microsoft.EntityFrameworkCore;

namespace Booking.Dtos;

public partial class PostgresContext : DbContext
{
	private readonly IConfiguration _configuration;

	public PostgresContext(IConfiguration configuration)
    {
		this._configuration = configuration;
	}

    public PostgresContext(DbContextOptions<PostgresContext> options, IConfiguration configuration) : base(options)
    {
		this._configuration = configuration;
	}

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingMovie> BookingMovies { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseNpgsql(_configuration.GetValue<string>("PostgresConnection"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("bookings_pkey");

            entity.ToTable("bookings");

            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.BasketId)
                .HasMaxLength(255)
                .HasColumnName("basket_id");
            entity.Property(e => e.BookingDate).HasColumnName("booking_date");
        });

        modelBuilder.Entity<BookingMovie>(entity =>
        {
            entity.HasKey(e => e.BookingMovieId).HasName("booking_movies_pkey");

            entity.ToTable("booking_movies");

            entity.Property(e => e.BookingMovieId).HasColumnName("booking_movie_id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.MovieId).HasColumnName("movie_id");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingMovies)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("booking_id");

            entity.HasOne(d => d.Movie).WithMany(p => p.BookingMovies)
                .HasForeignKey(d => d.MovieId)
                .HasConstraintName("movie_id");
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.MovieId).HasName("movies_pkey");

            entity.ToTable("movies");

            entity.Property(e => e.MovieId).HasColumnName("movie_id");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
