using Industrial.AlertService.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Industrial.AlertService.Tests;

public class ThresholdEvaluatorTests
{
    [Theory]
    [InlineData(80.1, 80.0, true)]
    [InlineData(80.0, 80.0, false)]
    [InlineData(79.9, 80.0, false)]
    public void Temperature_Threshold_Checks(decimal value, decimal threshold, bool expected)
    {
        var evaluator = new ThresholdEvaluator();
        evaluator.IsTemperatureExceeded(value, threshold).Should().Be(expected);
    }

    [Theory]
    [InlineData(60.1, 60.0, true)]
    [InlineData(60.0, 60.0, false)]
    [InlineData(59.9, 60.0, false)]
    public void Humidity_Threshold_Checks(decimal value, decimal threshold, bool expected)
    {
        var evaluator = new ThresholdEvaluator();
        evaluator.IsHumidityExceeded(value, threshold).Should().Be(expected);
    }
}


