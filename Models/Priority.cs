using System.ComponentModel.DataAnnotations;

namespace Ticketing_Tool.Models;

public class Priority
{
    [Key]
    public int PriorityId { get; set; }

    [Required, StringLength(80)]
    public string PriorityName { get; set; } = string.Empty;

    public int PriorityLevel { get; set; }

    [StringLength(80)]
    public string? ResponseTime { get; set; }

    [StringLength(80)]
    public string? ResolutionTime { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
