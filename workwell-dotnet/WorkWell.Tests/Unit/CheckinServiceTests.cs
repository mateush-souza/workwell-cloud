using AutoMapper;
using FluentAssertions;
using Moq;
using WorkWell.Application.DTOs;
using WorkWell.Application.Mappings;
using WorkWell.Application.Services;
using WorkWell.Domain.Entities;
using WorkWell.Domain.Interfaces;
using Xunit;

namespace WorkWell.Tests.Unit;

public class CheckinServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly CheckinService _checkinService;

    public CheckinServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AutoMapperProfile>();
        });
        _mapper = configuration.CreateMapper();
        
        _checkinService = new CheckinService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task CreateCheckinAsync_WithValidData_ShouldCreateCheckin()
    {
        // Arrange
        var usuarioId = 1;
        var usuario = new Usuario 
        { 
            Id = usuarioId, 
            Nome = "Test User",
            Email = "test@test.com",
            SenhaHash = "hash",
            EmpresaId = 1
        };

        var request = new CreateCheckinRequest
        {
            NivelStress = 5,
            HorasTrabalhadas = 8,
            HorasSono = 7,
            Sentimento = "Neutro"
        };

        _unitOfWorkMock.Setup(x => x.Usuarios.GetByIdAsync(usuarioId))
            .ReturnsAsync(usuario);

        _unitOfWorkMock.Setup(x => x.Checkins.GetByUsuarioAndDataAsync(usuarioId, It.IsAny<DateTime>()))
            .ReturnsAsync((CheckinDiario?)null);

        _unitOfWorkMock.Setup(x => x.Checkins.AddAsync(It.IsAny<CheckinDiario>()))
            .ReturnsAsync((CheckinDiario c) => c);

        // Act
        var result = await _checkinService.CreateCheckinAsync(usuarioId, request);

        // Assert
        result.Should().NotBeNull();
        result.UsuarioId.Should().Be(usuarioId);
        result.NivelStress.Should().Be(request.NivelStress);
        result.HorasTrabalhadas.Should().Be(request.HorasTrabalhadas);
        result.ScoreBemEstar.Should().BeGreaterThan(0);
        
        _unitOfWorkMock.Verify(x => x.Checkins.AddAsync(It.IsAny<CheckinDiario>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateCheckinAsync_WhenCheckinAlreadyExists_ShouldThrowException()
    {
        // Arrange
        var usuarioId = 1;
        var usuario = new Usuario 
        { 
            Id = usuarioId, 
            Nome = "Test User",
            Email = "test@test.com",
            SenhaHash = "hash",
            EmpresaId = 1
        };

        var existingCheckin = new CheckinDiario
        {
            Id = 1,
            UsuarioId = usuarioId,
            DataCheckin = DateTime.UtcNow
        };

        var request = new CreateCheckinRequest
        {
            NivelStress = 5,
            HorasTrabalhadas = 8
        };

        _unitOfWorkMock.Setup(x => x.Usuarios.GetByIdAsync(usuarioId))
            .ReturnsAsync(usuario);

        _unitOfWorkMock.Setup(x => x.Checkins.GetByUsuarioAndDataAsync(usuarioId, It.IsAny<DateTime>()))
            .ReturnsAsync(existingCheckin);

        // Act
        Func<Task> act = async () => await _checkinService.CreateCheckinAsync(usuarioId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Já existe um check-in para esta data");
    }

    [Fact]
    public async Task GetUserStatisticsAsync_WithCheckins_ShouldReturnStatistics()
    {
        // Arrange
        var usuarioId = 1;
        var checkins = new List<CheckinDiario>
        {
            new() { UsuarioId = usuarioId, NivelStress = 5, HorasTrabalhadas = 8, HorasSono = 7, ScoreBemEstar = 80 },
            new() { UsuarioId = usuarioId, NivelStress = 6, HorasTrabalhadas = 9, HorasSono = 6, ScoreBemEstar = 75 },
            new() { UsuarioId = usuarioId, NivelStress = 4, HorasTrabalhadas = 7, HorasSono = 8, ScoreBemEstar = 85 }
        };

        _unitOfWorkMock.Setup(x => x.Checkins.GetByUsuarioAsync(usuarioId, null, null))
            .ReturnsAsync(checkins);

        // Act
        var result = await _checkinService.GetUserStatisticsAsync(usuarioId);

        // Assert
        result.Should().NotBeNull();
        result.TotalCheckins.Should().Be(3);
        result.MediaStress.Should().Be(5);
        result.MediaHorasTrabalhadas.Should().Be(8);
        result.MediaHorasSono.Should().Be(7);
    }
}

