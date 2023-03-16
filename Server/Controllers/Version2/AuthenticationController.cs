using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Controllers.Version2.DTO;
using Server.Models.Authentication;
using Server.Models.Contexts;
using Server.Services;
using AccountDto = Server.Controllers.Version2.DTO.AccountDto;
using AuthorizeRequestDto = Server.Controllers.Version2.DTO.AuthorizeRequestDto;
using AuthorizeResponseDto = Server.Controllers.Version2.DTO.AuthorizeResponseDto;

namespace Server.Controllers.Version2;

using ModelAccount = Models.Organization.Accounts.Account;

[ApiController]
[Route("api/v2")]
public class AuthenticationController : ControllerBase
{
    public AuthenticationController(
        IAccessTokenService accessTokenService, 
        IRefreshTokenService refreshTokenService) 
    {
        _accessTokenService = accessTokenService;
        _refreshTokenService = refreshTokenService;
    }
    
    /// <summary>
    /// Аутентификация и авторизация пользователя
    /// </summary>
    [HttpPost("authorize")]
    public ActionResult<AuthorizeResponseDto>Authorize(AuthorizeRequestDto authorizeRequest)
    {
        using var authInfoContext = DbContexts.Get<AuthInfoContext>();
        
        var authInfo = authInfoContext.Info.Where(
                info => info.Login == authorizeRequest.Login &&
                        info.PasswordHash == authorizeRequest.PasswordHash
            ).Include(info => info.Account)
            .SingleOrDefault();
        if (authInfo is null) { return NotFound(); }
        
        authInfo.Account.UserGroups = authInfo.Account.GetUserGroups();

        var (accessToken, refreshToken, refreshTokenExpirationTime) = GenerateTokens(authInfo.AccountId); 
        
        // сохраняем сгенерированный refreshToken в базу данных 
        authInfo.RefreshToken = refreshToken; 
        authInfo.RefreshTokenExpirationTime = refreshTokenExpirationTime; 
        authInfoContext.SaveChanges();
        
        return Ok(new AuthorizeResponseDto(
            new AccountDto(authInfo.Account),
            new TokenPairDto(accessToken, refreshToken)
        ));
    }
    
    /// <summary>
    /// Обновление токена 
    /// </summary>
    [HttpPost("refresh")]
    public ActionResult<TokenPairDto> Refresh(TokenPairDto request) 
    {
        using var accountContext = DbContexts.Get<AccountContext>();
        using var authInfoContext = DbContexts.Get<AuthInfoContext>();

        var account = _accessTokenService.GetPrincipalFromToken(request.AccessToken); // todo invalid principal check
        var sidClaim = account.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Sid);
        if (sidClaim is null) return NotFound();

        var accountId = ulong.Parse(sidClaim.Value);
        var authInfo = authInfoContext.Info
            .SingleOrDefault(info => info.AccountId == accountId);
        if (authInfo is null) { return NotFound(); }

        var refreshTokenValid = _refreshTokenService.IsTokenValid(request.RefreshToken, authInfo.AccountId);
        if (!refreshTokenValid) { return Unauthorized(); }
        
        var (accessToken, refreshToken, refreshTokenExpirationTime) = GenerateTokens(authInfo.AccountId);

        authInfo.RefreshToken = refreshToken;
        authInfo.RefreshTokenExpirationTime = refreshTokenExpirationTime;
        authInfoContext.SaveChanges();
        
        return Ok(new TokenPairDto(accessToken, refreshToken));
    }

    /// <summary>
    /// Регистрация пользователя в системе
    /// </summary>
    [HttpPost("register")]
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
        
        var (accessToken, refreshToken, refreshTokenExpirationTime) = GenerateTokens(newAccount.Id);

        newAccount.AuthInfo.RefreshToken = refreshToken;
        newAccount.AuthInfo.RefreshTokenExpirationTime = refreshTokenExpirationTime;
        accountContext.SaveChanges();
        
        return Ok(new AuthorizeResponseDto(
            new AccountDto(newAccount), 
            new TokenPairDto(accessToken, refreshToken)
        ));
    }

    private (string accesToken, string refreshToken, DateTime refreshTokenExpirationTime) GenerateTokens(ulong ownerId)
    {
        var accessTokenClaims = new List<Claim>{
            CreateClaim(ClaimTypes.Sid, ownerId.ToString())
        };
        var accessToken = _accessTokenService.Generate(accessTokenClaims);
        var (refreshToken, expirationTime) = _refreshTokenService.Generate();

        return (accessToken, refreshToken, expirationTime);



        Claim CreateClaim(string type, string value) =>
            new Claim(type, value, ClaimValueTypes.String);
    }
    
    // private bool IsRefreshTokenValid(string token, )    

    private readonly IAccessTokenService _accessTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
}