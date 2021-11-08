using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ChatBot.Common.Fabio
{
    public class FabioHttpClient : IFabioHttpClient
    {
        private readonly HttpClient _client;

        public FabioHttpClient(HttpClient client)
        {
            _client = client;
        }
        public async Task<T> GetAsync<T>(string requestUri)
        {
            var uri = requestUri.StartsWith("http://") ? requestUri : $"http://{requestUri}";
            return await _client.GetFromJsonAsync<T>(uri);
        }
    }
}