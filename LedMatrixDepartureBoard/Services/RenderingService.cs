using System.Diagnostics;
using BdfFontParser;
using LedMatrixDepartureBoard.Models;

namespace LedMatrixDepartureBoard.Services;

public class RenderingService : BackgroundService
{
    private readonly ILedMatrix _matrix;
    private readonly DepartureCacheService _departureCacheService;
    private readonly AppConfig _appConfig;
    private readonly UserConfigService _userConfigService;
    private readonly StationInformationService _stationInformationService;
    private readonly BdfFont _font = new BdfFont("/home/ben/rpi-rgb-led-matrix/fonts/7x14.bdf");
    private string _lastInfoMessage = ""; 

    private int _currentInfoScrollPos = -249;
    private int _rowAnimateOffset;
    private int _fifteenFpsCounter;
    private int _activeSecondRowDeparture = 1;
    private Stopwatch _rowSwitchTimer = Stopwatch.StartNew();
    private UserConfig _userConfig;

    public RenderingService(ILedMatrix matrix, DepartureCacheService departureCacheService, AppConfig appConfig, UserConfigService userConfigService, StationInformationService stationInformationService)
    {
        _matrix = matrix;
        _departureCacheService = departureCacheService;
        _appConfig = appConfig;
        _userConfigService = userConfigService;
        _stationInformationService = stationInformationService;
        _userConfig = _userConfigService.Get();
        _userConfigService.ConfigChange += (_, config) => _userConfig = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_userConfig.Enabled)
        {
            await StopAsync(stoppingToken);
            return;
        }

        _rowSwitchTimer = Stopwatch.StartNew();
        
        while (!stoppingToken.IsCancellationRequested || !_userConfig.Enabled)
        {
            if (_departureCacheService.Data?.SingleboardResponse == null)
            {
                await Task.Delay(1000, stoppingToken);
                continue;
            }

            await DrawFrame(_departureCacheService.Data.SingleboardResponse, stoppingToken);
        }

        Console.WriteLine("Stopping Rendering from executing");
    }

    private async Task DrawFrame(SingleboardResponse singleboardResponse, CancellationToken stoppingToken)
    {
        var startTime = Stopwatch.GetTimestamp();
        _matrix.Clear();
        var departures = singleboardResponse.Departures;

        if (departures.Count > 0)
        {
            var firstDeparture = departures[0];
            DepartureRow row = new DepartureRow(_fifteenFpsCounter, _userConfig.GetTextColor(), 1,1, firstDeparture.AimedDeparture, firstDeparture.Platform, firstDeparture.Destination, firstDeparture.Status, firstDeparture.ExpectedDeparture);
            row.Draw(_matrix);
        }

        if (departures.Count > 1)
        {
            DrawSecondDepartureRow(departures);
        }

        if (!string.IsNullOrEmpty(_userConfig.CustomMessage))
        {
            string message = ProcessCustomMessage(_userConfig.CustomMessage);
            int pixelLength = message.Length * 7;
            var x = (int.Parse(_appConfig.FullCols) - pixelLength) / 2;
            _matrix.DrawText(_font, x, 60, _userConfig.GetTextColor(), message);
        }

        DrawInfoRow(singleboardResponse);

        _matrix.Update();

        int renderTime = Convert.ToInt32(Stopwatch.GetElapsedTime(startTime).TotalMilliseconds);
        
        // Frame limit to 60fps to allow smooth animations without slamming cpu
        int fps = 1000 / 60;
        if (renderTime < fps)
        {
            await Task.Delay(fps - renderTime, stoppingToken);
        }

        if (++_fifteenFpsCounter > 3)
            _fifteenFpsCounter = 0;
    }

    private string ProcessCustomMessage(string userConfigCustomMessage)
    {
        return userConfigCustomMessage
            .Replace("{Time}", DateTime.Now.ToString("HH:mm:ss"))
            .Replace("{Station}", _stationInformationService.GetName(_userConfig.StationCode));
    }

    private void DrawSecondDepartureRow(List<Departure> departures)
    {
        var secondDeparture = departures[1];
        DepartureRow row = new DepartureRow(_fifteenFpsCounter, _userConfig.GetTextColor(), 3, 2, secondDeparture.AimedDeparture, secondDeparture.Platform,
            secondDeparture.Destination, secondDeparture.Status, secondDeparture.ExpectedDeparture);
        
        if (departures.Count == 1)
        {
            row.Draw(_matrix);
            return;
        }

        bool runAnimation = _rowSwitchTimer.Elapsed.TotalSeconds > 8;
        
        var departure = departures[_activeSecondRowDeparture];
        DepartureRow activeRow = new DepartureRow(_fifteenFpsCounter, _userConfig.GetTextColor(), 3, _activeSecondRowDeparture + 1, departure.AimedDeparture, departure.Platform,
            departure.Destination, departure.Status, departure.ExpectedDeparture);

        if (!runAnimation)
        {
            activeRow.Draw(_matrix);
            return;
        }

        if (_fifteenFpsCounter == 3)
            _rowAnimateOffset++;
        
        int newIndex = _activeSecondRowDeparture == 1 ? 2 : 1;
        
        var newDeparture = departures[newIndex];
        DepartureRow newRow = new DepartureRow(_fifteenFpsCounter, _userConfig.GetTextColor(), 3, newIndex + 1, newDeparture.AimedDeparture, newDeparture.Platform,
            newDeparture.Destination, newDeparture.Status, newDeparture.ExpectedDeparture);
        
        if (_rowAnimateOffset > row.MaxFontHeight)
        {
            _rowAnimateOffset = 0;
            _rowSwitchTimer.Restart();
            _activeSecondRowDeparture = newIndex;
            newRow.Draw(_matrix);
            return;
        }

        activeRow.Draw(_matrix, _rowAnimateOffset);
        newRow.Draw(_matrix, 0, _rowAnimateOffset);
    }

    private void DrawInfoRow(SingleboardResponse singleboardResponse)
    {
        string info = singleboardResponse.Information;
        info = info.Replace("       ", "   ");

        if (info != _lastInfoMessage)
            _currentInfoScrollPos = -249;

        _matrix.DrawText(_font, 2 - _currentInfoScrollPos, 30, _userConfig.GetTextColor(), info);
        _currentInfoScrollPos += 1;

        if (_currentInfoScrollPos > 7 * info.Length)
            _currentInfoScrollPos = -249;

        _lastInfoMessage = info;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _matrix.Clear();
        _matrix.Update();
        _matrix.Reset();
        Thread.Sleep(500);
        _lastInfoMessage = "";
        _currentInfoScrollPos = -249;
        _rowAnimateOffset = 0;
        _activeSecondRowDeparture = 0;
        _fifteenFpsCounter = 0;
        _rowSwitchTimer.Reset();
        return base.StopAsync(cancellationToken);
    }
}