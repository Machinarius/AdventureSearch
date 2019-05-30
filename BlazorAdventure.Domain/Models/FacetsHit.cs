using System.Collections.Generic;

namespace BlazorAdventure.Domain.Models {
  public class FacetsHit : List<(string value, string friendlyValue)> {
    public string Name { get; set; }
    public string FriendlyName { get; set; }

    public FacetsHit(string name, string friendlyName, IEnumerable<(string value, string friendlyValue)> values) : base(values) {
      Name = name;
      FriendlyName = friendlyName;
    }
  }
}
