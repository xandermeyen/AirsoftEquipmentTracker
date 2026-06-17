using Microsoft.AspNetCore.Components.Forms;

namespace AirsoftEquipmentTracker.Services;

// Bewaart en verwijdert foto's voor equipment items in wwwroot/uploads.
public class ImageService
{
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly IWebHostEnvironment _environment;

    public ImageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    // Controleert bestandstype en grootte.
    // Geeft een foutmelding terug, of null als het bestand in orde is.
    public static string? Validate(IBrowserFile file)
    {
        var extension = Path.GetExtension(file.Name).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
            return "Only JPG, PNG and WebP images are allowed.";

        if (file.Size > MaxFileSize)
            return "The image is too large. Maximum size is 5 MB.";

        return null;
    }

    // Slaat de afbeelding op onder een GUID-naam zodat bestandsnamen
    // nooit botsen, en geeft het relatieve pad terug (bv. "uploads/abc.jpg").
    public async Task<string> SaveImageAsync(IBrowserFile file)
    {
        var extension = Path.GetExtension(file.Name).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{extension}";

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsFolder);

        var fullPath = Path.Combine(uploadsFolder, fileName);
        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.OpenReadStream(MaxFileSize).CopyToAsync(stream);

        return $"uploads/{fileName}";
    }

    // Verwijdert een eerder opgeslagen afbeelding,
    // bv. wanneer een foto vervangen wordt of een item verwijderd wordt.
    public void DeleteImage(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            return;

        var fullPath = Path.Combine(
            _environment.WebRootPath,
            imagePath.Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
}
