using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ticketing_Tool.Constants;
using Ticketing_Tool.Data;
using Ticketing_Tool.Models;
using Ticketing_Tool.ViewModels;

namespace Ticketing_Tool.Services;

public class TicketService(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    ITicketNumberGenerator ticketNumberGenerator,
    IFileStorageService fileStorageService,
    INotificationService notificationService,
    ITicketAuthorizationService authorizationService,
    IStatusTransitionService statusTransitionService) : ITicketService
{
    public async Task<DashboardViewModel> GetEmployeeDashboardAsync(string userId, CancellationToken cancellationToken = default)
    {
        var query = context.Tickets.AsNoTracking().Where(ticket => ticket.CreatedById == userId);
        return await BuildDashboardAsync(query, userId, "Employee Dashboard", cancellationToken);
    }

    public async Task<DashboardViewModel> GetSupportDashboardAsync(CancellationToken cancellationToken = default)
    {
        var query = context.Tickets.AsNoTracking();
        return await BuildDashboardAsync(query, null, "Support Dashboard", cancellationToken);
    }

    public async Task<TicketFilterViewModel> SearchTicketsAsync(TicketFilterViewModel filter, ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        var userId = userManager.GetUserId(user);
        var query = context.Tickets
            .AsNoTracking()
            .Include(ticket => ticket.Category)
            .Include(ticket => ticket.Department)
            .Include(ticket => ticket.Team)
            .Include(ticket => ticket.Priority)
            .Include(ticket => ticket.Status)
            .Include(ticket => ticket.CreatedBy)
            .Include(ticket => ticket.AssignedTo)
            .AsQueryable();

        if (!authorizationService.IsSupportUser(user))
        {
            query = query.Where(ticket => ticket.CreatedById == userId);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(ticket => ticket.TicketNumber.Contains(search) || ticket.Subject.Contains(search));
        }

        if (filter.StatusId.HasValue)
        {
            query = query.Where(ticket => ticket.StatusId == filter.StatusId.Value);
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(ticket => ticket.CategoryId == filter.CategoryId.Value);
        }

        if (filter.PriorityId.HasValue)
        {
            query = query.Where(ticket => ticket.PriorityId == filter.PriorityId.Value);
        }

        if (filter.DepartmentId.HasValue)
        {
            query = query.Where(ticket => ticket.DepartmentId == filter.DepartmentId.Value);
        }

        if (filter.TeamId.HasValue)
        {
            query = query.Where(ticket => ticket.TeamId == filter.TeamId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.AssignedToId))
        {
            query = query.Where(ticket => ticket.AssignedToId == filter.AssignedToId);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(ticket => ticket.CreatedAt >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            var inclusiveToDate = filter.ToDate.Value.Date.AddDays(1);
            query = query.Where(ticket => ticket.CreatedAt < inclusiveToDate);
        }

        var tickets = await query
            .OrderByDescending(ticket => ticket.CreatedAt)
            .Take(250)
            .ToListAsync(cancellationToken);

        filter.Tickets = tickets.Select(MapTicketListItem).ToList();
        return filter;
    }

    public async Task<Ticket> CreateTicketAsync(CreateTicketViewModel model, string userId, CancellationToken cancellationToken = default)
    {
        await ValidateTicketLookupsAsync(model, cancellationToken);

        var openStatus = await GetStatusByNameAsync(TicketStatusNames.Open, cancellationToken);
        var ticketNumber = await ticketNumberGenerator.GenerateAsync(cancellationToken);

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var ticket = new Ticket
        {
            TicketNumber = ticketNumber,
            CategoryId = model.CategoryId,
            DepartmentId = model.DepartmentId,
            TeamId = model.TeamId,
            PriorityId = model.PriorityId,
            StatusId = openStatus.StatusId,
            Subject = model.Subject.Trim(),
            Description = model.Description.Trim(),
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Tickets.Add(ticket);
        ticket.StatusHistory.Add(new TicketStatusHistory
        {
            NewStatusId = openStatus.StatusId,
            ChangedById = userId,
            ChangedAt = DateTime.UtcNow,
            Comment = "Ticket created"
        });

        await context.SaveChangesAsync(cancellationToken);

        foreach (var file in model.Attachments.Where(file => file.Length > 0))
        {
            var attachment = await fileStorageService.SaveAsync(ticket.TicketId, userId, file, cancellationToken);
            context.TicketAttachments.Add(attachment);
        }

        await context.SaveChangesAsync(cancellationToken);
        await notificationService.CreateAsync(
            userId,
            ticket.TicketId,
            NotificationTypes.TicketCreated,
            $"Ticket {ticket.TicketNumber} created",
            "Your ticket was submitted successfully.",
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return ticket;
    }

    public async Task<Ticket?> GetTicketDetailsAsync(int ticketId, CancellationToken cancellationToken = default)
    {
        return await context.Tickets
            .Include(ticket => ticket.Category)
            .Include(ticket => ticket.Department)
            .Include(ticket => ticket.Team)
            .Include(ticket => ticket.Priority)
            .Include(ticket => ticket.Status)
            .Include(ticket => ticket.CreatedBy)
            .Include(ticket => ticket.AssignedTo)
            .Include(ticket => ticket.Comments).ThenInclude(comment => comment.User)
            .Include(ticket => ticket.Attachments).ThenInclude(attachment => attachment.UploadedBy)
            .Include(ticket => ticket.Assignments).ThenInclude(assignment => assignment.AssignedTo)
            .Include(ticket => ticket.Assignments).ThenInclude(assignment => assignment.AssignedBy)
            .Include(ticket => ticket.StatusHistory).ThenInclude(history => history.OldStatus)
            .Include(ticket => ticket.StatusHistory).ThenInclude(history => history.NewStatus)
            .Include(ticket => ticket.StatusHistory).ThenInclude(history => history.ChangedBy)
            .SingleOrDefaultAsync(ticket => ticket.TicketId == ticketId, cancellationToken);
    }

    public async Task AddCommentAsync(int ticketId, string userId, string commentText, CancellationToken cancellationToken = default)
    {
        var ticket = await GetTicketForUpdateAsync(ticketId, cancellationToken);
        context.TicketComments.Add(new TicketComment
        {
            TicketId = ticketId,
            UserId = userId,
            CommentText = commentText.Trim(),
            CreatedAt = DateTime.UtcNow
        });

        ticket.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        await notificationService.NotifyParticipantsAsync(
            ticket,
            NotificationTypes.CommentAdded,
            $"Comment added to {ticket.TicketNumber}",
            commentText.Trim(),
            userId,
            cancellationToken: cancellationToken);
    }

    public async Task AddAttachmentAsync(int ticketId, string userId, IFormFile file, CancellationToken cancellationToken = default)
    {
        var ticket = await GetTicketForUpdateAsync(ticketId, cancellationToken);
        var attachment = await fileStorageService.SaveAsync(ticketId, userId, file, cancellationToken);
        context.TicketAttachments.Add(attachment);
        ticket.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        await notificationService.NotifyParticipantsAsync(
            ticket,
            NotificationTypes.CommentAdded,
            $"Attachment added to {ticket.TicketNumber}",
            attachment.OriginalFileName,
            userId,
            cancellationToken: cancellationToken);
    }

    public async Task AssignTicketAsync(int ticketId, string assignedToId, string assignedById, string? reason, CancellationToken cancellationToken = default)
    {
        var ticket = await context.Tickets
            .Include(item => item.Status)
            .Include(item => item.Assignments)
            .SingleOrDefaultAsync(item => item.TicketId == ticketId, cancellationToken)
            ?? throw new InvalidOperationException("Ticket was not found.");

        var assignedUser = await userManager.FindByIdAsync(assignedToId)
            ?? throw new InvalidOperationException("Assigned user was not found.");

        if (!assignedUser.IsActive)
        {
            throw new InvalidOperationException("Assigned user is inactive.");
        }

        var roles = await userManager.GetRolesAsync(assignedUser);
        if (!roles.Any(role => role is RoleNames.SupportAgent or RoleNames.TeamLead or RoleNames.Admin))
        {
            throw new InvalidOperationException("Tickets can only be assigned to support users.");
        }

        var now = DateTime.UtcNow;
        var previousAssignee = ticket.AssignedToId;
        foreach (var openAssignment in ticket.Assignments.Where(assignment => assignment.UnassignedAt is null))
        {
            openAssignment.UnassignedAt = now;
        }

        ticket.AssignedToId = assignedToId;
        ticket.UpdatedAt = now;
        ticket.Assignments.Add(new TicketAssignment
        {
            AssignedToId = assignedToId,
            AssignedById = assignedById,
            AssignedAt = now,
            AssignmentReason = reason?.Trim()
        });

        if (ticket.Status?.StatusName == TicketStatusNames.Open)
        {
            var assignedStatus = await GetStatusByNameAsync(TicketStatusNames.Assigned, cancellationToken);
            ticket.StatusHistory.Add(new TicketStatusHistory
            {
                OldStatusId = ticket.StatusId,
                NewStatusId = assignedStatus.StatusId,
                ChangedById = assignedById,
                ChangedAt = now,
                Comment = "Ticket assigned"
            });
            ticket.StatusId = assignedStatus.StatusId;
        }

        await context.SaveChangesAsync(cancellationToken);
        var notificationType = string.IsNullOrWhiteSpace(previousAssignee) ? NotificationTypes.TicketAssigned : NotificationTypes.TicketReassigned;
        await notificationService.NotifyParticipantsAsync(
            ticket,
            notificationType,
            $"Ticket {ticket.TicketNumber} assigned",
            $"Assigned to {DisplayUser(assignedUser)}.",
            assignedById,
            includeActor: true,
            cancellationToken);
    }

    public async Task ChangeStatusAsync(int ticketId, int newStatusId, string changedById, ClaimsPrincipal user, string? comment, string? resolution, CancellationToken cancellationToken = default)
    {
        var ticket = await GetTicketForUpdateAsync(ticketId, cancellationToken);
        var newStatus = await context.TicketStatuses.SingleOrDefaultAsync(status => status.StatusId == newStatusId, cancellationToken)
            ?? throw new InvalidOperationException("Selected status was not found.");

        if (ticket.StatusId == newStatus.StatusId)
        {
            return;
        }

        if (!statusTransitionService.CanTransition(ticket, newStatus, user))
        {
            throw new InvalidOperationException("That status change is not allowed.");
        }

        var now = DateTime.UtcNow;
        var oldStatusId = ticket.StatusId;
        var oldStatusName = ticket.Status?.StatusName ?? string.Empty;
        var normalizedComment = comment?.Trim();

        if (newStatus.StatusName == TicketStatusNames.Resolved)
        {
            if (string.IsNullOrWhiteSpace(resolution))
            {
                throw new InvalidOperationException("A resolution is required before resolving a ticket.");
            }

            ticket.Resolution = resolution.Trim();
            ticket.ResolvedAt = now;
            normalizedComment = string.IsNullOrWhiteSpace(normalizedComment) ? "Ticket resolved" : normalizedComment;
        }
        else if (newStatus.StatusName == TicketStatusNames.Closed)
        {
            ticket.ClosedAt = now;
            normalizedComment = string.IsNullOrWhiteSpace(normalizedComment) ? "Ticket closed" : normalizedComment;
        }
        else if (newStatus.StatusName == TicketStatusNames.InProgress &&
                 (oldStatusName == TicketStatusNames.Resolved || oldStatusName == TicketStatusNames.Closed))
        {
            ticket.ResolvedAt = null;
            ticket.ClosedAt = null;
            ticket.Resolution = null;
            normalizedComment = string.IsNullOrWhiteSpace(normalizedComment) ? "Ticket reopened" : normalizedComment;
        }

        ticket.StatusId = newStatus.StatusId;
        ticket.UpdatedAt = now;
        ticket.StatusHistory.Add(new TicketStatusHistory
        {
            OldStatusId = oldStatusId,
            NewStatusId = newStatus.StatusId,
            ChangedById = changedById,
            ChangedAt = now,
            Comment = normalizedComment
        });

        await context.SaveChangesAsync(cancellationToken);
        await notificationService.NotifyParticipantsAsync(
            ticket,
            GetNotificationTypeForStatus(newStatus.StatusName, oldStatusName),
            $"Ticket {ticket.TicketNumber} status changed",
            $"Status changed from {oldStatusName} to {newStatus.StatusName}.",
            changedById,
            includeActor: false,
            cancellationToken);
    }

    public async Task ConfirmResolutionAsync(int ticketId, string userId, bool isFixed, string? comment, ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        var targetStatusName = isFixed ? TicketStatusNames.Closed : TicketStatusNames.InProgress;
        var targetStatus = await GetStatusByNameAsync(targetStatusName, cancellationToken);
        var statusComment = isFixed
            ? "Requester confirmed the issue is fixed."
            : $"Requester reported the issue is not fixed. {comment}".Trim();

        await ChangeStatusAsync(ticketId, targetStatus.StatusId, userId, user, statusComment, null, cancellationToken);
    }

    public async Task<TicketAttachment?> GetAttachmentAsync(int attachmentId, CancellationToken cancellationToken = default)
    {
        return await context.TicketAttachments
            .Include(attachment => attachment.Ticket)
            .SingleOrDefaultAsync(attachment => attachment.AttachmentId == attachmentId, cancellationToken);
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetAssignableUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await userManager.Users
            .Include(user => user.Department)
            .Where(user => user.IsActive)
            .OrderBy(user => user.FullName)
            .ThenBy(user => user.Email)
            .ToListAsync(cancellationToken);

        var assignable = new List<ApplicationUser>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            if (roles.Any(role => role is RoleNames.SupportAgent or RoleNames.TeamLead or RoleNames.Admin))
            {
                assignable.Add(user);
            }
        }

        return assignable;
    }

    private async Task<DashboardViewModel> BuildDashboardAsync(IQueryable<Ticket> query, string? userId, string title, CancellationToken cancellationToken)
    {
        var recentTickets = await query
            .Include(ticket => ticket.Category)
            .Include(ticket => ticket.Department)
            .Include(ticket => ticket.Team)
            .Include(ticket => ticket.Priority)
            .Include(ticket => ticket.Status)
            .Include(ticket => ticket.CreatedBy)
            .Include(ticket => ticket.AssignedTo)
            .OrderByDescending(ticket => ticket.CreatedAt)
            .Take(8)
            .ToListAsync(cancellationToken);

        var unreadNotifications = string.IsNullOrWhiteSpace(userId)
            ? 0
            : await context.Notifications.CountAsync(notification => notification.UserId == userId && !notification.IsRead, cancellationToken);

        return new DashboardViewModel
        {
            Title = title,
            TotalTickets = await query.CountAsync(cancellationToken),
            OpenTickets = await query.CountAsync(ticket => ticket.Status!.StatusName == TicketStatusNames.Open, cancellationToken),
            AssignedTickets = await query.CountAsync(ticket => ticket.Status!.StatusName == TicketStatusNames.Assigned, cancellationToken),
            InProgressTickets = await query.CountAsync(ticket => ticket.Status!.StatusName == TicketStatusNames.InProgress, cancellationToken),
            PendingUserTickets = await query.CountAsync(ticket => ticket.Status!.StatusName == TicketStatusNames.PendingUser, cancellationToken),
            ResolvedTickets = await query.CountAsync(ticket => ticket.Status!.StatusName == TicketStatusNames.Resolved, cancellationToken),
            ClosedTickets = await query.CountAsync(ticket => ticket.Status!.StatusName == TicketStatusNames.Closed, cancellationToken),
            CriticalHighTickets = await query.CountAsync(ticket => ticket.Priority!.PriorityName == "Critical" || ticket.Priority!.PriorityName == "High", cancellationToken),
            UnreadNotifications = unreadNotifications,
            RecentTickets = recentTickets.Select(MapTicketListItem).ToList()
        };
    }

    private async Task ValidateTicketLookupsAsync(CreateTicketViewModel model, CancellationToken cancellationToken)
    {
        if (!await context.Categories.AnyAsync(category => category.CategoryId == model.CategoryId && category.IsActive, cancellationToken))
        {
            throw new InvalidOperationException("Select a valid category.");
        }

        if (!await context.Departments.AnyAsync(department => department.DepartmentId == model.DepartmentId && department.IsActive, cancellationToken))
        {
            throw new InvalidOperationException("Select a valid department.");
        }

        if (!await context.Priorities.AnyAsync(priority => priority.PriorityId == model.PriorityId, cancellationToken))
        {
            throw new InvalidOperationException("Select a valid priority.");
        }

        //if (model.TeamId.HasValue &&
        //    !await context.Teams.AnyAsync(team => team.TeamId == model.TeamId.Value && team.DepartmentId == model.DepartmentId && team.IsActive, cancellationToken))
        //{
        //    throw new InvalidOperationException("Select a valid team for the chosen department.");
        //}
    }

    private async Task<Ticket> GetTicketForUpdateAsync(int ticketId, CancellationToken cancellationToken)
    {
        return await context.Tickets
            .Include(ticket => ticket.Status)
            .Include(ticket => ticket.CreatedBy)
            .Include(ticket => ticket.AssignedTo)
            .SingleOrDefaultAsync(ticket => ticket.TicketId == ticketId, cancellationToken)
            ?? throw new InvalidOperationException("Ticket was not found.");
    }

    private async Task<TicketStatus> GetStatusByNameAsync(string statusName, CancellationToken cancellationToken)
    {
        return await context.TicketStatuses.SingleAsync(status => status.StatusName == statusName, cancellationToken);
    }

    private static TicketListItemViewModel MapTicketListItem(Ticket ticket) => new()
    {
        TicketId = ticket.TicketId,
        TicketNumber = ticket.TicketNumber,
        Subject = ticket.Subject,
        Category = ticket.Category?.CategoryName ?? string.Empty,
        Department = ticket.Department?.DepartmentName ?? string.Empty,
        Team = ticket.Team?.TeamName ?? "Unassigned",
        Priority = ticket.Priority?.PriorityName ?? string.Empty,
        Status = ticket.Status?.StatusName ?? string.Empty,
        CreatedBy = DisplayUser(ticket.CreatedBy),
        AssignedTo = DisplayUser(ticket.AssignedTo),
        CreatedAt = ticket.CreatedAt
    };

    private static string DisplayUser(ApplicationUser? user)
    {
        if (user is null)
        {
            return "Unassigned";
        }

        return !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : user.Email ?? user.UserName ?? "User";
    }

    private static string GetNotificationTypeForStatus(string newStatusName, string oldStatusName)
    {
        if (newStatusName == TicketStatusNames.Resolved)
        {
            return NotificationTypes.TicketResolved;
        }

        if (newStatusName == TicketStatusNames.Closed)
        {
            return NotificationTypes.TicketClosed;
        }

        if (newStatusName == TicketStatusNames.InProgress &&
            (oldStatusName == TicketStatusNames.Resolved || oldStatusName == TicketStatusNames.Closed))
        {
            return NotificationTypes.TicketReopened;
        }

        return NotificationTypes.StatusChanged;
    }
}
