using FluentAssertions;
using Nugetui.Services;

public class DotnetCliServiceTests
{
    private readonly DotNetCliService _service;

    public DotnetCliServiceTests()
    {
        _service = new DotNetCliService();
    }

    [Fact]
    public void GetInstalledPackages_ShouldReturnListOfPackages()
    {
        var packages = _service.GetInstalledPackages();

        packages.Should().NotBeNull();
    }

    [Theory]
    [InlineData("Microsoft.EntityFrameworkCore.Sqlite")]
    public async Task InstallPackageAsync_WithValidPackage_ShouldReturnSuccess(string packageId)
    {
        var (success, output) = await _service.InstallPackageAsync(packageId);

        success.Should().BeTrue();
        output.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UninstallPackageAsync_WithInstalledPackage_ShouldReturnSuccess()
    {
        var packageId = "Microsoft.EntityFrameworkCore.Sqlite";

        var (success, output) = await _service.UninstallPackageAsync(packageId);

        success.Should().BeTrue();
        output.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UninstallPackageAsync_WithNotInstalledPackage_ShouldReturnFailure()
    {
        // Arrange
        var packageId = "PackageThatDefinitelyDoesNotExist";

        // Act
        var (success, output) = await _service.UninstallPackageAsync(packageId);

        // Assert
        success.Should().BeFalse();
        output.Should().NotBeNullOrEmpty();
    }
}
