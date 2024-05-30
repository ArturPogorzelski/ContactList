using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ContactList.Gateway.Helpers
{
    public class AuthorizationHeaderOperationFilter : IOperationFilter
    {
        // Metoda implementowana z interfejsu IOperationFilter, służąca do modyfikacji definicji operacji w dokumentacji Swagger
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
                Required = true, // Określa, czy parametr jest wymagany
                Schema = new OpenApiSchema // Schemat danych dla parametru
                {
                    Type = "string", // Typ danych parametru
                    Default = new OpenApiString("Bearer ") // Ustawienie domyślnej wartości z prefiksem "Bearer"
                }
            });
        }
    }
}
