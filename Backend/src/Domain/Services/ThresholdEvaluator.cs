using Industrial.AlertService.Domain.Interfaces;

namespace Industrial.AlertService.Domain.Services;

public class ThresholdEvaluator : IThresholdEvaluator
{
    public bool IsTemperatureExceeded(decimal value, decimal threshold)
    {
        return value > threshold;
    }

    public bool IsHumidityExceeded(decimal value, decimal threshold)
    {
        return value > threshold;
    }
}


