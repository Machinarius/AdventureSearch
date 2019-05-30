using System.Collections.Generic;

namespace BlazorAdventure.Domain {
  public class ProductHitResult {
    public AWProduct Product { get; set; }
    public IEnumerable<string> HitHighlightsHtml { get; set; }
  }
}
