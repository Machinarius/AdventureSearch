using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorAdventure.Persistence.Configuration {
  public static class PersistenceDIConfiguration {
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration) {
      if (services == null) {
        throw new System.ArgumentNullException(nameof(services));
      }

      if (configuration == null) {
        throw new System.ArgumentNullException(nameof(configuration));
      }
      
      var azureSearchSection = configuration.GetSection("AzureSearch");

      services.AddOptions();
      services.Configure<AzSearchProductsRepository.ConfigurationOptions>(azureSearchSection);

      services.AddScoped<IProductsRepository, AzSearchProductsRepository>();
      return services;
    }
  }
}
