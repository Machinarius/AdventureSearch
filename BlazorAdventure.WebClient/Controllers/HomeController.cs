using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BlazorAdventure.Application;
using BlazorAdventure.WebClient.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlazorAdventure.WebClient.Controllers {
  public class HomeController : Controller {
    private readonly HomePageInteractor _interactor;

    public HomeController(HomePageInteractor interactor) {
      _interactor = interactor ?? throw new ArgumentNullException(nameof(interactor));
    }

    public async Task<IActionResult> Index([FromQuery] string filterText = null, [FromQuery] string facetsQuery = null) {
      var facetComponents = facetsQuery?.Split(',')
        .Select(facetFilter => facetFilter.Split('='));

      var facetsFilter = new Dictionary<string, string>();
      if (facetComponents?.Any() ?? false) {
        foreach (var component in facetComponents.Where(x => x.Length == 2)) {
          facetsFilter.TryAdd(component[0], component[1]);
        }
      }

      var searchResult = await _interactor.ExecuteSearchAsync(filterText, facetsFilter);
      return View(searchResult);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
      return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
  }
}
