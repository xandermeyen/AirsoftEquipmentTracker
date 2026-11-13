namespace AirsoftEquipmentTracker.Core.Interfaces;

// Contract voor het bewaren en verwijderen van foto's van equipment items.
public interface IImageService
{
    // Bewaart de foto en geeft het relatieve pad terug (bv. "uploads/abc.jpg").
    // Gooit een ArgumentException als het bestandstype of de grootte niet klopt.
    Task<string> SaveImageAsync(Stream content, string fileName, long size);

    void DeleteImage(string? imagePath);
}
