using System.Runtime.Serialization;

namespace Catalog.Jobs.Dtos
{
	[DataContract]
	public record Movie
	{
		[DataMember(Name = "movieId")]
		public int MovieId { get; set; }

		[DataMember(Name = "title")]
		public string? Title { get; set; }

		[DataMember(Name = "price")]
		public decimal? Price { get; set; }
	}
}
