namespace Booking.Messages;

// should exist in shared nuget library, but for simplicity we will keep it here
public class BasketPurchase
{
	public Guid BasketId { get; set; }
	public IReadOnlyList<int> Movies { get; set; } = [];
}