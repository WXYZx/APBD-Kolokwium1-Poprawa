namespace kolokwium1_P.Models;

public class ClientResponseDTo
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Address { get; set; }
    public List<RentalDto> Rentals { get; set; } = new();
}