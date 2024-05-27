using Ocelot.DependencyInjection;
using Ocelot.Provider.Polly;

namespace ContactList.Gateway.Extansion
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddOcelotWithPolly(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOcelot(configuration)
                .AddPolly();

            return services;
        }
    }
}
