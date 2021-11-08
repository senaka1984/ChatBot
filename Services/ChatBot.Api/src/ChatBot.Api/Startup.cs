using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Autofac;
using AutoMapper;
using ChatBot.Api.Services;
using ChatBot.Api.Swagger;
using ChatBot.Common.Mvc;
using ChatBot.Common.Redis;
using ChatBot.Common.RestEase;
using Telegram.Bot;
using ChatBot.Common.Consul;
using ChatBot.Common.Fabio;

namespace ChatBot.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration1 = configuration;
            BotConfig = Configuration1.GetSection("BotConfiguration").Get<BotConfiguration>();
            BotConfig1 = Configuration1.GetSection("restEase").Get<RestEaseSettings>();
        }

        public IConfiguration Configuration1 { get; }
        private BotConfiguration BotConfig { get; }
        private RestEaseSettings BotConfig1 { get; }

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)          
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

       

        public IConfigurationRoot Configuration { get; }
        public ILifetimeScope AutofacContainer { get; private set; }
        public IContainer Container { get; }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(Assembly.GetEntryAssembly()).AsImplementedInterfaces();
        }

       

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHostedService<ConfigureWebhook>();
            services.AddHttpClient("tgwebhook")
                    .AddTypedClient<ITelegramBotClient>(httpClient
                        => new TelegramBotClient(BotConfig.BotToken, httpClient));
           //services.AddScoped<HandleUpdateService>();

            services.AddControllers()
                  .AddNewtonsoftJson();

            

            services.AddAutoMapper(Assembly.GetEntryAssembly());
            services.AddCustomMvc();
            services.AddHttpContextAccessor();
            services.AddSwaggerDocs();
            services.AddConsul();
            services.AddFabio();
            services.AddRedis();
            //services.AddJwt();
            // services.AddSingleton(Log.Logger);
            services.RegisterServiceForwarder<IWebHookService>("chatbot-webhook-service");
            // services.Configure<RestEaseSettings>(Configuration.GetSection("restEase"));
            //services.Configure<BotConfiguration>(Configuration.GetSection("BotConfiguration"));

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
           // app.UseExceptionHandler(err => err.UseCustomErrors(env));
          //  AutofacContainer = app.ApplicationServices.GetAutofacRoot();
            app.UseRouting();
            // app.UseAuthorization();
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //    endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            //});

            app.UseEndpoints(endpoints =>
            {
                // Configure custom endpoint per Telegram API recommendations:
                // https://core.telegram.org/bots/api#setwebhook
                // If you'd like to make sure that the Webhook request comes from Telegram, we recommend
                // using a secret path in the URL, e.g. https://www.example.com/<token>.
                // Since nobody else knows your bot's token, you can be pretty sure it's us.
                var token = BotConfig.BotToken;
                endpoints.MapControllerRoute(name: "tgwebhook",
                                             pattern: $"bot/{token}",
                                             new { controller = "WebHook", action = "Post" });
                endpoints.MapControllers();
            });

         //   app.UseAllForwardedHeaders();
            app.UseServiceId();
            app.UseSwaggerDocs();
          //  app.UseHealthChecks();
          //  app.UseHealthChecksResponder();
           // app.UseAuthentication();
           // app.UseConsul();
        }
    }
}
