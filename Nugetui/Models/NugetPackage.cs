namespace Nugetui.Models;

public class NugetPackage
{
  public required string Id { get; set; }
  public required string Version { get; set; }
  public required string Description { get; set; }
  public required int TotalDownloads { get; set; }
}
