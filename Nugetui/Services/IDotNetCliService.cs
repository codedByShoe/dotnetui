namespace Nugetui.Services;

public interface IDotNetCliService
{
  Task<(bool success, string output)> InstallPackageAsync(string packageId, string selectedProject);
  Task<(bool success, string output)> UninstallPackageAsync(string packageId);
  void RestorePackageList();
  List<string> GetInstalledPackages(string? currentProject = null);
  List<string> GetProjectsFromSln();
}
