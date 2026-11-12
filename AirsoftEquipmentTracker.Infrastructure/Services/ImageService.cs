namespace AirsoftEquipmentTracker.Infrastructure.Services;

// Bewaart en verwijdert foto's voor equipment items.
// Werkt met een gewone Stream, zodat zowel Blazor (IBrowserFile) als een API (IFormFile) hem kan gebruiken.
public class ImageService
{
    public const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    // Map-naam onder wwwroot; ook het begin van het relatieve pad dat in de database komt
    public const string UploadsFolderName = "uploads";

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly string _uploadsFolder;

    // uploadsFolder is het volledige pad naar de map waar foto's komen (bv. wwwroot/uploads)
    public ImageService(string uploadsFolder)
    {
        _uploadsFolder = Path.GetFullPath(uploadsFolder);
    }

    // Controleert bestandstype en grootte.
    // Geeft een foutmelding terug, of null als het bestand in orde is.
    public static string? Validate(string fileName, long size)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
            return "Only JPG, PNG and WebP images are allowed.";

        if (size > MaxFileSize)
            return "The image is too large. Maximum size is 5 MB.";

        return null;
    }

    // Slaat de afbeelding op onder een GUID-naam zodat bestandsnamen
    // nooit botsen, en geeft het relatieve pad terug (bv. "uploads/abc.jpg").
    // Valideert zelf ook nog eens, zodat geen enkele aanroeper dat kan vergeten.
    public async Task<string> SaveImageAsync(Stream content, string fileName, long size)
    {
        var validationError = Validate(fileName, size);
        if (validationError is not null)
            throw new ArgumentException(validationError, nameof(fileName));

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var newFileName = $"{Guid.NewGuid()}{extension}";

        Directory.CreateDirectory(_uploadsFolder);

        var fullPath = Path.Combine(_uploadsFolder, newFileName);
        await using var stream = new FileStream(fullPath, FileMode.Create);
        await content.CopyToAsync(stream);

        return $"{UploadsFolderName}/{newFileName}";
    }

    // Verwijdert een eerder opgeslagen afbeelding,
    // bv. wanneer een foto vervangen wordt of een item verwijderd wordt.
    public void DeleteImage(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            return;

        // Alleen de bestandsnaam gebruiken, zodat een pad als "uploads/../appsettings.json"
        // nooit buiten de uploads-map kan wijzen.
        var fileName = Path.GetFileName(imagePath);
        if (string.IsNullOrEmpty(fileName))
            return;

        var fullPath = Path.Combine(_uploadsFolder, fileName);

        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
}
