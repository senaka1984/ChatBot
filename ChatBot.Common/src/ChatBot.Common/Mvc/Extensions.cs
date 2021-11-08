using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChatBot.Common.Mvc
{
    public static class Extensions
    {
        // private const string SQLConfigSectionName = "sql";
        // private const string RabbitMQConfigSectionName = "EventBusConnection";
        public static IMvcCoreBuilder AddCustomMvc(this IServiceCollection services)
        {

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var configuration = serviceProvider.GetService<IConfiguration>();
                services.Configure<AppOptions>(configuration.GetSection("app"));
                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                services.AddSingleton<IServiceId, ServiceId>();
                services.AddHealthChecks().AddCheck("self", () => HealthCheckResult.Healthy());
                //services.UseRabbitMQHealthChecks(configuration);
                //services.UseSQLHealthChecks(configuration);
                services.AddHealthChecksUI(setupSettings =>
                {

                    var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS")?.Split(';');
                    if (urls != null)
                    {
                        var uris = urls.Select(url => Regex.Replace(url, @"^(?<scheme>https?):\/\/((\+)|(\*)|(0.0.0.0))(?=[\:\/]|$)", "${scheme}://localhost"))
                                    .Select(uri => new Uri(uri, UriKind.Absolute)).ToArray();
                        var httpEndpoint = uris.FirstOrDefault(uri => uri.Scheme == "http");
                        var httpsEndpoint = uris.FirstOrDefault(uri => uri.Scheme == "https");
                        if (httpEndpoint != null) // Create an HTTP healthcheck endpoint
                        {
                            setupSettings.AddHealthCheckEndpoint("HTTP", new UriBuilder(httpEndpoint.Scheme, httpEndpoint.Host, httpEndpoint.Port, "/healthz").ToString());
                        }
                        if (httpsEndpoint != null) // Create an HTTPS healthcheck endpoint
                        {
                            setupSettings.AddHealthCheckEndpoint("HTTPS", new UriBuilder(httpEndpoint.Scheme, httpEndpoint.Host, httpEndpoint.Port, "/healthz").ToString());
                        }
                    }
                })
                .AddInMemoryStorage();
                // services.AddHealthChecksUI(opt =>
                //     {
                //         opt.SetEvaluationTimeInSeconds(15); //time in seconds between check
                //         opt.MaximumHistoryEntriesPerEndpoint(60); //maximum history of checks
                //         opt.SetApiMaxActiveRequests(1); //api requests concurrency
                //         opt.AddHealthCheckEndpoint("default api", "/healthz"); //map health check api
                //     })
                //     .AddInMemoryStorage();
                services.AddControllers();
            }
            return services
                .AddMvcCore()
                .ConfigureApiBehaviorOptions(options =>
                {
#if DEBUG
                    //for CWE-209: Generation of Error Message Containing Sensitive Information
                    //see: https://cwe.mitre.org/data/definitions/209.html

                    options.SuppressConsumesConstraintForFormFileParameters = true;
                    //options.SuppressInferBindingSourcesForParameters = true;
                    options.SuppressModelStateInvalidFilter = true;
                    options.SuppressMapClientErrors = true;
#endif
                })
                .AddDefaultJsonOptions();
        }

        public static IApplicationBuilder UseServiceId(this IApplicationBuilder builder)
        {
            var _builder = builder.Map("/id", c => c.Run(async ctx =>
            {
                using var scope = c.ApplicationServices.CreateScope();
                var id = scope.ServiceProvider.GetService<IServiceId>().Id;
                await ctx.Response.WriteAsync(id);
            }));
            return _builder;
        }

        public static IMvcCoreBuilder AddDefaultJsonOptions(this IMvcCoreBuilder builder) =>
            builder.AddNewtonsoftJson(o =>
            {
                o.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                o.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                o.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
                o.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
                o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                o.SerializerSettings.Formatting = Formatting.Indented;
                o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                o.SerializerSettings.Converters.Add(new StringEnumConverter());
            });

        public static IApplicationBuilder UseHealthChecks(this IApplicationBuilder builder) => builder.UseHealthChecks("/health");

        public static IApplicationBuilder UseHealthChecksResponder(this IApplicationBuilder builder)
            => builder.UseEndpoints(endpoints =>
             {
                 endpoints.MapHealthChecks("/healthz", new HealthCheckOptions
                 {
                     Predicate = _ => true,
                     ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                 });
                 endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
                 {
                     Predicate = r => r.Name.Contains("self")
                 });
                 endpoints.MapHealthChecksUI();
                 endpoints.MapDefaultControllerRoute();
             });

        public static IApplicationBuilder UseAllForwardedHeaders(this IApplicationBuilder builder)
            => builder.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });

        // public static IServiceCollection UseRabbitMQHealthChecks(this IServiceCollection services, IConfiguration configuration)
        // {
        //     var eventBusConnectionstring = configuration["EventBusConnection"];
        //     if (!string.IsNullOrEmpty(eventBusConnectionstring))
        //     {
        //         services.AddHealthChecks()
        //                     .AddRabbitMQ("amqp://" + eventBusConnectionstring, sslOption: null);
        //     }

        //     return services;
        // }

        // public static IServiceCollection UseSQLHealthChecks(this IServiceCollection services, IConfiguration configuration)
        // {
        //     //get DB connection string
        //     var section = configuration.GetSection(SQLConfigSectionName);
        //     var dbOptions = section.GetOptions<SqlDbOptions>(configuration.GetOptions<AppOptions>("app").Name);
        //     dbOptions.ConnectionString = GetEnvironmentVariableValue("DB_CONNECTIONSTRING") ?? dbOptions.ConnectionString;

        //     if(string.IsNullOrEmpty(dbOptions.ConnectionString)) {
        //         services.AddHealthChecks();
        //     } else {
        //         services.AddHealthChecks().AddSqlServer(dbOptions.ConnectionString);
        //     }
        //     return services;
        // }
    }
}