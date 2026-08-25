using System.Security.Claims;
using Ticketing_Tool.Models;
using Ticketing_Tool.ViewModels;

namespace Ticketing_Tool.Services;

public interface ITicketService
{
    Task<DashboardViewModel> GetEmployeeDashboardAsync(string userId, CancellationToken cancellationToken = default);

    Task<DashboardViewModel> GetSupportDashboardAsync(CancellationToken cancellationToken = default);

    Task<TicketFilterViewModel> SearchTicketsAsync(TicketFilterViewModel filter, ClaimsPrincipal user, CancellationToken cancellationToken = default);

    Task<Ticket> CreateTicketAsync(CreateTicketViewModel model, string userId, CancellationToken cancellationToken = default);

    Task<Ticket?> GetTicketDetailsAsync(int ticketId, CancellationToken cancellationToken = default);

    Task AddCommentAsync(int ticketId, string userId, string commentText, CancellationToken cancellationToken = default);

    Task AddAttachmentAsync(int ticketId, string userId, IFormFile file, CancellationToken cancellationToken = default);

    Task AssignTicketAsync(int ticketId, string assignedToId, string assignedById, string? reason, CancellationToken cancellationToken = default);

    Task ChangeStatusAsync(int ticketId, int newStatusId, string changedById, ClaimsPrincipal user, string? comment, string? resolution, CancellationToken cancellationToken = default);

    Task ConfirmResolutionAsync(int ticketId, string userId, bool isFixed, string? comment, ClaimsPrincipal user, CancellationToken cancellationToken = default);

    Task<TicketAttachment?> GetAttachmentAsync(int attachmentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ApplicationUser>> GetAssignableUsersAsync(CancellationToken cancellationToken = default);
}
