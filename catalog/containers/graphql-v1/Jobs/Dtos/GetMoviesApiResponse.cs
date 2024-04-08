using System.Runtime.Serialization;

namespace Catalog.Jobs.Dtos
{
	[DataContract]
	public class GetMoviesApiResponse
	{
		[DataMember(Name = "success")]
		public bool Success { get; set; }

		[DataMember(Name = "movies")]
		public List<Movie> Movies { get; set; } = [];
	}
}
