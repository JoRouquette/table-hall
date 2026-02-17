using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TableHall.Api.Tests.Integration;

public sealed class SmokeTests(WebApplicationFactory<Program> factory)
  : IClassFixture<WebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task Get_root_returns_success()
  {
    var res = await _client.GetAsync("/");
    // Selon le template, "/" peut être 404 : l'important ici est que l'host démarre.
    res.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
  }
}
