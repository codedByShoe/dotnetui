using Nugetui.Models;

namespace Nugetui.Services;

public interface INugetService
{
  Task<List<NugetPackage>> SearchPackagesAsync(string searchTerm);
}
