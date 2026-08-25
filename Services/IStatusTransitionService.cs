using System.Security.Claims;
using Ticketing_Tool.Models;

namespace Ticketing_Tool.Services;

public interface IStatusTransitionService
{
    IReadOnlyCollection<string> GetAllowedStatusNames(Ticket ticket, ClaimsPrincipal user);

    bool CanTransition(Ticket ticket, TicketStatus targetStatus, ClaimsPrincipal user);
}
