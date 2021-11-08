using System.Threading.Tasks;

namespace ChatBot.Common.Consul
{
    public interface IConsulHttpClient
    {
        Task<T> GetAsync<T>(string requestUri);
    }
}