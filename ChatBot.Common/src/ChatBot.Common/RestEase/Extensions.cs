using System;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestEase;
using ProtoBuf.Grpc.ClientFactory;
using ChatBot.Common.Fabio;
using Microsoft.Extensions.Options;
using ChatBot.Common.Consul;
using Consul;

namespace ChatBot.Common.RestEase
{
    public static class Extensions
    {
        public static void RegisterServiceForwarder<T>(this IServiceCollection services, string serviceName) where T : class
        {
            var options = ConfigureOptions(services);
            ConfigureClient<T>(services, serviceName, options);
        }

        public static void RegisterGrpcServiceToConsul<T>(this IServiceCollection services, string clientName) where T : class
        {
            ////todo: service locator anti-pattern?
            //IConsulServicesRegistry consulRegistry;
            //IOptions<ConsulOptions> consulOptions;
            //using var serviceProvider = services.BuildServiceProvider();
            //consulRegistry = serviceProvider.GetService<IConsulServicesRegistry>();
            //consulOptions = serviceProvider.GetService<IOptions<ConsulOptions>>();
            //var restServiceRegistration = new AgentServiceRegistration
            //{
            //    Name = clientName,
            //    ID = $"{Guid.NewGuid():N}",
            //    Address = consulOptions.Value.Address,
            //    Port = consulOptions.Value.GrpcPort,
            //    Tags = new[] { $"urlprefix-/{clientName} strip=/{clientName} proto=grpc" }
            //};

            ////todo: very ugly way to register, refactor
            //var check = new AgentServiceCheck
            //{
            //    Interval = TimeSpan.FromSeconds(consulOptions.Value.PingInterval <= 0 ? 5 : consulOptions.Value.PingInterval),
            //    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(consulOptions.Value.RemoveAfterInterval <= 0 ? 10 : consulOptions.Value.RemoveAfterInterval),
            //    GRPC = $"{consulOptions.Value.Address}{(consulOptions.Value.GrpcPort > 0 ? $":{consulOptions.Value.GrpcPort}" : string.Empty)}",
            //    GRPCUseTLS = false
            //};

            //restServiceRegistration.Check = check;

            //var result = consulRegistry.RegisterAsync(restServiceRegistration).Result;
            ////todo: handle failure of registration
            ////trying to use service discovery for registered service's address & port
        }

        private static RestEaseOptions ConfigureOptions(IServiceCollection services)
        {
            IConfiguration configuration;
            using (var serviceProvider = services.BuildServiceProvider())
            {
                configuration = serviceProvider.GetService<IConfiguration>();
            }

            services.Configure<RestEaseOptions>(configuration.GetSection("restEase"));
            return configuration.GetOptions<RestEaseOptions>("restEase");
        }

        private static void ConfigureClient<T>(IServiceCollection services, string clientName, RestEaseOptions options) where T : class
        {
            var svc = options.Services.SingleOrDefault(svc => svc.Name.Equals(clientName, StringComparison.InvariantCultureIgnoreCase));
            switch (svc.LoadBalancer?.ToLowerInvariant())
            {
                case "fabio":
                    services.AddHttpClient(clientName)
                        .AddHttpMessageHandler(c =>
                            new FabioMessageHandler(c.GetService<IOptions<FabioOptions>>(), svc.Name));
                    services.AddTransient<T>(c =>
                        new RestClient(c.GetService<IHttpClientFactory>().CreateClient(svc.Name)).For<T>()
                    );
                    break;
                case "consul":

                    services.AddHttpClient(clientName)
                        .AddHttpMessageHandler(c =>
                            new ConsulServiceDiscoveryMessageHandler(c.GetService<IConsulServicesRegistry>(),
                                c.GetService<IOptions<ConsulOptions>>(), svc.Name, overrideRequestUri: true));
                    services.AddTransient<T>(c =>
                        new RestClient(c.GetService<IHttpClientFactory>().CreateClient(svc.Name)).For<T>()
                    );
                    break;
                //case "grpc":
                //    //todo: service locator anti-pattern?
                //    IConsulServicesRegistry consulRegistry;
                //    IOptions<ConsulOptions> consulOptions;
                //    AgentService agentService;
                //    using (var serviceProvider = services.BuildServiceProvider())
                //    {
                //        consulRegistry = serviceProvider.GetService<IConsulServicesRegistry>();
                //        consulOptions = serviceProvider.GetService<IOptions<ConsulOptions>>();
                //        agentService = consulRegistry.GetAsync(svc.Name).Result;
                //    }

                //    services.AddCodeFirstGrpcClient<T>(grpcOpt =>
                //    {
                //        var scheme = agentService.Address.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) ? string.Empty : "http://";
                //        // for local env, we need to change internal docker host to localhost, since host can't call into docker dns
                //        grpcOpt.Address = new Uri(scheme + agentService.Address
                //                                            .Replace("docker.for.mac.localhost", "localhost")
                //                                            .Replace("docker.for.win.localhost", "localhost")
                //                                            .Replace("host.docker.internal", "localhost")
                //                            + ":" + agentService.Port);
                //    });
                //    //todo: register grpc service
                //    // services.AddTransient<T>(c =>
                //    //     new RestClient(c.GetService<IHttpClientFactory>().CreateClient(service.Name)).For<T>()
                //    // );
                //    break;
                default:
                    var service = options.Services.SingleOrDefault(s => s.Name.Equals(svc.Name,
                        StringComparison.InvariantCultureIgnoreCase));
                    services.AddHttpClient(clientName, client =>
                    {
                        if (service == null)
                        {
                            throw new RestEaseServiceNotFoundException($"RestEase service: '{svc.Name}' was not found.", svc.Name);
                        }

                        client.BaseAddress = new UriBuilder
                        {
                            Scheme = service.Scheme,
                            Host = service.Host,
                            Port = service.Port
                        }.Uri;
                    });
                    services.AddTransient<T>(c =>
                        new RestClient(c.GetService<IHttpClientFactory>().CreateClient(service.Name)).For<T>()
                    );
                    break;
            }
        }
    }
}
