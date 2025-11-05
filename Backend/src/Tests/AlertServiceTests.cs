using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Industrial.AlertService.Domain.DTOs;
using Industrial.AlertService.Domain.Entities;
using Industrial.AlertService.Domain.Interfaces;
using Industrial.AlertService.Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace Industrial.AlertService.Tests;

public class AlertServiceTests
{
    [Fact]
    public async Task CreateAlert_AddsAndBroadcasts()
    {
        var alertRepo = new Mock<IAlertRepository>();
        alertRepo.Setup(r => r.AddAsync(It.IsAny<Alert>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Alert a, CancellationToken _) => { a.Id = Guid.NewGuid(); return a; });

        var hubClients = new Mock<IHubClients<IAlertsClient>>();
        var all = new Mock<IAlertsClient>();
        hubClients.Setup(h => h.All).Returns(all.Object);
        var hubContext = new Mock<IHubContext<AlertsHub, IAlertsClient>>();
        hubContext.Setup(h => h.Clients).Returns(hubClients.Object);

        var service = new Infrastructure.Services.AlertService(alertRepo.Object, hubContext.Object);

        var dto = await service.CreateAlertAsync("Temperature", 101.2m, 80.0m);

        dto.Type.Should().Be("Temperature");
        dto.Status.Should().Be("Open");
        alertRepo.Verify(r => r.AddAsync(It.IsAny<Alert>(), It.IsAny<CancellationToken>()), Times.Once);
        all.Verify(c => c.NewAlert(It.IsAny<AlertDto>()), Times.Once);
    }
}


