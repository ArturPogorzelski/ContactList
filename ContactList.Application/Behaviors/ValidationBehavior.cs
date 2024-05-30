using ContactList.Application.Interfaces;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Application.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
     where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Walidacja tylko dla komend, pomijamy zapytania
            if (request is not IBaseCommand) return await next();

            var context = new ValidationContext<TRequest>(request);

            // Równoległa walidacja wszystkich walidatorów dla danego typu żądania
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            // Zbieranie wszystkich błędów walidacji
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            // Jeśli są błędy, rzucamy wyjątek ValidationException
            if (failures.Count != 0)
                throw new ValidationException(failures);

            // Jeśli walidacja przebiegła pomyślnie, przechodzimy do następnego handlera w pipeline
            return await next();
        }
    }
}
