namespace Aruba.WebApi;

public sealed record class WeatherForecast
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    public required DateOnly Date { get; init; }
    public required int TemperatureC { get; init; }
    public string? Summary { get; init; }
}
