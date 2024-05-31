using System;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Server.Utils
{
	public static class HttpRequestHelper
	{
		public static async Task<T> Get<T>(string url) where T : new()
		{
			var request = new HttpRequestMessage(HttpMethod.Get, url);

			using var httpClient = new HttpClient();
			var response = await httpClient.SendAsync(request).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();

			await using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
			var serializer = new DataContractJsonSerializer(typeof(T));

			T? obj = default;

			try
			{
				obj = (T)serializer.ReadObject(responseStream)!;
			}
			catch (Exception)
			{
				Console.WriteLine($"Unable to parse response from '{url}' into type '{typeof(T)}'");
			}

			return obj;
		}
	}
}