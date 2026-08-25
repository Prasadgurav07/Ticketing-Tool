using System.ComponentModel.DataAnnotations;

namespace Ticketing_Tool.Models;

public class TicketStatus
{
    [Key]
    public int StatusId { get; set; }

    [Required, StringLength(80)]
    public string StatusName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsFinal { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
