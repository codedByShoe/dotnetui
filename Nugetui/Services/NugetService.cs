using System.Text.Json;
using Nugetui.Models;

namespace Nugetui.Services;


public class NugetService : INugetService
{
    private readonly HttpClient _httpClient;

    public NugetService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<NugetPackage>> SearchPackagesAsync(string searchTerm)
    {
        try
        {
            var url = $"https://api-v2v3search-0.nuget.org/query?q={Uri.EscapeDataString(searchTerm)}&take=100&includeDelisted=false";
            var response = await _httpClient.GetStringAsync(url);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };


            var searchResult = JsonSerializer.Deserialize<NugetSearchResponse>(response, options);


            return searchResult?.Data ?? new List<NugetPackage>();
        }
        catch (Exception)
        {
            return new List<NugetPackage>();
        }
    }
}
