using AutoMapper;
using WorkWell.Application.DTOs;
using WorkWell.Application.Interfaces;
using WorkWell.Domain.Entities;
using WorkWell.Domain.Interfaces;

namespace WorkWell.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public AuthService(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IPasswordHasher passwordHasher,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var usuario = await _unitOfWork.Usuarios.GetByEmailAsync(request.Email);

        if (usuario == null || !_passwordHasher.VerifyPassword(request.Senha, usuario.SenhaHash))
        {
            throw new UnauthorizedAccessException("Email ou senha inválidos");
        }

        if (!usuario.Ativo)
        {
            throw new UnauthorizedAccessException("Usuário inativo");
        }

        // Atualizar último acesso
        usuario.DataUltimoAcesso = DateTime.UtcNow;
        await _unitOfWork.Usuarios.UpdateAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        var token = _jwtService.GenerateToken(usuario.Id, usuario.Email, usuario.Role.ToString());
        var refreshToken = _jwtService.GenerateRefreshToken();

        return new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(2),
            Usuario = _mapper.Map<UsuarioDto>(usuario)
        };
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        // Verificar se email já existe
        if (await _unitOfWork.Usuarios.EmailExistsAsync(request.Email))
        {
            throw new InvalidOperationException("Email já cadastrado");
        }

        // Verificar se empresa existe
        var empresa = await _unitOfWork.Empresas.GetByIdAsync(request.EmpresaId);
        if (empresa == null)
        {
            throw new InvalidOperationException("Empresa não encontrada");
        }

        // Criar usuário
        var usuario = _mapper.Map<Usuario>(request);
        usuario.SenhaHash = _passwordHasher.HashPassword(request.Senha);
        usuario.Ativo = true;

        await _unitOfWork.Usuarios.AddAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        var token = _jwtService.GenerateToken(usuario.Id, usuario.Email, usuario.Role.ToString());
        var refreshToken = _jwtService.GenerateRefreshToken();

        return new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(2),
            Usuario = _mapper.Map<UsuarioDto>(usuario)
        };
    }

    public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = _jwtService.ValidateToken(request.Token);
        if (principal == null)
        {
            throw new UnauthorizedAccessException("Token inválido");
        }

        var userId = int.Parse(principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value ?? "0");
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(userId);

        if (usuario == null || !usuario.Ativo)
        {
            throw new UnauthorizedAccessException("Usuário inválido");
        }

        var newToken = _jwtService.GenerateToken(usuario.Id, usuario.Email, usuario.Role.ToString());
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        return new LoginResponse
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(2),
            Usuario = _mapper.Map<UsuarioDto>(usuario)
        };
    }

    public async Task LogoutAsync(string token)
    {
        // Implementar blacklist de tokens se necessário
        await Task.CompletedTask;
    }
}

