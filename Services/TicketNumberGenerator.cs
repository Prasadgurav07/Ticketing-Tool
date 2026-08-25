using Microsoft.EntityFrameworkCore;
using Ticketing_Tool.Data;

namespace Ticketing_Tool.Services;

public class TicketNumberGenerator(ApplicationDbContext context) : ITicketNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var prefix = $"INC-{DateTime.UtcNow.Year}-";
        var nextNumber = await context.Tickets
            .CountAsync(ticket => ticket.TicketNumber.StartsWith(prefix), cancellationToken) + 1;

        for (var attempt = 0; attempt < 20; attempt++)
        {
            var candidate = $"{prefix}{nextNumber + attempt:000000}";
            if (!await context.Tickets.AnyAsync(ticket => ticket.TicketNumber == candidate, cancellationToken))
            {
                return candidate;
            }
        }

        return $"{prefix}{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
    }
}
