using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ChatBot.Api.Swagger
{
    public class AssignContentTypeFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            foreach (var response in operation.Responses)
            {
                foreach (var content in response.Value.Content)
                {
                    if (content.Key != "application/json")
                    {
                        response.Value.Content.Remove(content.Key);
                    }
                }
            }
            if (operation.RequestBody != null)
            {
                foreach (var request in operation.RequestBody.Content)
                {
                    if (request.Key != "application/json")
                    {
                        operation.RequestBody.Content.Remove(request.Key);
                    }
                }
            }
        }
    }
}