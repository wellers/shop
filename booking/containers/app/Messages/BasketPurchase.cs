namespace Booking.Messages;

public class BasketPurchase
{
	public Guid BasketId { get; set; }
	public IReadOnlyList<int> Movies { get; set; } = [];
}