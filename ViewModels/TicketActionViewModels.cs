using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Ticketing_Tool.ViewModels;

public class AddCommentViewModel
{
    [Required]
    public int TicketId { get; set; }

    [Required, StringLength(4000, MinimumLength = 2)]
    public string CommentText { get; set; } = string.Empty;
}

public class AssignTicketViewModel
{
    [Required]
    public int TicketId { get; set; }

    [Required]
    public string AssignedToId { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? AssignmentReason { get; set; }
}

public class ChangeStatusViewModel
{
    [Required]
    public int TicketId { get; set; }

    [Required]
    public int StatusId { get; set; }

    [StringLength(1000)]
    public string? Comment { get; set; }

    [StringLength(4000)]
    public string? Resolution { get; set; }
}

public class UploadAttachmentViewModel
{
    [Required]
    public int TicketId { get; set; }

    [Required]
    public IFormFile File { get; set; } = default!;
}

public class ResolutionConfirmationViewModel
{
    [Required]
    public int TicketId { get; set; }

    public bool IsFixed { get; set; }

    [StringLength(1000)]
    public string? Comment { get; set; }
}
