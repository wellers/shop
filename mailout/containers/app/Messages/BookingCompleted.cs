namespace Mailout.Messages;

// should exist in shared nuget library, but for simplicity we will keep it here
public class BookingCompleted
{
	public Guid BasketId { get; set; }
	public DateTime CompletedAt { get; set; }
}