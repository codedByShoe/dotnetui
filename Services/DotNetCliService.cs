
namespace Nugetui.Services;

public class DotNetCliService : IDotNetCliService
{

  public async Task<bool> InstallPackageAsync(string packageId)
  {
    try
    {
      var processInfo = new System.Diagnostics.ProcessStartInfo
      {
        FileName = "dotnet",
        Arguments = $"add package {packageId}",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };
      using var process = System.Diagnostics.Process.Start(processInfo);
      if (process == null)
      {
        return false;
      }

      await process.WaitForExitAsync();
      return process.ExitCode == 0;

    }
    catch
    {
      return false;
    }
  }
  public async Task<bool> UninstallPackageAsync(string packageId)
  {
    try
    {
      var processInfo = new System.Diagnostics.ProcessStartInfo
      {
        FileName = "dotnet",
        Arguments = $"remove package {packageId}",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };
      using var process = System.Diagnostics.Process.Start(processInfo);
      if (process == null)
      {
        return false;
      }

      await process.WaitForExitAsync();
      var restoreSuccess = await RestorePackageListAsync();
      if (!restoreSuccess) return false;
      return process.ExitCode == 0;
    }
    catch
    {
      return false;
    }
  }

  public async Task<bool> RestorePackageListAsync()
  {
    try
    {
      var processInfo = new System.Diagnostics.ProcessStartInfo
      {
        FileName = "dotnet",
        Arguments = "restore",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };
      using var process = System.Diagnostics.Process.Start(processInfo);
      if (process == null) return false;
      await process.WaitForExitAsync();
      return process.ExitCode == 0;
    }
    catch
    {
      return false;
    }
  }

  public List<string> GetInstalledPackages()
  {

    var packages = new List<string>();

    try
    {
      var processInfo = new System.Diagnostics.ProcessStartInfo
      {
        FileName = "dotnet",
        Arguments = "list package",
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };

      using var process = System.Diagnostics.Process.Start(processInfo);
      if (process == null)
      {
        packages.Add("Error: Could not start dotnet process");
        return packages;
      }

      var output = process.StandardOutput.ReadToEnd();
      process.WaitForExit();

      if (string.IsNullOrWhiteSpace(output))
      {
        packages.Add("No Packages Found In Project");
        return packages;
      }

      var lines = output.Split("\n");
      var packageLines = lines.Skip(2)
                                    .Where(line => !string.IsNullOrWhiteSpace(line))
                                    .Select(line => line.Trim());

      packages.AddRange(packageLines);

      if (!packages.Any())
      {
        packages.Add("No packages found in project");
      }

    }
    catch (Exception ex)
    {
      packages.Add($"Error: {ex.Message}");
    }

    return packages;
  }
}
