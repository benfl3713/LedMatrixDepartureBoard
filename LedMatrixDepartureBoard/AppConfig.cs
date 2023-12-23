using RpiLedMatrix;

namespace LedMatrixDepartureBoard;

public class AppConfig
{
    public required string FullRows { get; init; }
    public required string FullCols { get; init; }
    public required string MainColor { get; set; } = "255, 151, 41";
    public required RGBLedMatrixOptions Matrix { get; init; }

    public Color GetMainColor()
    {
        string[] parts = MainColor.Split(',');
        if (parts.Length != 3)
            throw new Exception("Invalid MainColor");
        
        return new Color(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
    }
}