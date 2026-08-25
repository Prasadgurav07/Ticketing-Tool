using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketing_Tool.Constants;
using Ticketing_Tool.Services;

namespace Ticketing_Tool.Controllers;

[Authorize(Roles = RoleNames.SupportOrAdmin)]
public class SupportController(ITicketService ticketService) : Controller
{
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var model = await ticketService.GetSupportDashboardAsync(cancellationToken);
        return View(model);
    }
}
