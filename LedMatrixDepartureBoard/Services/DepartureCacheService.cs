using LedMatrixDepartureBoard.Models;

namespace LedMatrixDepartureBoard.Services;

public class DepartureCacheService
{
    public DepartureData Data { get; set; }
}

public class DepartureData
{
    public SingleboardResponse SingleboardResponse { get; set; }
}