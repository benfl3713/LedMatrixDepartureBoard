namespace LedMatrixDepartureBoard.Services;

public class StationInformationService
{
    private readonly HttpClient _httpClient;
    private Dictionary<string, Station>? _stations;

    public StationInformationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Station>?> GetAll()
    {
        if (_stations != null)
            return _stations.Values.ToList();
        var response = await _httpClient.GetAsync("https://api.leddepartureboard.com/api/StationLookup");
        _stations = (await response.Content.ReadFromJsonAsync<List<Station>>()).ToDictionary(k => k.Code);
        return _stations.Values.ToList();
    }

    public string GetName(string code)
    {
        if (_stations == null)
            GetAll().Wait();
        
        return _stations[code].Name;
    }

    public class Station
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}