using Moq;
using Server.Data;
using Server.Domain;
using Server.Models;

namespace Server.Tests.Domain;

[Collection("DatabaseTests")]
public class PortfolioServiceTest
{
    private readonly Mock<IPortfolioRepository> _portfolioRepo = new();
    private readonly Mock<IHoldingRepository>   _holdingRepo   = new();

    [Fact]
    public async Task GetByUserId_ReturnsPortfolioResponse_WhenPortfolioExists()
    {
        _portfolioRepo.Setup(r => r.GetByUserId(1))
            .ReturnsAsync(new Portfolio
            {
                PortfolioId = 1,
                AppUserId   = 1,
                CashBalance = 850.00m,
                CreatedAt   = DateTime.UtcNow
            });

        _holdingRepo.Setup(r => r.GetAllByPortfolioId(1))
            .ReturnsAsync(new List<Holding>());

        var service = new PortfolioService(_portfolioRepo.Object, _holdingRepo.Object);
        var result  = await service.GetByUserId(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1,       result.Payload!.PortfolioId);
        Assert.Equal(850.00m, result.Payload.CashBalance);
        Assert.Equal(850.00m, result.Payload.TotalValue);       
        Assert.Equal(0,       result.Payload.HoldingCount);
        Assert.Equal(0,       result.Payload.TotalUnrealizedGl);
    }

    [Fact]
    public async Task GetByUserId_ReturnsNotFound_WhenPortfolioDoesNotExist()
    {
        _portfolioRepo.Setup(r => r.GetByUserId(999))
            .ReturnsAsync((Portfolio?)null);

        var service = new PortfolioService(_portfolioRepo.Object, _holdingRepo.Object);
        var result  = await service.GetByUserId(999);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultType.NotFound, result.Type);
    }
}