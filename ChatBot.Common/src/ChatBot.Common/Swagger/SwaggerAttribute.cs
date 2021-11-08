using System;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Models;

namespace ChatBot.Common.Swagger
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SwaggerExcludeAttribute : Attribute
    {
    }

    public class SwaggerExcludeSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema?.Properties == null)
            {
                return;
            }

            var excludedProperties =
                context.Type.GetProperties().Where(
                    t => t.GetCustomAttribute<SwaggerExcludeAttribute>() != null);

            foreach (var excludedProperty in excludedProperties)
            {
                var propertyToRemove =
                    schema.Properties.Keys.SingleOrDefault(
                        x => string.Equals(x, excludedProperty.Name, StringComparison.OrdinalIgnoreCase));

                if (propertyToRemove != null)
                {
                    schema.Properties.Remove(propertyToRemove);
                }
            }
        }
    }
}
