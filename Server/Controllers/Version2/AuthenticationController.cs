using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Controllers.Version2.DTO;
using Server.Models.Authentication;
using Server.Models.Contexts;
using AccountDto = Server.Controllers.Version2.DTO.AccountDto;
using AuthorizeRequestDto = Server.Controllers.Version2.DTO.AuthorizeRequestDto;
using AuthorizeResponseDto = Server.Controllers.Version2.DTO.AuthorizeResponseDto;

namespace Server.Controllers.Version2;

using ModelAccount = Models.Organization.Accounts.Account;

[ApiController]
[Route("api/v2")]
public class AuthenticationController : ControllerBase
{
    /// <summary>
    /// Аутентификация и авторизация пользователя
    /// </summary>
    [HttpPost("/authorize")]
    public ActionResult<AuthorizeResponseDto> Authorize(AuthorizeRequestDto authorizeRequest)
    {
        using var authInfoContext = DbContexts.Get<AuthInfoContext>();
        
        var authInfo = authInfoContext.Info.Where(
                info => info.Login == authorizeRequest.Login &&
                        info.PasswordHash == authorizeRequest.PasswordHash
            ).Include(info => info.Account)
            .SingleOrDefault();
        if (authInfo is null) { return NotFound(); }
        
        authInfo.Account.UserGroups = authInfo.Account.GetUserGroups();

        var (accessToken, refreshToken) = AuthenticationManager.GenerateTokens(authInfo.Account);
        var encodedAccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken);
        var encodedRefreshToken = new JwtSecurityTokenHandler().WriteToken(refreshToken);
        
        // сохраняем сгенерированный refreshToken в базу данных
        authInfo.RefreshToken = encodedRefreshToken; 
        authInfoContext.SaveChanges();
        
        return Ok(new AuthorizeResponseDto(
            new AccountDto(authInfo.Account),
            new TokenPairDto(encodedAccessToken, encodedRefreshToken)
        ));
    }

    /// <summary>
    /// Метод проверяет валидность токена доступа
    /// </summary>
    /// <param name="accessToken">Проверяемый токен доступа</param>
    /// <returns>true в случае, если токен валиден, иначе - false</returns>
    [HttpPost("/try_authorize")]
    public ActionResult<bool> TryAuthorize([FromBody] string accessToken)
    {
        return Ok(AuthenticationManager.IsTokenValid(accessToken, AuthenticationManager.TokenType.Access));
    }

    /// <summary>
    /// Обновление токена 
    /// </summary>
    [HttpPost("/refresh")]
    public ActionResult<TokenPairDto> Refresh(RefreshTokenRequestDto request) 
    {
        using var accountContext = DbContexts.Get<AccountContext>();
        using var authInfoContext = DbContexts.Get<AuthInfoContext>();
        
        var authInfo = authInfoContext.Info
            .Where(info => info.Login == request.Login)
            .Include(info => info.Account)
            .SingleOrDefault();
        if (authInfo is null) { return NotFound(); }

        var refreshTokenValid = AuthenticationManager.IsTokenValid(
            request.RefreshToken,
            AuthenticationManager.TokenType.Refresh
        );
        if (!refreshTokenValid) { return Unauthorized(); }

        var (accessToken, refreshToken) = AuthenticationManager.GenerateTokens(authInfo.Account);
        var encodedAccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken);
        var encodedRefreshToken = new JwtSecurityTokenHandler().WriteToken(refreshToken);
        
        authInfo.RefreshToken = encodedRefreshToken;
        authInfoContext.SaveChanges();
        
        return Ok(new TokenPairDto(encodedAccessToken, encodedRefreshToken));
    }

    /// <summary>
    /// Регистрация пользователя в системе
    /// </summary>
    [HttpPost("/register")]
    public ActionResult<AuthorizeResponseDto> Register(RegisterAccountRequestDto account)
    {
        using var authInfoContext = DbContexts.Get<AuthInfoContext>();
        if (authInfoContext.Info.Any(info => info.Login == account.Login)) { return BadRequest("login already in use"); }
        
        using var accountContext = DbContexts.Get<AccountContext>();
        
        var newAccount = new ModelAccount
        {
            Name = account.Name,
            AuthInfo = new AuthInfo
            {
                Login = account.Login, 
                PasswordHash = account.PasswordHash
            }
        };
        newAccount = accountContext.Add(newAccount).Entity;
        accountContext.SaveChanges();
        
        var (accessToken, refreshToken) = AuthenticationManager.GenerateTokens(newAccount);
        var encodedAccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken);
        var encodedRefreshToken = new JwtSecurityTokenHandler().WriteToken(refreshToken);

        newAccount.AuthInfo.RefreshToken = encodedRefreshToken;
        accountContext.SaveChanges();
        
        return Ok(new AuthorizeResponseDto(
            new AccountDto(newAccount), 
            new TokenPairDto(encodedAccessToken, encodedRefreshToken)
        ));
    }
}