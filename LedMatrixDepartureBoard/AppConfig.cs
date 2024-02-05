using RpiLedMatrix;

namespace LedMatrixDepartureBoard;

public class AppConfig
{
    public required string FullRows { get; init; }
    public required string FullCols { get; init; }
    public required RGBLedMatrixOptions Matrix { get; init; }
}