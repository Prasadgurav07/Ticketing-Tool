using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ticketing_Tool.Constants;
using Ticketing_Tool.Data;
using Ticketing_Tool.Models;
using Ticketing_Tool.Services;
using Ticketing_Tool.ViewModels;

namespace Ticketing_Tool.Controllers;

[Authorize]
public class TicketsController(
    ApplicationDbContext context,
    ITicketService ticketService,
    ITicketAuthorizationService authorizationService,
    IStatusTransitionService statusTransitionService,
    IFileStorageService fileStorageService,
    UserManager<ApplicationUser> userManager,
    ILogger<TicketsController> logger) : Controller
{
    public async Task<IActionResult> Index([FromQuery] TicketFilterViewModel filter, CancellationToken cancellationToken)
    {
        var model = await ticketService.SearchTicketsAsync(filter, User, cancellationToken);
        await PopulateFilterLookupsAsync(model, authorizationService.IsSupportUser(User), cancellationToken);
        return View(model);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new CreateTicketViewModel();
        await PopulateCreateLookupsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTicketViewModel model, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (!ModelState.IsValid)
        {
            await PopulateCreateLookupsAsync(model, cancellationToken);
            return View(model);
        }

        try
        {
            var ticket = await ticketService.CreateTicketAsync(model, userId, cancellationToken);
            TempData["Success"] = $"Ticket created successfully. Ticket number: {ticket.TicketNumber}.";
            return RedirectToAction(nameof(Details), new { id = ticket.TicketId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ticket creation failed for user {UserId}", userId);
            ModelState.AddModelError(string.Empty, "Something went wrong while creating the ticket. Please try again.");
        }

        await PopulateCreateLookupsAsync(model, cancellationToken);
        return View(model);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var ticket = await ticketService.GetTicketDetailsAsync(id, cancellationToken);
        if (ticket is null)
        {
            return NotFound();
        }

        if (!authorizationService.CanViewTicket(ticket, User))
        {
            return Forbid();
        }

        var allowedStatusNames = statusTransitionService.GetAllowedStatusNames(ticket, User);
        var allowedStatuses = await context.TicketStatuses
            .Where(status => allowedStatusNames.Contains(status.StatusName))
            .OrderBy(status => status.StatusId)
            .ToListAsync(cancellationToken);

        var model = new TicketDetailsViewModel
        {
            Ticket = ticket,
            AllowedStatuses = allowedStatuses,
            AssignableUsers = authorizationService.IsSupportUser(User)
                ? await ticketService.GetAssignableUsersAsync(cancellationToken)
                : []
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(AddCommentViewModel model, CancellationToken cancellationToken)
    {
        var ticket = await ticketService.GetTicketDetailsAsync(model.TicketId, cancellationToken);
        if (ticket is null)
        {
            return NotFound();
        }

        if (!authorizationService.CanAddComment(ticket, User))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Enter a comment before posting.";
            return RedirectToAction(nameof(Details), new { id = model.TicketId });
        }

        try
        {
            await ticketService.AddCommentAsync(model.TicketId, GetCurrentUserId(), model.CommentText, cancellationToken);
            TempData["Success"] = "Comment added.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Adding comment failed for ticket {TicketId}", model.TicketId);
            TempData["Error"] = "Something went wrong while adding the comment.";
        }

        return RedirectToAction(nameof(Details), new { id = model.TicketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAttachment(UploadAttachmentViewModel model, CancellationToken cancellationToken)
    {
        if (model.File is null)
        {
            TempData["Error"] = "Choose a file before uploading.";
            return RedirectToAction(nameof(Details), new { id = model.TicketId });
        }

        var ticket = await ticketService.GetTicketDetailsAsync(model.TicketId, cancellationToken);
        if (ticket is null)
        {
            return NotFound();
        }

        if (!authorizationService.CanAddComment(ticket, User))
        {
            return Forbid();
        }

        try
        {
            await ticketService.AddAttachmentAsync(model.TicketId, GetCurrentUserId(), model.File, cancellationToken);
            TempData["Success"] = "Attachment uploaded.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Uploading attachment failed for ticket {TicketId}", model.TicketId);
            TempData["Error"] = "Something went wrong while uploading the attachment.";
        }

        return RedirectToAction(nameof(Details), new { id = model.TicketId });
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.SupportOrAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(AssignTicketViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Choose a support user before assigning the ticket.";
            return RedirectToAction(nameof(Details), new { id = model.TicketId });
        }

        try
        {
            await ticketService.AssignTicketAsync(model.TicketId, model.AssignedToId, GetCurrentUserId(), model.AssignmentReason, cancellationToken);
            TempData["Success"] = "Ticket assignment updated.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = model.TicketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(ChangeStatusViewModel model, CancellationToken cancellationToken)
    {
        var ticket = await ticketService.GetTicketDetailsAsync(model.TicketId, cancellationToken);
        if (ticket is null)
        {
            return NotFound();
        }

        if (!authorizationService.CanViewTicket(ticket, User))
        {
            return Forbid();
        }

        try
        {
            await ticketService.ChangeStatusAsync(model.TicketId, model.StatusId, GetCurrentUserId(), User, model.Comment, model.Resolution, cancellationToken);
            TempData["Success"] = "Ticket status updated.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = model.TicketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmResolution(ResolutionConfirmationViewModel model, CancellationToken cancellationToken)
    {
        var ticket = await ticketService.GetTicketDetailsAsync(model.TicketId, cancellationToken);
        if (ticket is null)
        {
            return NotFound();
        }

        if (!authorizationService.CanViewTicket(ticket, User))
        {
            return Forbid();
        }

        try
        {
            await ticketService.ConfirmResolutionAsync(model.TicketId, GetCurrentUserId(), model.IsFixed, model.Comment, User, cancellationToken);
            TempData["Success"] = model.IsFixed ? "Ticket closed." : "Ticket reopened.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = model.TicketId });
    }

    public async Task<IActionResult> DownloadAttachment(int id, CancellationToken cancellationToken)
    {
        var attachment = await ticketService.GetAttachmentAsync(id, cancellationToken);
        if (attachment?.Ticket is null)
        {
            return NotFound();
        }

        if (!authorizationService.CanViewTicket(attachment.Ticket, User))
        {
            return Forbid();
        }

        var path = fileStorageService.GetPhysicalPath(attachment.StoragePath);
        if (!System.IO.File.Exists(path))
        {
            return NotFound();
        }

        return PhysicalFile(path, attachment.FileType, attachment.OriginalFileName);
    }

    private string GetCurrentUserId() =>
        userManager.GetUserId(User) ?? throw new InvalidOperationException("Signed-in user was not found.");

    private async Task PopulateCreateLookupsAsync(CreateTicketViewModel model, CancellationToken cancellationToken)
    {
        model.Categories = await context.Categories
            .Where(category => category.IsActive)
            .OrderBy(category => category.CategoryName)
            .Select(category => new SelectListItem(category.CategoryName, category.CategoryId.ToString(), category.CategoryId == model.CategoryId))
            .ToListAsync(cancellationToken);
        model.Departments = await context.Departments
            .Where(department => department.IsActive)
            .OrderBy(department => department.DepartmentName)
            .Select(department => new SelectListItem(department.DepartmentName, department.DepartmentId.ToString(), department.DepartmentId == model.DepartmentId))
            .ToListAsync(cancellationToken);
        model.Teams = await context.Teams
            .Where(team => team.IsActive)
            .OrderBy(team => team.TeamName)
            .Select(team => new SelectListItem($"{team.TeamName} ({team.Department!.DepartmentName})", team.TeamId.ToString(), team.TeamId == model. TeamId))
            .ToListAsync(cancellationToken);
        model.Priorities = await context.Priorities
            .OrderBy(priority => priority.PriorityLevel)
            .Select(priority => new SelectListItem(priority.PriorityName, priority.PriorityId.ToString(), priority.PriorityId == model.PriorityId))
            .ToListAsync(cancellationToken);
    }

    private async Task PopulateFilterLookupsAsync(TicketFilterViewModel model, bool includeAgents, CancellationToken cancellationToken)
    {
        model.Statuses = await context.TicketStatuses.OrderBy(status => status.StatusId)
            .Select(status => new SelectListItem(status.StatusName, status.StatusId.ToString(), status.StatusId == model.StatusId))
            .ToListAsync(cancellationToken);
        model.Categories = await context.Categories.Where(category => category.IsActive).OrderBy(category => category.CategoryName)
            .Select(category => new SelectListItem(category.CategoryName, category.CategoryId.ToString(), category.CategoryId == model.CategoryId))
            .ToListAsync(cancellationToken);
        model.Priorities = await context.Priorities.OrderBy(priority => priority.PriorityLevel)
            .Select(priority => new SelectListItem(priority.PriorityName, priority.PriorityId.ToString(), priority.PriorityId == model.PriorityId))
            .ToListAsync(cancellationToken);
        model.Departments = await context.Departments.Where(department => department.IsActive).OrderBy(department => department.DepartmentName)
            .Select(department => new SelectListItem(department.DepartmentName, department.DepartmentId.ToString(), department.DepartmentId == model.DepartmentId))
            .ToListAsync(cancellationToken);
        model.Teams = await context.Teams.Where(team => team.IsActive).OrderBy(team => team.TeamName)
            .Select(team => new SelectListItem($"{team.TeamName} ({team.Department!.DepartmentName})", team.TeamId.ToString(), team.TeamId == model.TeamId))
            .ToListAsync(cancellationToken);
        model.Agents = includeAgents
            ? (await ticketService.GetAssignableUsersAsync(cancellationToken))
                .Select(agent => new SelectListItem(!string.IsNullOrWhiteSpace(agent.FullName) ? agent.FullName : agent.Email, agent.Id, agent.Id == model.AssignedToId))
                .ToList()
            : [];
    }
}
