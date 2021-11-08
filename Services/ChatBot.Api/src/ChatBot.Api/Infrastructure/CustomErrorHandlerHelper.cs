using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace ChatBot.Services.Api.ErrorHandler
{
    public static class CustomErrorHandlerHelper
    {
        public static void UseCustomErrors(this IApplicationBuilder app, IHostEnvironment environment)
        {
            if (environment.IsEnvironment("hcloud") || environment.IsEnvironment("hcloud-prd"))
            {
                app.Use(WriteProductionResponse);
            }
            else
            {
                app.Use(WriteDevelopmentResponse);
            }
        }

        private static Task WriteProductionResponse(HttpContext httpContext, Func<Task> next)
            => WriteResponse(httpContext, includeDetails: false);

        private static Task WriteDevelopmentResponse(HttpContext httpContext, Func<Task> next)
            => WriteResponse(httpContext, includeDetails: true);

        private static async Task WriteResponse(HttpContext httpContext, bool includeDetails)
        {
            // Try and retrieve the error from the ExceptionHandler middleware
            var exceptionDetails = httpContext.Features.Get<IExceptionHandlerFeature>();
            var ex = exceptionDetails?.Error;

            // Should always exist, but best to be safe!
            if (ex != null)
            {
                // ProblemDetails has it's own content type
                httpContext.Response.ContentType = "application/problem+json";

                // Get the details to display, depending on whether we want to expose the raw exception
                var title = ex.Message;
                var details = includeDetails ? ex.ToString() : null;

                var problem = new ProblemDetails
                {
                    Status = 200,
                    Title = title,
                    Detail = details
                };

                // This is often very handy information for tracing the specific request
                var traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;
                if (traceId != null)
                {
                    problem.Extensions["traceId"] = traceId;
                }
                httpContext.Response.ContentType = "application/json";
                //Serialize the problem details object to the Response as JSON (using System.Text.Json)
                var stream = httpContext.Response.Body;
                await JsonSerializer.SerializeAsync(stream, problem);
            }
        }
    }
}