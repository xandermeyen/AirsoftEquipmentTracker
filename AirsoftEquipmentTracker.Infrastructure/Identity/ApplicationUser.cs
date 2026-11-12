using Microsoft.AspNetCore.Identity;

namespace AirsoftEquipmentTracker.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
