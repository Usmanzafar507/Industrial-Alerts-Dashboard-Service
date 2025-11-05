using Industrial.AlertService.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Industrial.AlertService.Infrastructure.Services;

public class TelemetrySimulatorService : BackgroundService
{
    private readonly ILogger<TelemetrySimulatorService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IThresholdEvaluator _evaluator;
    private decimal _tempMax;
    private decimal _humidityMax;
    private readonly Random _random = Random.Shared; 

    public TelemetrySimulatorService(
        ILogger<TelemetrySimulatorService> logger,
        IServiceScopeFactory scopeFactory,
        IThresholdEvaluator evaluator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Telemetry simulator starting...");

        await LoadConfig(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var temperature = NextDecimal(10, 120);
                var humidity = NextDecimal(0, 100);

                using var scope = _scopeFactory.CreateScope();
                var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();

                if (_evaluator.IsTemperatureExceeded(temperature, _tempMax))
                {
                    _logger.LogInformation("Temperature alert triggered: {Temperature} > {Max}", temperature, _tempMax);
                    await alertService.CreateAlertAsync("Temperature", temperature, _tempMax, stoppingToken);
                }

                if (_evaluator.IsHumidityExceeded(humidity, _humidityMax))
                {
                    _logger.LogInformation("Humidity alert triggered: {Humidity} > {Max}", humidity, _humidityMax);
                    await alertService.CreateAlertAsync("Humidity", humidity, _humidityMax, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Telemetry simulator stopping gracefully...");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in telemetry simulation loop.");
            }

            // small randomized delay between cycles
            var delay = TimeSpan.FromSeconds(_random.Next(3, 6));
            await Task.Delay(delay, stoppingToken);
             
            await LoadConfig(stoppingToken);
        }

        _logger.LogInformation("Telemetry simulator stopped.");
    }

    private async Task LoadConfig(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var configService = scope.ServiceProvider.GetRequiredService<IConfigService>();
            var config = await configService.GetAsync(ct);

            _tempMax = config.TempMax;
            _humidityMax = config.HumidityMax;

            _logger.LogDebug("Configuration loaded: TempMax={TempMax}, HumidityMax={HumidityMax}", _tempMax, _humidityMax);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load configuration; keeping previous values.");
        }
    }

    private static decimal NextDecimal(int min, int max)
        => (decimal)(Random.Shared.NextDouble() * (max - min) + min);
}
