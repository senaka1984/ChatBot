using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Autofac;
using AutoMapper;
using ChatBot.Common.Mvc;
using MediatR;
using ChatBot.Services.WebHook.Services.Interfaces;
using FluentValidation.AspNetCore;
using Serilog;
using Telegram.Bot;

namespace ChatBot.Services.WebHook
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration1 = configuration;
            BotConfig = Configuration1.GetSection("BotConfiguration").Get<BotConfiguration>();
           
        }

        public IConfiguration Configuration1 { get; }
        private BotConfiguration BotConfig { get; }


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

            services.AddControllers()
                  .AddNewtonsoftJson();

            services.AddMvc()
                .AddFluentValidation(fv =>
                {
                    fv.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
                    fv.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
                });
            services.AddAutoMapper(Assembly.GetEntryAssembly());
            services.AddCustomMvc();
            services.AddSingleton(Log.Logger);

            services.AddTransient<ChatBotService>();

            services.AddMediatR(typeof(Startup));      
        }



        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {            
            app.UseRouting();        

            app.UseEndpoints(endpoints =>
            {                
                var token = BotConfig.BotToken;
                endpoints.MapControllerRoute(name: "tgwebhook",
                                             pattern: $"bot/{token}",
                                             new { controller = "WebHook", action = "Post" });
                endpoints.MapControllers();
            });           
            app.UseServiceId();          
        }
    }
}
