
namespace Nugetui.Models;

public class ProjectInfo
{
  public required string ProjectPath { get; set; }
  public List<PackageInfo> Packages { get; set; } = new();
}
