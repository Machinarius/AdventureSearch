using Microsoft.Extensions.DependencyInjection;

namespace BlazorAdventure.Application.Configuration {
  public static class ApplicationDIConfigurator {
    public static IServiceCollection AddApplicationServices(this IServiceCollection services) {
      if (services == null) {
        throw new System.ArgumentNullException(nameof(services));
      }

      services.AddScoped<HomePageInteractor>();
      return services;
    }
  }
}
