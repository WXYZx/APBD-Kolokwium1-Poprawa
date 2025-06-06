namespace kolokwium1_P.Models;

public class Car
{
    public int Id { get; set; }
    public string VIN { get; set; }
    public string Name { get; set; }
    public int Seats { get; set; }
    public int PricePerDay { get; set; }
    public int ModelId { get; set; }
    public int ColorId { get; set; }
}