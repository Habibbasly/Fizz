using Microsoft.Extensions.DependencyInjection;

namespace FizzBuzz.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Point d'enregistrement des implémentations techniques (persistance, services externes...).
    /// Vide pour l'instant : le calcul FizzBuzz ne dépend d'aucune ressource externe.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        return services;
    }
}
