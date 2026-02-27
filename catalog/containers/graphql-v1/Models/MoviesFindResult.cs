namespace Catalog.Models
{	
	public class MoviesFindResult
	{
		public bool Success { get; set; }
		public string Message { get; set; } = string.Empty;
		public IReadOnlyList<Movie> Docs { get; set; } = [];
	}
}
