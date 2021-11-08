using System.Threading.Tasks;

namespace ChatBot.Common.Fabio
{
    public interface IFabioHttpClient
    {
        Task<T> GetAsync<T>(string requestUri);
    }
}