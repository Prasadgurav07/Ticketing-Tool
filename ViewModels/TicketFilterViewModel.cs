using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ticketing_Tool.ViewModels;

public class TicketFilterViewModel
{
    public string? Search { get; set; }

    public int? StatusId { get; set; }

    public int? CategoryId { get; set; }

    public int? PriorityId { get; set; }

    public int? DepartmentId { get; set; }

    public int? TeamId { get; set; }

    public string? AssignedToId { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public IReadOnlyList<TicketListItemViewModel> Tickets { get; set; } = [];

    public IEnumerable<SelectListItem> Statuses { get; set; } = [];

    public IEnumerable<SelectListItem> Categories { get; set; } = [];

    public IEnumerable<SelectListItem> Priorities { get; set; } = [];

    public IEnumerable<SelectListItem> Departments { get; set; } = [];

    public IEnumerable<SelectListItem> Teams { get; set; } = [];

    public IEnumerable<SelectListItem> Agents { get; set; } = [];
}
