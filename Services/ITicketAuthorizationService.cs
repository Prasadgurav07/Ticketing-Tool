using System.Security.Claims;
using Ticketing_Tool.Models;

namespace Ticketing_Tool.Services;

public interface ITicketAuthorizationService
{
    bool IsSupportUser(ClaimsPrincipal user);

    bool CanViewTicket(Ticket ticket, ClaimsPrincipal user);

    bool CanManageTicket(Ticket ticket, ClaimsPrincipal user);

    bool CanAddComment(Ticket ticket, ClaimsPrincipal user);
}
