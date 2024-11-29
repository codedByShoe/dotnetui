using System.Diagnostics;
using System.Text;
using Nugetui.Models;
using Nugetui.UI.Dialogs;
using Terminal.Gui;

namespace Nugetui.Services;
public class DotNetCliService : IDotNetCliService
{

    private List<string> _projects;
    public DotNetCliService()
    {
        _projects = GetProjectsFromSln();
    }

    private async Task<(bool success, string projectPath)> FindPackageProjectAsync(string packageId)
    {
        var projects = await MapPackagesToProjectsAsync();
        var project = projects.Find(p => p.Packages.Any(p => p.Name == packageId));
        if (project == null) return (false, "");
        return (true, project.ProjectPath);
    }

    private async Task<List<ProjectInfo>> MapPackagesToProjectsAsync()
    {
        var projectPackageInfo = new List<ProjectInfo>();
        var projects = GetProjectsFromSln();
        foreach (var project in projects)
        {
            ProjectInfo? currentProject = null;
            var processInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"list {project} package",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);

            var output = process?.StandardOutput.ReadToEnd();
            await process.WaitForExitAsync();

            var lines = output?.Split('\n');

            if (lines == null) return projectPackageInfo;

            foreach (var line in lines)
            {
                var packageLine = line.Trim();

                if (packageLine.StartsWith(">"))
                {
                    currentProject = new ProjectInfo { ProjectPath = project };
                    var parts = packageLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 4)
                    {
                        currentProject.Packages.Add(new PackageInfo
                        {
                            Name = parts[1]
                        });
                    }
                }
            }
            projectPackageInfo.Add(currentProject!);
        }
        Application.MainLoop.Invoke(() =>
        {
            Application.Shutdown();
        });
        Console.Write(projectPackageInfo);
        return projectPackageInfo;
    }

    public List<string> GetProjectsFromSln()
    {

        var projects = new List<string>();

        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "sln list",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                projects.Add("Error: No Sln File Found");
                return projects;
            }

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (string.IsNullOrWhiteSpace(output))
            {
                projects.Add("No Projects Found In Sln");
                return projects;
            }

            var lines = output.Split("\n");
            var packageLines = lines.Skip(2)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Trim());

            projects.AddRange(packageLines);

            if (!projects.Any())
            {
                projects.Add("Error: Read Sln Failed");
            }

        }
        catch (Exception ex)
        {
            projects.Add($"Error: {ex.Message}");
        }

        return projects;
    }


    public async Task<(bool success, string output)> InstallPackageAsync(string packageId, string selectedProject)
    {
        try
        {

            var processInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"add {selectedProject} package {packageId}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                return (false, "Failed to start process");
            }

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            var outputBuilder = new StringBuilder();
            outputBuilder.AppendLine(await outputTask);
            outputBuilder.AppendLine(await errorTask);

            return (process.ExitCode == 0, outputBuilder.ToString());

        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool success, string output)> UninstallPackageAsync(string packageId)
    {

        var (success, projectPath) = await ProgressDialog.RunAsync(
            "Searching Projects",
            $"Searching for Projects with {packageId}",
            () => FindPackageProjectAsync(packageId)
            );

        if (!success)
        {
            return (false, $"Package '{packageId}' not found in any project");
        }
        Console.WriteLine(projectPath);

        if (!string.IsNullOrWhiteSpace(projectPath))
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"remove {projectPath} package {packageId}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);

                if (process == null)
                {
                    return (false, "Failed to start process");
                }

                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                var outputBuilder = new StringBuilder();
                outputBuilder.AppendLine(await outputTask);
                outputBuilder.AppendLine(await errorTask);

                RestorePackageList();

                return (process.ExitCode == 0, outputBuilder.ToString());
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        return (false, "No project found");
    }

    public void RestorePackageList()
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "restore",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(processInfo);
            if (process == null) return;
            process.WaitForExit();
        }
        catch
        {
            // TODO: Implement error handling
            return;
        }
    }

    public List<string> GetInstalledPackages(string? currentProject = null)
    {

        var packages = new List<string>();

        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"list {currentProject} package",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
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
