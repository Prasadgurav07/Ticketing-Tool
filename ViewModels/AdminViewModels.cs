using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ticketing_Tool.ViewModels;

public class AdminIndexViewModel
{
    public int Users { get; set; }

    public int Roles { get; set; }

    public int Departments { get; set; }

    public int Teams { get; set; }

    public int Categories { get; set; }

    public int Priorities { get; set; }

    public int Statuses { get; set; }

    public int Tickets { get; set; }
}

public class AdminUserViewModel
{
    public string UserId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? EmployeeCode { get; set; }

    public int? DepartmentId { get; set; }

    public string Department { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public string Roles { get; set; } = string.Empty;

    public IEnumerable<SelectListItem> Departments { get; set; } = [];

    public IEnumerable<SelectListItem> AvailableRoles { get; set; } = [];
}
