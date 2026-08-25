namespace Ticketing_Tool.ViewModels;

public class DashboardViewModel
{
    public string Title { get; set; } = "Dashboard";

    public int TotalTickets { get; set; }

    public int OpenTickets { get; set; }

    public int AssignedTickets { get; set; }

    public int InProgressTickets { get; set; }

    public int PendingUserTickets { get; set; }

    public int ResolvedTickets { get; set; }

    public int ClosedTickets { get; set; }

    public int CriticalHighTickets { get; set; }

    public int UnreadNotifications { get; set; }

    public IReadOnlyList<TicketListItemViewModel> RecentTickets { get; set; } = [];
}
