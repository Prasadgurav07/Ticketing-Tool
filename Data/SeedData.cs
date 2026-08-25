using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ticketing_Tool.Constants;
using Ticketing_Tool.Models;

namespace Ticketing_Tool.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");

        await context.Database.MigrateAsync();

        foreach (var roleName in new[] { RoleNames.Employee, RoleNames.SupportAgent, RoleNames.TeamLead, RoleNames.Admin })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        await SeedDepartmentsAsync(context);
        await SeedTeamsAsync(context);
        await SeedCategoriesAsync(context);
        await SeedPrioritiesAsync(context);
        await SeedStatusesAsync(context);

        var adminEmail = configuration["DevelopmentSeed:AdminEmail"];
        var adminPassword = configuration["DevelopmentSeed:AdminPassword"];

        if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
        {
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser is null)
            {
                var itDepartmentId = await context.Departments
                    .Where(department => department.DepartmentName == "IT")
                    .Select(department => department.DepartmentId)
                    .SingleAsync();

                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Development Admin",
                    EmployeeCode = "ADMIN-001",
                    DepartmentId = itDepartmentId,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (!result.Succeeded)
                {
                    logger.LogWarning("Development admin user was not created: {Errors}", string.Join("; ", result.Errors.Select(error => error.Description)));
                    return;
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, RoleNames.Admin))
            {
                await userManager.AddToRoleAsync(adminUser, RoleNames.Admin);
            }

            if (!await userManager.IsInRoleAsync(adminUser, RoleNames.SupportAgent))
            {
                await userManager.AddToRoleAsync(adminUser, RoleNames.SupportAgent);
            }
        }
    }

    private static async Task SeedDepartmentsAsync(ApplicationDbContext context)
    {
        var values = new[]
        {
            ("IT", "Information technology and support services"),
            ("HR", "Human resources"),
            ("Finance", "Finance and accounting"),
            ("Operations", "Operations support"),
            ("Sales", "Sales organization"),
            ("Admin", "Administrative services")
        };

        foreach (var (name, description) in values)
        {
            if (!await context.Departments.AnyAsync(department => department.DepartmentName == name))
            {
                context.Departments.Add(new Department { DepartmentName = name, Description = description });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedTeamsAsync(ApplicationDbContext context)
    {
        var departments = await context.Departments.ToDictionaryAsync(department => department.DepartmentName, department => department.DepartmentId);
        var values = new[]
        {
            ("IT", "Network Support", "Network, VPN, Wi-Fi, and connectivity"),
            ("IT", "IT Helpdesk", "General workplace support"),
            ("Finance", "Application Support", "Finance and business application support"),
            ("Operations", "Operations Support", "Operations systems support")
        };

        foreach (var (departmentName, teamName, description) in values)
        {
            if (departments.TryGetValue(departmentName, out var departmentId) &&
                !await context.Teams.AnyAsync(team => team.DepartmentId == departmentId && team.TeamName == teamName))
            {
                context.Teams.Add(new Team { DepartmentId = departmentId, TeamName = teamName, Description = description });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        foreach (var name in new[] { "Hardware", "Software", "Network", "Email", "Access", "Application", "Database", "Other" })
        {
            if (!await context.Categories.AnyAsync(category => category.CategoryName == name))
            {
                context.Categories.Add(new Category { CategoryName = name });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedPrioritiesAsync(ApplicationDbContext context)
    {
        var values = new[]
        {
            ("Critical", 1, "15 minutes", "4 hours"),
            ("High", 2, "1 hour", "1 business day"),
            ("Medium", 3, "4 hours", "3 business days"),
            ("Low", 4, "1 business day", "5 business days")
        };

        foreach (var (name, level, responseTime, resolutionTime) in values)
        {
            if (!await context.Priorities.AnyAsync(priority => priority.PriorityName == name))
            {
                context.Priorities.Add(new Priority
                {
                    PriorityName = name,
                    PriorityLevel = level,
                    ResponseTime = responseTime,
                    ResolutionTime = resolutionTime
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedStatusesAsync(ApplicationDbContext context)
    {
        var values = new[]
        {
            (TicketStatusNames.Open, "Ticket has been submitted", false),
            (TicketStatusNames.Assigned, "Ticket has an owner", false),
            (TicketStatusNames.InProgress, "Support is actively working on the ticket", false),
            (TicketStatusNames.PendingUser, "Support needs more information from the requester", false),
            (TicketStatusNames.Resolved, "Support has entered a resolution", false),
            (TicketStatusNames.Closed, "Requester or authorized user has closed the ticket", true),
            (TicketStatusNames.Cancelled, "Ticket was cancelled", true)
        };

        foreach (var (name, description, isFinal) in values)
        {
            if (!await context.TicketStatuses.AnyAsync(status => status.StatusName == name))
            {
                context.TicketStatuses.Add(new TicketStatus { StatusName = name, Description = description, IsFinal = isFinal });
            }
        }

        await context.SaveChangesAsync();
    }
}
