
using System.Runtime.Serialization;

namespace Server.Dtos
{
	[DataContract]
	public class Movie
	{
		[DataMember(Name = "id")]
		public int Id { get; set; }
		
		[DataMember(Name = "title")]
		public string Title { get; set; } = string.Empty;
		
		[DataMember(Name = "price")]
		public decimal Price { get; set; } = 0m;
	}
}
