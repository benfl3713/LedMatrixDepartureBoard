using BdfFontParser;
using RpiLedMatrix;

namespace LedMatrixDepartureBoard.Models;

public record DepartureRow(Color MainColor, int Order, int rowCount, DateTime? AimedDeparture, string? Platform, string Destination, Departure.ServiceStatus Status, DateTime? ExpectedDeparture, bool IsCancelled = false)
{
    private static BdfFont _font = new BdfFont("/home/ben/rpi-rgb-led-matrix/fonts/7x14.bdf");
    public readonly int MaxFontHeight = _font.BoundingBox.Y;
    
    public void Draw(ILedMatrix matrix, int lineOffset = 0, int? maxLineNumber = null)
    {
        int y = (Order - 1) * 20 + 15;
        y -= lineOffset;

        if (maxLineNumber.HasValue)
            y += MaxFontHeight - maxLineNumber.Value;
        
        matrix.DrawText(_font, 1, y, MainColor, $"{rowCount}{GetOrderExtention()}", lineOffset, maxLineNumber);
        matrix.DrawText(_font, 25, y, MainColor, AimedDeparture.Value.ToString("HH:mm"), lineOffset, maxLineNumber);

        if (!string.IsNullOrEmpty(Platform))
        {
            int platformPos = 66;
            if (Platform.Length > 1)
                platformPos = (int)Math.Round(platformPos - (Platform.Length - 1) * 3.5);
            matrix.DrawText(_font, platformPos, y, MainColor, Platform, lineOffset, maxLineNumber);
        }
        
        if (Destination.Length > 16)
        {
            int statusLength = GetStatusText().Item1.Length;
            int maxLength = 22 - statusLength;
            matrix.DrawText(_font, 85, y, MainColor, Destination[..maxLength] + "..", lineOffset, maxLineNumber);
        }
        else
            matrix.DrawText(_font, 85, y, MainColor, Destination, lineOffset, maxLineNumber);
        DrawStatus(matrix, y, lineOffset, maxLineNumber);
    }

    private void DrawStatus(ILedMatrix matrix, int y, int lineOffset, int? maxLineNumber = null)
    {
        var (text, color) = GetStatusText();
        int x = 256 - 7 * text.Length;
        matrix.DrawText(_font, x, y, color, text, lineOffset, maxLineNumber);
    }

    private (string, Color) GetStatusText()
    {
        if (Status == Departure.ServiceStatus.ONTIME)
            return ("On time", new Color(0, 151, 0));
        
        if (Status == Departure.ServiceStatus.LATE)
            return ($"Exp {ExpectedDeparture:HH:mm}", new Color(151, 0, 0));
        
        if (Status == Departure.ServiceStatus.CANCELLED)
            return ("Cancelled", new Color(0, 151, 200));
        
        if (Status == Departure.ServiceStatus.ARRIVED)
            return ("Arrived", new Color(200, 66, 245));

        return ("", new Color(0, 0, 0));
    }

    private string GetOrderExtention()
    {
        int lastDigit = rowCount % 10;
        return lastDigit switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };
    }
}