using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatBot.Common.Redis
{
    public static class Extensions
    {
        private const string SectionName = "redis";

        public static IServiceCollection AddRedis(this IServiceCollection services)
        {
            IConfiguration configuration;
            using (var serviceProvider = services.BuildServiceProvider())
            {
                configuration = serviceProvider.GetService<IConfiguration>();
            }

            services.Configure<RedisOptions>(configuration.GetSection(SectionName));
            var options = configuration.GetOptions<RedisOptions>(SectionName);
            services.AddDistributedRedisCache(o =>
            {
                o.Configuration = options.ConnectionString;
                o.InstanceName = options.Instance;
            });

            return services;
        }
    }
}