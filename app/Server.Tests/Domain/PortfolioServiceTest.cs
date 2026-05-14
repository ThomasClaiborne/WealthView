using Moq;
using Server.Data;
using Server.Domain;
using Server.Models;

namespace Server.Tests.Domain;

[Collection("DatabaseTests")]
public class PortfolioServiceTest
{
    // ── GetByUserId ───────────────────────────────────────────────────

    [Fact]
    public async Task GetByUserId_ReturnsPortfolioResponse_WhenPortfolioExists()
    {
        var mockRepo = new Mock<IPortfolioRepository>();
        mockRepo.Setup(r => r.GetByUserId(1))
            .ReturnsAsync(new Portfolio
            {
                PortfolioId  = 1,
                AppUserId    = 1,
                CashBalance  = 850.00m,
                CreatedAt    = DateTime.UtcNow
            });

        var service = new PortfolioService(mockRepo.Object);
        var result  = await service.GetByUserId(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1,        result.Payload!.PortfolioId);
        Assert.Equal(850.00m,  result.Payload.CashBalance);
        Assert.Equal(850.00m,  result.Payload.TotalValue);       // equals CashBalance this slice
        Assert.Equal(0,        result.Payload.HoldingCount);
        Assert.Equal(0,        result.Payload.TotalUnrealizedGl);
    }

    [Fact]
    public async Task GetByUserId_ReturnsNotFound_WhenPortfolioDoesNotExist()
    {
        var mockRepo = new Mock<IPortfolioRepository>();
        mockRepo.Setup(r => r.GetByUserId(999))
            .ReturnsAsync((Portfolio?)null);

        var service = new PortfolioService(mockRepo.Object);
        var result  = await service.GetByUserId(999);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultType.NotFound, result.Type);
    }
}