namespace Catalog.Models
{
	public class MovieInsertInput
	{
		public string Title { get; set; } = string.Empty;
		public decimal Price { get; set; } = 0m;
	}	
}
