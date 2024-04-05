namespace Catalog.Models
{	
	public class MoviesFindResult
	{
		public bool Success { get; set; }
		public string Message { get; set; } = string.Empty;
		public List<Movie> Docs { get; set; } = new List<Movie>();
	}
}
