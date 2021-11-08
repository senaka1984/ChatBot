using System.Threading.Tasks;
using Consul;

namespace ChatBot.Common.Consul
{
    public interface IConsulServicesRegistry
    {
        Task<AgentService> GetAsync(string name);
        Task<WriteResult> RegisterAsync(AgentServiceRegistration agentServiceRegistration);
    }
}