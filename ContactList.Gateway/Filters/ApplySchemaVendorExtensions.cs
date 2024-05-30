using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ContactList.Gateway.Filters
{
    // Filtr do dodawania dodatkowych metadanych do schematów
    public class ApplySchemaVendorExtensions : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            schema.Extensions.Add("X-company", new OpenApiString("YourCompany"));
        }
    }
}
