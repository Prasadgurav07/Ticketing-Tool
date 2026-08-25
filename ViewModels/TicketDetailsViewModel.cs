using Ticketing_Tool.Models;

namespace Ticketing_Tool.ViewModels;

public class TicketDetailsViewModel
{
    public Ticket Ticket { get; set; } = new();

    public IReadOnlyList<TicketStatus> AllowedStatuses { get; set; } = [];

    public IReadOnlyList<ApplicationUser> AssignableUsers { get; set; } = [];
}
