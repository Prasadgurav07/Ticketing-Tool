using System.ComponentModel.DataAnnotations;

namespace Ticketing_Tool.Models;

public class TicketStatusHistory
{
    [Key]
    public int HistoryId { get; set; }

    public int TicketId { get; set; }

    public Ticket? Ticket { get; set; }

    public int? OldStatusId { get; set; }

    public TicketStatus? OldStatus { get; set; }

    public int NewStatusId { get; set; }

    public TicketStatus? NewStatus { get; set; }

    [Required]
    public string ChangedById { get; set; } = string.Empty;

    public ApplicationUser? ChangedBy { get; set; }

    [StringLength(1000)]
    public string? Comment { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
