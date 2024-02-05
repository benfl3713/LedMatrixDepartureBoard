using System.Drawing;
using Color = RpiLedMatrix.Color;

namespace LedMatrixDepartureBoard;

public class UserConfig
{
    public bool Enabled { get; set; }
    public string StationCode { get; set; }
    public string TextColor { get; set; } = "#FF9729";
    public string? CustomMessage { get; set; }

    public Color GetTextColor()
    {
        var color = ColorTranslator.FromHtml(TextColor);
        int r = Convert.ToInt16(color.R);
        int g = Convert.ToInt16(color.G);
        int b = Convert.ToInt16(color.B);

        return new Color(r, g, b);
    }
}