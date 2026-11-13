using System.Security.Claims;
using AirsoftEquipmentTracker.Api.DTOs.Dashboard;
using AirsoftEquipmentTracker.Core.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirsoftEquipmentTracker.Api.Controllers;

// Dezelfde dashboardcijfers als in de Blazor-app, berekend door EquipmentService.
[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController(IEquipmentService equipmentService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<DashboardDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardDto>> Get()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("The token has no user id.");

        var stats = await equipmentService.GetDashboardStatsAsync(userId);
        return Ok(stats.Adapt<DashboardDto>());
    }
}
