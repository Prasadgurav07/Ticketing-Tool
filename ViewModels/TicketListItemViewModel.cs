namespace Ticketing_Tool.ViewModels;

public class TicketListItemViewModel
{
    public int TicketId { get; set; }

    public string TicketNumber { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string Team { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;

    public string AssignedTo { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
