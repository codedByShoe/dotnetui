namespace Nugetui;

using Terminal.Gui;
using System.Text.Json;

class Program
{
  private class NugetSearchResponse
  {
    public required List<NugetPackage> Data { get; set; }
    public int TotalHits { get; set; }
  }

  private class NugetPackage
  {
    public required string Id { get; set; }
    public required string Version { get; set; }
    public required string Description { get; set; }
    public required int TotalDownloads { get; set; }
  }

  public class ProgressDialog : Dialog
  {
    private readonly Label messageLabel;

    public ProgressDialog(string title, string message) : base(title, 60, 8)
    {
      this.Border.BorderStyle = BorderStyle.Single;
      this.Border.Effect3D = false;
      this.Border.BorderBrush = Color.White;

      var dialogScheme = new ColorScheme()
      {
        Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
        Focus = Application.Driver.MakeAttribute(Color.White, Color.Black),
        HotNormal = Application.Driver.MakeAttribute(Color.White, Color.Black),
        HotFocus = Application.Driver.MakeAttribute(Color.White, Color.Black)
      };

      this.ColorScheme = dialogScheme;

      messageLabel = new Label(message)
      {
        X = Pos.Center(),
        Y = Pos.Center(),
        Width = Dim.Fill() - 4
      };

      Add(messageLabel);

    }

    public void UpdateMessage(string message)
    {
      messageLabel.Text = message;
    }

    public void ShowDialog()
    {
      Application.Run(this);
    }
  }

  private static readonly HttpClient httpClient = new HttpClient();
  private static List<NugetPackage> currentPackages = new();

  private static async Task<T> RunWithProgressDialog<T>(string title, string message, Func<ProgressDialog, Task<T>> task)
  {
    var dialog = new ProgressDialog(title, message);
    var tcs = new TaskCompletionSource<T>();

    Application.MainLoop.Invoke(async () =>
    {
      try
      {
        var result = await task(dialog);
        tcs.SetResult(result);
      }
      catch (Exception ex)
      {
        tcs.SetException(ex);
      }
      finally
      {
        Application.MainLoop.Invoke(() =>
        {
          dialog.Running = false;
        });
      }
    });

    dialog.ShowDialog();
    return await tcs.Task;
  }

  private static bool ShowConfirmationDialog(string title, string message)
  {
    var result = false;
    var dialog = new Dialog(title, 60, 10);

    // Style the dialog
    dialog.Border.BorderStyle = BorderStyle.Single;
    dialog.ColorScheme = new ColorScheme
    {
      Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
      Focus = Application.Driver.MakeAttribute(Color.White, Color.Black),
      HotNormal = Application.Driver.MakeAttribute(Color.White, Color.Black),
      HotFocus = Application.Driver.MakeAttribute(Color.White, Color.Black)
    };

    // Center the dialog
    dialog.X = Pos.Center();
    dialog.Y = Pos.Center();

    // Add the message
    var label = new Label(message)
    {
      X = Pos.Center(),
      Y = Pos.Center() - 1,
      ColorScheme = dialog.ColorScheme
    };

    // Style for buttons
    var buttonScheme = new ColorScheme
    {
      Normal = Application.Driver.MakeAttribute(Color.White, Color.Blue),
      Focus = Application.Driver.MakeAttribute(Color.White, Color.BrightBlue),
      HotNormal = Application.Driver.MakeAttribute(Color.White, Color.Blue),
      HotFocus = Application.Driver.MakeAttribute(Color.White, Color.BrightBlue)
    };

    // Add No button
    var noButton = new Button("No")
    {
      X = Pos.Center() - 6,
      Y = Pos.Center() + 1,
      ColorScheme = buttonScheme,
      IsDefault = true
    };
    noButton.Clicked += () =>
    {
      result = false;
      Application.RequestStop();
    };

    // Add Yes button
    var yesButton = new Button("Yes")
    {
      X = Pos.Center() + 6,
      Y = Pos.Center() + 1,
      ColorScheme = buttonScheme
    };
    yesButton.Clicked += () =>
    {
      result = true;
      Application.RequestStop();
    };

    // Add controls to dialog
    dialog.Add(label, noButton, yesButton);

    // Run the dialog
    Application.Run(dialog);

    // Return the result
    return result;
  }

