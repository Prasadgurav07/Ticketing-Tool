using System.ComponentModel.DataAnnotations;

namespace Ticketing_Tool.Models;

public class Ticket
{
    [Key]
    public int TicketId { get; set; }

    [Required, StringLength(32)]
    public string TicketNumber { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required, StringLength(4000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    public Department? Department { get; set; }

    public int? TeamId { get; set; }

    public Team? Team { get; set; }

    [Required]
    public int PriorityId { get; set; }

    public Priority? Priority { get; set; }

    [Required]
    public int StatusId { get; set; }

    public TicketStatus? Status { get; set; }

    [Required]
    public string CreatedById { get; set; } = string.Empty;

    public ApplicationUser? CreatedBy { get; set; }

    public string? AssignedToId { get; set; }

    public ApplicationUser? AssignedTo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    [StringLength(4000)]
    public string? Resolution { get; set; }

    public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();

    public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();

    public ICollection<TicketAssignment> Assignments { get; set; } = new List<TicketAssignment>();

    public ICollection<TicketStatusHistory> StatusHistory { get; set; } = new List<TicketStatusHistory>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
