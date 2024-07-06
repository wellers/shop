namespace Booking.Messages;

public class BasketPurchase
{
	public Guid BasketId { get; set; }
	public List<int> Movies { get; set; } = [];
}