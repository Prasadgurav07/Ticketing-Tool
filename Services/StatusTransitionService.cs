using System.Security.Claims;
using Ticketing_Tool.Constants;
using Ticketing_Tool.Models;

namespace Ticketing_Tool.Services;

public class StatusTransitionService(ITicketAuthorizationService authorizationService) : IStatusTransitionService
{
    private static readonly Dictionary<string, string[]> SupportTransitions = new(StringComparer.OrdinalIgnoreCase)
    {
        [TicketStatusNames.Open] = [TicketStatusNames.Assigned, TicketStatusNames.InProgress, TicketStatusNames.Cancelled],
        [TicketStatusNames.Assigned] = [TicketStatusNames.InProgress, TicketStatusNames.PendingUser, TicketStatusNames.Resolved, TicketStatusNames.Cancelled],
        [TicketStatusNames.InProgress] = [TicketStatusNames.PendingUser, TicketStatusNames.Resolved, TicketStatusNames.Cancelled],
        [TicketStatusNames.PendingUser] = [TicketStatusNames.InProgress, TicketStatusNames.Cancelled],
        [TicketStatusNames.Resolved] = [TicketStatusNames.Closed, TicketStatusNames.InProgress],
        [TicketStatusNames.Closed] = [TicketStatusNames.InProgress],
        [TicketStatusNames.Cancelled] = [TicketStatusNames.InProgress]
    };

    public IReadOnlyCollection<string> GetAllowedStatusNames(Ticket ticket, ClaimsPrincipal user)
    {
        var currentStatus = ticket.Status?.StatusName;
        if (string.IsNullOrWhiteSpace(currentStatus))
        {
            return Array.Empty<string>();
        }

        if (authorizationService.IsSupportUser(user))
        {
            return SupportTransitions.TryGetValue(currentStatus, out var transitions)
                ? transitions
                : Array.Empty<string>();
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (ticket.CreatedById == userId && currentStatus.Equals(TicketStatusNames.Resolved, StringComparison.OrdinalIgnoreCase))
        {
            return [TicketStatusNames.Closed, TicketStatusNames.InProgress];
        }

        if (ticket.CreatedById == userId && currentStatus.Equals(TicketStatusNames.PendingUser, StringComparison.OrdinalIgnoreCase))
        {
            return [TicketStatusNames.InProgress];
        }

        return Array.Empty<string>();
    }

    public bool CanTransition(Ticket ticket, TicketStatus targetStatus, ClaimsPrincipal user) =>
        GetAllowedStatusNames(ticket, user).Contains(targetStatus.StatusName, StringComparer.OrdinalIgnoreCase);
}
