using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ticketing_Tool.Data;
using Ticketing_Tool.Models;
using Ticketing_Tool.Services;

namespace Ticketing_Tool.Controllers;

[Authorize]
public class NotificationsController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    INotificationService notificationService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var notifications = await context.Notifications
            .AsNoTracking()
            .Include(notification => notification.Ticket)
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        return View(notifications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Read(int id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var notification = await context.Notifications
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.NotificationId == id && item.UserId == userId, cancellationToken);
        if (notification is null)
        {
            return NotFound();
        }

        await notificationService.MarkReadAsync(id, userId, cancellationToken);
        return RedirectToAction("Details", "Tickets", new { id = notification.TicketId });
    }
}
