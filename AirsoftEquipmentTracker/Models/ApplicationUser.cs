using Microsoft.AspNetCore.Identity;

namespace AirsoftEquipmentTracker.Models;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
