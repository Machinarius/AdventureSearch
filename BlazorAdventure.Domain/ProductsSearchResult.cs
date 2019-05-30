using System.Collections.Generic;
using BlazorAdventure.Domain.Models;

namespace BlazorAdventure.Domain {
  public class ProductsSearchResult {
    public IEnumerable<ProductHitResult> ProductHits { get; set; }
    public IEnumerable<FacetsHit> Facets { get; set; }
    public long ResultsCount { get; set; }
    public long SkipCount { get; set; }
  }
}
