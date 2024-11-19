namespace Nugetui.Services;

public interface IDotNetCliService
{
  Task<bool> InstallPackageAsync(string packageId);
  Task<bool> UninstallPackageAsync(string packageId);
  Task<bool> RestorePackageListAsync();
  Task<List<string>> GetInstalledPackagesAsync();
}
