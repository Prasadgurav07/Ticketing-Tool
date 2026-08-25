using Microsoft.AspNetCore.Identity;

namespace Ticketing_Tool.Models;

public class ApplicationUser : IdentityUser
{
    public string? EmployeeCode { get; set; }

    public string FullName { get; set; } = string.Empty;

    public int? DepartmentId { get; set; }

    public Department? Department { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();

    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();

    public ICollection<TicketComment> TicketComments { get; set; } = new List<TicketComment>();

    public ICollection<TicketAttachment> UploadedAttachments { get; set; } = new List<TicketAttachment>();

    public ICollection<TicketAssignment> AssignmentsReceived { get; set; } = new List<TicketAssignment>();

    public ICollection<TicketAssignment> AssignmentsMade { get; set; } = new List<TicketAssignment>();

    public ICollection<TicketStatusHistory> StatusChanges { get; set; } = new List<TicketStatusHistory>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
