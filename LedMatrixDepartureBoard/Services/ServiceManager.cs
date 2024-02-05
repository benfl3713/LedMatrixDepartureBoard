namespace LedMatrixDepartureBoard.Services;

public class ServiceManager : BackgroundService
{
    private readonly ILogger<ServiceManager> _logger;
    private readonly UserConfigService _userConfigService;
    private readonly RenderingService _renderingService;
    private readonly DepartureDataService _departureDataService;
    private UserConfig _userConfig;

    public ServiceManager(ILogger<ServiceManager> logger, UserConfigService userConfigService, RenderingService renderingService, DepartureDataService departureDataService)
    {
        _logger = logger;
        _userConfigService = userConfigService;
        _renderingService = renderingService;
        _departureDataService = departureDataService;
        _userConfig = _userConfigService.Get();
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _userConfigService.ConfigChange += UserConfigServiceOnConfigChange;
        return Task.CompletedTask;
    }

    private async void UserConfigServiceOnConfigChange(object? sender, UserConfig e)
    {
        await Task.Delay(300);
        if (e.Enabled != _userConfig.Enabled)
        {
            if (e.Enabled)
            {
                _logger.LogInformation("Starting Board");
                _departureDataService.StartAsync(default);
                _renderingService.StartAsync(default);
            }
            else
            {
                _logger.LogInformation("Stopping Board");
                _departureDataService.StopAsync(default);
                _renderingService.StopAsync(default);
            }
        }
        else
        {
            _logger.LogInformation("Not difference found {Enabled}", _userConfig.Enabled);
        }

        _userConfig = e;
    }
}