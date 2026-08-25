namespace Ticketing_Tool.Services;

public interface ITicketNumberGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
