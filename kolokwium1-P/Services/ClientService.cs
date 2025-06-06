using Microsoft.Data.SqlClient;
using kolokwium1_P.Models;

namespace kolokwium1_P.Services
{
    public class ClientService : IClientService
    {
        private readonly string _connStr;
        private IClientService _clientServiceImplementation;

        public ClientService(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection");
        }

        public async Task<ClientResponseDTo?> GetClientWithRentalsAsync(int clientId)
        {
            using var con = new SqlConnection(_connStr);
            await con.OpenAsync();

            var clientCmd = new SqlCommand("SELECT Id, FirstName, LastName, Address FROM clients WHERE Id=@Id", con);
            clientCmd.Parameters.AddWithValue("@Id", clientId);
            using var clientReader = await clientCmd.ExecuteReaderAsync();
            if (!await clientReader.ReadAsync())
                return null;

            var client = new ClientResponseDTo()
            {
                Id = clientReader.GetInt32(0),
                FirstName = clientReader.GetString(1),
                LastName = clientReader.GetString(2),
                Address = clientReader.GetString(3),
            };
            await clientReader.CloseAsync();
            
            var rentalsCmd = new SqlCommand(@"
                SELECT c.VIN, co.Name as Color, m.Name as Model, cr.DateFrom, cr.DateTo, cr.TotalPrice
                FROM car_rentals cr
                JOIN cars c ON c.Id = cr.CarID
                JOIN colors co ON c.ColorID = co.Id
                JOIN models m ON c.ModelID = m.Id
                WHERE cr.ClientID = @Id

            ", con);
            rentalsCmd.Parameters.AddWithValue("@Id", clientId);

            using var rentalsReader = await rentalsCmd.ExecuteReaderAsync();
            while (await rentalsReader.ReadAsync())
            {
                client.Rentals.Add(new RentalDto
                {
                    Vin = rentalsReader.GetString(0),
                    Color = rentalsReader.GetString(1),
                    Model = rentalsReader.GetString(2),
                    DateFrom = rentalsReader.GetDateTime(3),
                    DateTo = rentalsReader.GetDateTime(4),
                    TotalPrice = rentalsReader.GetInt32(5)
                });
            }

            return client;
        }

        public async Task<ClientResponseDTo?> AddClientWithRentalAsync(ClientRentalRequestsDTo request)
        {
            using var con = new SqlConnection(_connStr);
            await con.OpenAsync();
            using var tran = con.BeginTransaction();

            try
            {
                var carCmd = new SqlCommand("SELECT PricePerDay FROM cars WHERE Id=@Id", con, tran);
                carCmd.Parameters.AddWithValue("@Id", request.CarId);
                var carRes = await carCmd.ExecuteScalarAsync();
                if (carRes == null) return null;
                int pricePerDay = (int)carRes;

                var clientCmd = new SqlCommand(@"
                    INSERT INTO clients (FirstName, LastName, Address) OUTPUT INSERTED.Id
                    VALUES (@FirstName, @LastName, @Address)", con, tran);
                clientCmd.Parameters.AddWithValue("@FirstName", request.Client.FirstName);
                clientCmd.Parameters.AddWithValue("@LastName", request.Client.LastName);
                clientCmd.Parameters.AddWithValue("@Address", request.Client.Address);

                var clientId = (int)await clientCmd.ExecuteScalarAsync();

                var days = (request.DateTo - request.DateFrom).Days;
                if (days <= 0) throw new Exception("Podaj inną date");
                int totalPrice = pricePerDay * days;

                var rentalCmd = new SqlCommand(@"
                    INSERT INTO car_rentals (ClientID, CarID, DateFrom, DateTo, TotalPrice, Discount)
                    VALUES (@ClientID, @CarID, @DateFrom, @DateTo, @TotalPrice, 0)", con, tran);
                rentalCmd.Parameters.AddWithValue("@ClientID", clientId);
                rentalCmd.Parameters.AddWithValue("@CarID", request.CarId);
                rentalCmd.Parameters.AddWithValue("@DateFrom", request.DateFrom);
                rentalCmd.Parameters.AddWithValue("@DateTo", request.DateTo);
                rentalCmd.Parameters.AddWithValue("@TotalPrice", totalPrice);
                await rentalCmd.ExecuteNonQueryAsync();

                tran.Commit();
                return await GetClientWithRentalsAsync(clientId);
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
    }
}