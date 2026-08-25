using System.ComponentModel.DataAnnotations;

namespace Ticketing_Tool.Models;

public class Department
{
    [Key]
    public int DepartmentId { get; set; }

    [Required, StringLength(120)]
    public string DepartmentName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

    public ICollection<Team> Teams { get; set; } = new List<Team>();

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
