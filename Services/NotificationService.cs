using Microsoft.EntityFrameworkCore;
using Ticketing_Tool.Data;
using Ticketing_Tool.Models;

namespace Ticketing_Tool.Services;

public class NotificationService(ApplicationDbContext context) : INotificationService
{
    public async Task CreateAsync(string userId, int ticketId, string notificationType, string title, string message, CancellationToken cancellationToken = default)
    {
        context.Notifications.Add(new Notification
        {
            UserId = userId,
            TicketId = ticketId,
            NotificationType = notificationType,
            Title = title,
            Message = message,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task NotifyParticipantsAsync(Ticket ticket, string notificationType, string title, string message, string? actorId = null, bool includeActor = false, CancellationToken cancellationToken = default)
    {
        var recipients = new HashSet<string>(StringComparer.Ordinal)
        {
            ticket.CreatedById
        };

        if (!string.IsNullOrWhiteSpace(ticket.AssignedToId))
        {
            recipients.Add(ticket.AssignedToId);
        }

        foreach (var recipient in recipients)
        {
            if (!includeActor && !string.IsNullOrWhiteSpace(actorId) && recipient == actorId)
            {
                continue;
            }

            context.Notifications.Add(new Notification
            {
                UserId = recipient,
                TicketId = ticket.TicketId,
                NotificationType = notificationType,
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkReadAsync(int notificationId, string userId, CancellationToken cancellationToken = default)
    {
        var notification = await context.Notifications
            .SingleOrDefaultAsync(item => item.NotificationId == notificationId && item.UserId == userId, cancellationToken);

        if (notification is null)
        {
            return;
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
    }
}
