using System.ComponentModel.DataAnnotations;

namespace Ticketing_Tool.Models;

public class Category
{
    [Key]
    public int CategoryId { get; set; }

    [Required, StringLength(120)]
    public string CategoryName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
