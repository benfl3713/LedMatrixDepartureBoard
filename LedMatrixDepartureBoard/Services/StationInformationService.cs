namespace LedMatrixDepartureBoard.Services;

public class StationInformationService
{
    private readonly HttpClient _httpClient;
    private List<Station>? _stations;

    public StationInformationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Station>?> GetAll()
    {
        if (_stations != null)
            return _stations;
        var response = await _httpClient.GetAsync("https://api.leddepartureboard.com/api/StationLookup");
        _stations = await response.Content.ReadFromJsonAsync<List<Station>>();
        return _stations;
    }

    public class Station
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}