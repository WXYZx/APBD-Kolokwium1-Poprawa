namespace kolokwium1_P.Models;

public class ClientRentalRequestsDTo
{
    public FClientCreateDto Client { get; set; } //to
    public int CarId { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
}