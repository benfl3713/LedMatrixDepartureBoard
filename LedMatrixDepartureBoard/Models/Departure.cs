using System.Text.Json.Serialization;

namespace LedMatrixDepartureBoard.Models;

public class Departure
{
    public DateTime LastUpdated { get; set; }
    public string StationName { get; set; }
    public string StationCode { get; set; }
    public string Platform { get; set; }
    public string OperatorName { get; set; }
    public DateTime AimedDeparture { get; set; }
    public DateTime? ExpectedDeparture { get; set; }
    public string Origin { get; set; }
    public string Destination { get; set; }
    public ServiceStatus Status { get; set; }
    [JsonIgnore]
    public string ServiceTimeTableUrl { get; set; }
    [JsonIgnore]
    public Type FromDataSouce { get; set; }
    public int Length { get; set; }
    public Dictionary<string, object> ExtraDetails { get; set; } = null;
    
    public enum ServiceStatus
    {
        ONTIME,
        LATE,
        CANCELLED,
        ARRIVED
    }
}