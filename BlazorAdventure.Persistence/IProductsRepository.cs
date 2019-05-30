using System.Threading.Tasks;
using BlazorAdventure.Domain;
using BlazorAdventure.Persistence.Models;

namespace BlazorAdventure.Persistence {
  public interface IProductsRepository {
    Task<ProductsSearchResult> SearchForProducts(SearchArguments args = null);
  }
}
