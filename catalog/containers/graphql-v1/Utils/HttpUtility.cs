using System.Runtime.Serialization.Json;

namespace Catalog.Utils
{
	public sealed class HttpRequestHelper
	{
		public static async Task<T> Get<T>(string url) where T : new()
		{
			var request = new HttpRequestMessage(HttpMethod.Get, url);

			HttpResponseMessage response;
			using var httpClient = new HttpClient();
			response = await httpClient.SendAsync(request).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();

			using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
			var serializer = new DataContractJsonSerializer(typeof(T));

			T? obj = default;

			try
			{
				obj = (T)serializer.ReadObject(responseStream);
			}
			catch (Exception)
			{
				Console.WriteLine($"Unable to parse response from '{url}' into type '{typeof(T)}'");
			}

			return obj;
		}
	}
}