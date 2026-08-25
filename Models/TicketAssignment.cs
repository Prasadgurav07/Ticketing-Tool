using System.ComponentModel.DataAnnotations;

namespace Ticketing_Tool.Models;

public class TicketAssignment
{
    [Key]
    public int AssignmentId { get; set; }

    public int TicketId { get; set; }

    public Ticket? Ticket { get; set; }

    [Required]
    public string AssignedToId { get; set; } = string.Empty;

    public ApplicationUser? AssignedTo { get; set; }

    [Required]
    public string AssignedById { get; set; } = string.Empty;

    public ApplicationUser? AssignedBy { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UnassignedAt { get; set; }

    [StringLength(1000)]
    public string? AssignmentReason { get; set; }
}
