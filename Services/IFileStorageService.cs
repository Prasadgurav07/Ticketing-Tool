using Ticketing_Tool.Models;

namespace Ticketing_Tool.Services;

public interface IFileStorageService
{
    Task<TicketAttachment> SaveAsync(int ticketId, string uploadedById, IFormFile file, CancellationToken cancellationToken = default);

    string GetPhysicalPath(string storagePath);
}
