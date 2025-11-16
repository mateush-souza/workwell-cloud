using FluentAssertions;
using Moq;
using WorkWell.Application.Services;
using WorkWell.Domain.Entities;
using WorkWell.Domain.Enums;
using WorkWell.Domain.Interfaces;
using Xunit;

namespace WorkWell.Tests.Unit;

public class BurnoutPredictionServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly BurnoutPredictionService _burnoutService;

    public BurnoutPredictionServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _burnoutService = new BurnoutPredictionService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task PredictBurnoutRiskAsync_WithHighStressLevels_ShouldReturnHighRisk()
    {
        // Arrange
        var usuarioId = 1;
        var checkins = GenerateHighStressCheckins(usuarioId);

        _unitOfWorkMock.Setup(x => x.Checkins.GetByUsuarioAsync(usuarioId, It.IsAny<DateTime>(), null))
            .ReturnsAsync(checkins);

        // Act
        var result = await _burnoutService.PredictBurnoutRiskAsync(usuarioId);

        // Assert
        result.Should().NotBeNull();
        result.UsuarioId.Should().Be(usuarioId);
        result.NivelRisco.Should().BeOneOf(NivelRisco.Alto, NivelRisco.Critico);
        result.ScoreRisco.Should().BeGreaterThan(50);
        result.Recomendacoes.Should().NotBeEmpty();
    }

    [Fact]
    public async Task PredictBurnoutRiskAsync_WithLowStressLevels_ShouldReturnLowRisk()
    {
        // Arrange
        var usuarioId = 1;
        var checkins = GenerateLowStressCheckins(usuarioId);

        _unitOfWorkMock.Setup(x => x.Checkins.GetByUsuarioAsync(usuarioId, It.IsAny<DateTime>(), null))
            .ReturnsAsync(checkins);

        // Act
        var result = await _burnoutService.PredictBurnoutRiskAsync(usuarioId);

        // Assert
        result.Should().NotBeNull();
        result.UsuarioId.Should().Be(usuarioId);
        result.NivelRisco.Should().Be(NivelRisco.Baixo);
        result.ScoreRisco.Should().BeLessThan(25);
    }

    [Fact]
    public async Task PredictBurnoutRiskAsync_WithNoData_ShouldReturnLowRisk()
    {
        // Arrange
        var usuarioId = 1;
        var checkins = new List<CheckinDiario>();

        _unitOfWorkMock.Setup(x => x.Checkins.GetByUsuarioAsync(usuarioId, It.IsAny<DateTime>(), null))
            .ReturnsAsync(checkins);

        // Act
        var result = await _burnoutService.PredictBurnoutRiskAsync(usuarioId);

        // Assert
        result.Should().NotBeNull();
        result.NivelRisco.Should().Be(NivelRisco.Baixo);
        result.Descricao.Should().Contain("Dados insuficientes");
    }

    private List<CheckinDiario> GenerateHighStressCheckins(int usuarioId)
    {
        var checkins = new List<CheckinDiario>();
        for (int i = 0; i < 30; i++)
        {
            checkins.Add(new CheckinDiario
            {
                Id = i + 1,
                UsuarioId = usuarioId,
                DataCheckin = DateTime.UtcNow.AddDays(-i),
                NivelStress = 8 + (i % 3),
                HorasTrabalhadas = 11 + (i % 2),
                HorasSono = 5,
                ScoreBemEstar = 40
            });
        }
        return checkins;
    }

    private List<CheckinDiario> GenerateLowStressCheckins(int usuarioId)
    {
        var checkins = new List<CheckinDiario>();
        for (int i = 0; i < 30; i++)
        {
            checkins.Add(new CheckinDiario
            {
                Id = i + 1,
                UsuarioId = usuarioId,
                DataCheckin = DateTime.UtcNow.AddDays(-i),
                NivelStress = 2 + (i % 2),
                HorasTrabalhadas = 7 + (i % 2),
                HorasSono = 8,
                ScoreBemEstar = 85
            });
        }
        return checkins;
    }
}

