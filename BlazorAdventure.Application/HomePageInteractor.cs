using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorAdventure.Application.Models;
using BlazorAdventure.Persistence;
using BlazorAdventure.Persistence.Models;

namespace BlazorAdventure.Application {
  public class HomePageInteractor {
    private readonly IProductsRepository _productsRepository;

    public HomePageInteractor(IProductsRepository productsRepository) {
      _productsRepository = productsRepository ?? throw new ArgumentNullException(nameof(productsRepository));
    }

    public async Task<HomePageModel> ExecuteSearchAsync(string filterText, Dictionary<string, string> facetValues = null) {
      var searchResults = await _productsRepository.SearchForProducts(new SearchArguments {
        FilterText = filterText,
        FacetValues = facetValues,
        PageSize = 10
      });

      return new HomePageModel {
        SearchResult = searchResults,
        FilterText = filterText,
        ActiveFacetFilters = facetValues
      };
    }
  }
}
