using Server.DTOs.Requests;
using Server.DTOs.Responses;

namespace Server.Domain;

public interface IAuthService
{
    Task<Result<AuthResponse>> Register(RegisterRequest request);
    Task<Result<AuthResponse>> Login(LoginRequest request);
}