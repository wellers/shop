namespace Booking.Dtos;

public partial class Movie
{
    public int MovieId { get; set; }

    public string? Title { get; set; }

    public decimal? Price { get; set; }

    public virtual ICollection<BookingMovie> BookingMovies { get; set; } = [];
}
