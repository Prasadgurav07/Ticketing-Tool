using System.Globalization;
using Ticketing_Tool.Models;

namespace Ticketing_Tool.Services;

public class LocalTicketFileStorageService(IWebHostEnvironment environment) : IFileStorageService
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".pdf",
        ".doc",
        ".docx",
        ".xls",
        ".xlsx",
        ".txt",
        ".log"
    };

    public async Task<TicketAttachment> SaveAsync(int ticketId, string uploadedById, IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0)
        {
            throw new InvalidOperationException("Upload a non-empty file.");
        }

        if (file.Length > MaxFileSize)
        {
            throw new InvalidOperationException("Files must be 10 MB or smaller.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("This file type is not allowed.");
        }

        var safeExtension = extension.ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{safeExtension}";
        var ticketFolder = Path.Combine(GetUploadRoot(), "tickets", ticketId.ToString(CultureInfo.InvariantCulture));
        Directory.CreateDirectory(ticketFolder);

        var physicalPath = Path.Combine(ticketFolder, fileName);
        await using (var stream = File.Create(physicalPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        return new TicketAttachment
        {
            TicketId = ticketId,
            FileName = fileName,
            OriginalFileName = Path.GetFileName(file.FileName),
            FileType = string.IsNullOrWhiteSpace(file.ContentType) ? safeExtension.TrimStart('.') : file.ContentType,
            FileSize = file.Length,
            StoragePath = $"tickets/{ticketId.ToString(CultureInfo.InvariantCulture)}/{fileName}",
            UploadedById = uploadedById,
            UploadedAt = DateTime.UtcNow
        };
    }

    public string GetPhysicalPath(string storagePath)
    {
        var normalizedPath = storagePath.Replace('/', Path.DirectorySeparatorChar);
        var root = Path.GetFullPath(GetUploadRoot());
        var candidate = Path.GetFullPath(Path.Combine(root, normalizedPath));

        if (!candidate.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid attachment path.");
        }

        return candidate;
    }

    private string GetUploadRoot() => Path.Combine(environment.ContentRootPath, "SecureUploads");
}
