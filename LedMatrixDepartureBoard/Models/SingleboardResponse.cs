namespace LedMatrixDepartureBoard.Models;

public class SingleboardResponse
{
    public List<Departure> Departures { get; set; }
    public string Information { get; set; }
}