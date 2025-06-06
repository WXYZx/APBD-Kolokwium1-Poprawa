using kolokwium1_P.Models;

namespace kolokwium1_P.Services
{
    public interface IClientService
    {
        Task<ClientResponseDTo?> GetClientWithRentalsAsync(int clientId);
        Task<ClientResponseDTo?> AddClientWithRentalAsync(ClientRentalRequestsDTo request);
    }
}
