using FluentAssertions;
using WorkWell.Application.DTOs;
using WorkWell.Application.Validators;
using Xunit;

namespace WorkWell.Tests.Unit;

public class ValidatorsTests
{
    [Fact]
    public void LoginRequestValidator_WithValidData_ShouldPass()
    {
        // Arrange
        var validator = new LoginRequestValidator();
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Senha = "Password123"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void LoginRequestValidator_WithInvalidEmail_ShouldFail()
    {
        // Arrange
        var validator = new LoginRequestValidator();
        var request = new LoginRequest
        {
            Email = "invalid-email",
            Senha = "Password123"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void RegisterRequestValidator_WithValidData_ShouldPass()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Nome = "Test User",
            Email = "test@example.com",
            Senha = "SecurePass123",
            EmpresaId = 1
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RegisterRequestValidator_WithWeakPassword_ShouldFail()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Nome = "Test User",
            Email = "test@example.com",
            Senha = "weak",
            EmpresaId = 1
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Senha");
    }

    [Fact]
    public void CheckinRequestValidator_WithValidData_ShouldPass()
    {
        // Arrange
        var validator = new CreateCheckinRequestValidator();
        var request = new CreateCheckinRequest
        {
            NivelStress = 5,
            HorasTrabalhadas = 8,
            HorasSono = 7,
            Sentimento = "Neutro"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CheckinRequestValidator_WithInvalidStressLevel_ShouldFail()
    {
        // Arrange
        var validator = new CreateCheckinRequestValidator();
        var request = new CreateCheckinRequest
        {
            NivelStress = 15, // Invalid: should be between 1-10
            HorasTrabalhadas = 8
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NivelStress");
    }
}

