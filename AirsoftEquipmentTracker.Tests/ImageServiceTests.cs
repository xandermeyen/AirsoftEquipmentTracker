using System.Text;
using AirsoftEquipmentTracker.Infrastructure.Services;
using Xunit;

namespace AirsoftEquipmentTracker.Tests;

// Tests voor ImageService. Elke test krijgt een eigen tijdelijke map,
// die na de test weer verwijderd wordt.
public class ImageServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly string _uploadsFolder;
    private readonly ImageService _service;

    public ImageServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "airsoft-tests-" + Guid.NewGuid());
        _uploadsFolder = Path.Combine(_tempRoot, ImageService.UploadsFolderName);
        _service = new ImageService(_uploadsFolder);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    private static MemoryStream FakeImage() => new(Encoding.UTF8.GetBytes("not really an image"));

    [Theory]
    [InlineData("photo.jpg")]
    [InlineData("photo.JPEG")]
    [InlineData("photo.png")]
    [InlineData("photo.webp")]
    public void Validate_AllowedExtension_ReturnsNull(string fileName)
    {
        Assert.Null(ImageService.Validate(fileName, 1024));
    }

    [Theory]
    [InlineData("photo.gif")]
    [InlineData("script.exe")]
    [InlineData("noextension")]
    public void Validate_OtherExtension_ReturnsError(string fileName)
    {
        Assert.NotNull(ImageService.Validate(fileName, 1024));
    }

    [Fact]
    public void Validate_TooLarge_ReturnsError()
    {
        Assert.NotNull(ImageService.Validate("photo.jpg", ImageService.MaxFileSize + 1));
    }

    [Fact]
    public async Task SaveImage_WritesFile_AndReturnsRelativePath()
    {
        using var content = FakeImage();

        var path = await _service.SaveImageAsync(content, "photo.PNG", content.Length);

        Assert.StartsWith("uploads/", path);
        Assert.EndsWith(".png", path);
        Assert.True(File.Exists(Path.Combine(_uploadsFolder, Path.GetFileName(path))));
    }

    [Fact]
    public async Task SaveImage_InvalidFile_Throws_AndWritesNothing()
    {
        using var content = FakeImage();

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.SaveImageAsync(content, "script.exe", content.Length));

        Assert.False(Directory.Exists(_uploadsFolder) && Directory.EnumerateFiles(_uploadsFolder).Any());
    }

    [Fact]
    public async Task DeleteImage_RemovesFile()
    {
        using var content = FakeImage();
        var path = await _service.SaveImageAsync(content, "photo.jpg", content.Length);

        _service.DeleteImage(path);

        Assert.False(File.Exists(Path.Combine(_uploadsFolder, Path.GetFileName(path))));
    }

    [Fact]
    public void DeleteImage_PathOutsideUploads_DoesNotDeleteIt()
    {
        // Een bestand naast de uploads-map mag nooit via "../" verwijderd worden
        Directory.CreateDirectory(_uploadsFolder);
        var outsideFile = Path.Combine(_tempRoot, "secret.json");
        File.WriteAllText(outsideFile, "{}");

        _service.DeleteImage("uploads/../secret.json");

        Assert.True(File.Exists(outsideFile));
    }

    [Fact]
    public void DeleteImage_NullOrEmpty_DoesNothing()
    {
        _service.DeleteImage(null);
        _service.DeleteImage("");
    }
}
