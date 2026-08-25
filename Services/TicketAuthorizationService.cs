using System.Security.Claims;
using Ticketing_Tool.Constants;
using Ticketing_Tool.Models;

namespace Ticketing_Tool.Services;

public class TicketAuthorizationService : ITicketAuthorizationService
{
    public bool IsSupportUser(ClaimsPrincipal user) =>
        user.IsInRole(RoleNames.Admin) ||
        user.IsInRole(RoleNames.TeamLead) ||
        user.IsInRole(RoleNames.SupportAgent);

    public bool CanViewTicket(Ticket ticket, ClaimsPrincipal user)
    {
        if (IsSupportUser(user))
        {
            return true;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrWhiteSpace(userId) && ticket.CreatedById == userId;
    }

    public bool CanManageTicket(Ticket ticket, ClaimsPrincipal user) => IsSupportUser(user);

    public bool CanAddComment(Ticket ticket, ClaimsPrincipal user) => CanViewTicket(ticket, user);
}
