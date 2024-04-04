namespace Booking.Dtos;

public partial class Booking
{
    public int BookingId { get; set; }

    public string? BasketId { get; set; }

    public DateTime? BookingDate { get; set; }

    public virtual ICollection<BookingMovie> BookingMovies { get; set; } = [];
}
