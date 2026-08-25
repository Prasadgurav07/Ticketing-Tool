using Ticketing_Tool.Models;

namespace Ticketing_Tool.Services;

public interface INotificationService
{
    Task CreateAsync(string userId, int ticketId, string notificationType, string title, string message, CancellationToken cancellationToken = default);

    Task NotifyParticipantsAsync(Ticket ticket, string notificationType, string title, string message, string? actorId = null, bool includeActor = false, CancellationToken cancellationToken = default);

    Task MarkReadAsync(int notificationId, string userId, CancellationToken cancellationToken = default);
}