  private static async Task<List<NugetPackage>> SearchNugetPackagesAsync(string searchTerm)
  {
    try
    {
      var url = $"https://api-v2v3search-0.nuget.org/query?q={Uri.EscapeDataString(searchTerm)}&take=100&includeDelisted=false";
      var response = await httpClient.GetStringAsync(url);

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

  private static void updatePackageDetails(ListView detailsList, NugetPackage package)
  {
    var details = new List<string>();

    details.Add($"Id: {package.Id}");
    details.Add($" Version: {package.Version}");
    details.Add($"Downloads: {package.TotalDownloads}");
    details.Add($"Description: {package.Description}");

    Application.MainLoop.Invoke(() =>
    {
      detailsList.SetSource(details);
    });

  }

  private static List<string> getProjectPackages()
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

  static void refreshProjectPackages(ListView packageList)
  {
    try
    {
      var packages = getProjectPackages();
      Application.MainLoop.Invoke(() =>
      {
        packageList.SetSource(packages);
      });
    }
    catch (Exception ex)
    {
      Application.MainLoop.Invoke(() =>
      {
        packageList.SetSource(new List<string> { $"Error: {ex.Message}" });
      });
    }
  }


  static void HandleVimNavigation(ListView listView, KeyEvent keyEvent)
  {
    if (listView.Source?.Count > 0)
    {
      switch (keyEvent.Key)
      {
        case Key.j:
        case Key.J:
          // Move down (j)
          if (listView.SelectedItem < listView.Source.Count - 1)
          {
            listView.SelectedItem++;
          }
          break;

        case Key.k:
        case Key.K:
          // Move up (k)
          if (listView.SelectedItem > 0)
          {
            listView.SelectedItem--;
          }
          break;
      }
    }
  }

  private static async Task<bool> InstallPackageAsync(string packageId)
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

  private static async Task<bool> RestorePackageListAsync()
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

  private static async Task<bool> UninstallPackageAsync(string packageId)
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

  static void Main(string[] args)
  {
    Application.Init();

    // Create custom color scheme for input field
    var inputColorScheme = new ColorScheme()
    {
      Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
      Focus = Application.Driver.MakeAttribute(Color.White, Color.Black),
      HotNormal = Application.Driver.MakeAttribute(Color.White, Color.Black),
      HotFocus = Application.Driver.MakeAttribute(Color.White, Color.Black)
    };

    var packageResults = new List<string>();

    var win = new Window("NugeTUI")
    {
      X = 0,
      Y = 1,
      Width = Dim.Fill(),
      Height = Dim.Fill() - 1,
      ColorScheme = Colors.TopLevel
    };

    var leftFrame = new FrameView("Nuget Packages")
    {
      X = 0,
      Y = 0,
      Width = Dim.Percent(50),
      Height = Dim.Fill() - 4,
    };

    // Left list box
    var leftList = new ListView(packageResults)
    {
      X = 0,
      Y = 0,
      Width = Dim.Fill(),
      Height = Dim.Fill(),
      ColorScheme = Colors.TopLevel,
    };



    leftFrame.Add(leftList);
    win.Add(leftFrame);


    var rightTopFrame = new FrameView("Package Details")
    {
      X = Pos.Percent(50),
      Y = 0,
      Width = Dim.Fill(),
      Height = Dim.Percent(50),
    };

    // Right top list box
    var rightTopList = new ListView()
    {
      X = 0,
      Y = 0,
      Width = Dim.Fill(),
      Height = Dim.Fill(),
      ColorScheme = Colors.TopLevel
    };
    rightTopFrame.Add(rightTopList);
    win.Add(rightTopFrame);
    rightTopList.KeyPress += (args) =>
        {
          HandleVimNavigation(rightTopList, args.KeyEvent);
        };

    var rightBottomFrame = new FrameView("Installed Packages")
    {
      X = Pos.Percent(50),
      Y = Pos.Percent(50),
      Width = Dim.Fill(),
      Height = Dim.Fill() - 4,
    };
    // Right bottom list box
    var rightBottomList = new ListView()
    {
      X = 0,
      Y = 0,
      Width = Dim.Fill(),
      Height = Dim.Fill(),
      ColorScheme = Colors.TopLevel
    };

    rightBottomFrame.Add(rightBottomList);
    win.Add(rightBottomFrame);

    rightBottomList.KeyPress += async (args) =>
    {
      HandleVimNavigation(rightBottomList, args.KeyEvent);
      if ((args.KeyEvent.Key == Key.x || args.KeyEvent.Key == Key.X) && rightBottomList.SelectedItem >= 0 && rightBottomList.Source?.Count > 0)
      {
        var selectedLine = rightBottomList.Source.ToList()[rightBottomList.SelectedItem]
            .ToString();

        var packageId = selectedLine.Split(' ')[1];

        if (!string.IsNullOrWhiteSpace(packageId))
        {
          if (ShowConfirmationDialog(
              "Confirm Delete",
              $"Are your sure you want to remove package {packageId}?"))
          {
            var success = await RunWithProgressDialog(
                "Deleting",
                $"Deleting package {packageId}",
                async (dialog) => await UninstallPackageAsync(packageId)
                );
            if (success)
            {
              refreshProjectPackages(rightBottomList);
            }
          }
        }
      }
    };


    var inputLabel = new Label("Quit: Ctrl+c | Navigate: j/k | Install Package: Enter")
    {
      X = 0,
      Y = Pos.AnchorEnd(4),
    };

    win.Add(inputLabel);
    var inputFrame = new FrameView("Search Packages: ")
    {
      X = 0,
      Y = Pos.AnchorEnd(3),
      Width = Dim.Fill(),
      Height = 3
    };

    // Bottom input field
    var inputField = new TextField("")
    {
      X = 0,
      Y = 0,
      Width = Dim.Fill(),
      Height = Dim.Fill(),
      ColorScheme = inputColorScheme
    };
    inputFrame.Add(inputField);
    win.Add(inputFrame);


    leftList.SelectedItemChanged += (args) =>
    {
      if (leftList.SelectedItem >= 0 && leftList.SelectedItem < currentPackages.Count)
      {
        var selectedPackage = currentPackages[leftList.SelectedItem];
        updatePackageDetails(rightTopList, selectedPackage);
      }
    };

    refreshProjectPackages(rightBottomList);

    leftList.KeyPress += async (args) =>
        {
          HandleVimNavigation(leftList, args.KeyEvent);
          if (args.KeyEvent.Key == Key.Enter && leftList.SelectedItem >= 0 && leftList.SelectedItem < currentPackages.Count)
          {
            var selectedPackage = currentPackages[leftList.SelectedItem];
            var success = await RunWithProgressDialog(
                "Installing Package",
                $"Installing {selectedPackage.Id}",
                async (dialog) => await InstallPackageAsync(selectedPackage.Id)
                );
            if (success)
            {
              refreshProjectPackages(rightBottomList);
            }
          }
        };

    // Handle input field submission
    inputField.KeyPress += async (args) =>
    {
      if (args.KeyEvent.Key == Key.Enter && !string.IsNullOrWhiteSpace(inputField.Text.ToString()))
      {
        var searchTerm = inputField.Text.ToString();

        if (searchTerm is null)
        {
          return;
        }

        try
        {

          packageResults.Clear();
          packageResults.Add("Searching...");
          leftList.SetSource(packageResults);
          // currentPackages = await SearchNugetPackagesAsync(searchTerm);
          currentPackages = await RunWithProgressDialog(
              "Searching NuGet",
             $"Searching for {searchTerm}",
          async (dialog) => await SearchNugetPackagesAsync(searchTerm)
              );
          Application.MainLoop.Invoke(() =>
            {
              packageResults.Clear();
              packageResults.AddRange(currentPackages.Select(p => $"{p.Id} {p.Version}"));
              leftList.SetSource(packageResults);
              inputField.Text = "";
              leftList.SetFocus();
              leftList.SelectedItem = 0;
              updatePackageDetails(rightTopList, currentPackages[0]);
            });
        }
        catch (Exception ex)
        {
          Application.MainLoop.Invoke(() =>
          {
            packageResults.Clear();
            packageResults.Add(ex.Message);
            inputField.Text = "";
          });
        }


        args.Handled = true;
      }
    };

    // Add global key handler for Ctrl+C
    Application.Top.KeyPress += (args) =>
    {
      if (args.KeyEvent.Key == (Key.CtrlMask | Key.C))
      {
        Application.RequestStop();
        args.Handled = true;
      }
    };

    Application.Top.Add(win);
    inputField.SetFocus();
    Application.Run();
  }
}
