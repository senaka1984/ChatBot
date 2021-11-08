using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ChatBot.Api.Swagger
{
    public class AssignPropertyRequiredFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            _ = context.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in schema.Properties)
            {
                AddPropertyToRequired(schema, property.Key);
            }
        }

        private static void AddPropertyToRequired(OpenApiSchema schema, string propertyName)
        {
            if (schema.Required == null)
            {
                schema.Required = new HashSet<string>();
            }

            if (!schema.Required.Contains(propertyName))
            {
                schema.Required.Add(propertyName);
            }
        }
    }
}