using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ticketing_Tool.Models;
using Ticketing_Tool.Services;

namespace Ticketing_Tool.Controllers;

[Authorize]
public class DashboardController(ITicketService ticketService, UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var model = await ticketService.GetEmployeeDashboardAsync(userId, cancellationToken);
        return View(model);
    }
}
