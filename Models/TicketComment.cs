using System.ComponentModel.DataAnnotations;

namespace Ticketing_Tool.Models;

public class TicketComment
{
    [Key]
    public int CommentId { get; set; }

    public int TicketId { get; set; }

    public Ticket? Ticket { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    [Required, StringLength(4000)]
    public string CommentText { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
