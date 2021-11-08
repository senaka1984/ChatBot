using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http.Json;

namespace ChatBot.Common.Consul
{
    public class ConsulHttpClient : IConsulHttpClient
    {
        private readonly HttpClient _client;

        public ConsulHttpClient(HttpClient client)
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