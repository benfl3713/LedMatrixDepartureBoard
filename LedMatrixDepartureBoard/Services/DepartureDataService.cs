using LedMatrixDepartureBoard.Models;

namespace LedMatrixDepartureBoard.Services;

public class DepartureDataService : BackgroundService
{
    private readonly HttpClient _httpClient;
    private readonly DepartureCacheService _cacheService;
    private readonly AppConfig _appConfig;
    private readonly UserConfigService _userConfigService;
    private UserConfig _userConfig;

    public DepartureDataService(HttpClient httpClient, DepartureCacheService cacheService, AppConfig appConfig, UserConfigService userConfigService)
    {
        _httpClient = httpClient;
        _cacheService = cacheService;
        _appConfig = appConfig;
        _userConfigService = userConfigService;
        _userConfig = _userConfigService.Get();
        _userConfigService.ConfigChange += (_, config) =>
        {
            _userConfig = config;
            UpdateData();
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // if (!_userConfig.Enabled)
        // {
        //     await StopAsync(stoppingToken);
        //     return;
        // }
        
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_userConfig.Enabled)
                await UpdateData();
            await Task.Delay(20000, stoppingToken);
        }
    }

    private async Task UpdateData()
    {
        var response =
            await _httpClient.GetAsync(
                $"https://api.leddepartureboard.com/api/LiveDepartures/GetLatestDepaturesSingleBoard?stationCode={_userConfig.StationCode}");

        SingleboardResponse? singleboard = await response.Content.ReadFromJsonAsync<SingleboardResponse>();

        if (singleboard == null)
            return;

        _cacheService.Data = new DepartureData
        {
            SingleboardResponse = singleboard
        };
    }
}