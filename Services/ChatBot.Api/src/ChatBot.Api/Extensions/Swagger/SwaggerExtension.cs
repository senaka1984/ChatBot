using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using ChatBot.Common;
using ChatBot.Common.Swagger;

namespace ChatBot.Api.Swagger
{
    public static class Extensions
    {
        public static IServiceCollection AddSwaggerDocs(this IServiceCollection services)
        {
            SwaggerOptions options;
            using (var serviceProvider = services.BuildServiceProvider())
            {
                var configuration = serviceProvider.GetService<IConfiguration>();
                services.Configure<SwaggerOptions>(configuration.GetSection("swagger"));
                options = configuration.GetOptions<SwaggerOptions>("swagger");
            }
            if (!options.Enabled)
            {
                return services;
            }
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(options.Name, new OpenApiInfo { Title = options.Title, Version = options.Version });
                c.SchemaFilter<SwaggerExcludeSchemaFilter>();
                if (options.IncludeSecurity)
                {
                    var security = new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                            },
                            new[] { "readAccess", "writeAccess" }
                        }
                    };
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey
                    });
                    c.AddSecurityRequirement(security);
                    c.CustomOperationIds(e => $"{e.ActionDescriptor.RouteValues["action"]}");
                    c.OperationFilter<AssignContentTypeFilter>();
                    c.SchemaFilter<AssignPropertyRequiredFilter>();
                    c.UseAllOfForInheritance();
                }
            });
            return services.AddSwaggerGenNewtonsoftSupport();
        }

        public static IApplicationBuilder UseSwaggerDocs(this IApplicationBuilder builder)
        {
            var options = builder.ApplicationServices.GetService<IConfiguration>()
                .GetOptions<SwaggerOptions>("swagger");
            if (!options.Enabled)
            {
                return builder;
            }

            var routePrefix = string.IsNullOrWhiteSpace(options.RoutePrefix) ? "swagger" : options.RoutePrefix;

            builder.UseStaticFiles()
                .UseSwagger(c => c.RouteTemplate = routePrefix + "/{documentName}/swagger.json");

            return builder.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"/{routePrefix}/{options.Name}/swagger.json", options.Title);
                    c.RoutePrefix = routePrefix;
                });
        }
    }
}