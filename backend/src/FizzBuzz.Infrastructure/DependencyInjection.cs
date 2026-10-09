using Microsoft.Extensions.DependencyInjection;

namespace FizzBuzz.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Point d'enregistrement des implémentations techniques (persistance, services externes...).
    /// Vide pour l'instant : le calcul FizzBuzz ne dépend d'aucune ressource externe.
    /// </summary>
    /// <remarks>
    /// Projet conservé volontairement (voir « Choix d'architecture » dans le README) : une évolution comme
    /// l'historisation des requêtes ou des statistiques d'usage s'implémenterait ici, derrière une interface
    /// déclarée dans FizzBuzz.Application, sans toucher au domaine ni à l'API.
    /// </remarks>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        return services;
    }
}
