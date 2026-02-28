namespace Basket.Messages;

public class BookingCompleted
{
	public Guid BasketId { get; set; }
	public DateTime CompletedAt { get; set; }
}