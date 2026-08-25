using System.ComponentModel.DataAnnotations;

namespace Ticketing_Tool.Models;

public class Notification
{
    [Key]
    public int NotificationId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    public int TicketId { get; set; }

    public Ticket? Ticket { get; set; }

    [Required, StringLength(80)]
    public string NotificationType { get; set; } = string.Empty;

    [Required, StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(1000)]
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAt { get; set; }
}
