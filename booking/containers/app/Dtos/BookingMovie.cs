namespace Booking.Dtos;

public partial class BookingMovie
{
    public int BookingMovieId { get; set; }

    public int? BookingId { get; set; }

    public int? MovieId { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual Movie? Movie { get; set; }
}
