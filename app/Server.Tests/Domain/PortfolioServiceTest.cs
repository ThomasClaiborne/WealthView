using Moq;
using Server.Data;
using Server.Domain;
using Server.Models;

namespace Server.Tests.Domain;

[Collection("DatabaseTests")]
public class PortfolioServiceTest
{
    private readonly Mock<IPortfolioRepository> _portfolioRepo = new();
    private readonly Mock<IHoldingRepository> _holdingRepo = new();

// ── GetByUserId ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByUserId_ReturnsPortfolioResponse_WhenPortfolioExists()
    {
        _portfolioRepo.Setup(r => r.GetByUserId(1))
            .ReturnsAsync(new Portfolio
            {
                PortfolioId = 1,
                AppUserId = 1,
                CashBalance = 850.00m,
                CreatedAt = DateTime.UtcNow
            });

        _portfolioRepo.Setup(r => r.SnapshotExistsForToday(1)).ReturnsAsync(true);

        _holdingRepo.Setup(r => r.GetAllByPortfolioId(1))
            .ReturnsAsync(new List<Holding>());

        var service = new PortfolioService(_portfolioRepo.Object, _holdingRepo.Object);
        var result = await service.GetByUserId(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Payload!.PortfolioId);
        Assert.Equal(850.00m, result.Payload.CashBalance);
        Assert.Equal(850.00m, result.Payload.TotalValue);
        Assert.Equal(0, result.Payload.HoldingCount);
        Assert.Equal(0, result.Payload.TotalUnrealizedGl);
    }

    [Fact]
    public async Task GetByUserId_ReturnsNotFound_WhenPortfolioDoesNotExist()
    {
        _portfolioRepo.Setup(r => r.GetByUserId(999))
            .ReturnsAsync((Portfolio?)null);

        var service = new PortfolioService(_portfolioRepo.Object, _holdingRepo.Object);
        var result = await service.GetByUserId(999);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultType.NotFound, result.Type);
    }

    // ── EnsureTodaySnapshot (tested via GetByUserId) ──────────────────────

    [Fact]
    public async Task GetByUserId_CreatesSnapshot_WhenNoneExistsForToday()
    {
        _portfolioRepo.Setup(r => r.GetByUserId(1))
            .ReturnsAsync(new Portfolio
            { PortfolioId = 1, AppUserId = 1, CashBalance = 1000m });

        _holdingRepo.Setup(r => r.GetAllByPortfolioId(1))
            .ReturnsAsync(new List<Holding>());

        _portfolioRepo.Setup(r => r.SnapshotExistsForToday(1))
            .ReturnsAsync(false);

        _portfolioRepo.Setup(r => r.CreateSnapshot(It.IsAny<PortfolioSnapshot>()))
            .ReturnsAsync((PortfolioSnapshot s) => s);

        var service = new PortfolioService(_portfolioRepo.Object, _holdingRepo.Object);
        await service.GetByUserId(1);

        _portfolioRepo.Verify(r => r.CreateSnapshot(
            It.Is<PortfolioSnapshot>(s =>
                s.PortfolioId == 1 &&
                s.TotalValue == 1000m &&
                s.SnapshotDate == DateOnly.FromDateTime(DateTime.UtcNow))),
            Times.Once);
    }

    [Fact]
    public async Task GetByUserId_DoesNotCreateSnapshot_WhenAlreadyExistsForToday()
    {
        _portfolioRepo.Setup(r => r.GetByUserId(1))
            .ReturnsAsync(new Portfolio
            { PortfolioId = 1, AppUserId = 1, CashBalance = 1000m });

        _holdingRepo.Setup(r => r.GetAllByPortfolioId(1))
            .ReturnsAsync(new List<Holding>());

        _portfolioRepo.Setup(r => r.SnapshotExistsForToday(1))
            .ReturnsAsync(true);

        var service = new PortfolioService(_portfolioRepo.Object, _holdingRepo.Object);
        await service.GetByUserId(1);

        _portfolioRepo.Verify(r => r.CreateSnapshot(It.IsAny<PortfolioSnapshot>()), Times.Never);
    }

    // ── GetSnapshotHistory ────────────────────────────────────────────────

    [Fact]
    public async Task GetSnapshotHistory_ReturnsSnapshots_MappedToResponse()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var yesterday = today.AddDays(-1);

        _portfolioRepo.Setup(r => r.GetSnapshotsByPortfolioId(1, 30))
            .ReturnsAsync(new List<PortfolioSnapshot>
            {
            new() { SnapshotId = 1, PortfolioId = 1, SnapshotDate = yesterday, TotalValue = 9000m },
            new() { SnapshotId = 2, PortfolioId = 1, SnapshotDate = today,     TotalValue = 9500m }
            });

        var service = new PortfolioService(_portfolioRepo.Object, _holdingRepo.Object);
        var result = await service.GetSnapshotHistory(1);

        Assert.Equal(ResultType.Success, result.Type);
        Assert.Equal(2, result.Payload!.Count);
        Assert.Equal(9000m, result.Payload[0].TotalValue);
        Assert.Equal(9500m, result.Payload[1].TotalValue);
        Assert.Equal(yesterday, result.Payload[0].SnapshotDate);
    }

    [Fact]
    public async Task GetSnapshotHistory_ReturnsEmpty_WhenNoSnapshots()
    {
        _portfolioRepo.Setup(r => r.GetSnapshotsByPortfolioId(1, 30))
            .ReturnsAsync(new List<PortfolioSnapshot>());

        var service = new PortfolioService(_portfolioRepo.Object, _holdingRepo.Object);
        var result = await service.GetSnapshotHistory(1);

        Assert.Equal(ResultType.Success, result.Type);
        Assert.Empty(result.Payload!);
    }
}