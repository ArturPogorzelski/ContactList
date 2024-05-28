using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ContactList.API.Helpers
{
    public class AddRequiredHeaderParameter : IOperationFilter
    {
        // Metoda implementowana z interfejsu IOperationFilter, która modyfikuje definicje operacji Swaggera
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Jeśli lista parametrów operacji jest pusta, inicjalizuje nową listę
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            // Dodanie nowego parametru do nagłówka każdej operacji
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Authorization", // Nazwa parametru
                In = ParameterLocation.Header, // Lokalizacja parametru, w tym przypadku w nagłówku
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"", // Opis parametru
                Required = true // Określa, czy parametr jest wymagany; ustaw na false, jeśli autoryzacja nie jest wymagana dla wszystkich endpointów
            });
        }
    }
}
