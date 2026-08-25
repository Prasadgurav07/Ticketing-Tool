using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ticketing_Tool.ViewModels;

public class CreateTicketViewModel
{
    [Required, Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Required, Display(Name = "Department")]
    public int DepartmentId { get; set; }

    [Display(Name = "Team")]
    public int? TeamId { get; set; }

    [Required, Display(Name = "Priority")]
    public int PriorityId { get; set; }

    [Required, StringLength(200, MinimumLength = 5)]
    public string Subject { get; set; } = string.Empty;

    [Required, StringLength(4000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    public List<IFormFile> Attachments { get; set; } = [];

    public IEnumerable<SelectListItem> Categories { get; set; } = [];

    public IEnumerable<SelectListItem> Departments { get; set; } = [];

    public IEnumerable<SelectListItem> Teams { get; set; } = [];

    public IEnumerable<SelectListItem> Priorities { get; set; } = [];
}
