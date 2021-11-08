using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Consul;

namespace ChatBot.Common.Consul
{
    public class ConsulServicesRegistry : IConsulServicesRegistry
    {
        private readonly IConsulClient _client;
        private readonly IDictionary<string, ISet<string>> _usedServices = new Dictionary<string, ISet<string>>();

        public ConsulServicesRegistry(IConsulClient client)
        {
            _client = client;
        }

        public async Task<AgentService> GetAsync(string name)
        {
            var allServices = await _client.Agent.Services();
            var services = GetServices(allServices.Response, name);
            if (services.Count == 0)
            {
                return null;
            }
            if (!_usedServices.ContainsKey(name))
            {
                _usedServices[name] = new HashSet<string>();
            }
            else if (services.Count == _usedServices[name].Count)
            {
                _usedServices[name].Clear();
            }

            return GetService(services, name);
        }

        public async Task<WriteResult> RegisterAsync(AgentServiceRegistration agentServiceRegistration)
        {
            return await _client.Agent.ServiceRegister(agentServiceRegistration);
        }


        private AgentService GetService(IList<AgentService> services, string name)
        {
            AgentService service = null;
            var unusedServices = services.Where(s => !_usedServices[name].Contains(s.ID)).ToList();
            if (unusedServices.Count > 0)
            {
                service = unusedServices[RandomNumberGenerator.GetInt32(0, unusedServices.Count)];
            }
            else
            {
                service = services[0];
                _usedServices[name].Clear();
            }
            _usedServices[name].Add(service.ID);

            return service;
        }

        private static IList<AgentService> GetServices(IDictionary<string, AgentService> allServices, string name)
            => allServices?.Where(s => s.Value.Service.Equals(name,
                       StringComparison.InvariantCultureIgnoreCase))
                   .Select(x => x.Value).ToList() ?? new List<AgentService>();
    }
}