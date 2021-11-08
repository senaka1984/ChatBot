
using System;
using Consul;
using ChatBot.Common.Fabio;
using ChatBot.Common.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ChatBot.Common.Consul
{
    public static class Extensions
    {
        private const string ConsulSectionName = "consul";
        private const string FabioSectionName = "fabio";

        public static IServiceCollection AddConsul(this IServiceCollection services)
        {
            IConfiguration configuration;
            using (var serviceProvider = services.BuildServiceProvider())
            {
                configuration = serviceProvider.GetService<IConfiguration>();
            }

            var options = configuration.GetOptions<ConsulOptions>(ConsulSectionName);
            services.Configure<ConsulOptions>(configuration.GetSection(ConsulSectionName));
            services.Configure<FabioOptions>(configuration.GetSection(FabioSectionName));
            services.AddTransient<IConsulServicesRegistry, ConsulServicesRegistry>();
            services.AddTransient<ConsulServiceDiscoveryMessageHandler>();
            services.AddHttpClient<IConsulHttpClient, ConsulHttpClient>()
                .AddHttpMessageHandler<ConsulServiceDiscoveryMessageHandler>();

            return services.AddSingleton<IConsulClient>(_ => new ConsulClient(cfg =>
            {
                if (!string.IsNullOrEmpty(options.Url))
                {
                    cfg.Address = new Uri(options.Url);
                }
            }));
        }


        public static string UseConsul(this IApplicationBuilder app)
        {
            return UseConsul(app, false);
        }

        /// <summary>
        /// Registering Grpc endpoint to Consul
        /// This method will retrieve ConsulOptions in appsettings.json
        /// </summary>
        /// <remarks>
        /// <paramref name="GrpcPort"/> needs to be present and will be used to configure Consul and Fabio with "proto=grpc".
        /// <paramref name="Address"/> As per the HTTP/2 spec, the host header is not required, so host matching is not supported for GRPC proxying.
        /// See <see>https://fabiolb.net/feature/grpc-proxy/</see>
        /// </remarks>

        public static string UseConsulWithGrpc(this IApplicationBuilder app)
        {
            return UseConsul(app, true);
        }
        private static string UseConsul(this IApplicationBuilder app, bool useGrpc)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var consulOptions = scope.ServiceProvider.GetService<IOptions<ConsulOptions>>();
            var fabioOptions = scope.ServiceProvider.GetService<IOptions<FabioOptions>>();
            var enabled = consulOptions.Value.Enabled;
            var consulEnabled = Environment.GetEnvironmentVariable("CONSUL_ENABLED")?.ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(consulEnabled))
            {
                enabled = consulEnabled == "true" || consulEnabled == "1";
            }
            if (!enabled)
            {
                return string.Empty;
            }

            var address = consulOptions.Value.Address;
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentException("Consul address can not be empty: {0}", consulOptions.Value.PingEndpoint);
            }
            var uniqueId = scope.ServiceProvider.GetService<IServiceId>().Id;
            var client = scope.ServiceProvider.GetService<IConsulClient>();
            // var serviceName = useGrpc ? consulOptions.Value.Service + "-grpc" : consulOptions.Value.Service;
            // var serviceId = useGrpc ? $"{serviceName}:{uniqueId}-grpc" : $"{serviceName}:{uniqueId}"
            var serviceName = consulOptions.Value.Service;
            var serviceId = $"{serviceName}:{uniqueId}";
            var port = consulOptions.Value.Port;
            var pingEndpoint = string.IsNullOrEmpty(consulOptions.Value.PingEndpoint) ? "health" : consulOptions.Value.PingEndpoint;
            var pingInterval = consulOptions.Value.PingInterval <= 0 ? 5 : consulOptions.Value.PingInterval;
            var removeAfterInterval = consulOptions.Value.RemoveAfterInterval <= 0 ? 10 : consulOptions.Value.RemoveAfterInterval;

            var restServiceRegistration = new AgentServiceRegistration
            {
                Name = serviceName,
                ID = serviceId,
                Address = address,
                Port = useGrpc ? consulOptions.Value.GrpcPort: consulOptions.Value.Port,
                Tags = fabioOptions.Value.Enabled ? GetFabioTags(serviceName, useGrpc) : null
            };

            if (consulOptions.Value.PingEnabled || fabioOptions.Value.Enabled)
            {
                var scheme = address.StartsWith("http", StringComparison.InvariantCultureIgnoreCase)
                    ? string.Empty
                    : "http://";
                var check = new AgentServiceCheck
                {
                    Interval = TimeSpan.FromSeconds(pingInterval),
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(removeAfterInterval)
                };
                if (useGrpc)
                {
                    check.GRPC = $"{address}{(consulOptions.Value.GrpcPort > 0 ? $":{consulOptions.Value.GrpcPort}" : string.Empty)}";
                    check.GRPCUseTLS = false;
                }
                else
                {
                    check.HTTP = $"{scheme}{address}{(port > 0 ? $":{port}" : string.Empty)}/{pingEndpoint}";
                }
                restServiceRegistration.Check = check;
            }
            client.Agent.ServiceRegister(restServiceRegistration);

            //see: https://github.com/dotnet/extensions/issues/2827#issuecomment-609109614
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                client.Agent.ServiceDeregister(serviceId).Wait();
            };
            return serviceId;
        }

        private static string[] GetFabioTags(string service, bool useGrpc)
        {
            if(useGrpc)
            {
                return new[] { $"urlprefix-/{service} strip=/{service} proto=grpc" };
            } else {
                return new[] { $"urlprefix-/{service} strip=/{service}" };
            }
        }
    }
}