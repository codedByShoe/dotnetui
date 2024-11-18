namespace Nugetui.Models;

public class NugetSearchResponse
{
  public required List<NugetPackage> Data { get; set; }
  public int TotalHits { get; set; }
}
