using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorAdventure.Domain;
using BlazorAdventure.Domain.Models;
using BlazorAdventure.Persistence.Models;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Options;

namespace BlazorAdventure.Persistence {
  public class AzSearchProductsRepository : IProductsRepository {
    private readonly SearchIndexClient _searchClient;
    private readonly Func<ConfigurationOptions> _getConfiguration;

    public AzSearchProductsRepository(IOptions<ConfigurationOptions> options) {
      if (options == null) {
        throw new ArgumentNullException(nameof(options));
      }

      _getConfiguration = () => options.Value;
      var config = _getConfiguration();

      if (string.IsNullOrEmpty(config.QueryKey)) {
        throw new InvalidOperationException($"AzureSearch configuration value '{nameof(config.QueryKey)}' is missing");
      }

      if (string.IsNullOrEmpty(config.ServiceName)) {
        throw new InvalidOperationException($"AzureSearch configuration value '{nameof(config.ServiceName)}' is missing");
      }

      if (string.IsNullOrEmpty(config.ProductsIndexName)) {
        throw new InvalidOperationException($"AzureSearch configuration value '{nameof(config.ProductsIndexName)}' is missing");
      }

      var searchApiCredentials = new SearchCredentials(config.QueryKey);
      _searchClient = new SearchIndexClient(searchApiCredentials) {
        SearchServiceName = config.ServiceName,
        IndexName = config.ProductsIndexName
      };
    }

    public async Task<ProductsSearchResult> SearchForProducts(SearchArguments args = null) {
      var config = _getConfiguration();

      string facetsFilter = null;
      if (args?.FacetValues?.Any() ?? false) {
        facetsFilter = string.Join(" and ", args?.FacetValues?.Select(GenerateFacetFilterString).ToArray());
      }

      var searchParameters = new SearchParameters {
        SearchMode = SearchMode.Any,
        IncludeTotalResultCount = true,
        HighlightFields = config.HighlightFields?.ToList(),
        Skip = (int)args?.SkipAmount,
        Facets = config.ProductFacets?.Select(GenerateFacetString).ToList(),
        Filter = facetsFilter,
        Top = args?.PageSize
      };

      var productsResult = await _searchClient.Documents.SearchAsync<AWProduct>(args?.FilterText, searchParameters);
      var pageProducts = productsResult.Results.Select(productResult => new ProductHitResult {
        Product = productResult.Document,
        HitHighlightsHtml = productResult.Highlights?.SelectMany(x => x.Value)
      }).ToArray();
      var productFacets = productsResult.Facets.Select(facet => {
        var facetName = facet.Key;
        var friendlyName = GetFacetFriendlyName(facet.Key);
        var values = facet.Value.Select(value => GetFacetValue(facet.Key, value));
        return new FacetsHit(facetName, friendlyName, values);
      }).ToArray();

      var searchResult = new ProductsSearchResult {
        ProductHits = pageProducts,
        Facets = productFacets,
        ResultsCount = productsResult.Count ?? 0L,
        SkipCount = args?.SkipAmount ?? 0L
      };

      return searchResult;
    }

    private (string value, string friendlyValue) GetFacetValue(string facetName, FacetResult facetResult) {
      var matchingFacet = GetFacetObject(facetName);
      var facetValue = facetResult.Value?.ToString();
      if (matchingFacet.Type == FacetType.IntegerRange) {
        if (!double.TryParse(facetValue, out var facetNumericValue)) {
          throw new InvalidOperationException("Cannot filter a numeric facet with a non-numeric value");
        }
        return (facetValue, string.Concat(facetValue, " - ", facetNumericValue + matchingFacet.Interval - 1));
      }

      if (!matchingFacet.UserFriendlyValues.TryGetValue(facetValue, out var friendlyValue)) {
        return (facetValue, facetValue);
      }

      return (facetValue, friendlyValue + " - " + facetValue);
    }

    private string GetFacetFriendlyName(string facetName) {
      var matchingFacet = GetFacetObject(facetName);
      return matchingFacet.UserFriendlyName;
    }

    private string GenerateFacetString(FacetConfig facet) {
      switch (facet.Type) {
        case FacetType.IntegerRange:
          return facet.Name + ",interval:" + facet.Interval;
        default:
          return facet.Name;
      }
    }

    private string GenerateFacetFilterString(KeyValuePair<string, string> facetSpec) {
      string filterResult;

      var matchingFacet = GetFacetObject(facetSpec.Key);
      switch (matchingFacet.Type) {
        case FacetType.IntegerRange:
          if (!long.TryParse(facetSpec.Value, out var facetValue)) {
            throw new InvalidOperationException("Cannot filter a numeric facet with a non-numeric value");
          }

          filterResult = string.Concat(facetSpec.Key, " ge ", facetValue, " and ", facetSpec.Key, " lt ", facetValue + matchingFacet.Interval);
          break;
        default:
          filterResult = string.Concat(facetSpec.Key, " eq '", facetSpec.Value, "'");
          break;
      }

      return filterResult;
    }

    private FacetConfig GetFacetObject(string facetName) {
      var config = _getConfiguration();
      var matchingFacet = config.ProductFacets.FirstOrDefault(x => x.Name == facetName);
      if (matchingFacet == null) {
        throw new InvalidOperationException($"Undefined Facet found in filter '{facetName}'");
      }

      return matchingFacet;
    }

    public class ConfigurationOptions {
      public string QueryKey { get; set; }
      public string ServiceName { get; set; }
      public string ProductsIndexName { get; set; }
      public IEnumerable<FacetConfig> ProductFacets { get; set; }
      public IEnumerable<string> HighlightFields { get; set; }
    }

    public class FacetConfig {
      public string Name { get; set; }
      public string UserFriendlyName { get; set; }
      public FacetType Type { get; set; }
      public int Interval { get; set; }
      public Dictionary<string, string> UserFriendlyValues { get; set; } = new Dictionary<string, string>();
    }

    public enum FacetType {
      Value,
      IntegerRange
    }
  }
}
