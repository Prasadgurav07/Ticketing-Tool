using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ticketing_Tool.Constants;
using Ticketing_Tool.Data;
using Ticketing_Tool.Models;
using Ticketing_Tool.ViewModels;

namespace Ticketing_Tool.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public class AdminController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = new AdminIndexViewModel
        {
            Users = await userManager.Users.CountAsync(cancellationToken),
            Roles = await roleManager.Roles.CountAsync(cancellationToken),
            Departments = await context.Departments.CountAsync(cancellationToken),
            Teams = await context.Teams.CountAsync(cancellationToken),
            Categories = await context.Categories.CountAsync(cancellationToken),
            Priorities = await context.Priorities.CountAsync(cancellationToken),
            Statuses = await context.TicketStatuses.CountAsync(cancellationToken),
            Tickets = await context.Tickets.CountAsync(cancellationToken)
        };

        return View(model);
    }

    public async Task<IActionResult> Departments(CancellationToken cancellationToken) =>
        View(await context.Departments.OrderBy(department => department.DepartmentName).ToListAsync(cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveDepartment(Department input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.DepartmentName))
        {
            TempData["Error"] = "Department name is required.";
            return RedirectToAction(nameof(Departments));
        }

        if (input.DepartmentId == 0)
        {
            context.Departments.Add(new Department
            {
                DepartmentName = input.DepartmentName.Trim(),
                Description = input.Description?.Trim(),
                IsActive = input.IsActive
            });
        }
        else
        {
            var department = await context.Departments.FindAsync([input.DepartmentId], cancellationToken);
            if (department is null)
            {
                return NotFound();
            }

            department.DepartmentName = input.DepartmentName.Trim();
            department.Description = input.Description?.Trim();
            department.IsActive = input.IsActive;
        }

        await context.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Department saved.";
        return RedirectToAction(nameof(Departments));
    }

    public async Task<IActionResult> Teams(CancellationToken cancellationToken)
    {
        ViewBag.Departments = await DepartmentOptionsAsync(cancellationToken);
        return View(await context.Teams.Include(team => team.Department).OrderBy(team => team.TeamName).ToListAsync(cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTeam(Team input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.TeamName) || input.DepartmentId == 0)
        {
            TempData["Error"] = "Team name and department are required.";
            return RedirectToAction(nameof(Teams));
        }

        if (input.TeamId == 0)
        {
            context.Teams.Add(new Team
            {
                DepartmentId = input.DepartmentId,
                TeamName = input.TeamName.Trim(),
                Description = input.Description?.Trim(),
                IsActive = input.IsActive
            });
        }
        else
        {
            var team = await context.Teams.FindAsync([input.TeamId], cancellationToken);
            if (team is null)
            {
                return NotFound();
            }

            team.DepartmentId = input.DepartmentId;
            team.TeamName = input.TeamName.Trim();
            team.Description = input.Description?.Trim();
            team.IsActive = input.IsActive;
        }

        await context.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Team saved.";
        return RedirectToAction(nameof(Teams));
    }

    public async Task<IActionResult> Categories(CancellationToken cancellationToken) =>
        View(await context.Categories.OrderBy(category => category.CategoryName).ToListAsync(cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCategory(Category input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.CategoryName))
        {
            TempData["Error"] = "Category name is required.";
            return RedirectToAction(nameof(Categories));
        }

        if (input.CategoryId == 0)
        {
            context.Categories.Add(new Category
            {
                CategoryName = input.CategoryName.Trim(),
                Description = input.Description?.Trim(),
                IsActive = input.IsActive
            });
        }
        else
        {
            var category = await context.Categories.FindAsync([input.CategoryId], cancellationToken);
            if (category is null)
            {
                return NotFound();
            }

            category.CategoryName = input.CategoryName.Trim();
            category.Description = input.Description?.Trim();
            category.IsActive = input.IsActive;
        }

        await context.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Category saved.";
        return RedirectToAction(nameof(Categories));
    }

    public async Task<IActionResult> Priorities(CancellationToken cancellationToken) =>
        View(await context.Priorities.OrderBy(priority => priority.PriorityLevel).ToListAsync(cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePriority(Priority input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.PriorityName))
        {
            TempData["Error"] = "Priority name is required.";
            return RedirectToAction(nameof(Priorities));
        }

        if (input.PriorityId == 0)
        {
            context.Priorities.Add(new Priority
            {
                PriorityName = input.PriorityName.Trim(),
                PriorityLevel = input.PriorityLevel,
                ResponseTime = input.ResponseTime?.Trim(),
                ResolutionTime = input.ResolutionTime?.Trim()
            });
        }
        else
        {
            var priority = await context.Priorities.FindAsync([input.PriorityId], cancellationToken);
            if (priority is null)
            {
                return NotFound();
            }

            priority.PriorityName = input.PriorityName.Trim();
            priority.PriorityLevel = input.PriorityLevel;
            priority.ResponseTime = input.ResponseTime?.Trim();
            priority.ResolutionTime = input.ResolutionTime?.Trim();
        }

        await context.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Priority saved.";
        return RedirectToAction(nameof(Priorities));
    }

    public async Task<IActionResult> Statuses(CancellationToken cancellationToken) =>
        View(await context.TicketStatuses.OrderBy(status => status.StatusId).ToListAsync(cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveStatus(TicketStatus input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.StatusName))
        {
            TempData["Error"] = "Status name is required.";
            return RedirectToAction(nameof(Statuses));
        }

        if (input.StatusId == 0)
        {
            context.TicketStatuses.Add(new TicketStatus
            {
                StatusName = input.StatusName.Trim(),
                Description = input.Description?.Trim(),
                IsFinal = input.IsFinal
            });
        }
        else
        {
            var status = await context.TicketStatuses.FindAsync([input.StatusId], cancellationToken);
            if (status is null)
            {
                return NotFound();
            }

            status.StatusName = input.StatusName.Trim();
            status.Description = input.Description?.Trim();
            status.IsFinal = input.IsFinal;
        }

        await context.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Status saved.";
        return RedirectToAction(nameof(Statuses));
    }

    public async Task<IActionResult> Users(CancellationToken cancellationToken)
    {
        var users = await userManager.Users.Include(user => user.Department).OrderBy(user => user.Email).ToListAsync(cancellationToken);
        var departments = await DepartmentOptionsAsync(cancellationToken);
        var roles = await roleManager.Roles.OrderBy(role => role.Name).Select(role => new SelectListItem(role.Name!, role.Name!)).ToListAsync(cancellationToken);
        var model = new List<AdminUserViewModel>();

        foreach (var user in users)
        {
            var userRoles = await userManager.GetRolesAsync(user);
            model.Add(new AdminUserViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? user.UserName ?? string.Empty,
                FullName = user.FullName,
                EmployeeCode = user.EmployeeCode,
                DepartmentId = user.DepartmentId,
                Department = user.Department?.DepartmentName ?? string.Empty,
                IsActive = user.IsActive,
                Roles = string.Join(", ", userRoles),
                Departments = departments,
                AvailableRoles = roles
            });
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveUser(AdminUserViewModel input, string? roleName)
    {
        var user = await userManager.FindByIdAsync(input.UserId);
        if (user is null)
        {
            return NotFound();
        }

        user.FullName = input.FullName?.Trim() ?? string.Empty;
        user.EmployeeCode = input.EmployeeCode?.Trim();
        user.DepartmentId = input.DepartmentId;
        user.IsActive = input.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            await userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        if (!string.IsNullOrWhiteSpace(roleName) && await roleManager.RoleExistsAsync(roleName))
        {
            await userManager.AddToRoleAsync(user, roleName);
        }

        TempData["Success"] = "User saved.";
        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> Roles(CancellationToken cancellationToken) =>
        View(await roleManager.Roles.OrderBy(role => role.Name).ToListAsync(cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRole(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            TempData["Error"] = "Role name is required.";
            return RedirectToAction(nameof(Roles));
        }

        if (!await roleManager.RoleExistsAsync(roleName.Trim()))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName.Trim()));
        }

        TempData["Success"] = "Role saved.";
        return RedirectToAction(nameof(Roles));
    }

    private async Task<List<SelectListItem>> DepartmentOptionsAsync(CancellationToken cancellationToken)
    {
        return await context.Departments
            .Where(department => department.IsActive)
            .OrderBy(department => department.DepartmentName)
            .Select(department => new SelectListItem(department.DepartmentName, department.DepartmentId.ToString()))
            .ToListAsync(cancellationToken);
    }
}
