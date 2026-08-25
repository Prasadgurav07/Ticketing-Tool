using System.ComponentModel.DataAnnotations;

namespace Ticketing_Tool.Models;

public class TicketAttachment
{
    [Key]
    public int AttachmentId { get; set; }

    public int TicketId { get; set; }

    public Ticket? Ticket { get; set; }

    [Required, StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string FileType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    [Required, StringLength(600)]
    public string StoragePath { get; set; } = string.Empty;

    [Required]
    public string UploadedById { get; set; } = string.Empty;

    public ApplicationUser? UploadedBy { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
