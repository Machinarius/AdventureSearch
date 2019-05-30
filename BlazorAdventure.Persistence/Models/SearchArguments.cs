using System.Collections.Generic;

namespace BlazorAdventure.Persistence.Models {
  public class SearchArguments {
    public string FilterText { get; set; }
    public long SkipAmount { get; set; }
    public Dictionary<string, string> FacetValues { get; set; }
    public int PageSize { get; set; }
  }
}
