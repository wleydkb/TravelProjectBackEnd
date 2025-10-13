namespace Travel.Application.Interfaces
{
    public interface IAmadeusAuthService
    {
        Task<string> GetTokenAsync(CancellationToken ct = default);
    }
}
