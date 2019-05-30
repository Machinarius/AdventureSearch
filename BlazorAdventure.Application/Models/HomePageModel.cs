using System.Collections.Generic;
using BlazorAdventure.Domain;

namespace BlazorAdventure.Application.Models {
  public class HomePageModel {
    public string FilterText { get; set; }
    public ProductsSearchResult SearchResult { get; set; }
    public Dictionary<string, string> ActiveFacetFilters { get; set; } = new Dictionary<string, string>();
    public int PageSize { get; set; }
  }
}
