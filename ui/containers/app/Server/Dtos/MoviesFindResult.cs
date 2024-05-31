using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Server.Dtos
{
	[DataContract]
	public class ResponseType
	{
		[DataMember(Name = "find_movies")]
		public MoviesFindResult FindMovies { get; set; }
	}
	
	[DataContract]
	public class MoviesFindResult
	{
		[DataMember(Name = "success")]
		public bool Success { get; set; }
		
		[DataMember(Name = "message")]
		public string Message { get; set; } = string.Empty;
		
		[DataMember(Name = "docs")]
		public List<Movie> Docs { get; set; } = [];
	}
}
