using FizzBuzz.Application.FizzBuzz;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FizzBuzz.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<FizzBuzzOptions>()
            .Bind(configuration.GetSection(FizzBuzzOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IFizzBuzzService, FizzBuzzService>();
        return services;
    }
}
