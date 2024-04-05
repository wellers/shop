
namespace Catalog.Models
{
	public sealed class Movie
	{
		public int Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public decimal Price { get; set; } = 0m;
	}
}
