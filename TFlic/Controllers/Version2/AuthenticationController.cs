﻿using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TFlic.Controllers.Version2.DTO;
using TFlic.Models.Authentication;
using TFlic.Models.Contexts;
using TFlic.Services;
using AuthorizeRequestDto = TFlic.Controllers.Version2.DTO.AuthorizeRequestDto;
using AuthorizeResponseDto = TFlic.Controllers.Version2.DTO.AuthorizeResponseDto;

namespace TFlic.Controllers.Version2;

using ModelAccount = Models.Organization.Accounts.Account;

[ApiController]
[Route("api/v2")]
public class AuthenticationController : ControllerBase
{
    public AuthenticationController(
        AccountContext accountContext,
        AuthInfoContext authInfoContext, 
        IAccessTokenService accessTokenService,
        IRefreshTokenService refreshTokenService)
    {
        _accountContext = accountContext;
        _authInfoContext = authInfoContext;
        _accessTokenService = accessTokenService;
        _refreshTokenService = refreshTokenService;
    }
    
    /// <summary>
    /// Аутентификация и авторизация пользователя
    /// </summary>
    [HttpPost("authorize")]
    public ActionResult<AuthorizeResponseDto>Authorize(AuthorizeRequestDto authorizeRequest)
    {
        var passwordHash = HashPassword(authorizeRequest.Password);
        var authInfo = FindAccountsAuthInfo();
        if (authInfo is null) { return NotFound(); }
        
        authInfo.Account.UserGroups = authInfo.Account.GetUserGroups();

        var (accessToken, refreshToken, refreshTokenExpirationTime) = GenerateTokens(authInfo.AccountId); 
        
        // сохраняем сгенерированный refreshToken в базу данных 
        authInfo.RefreshToken = refreshToken; 
        authInfo.RefreshTokenExpirationTime = refreshTokenExpirationTime; 
        _authInfoContext.SaveChanges();
        
        return Ok(new AuthorizeResponseDto(
            new AccountDto(authInfo.Account),
            new TokenPairDto(accessToken, refreshToken)
        ));



        AuthInfo? FindAccountsAuthInfo() =>
            _authInfoContext.Info.Where(
                    info => info.Login == authorizeRequest.Login &&
                            info.PasswordHash == passwordHash
                ).Include(info => info.Account)
                .SingleOrDefault();
        
    }
    
    /// <summary>
    /// Обновление токена 
    /// </summary>
    [HttpPost("refresh")]
    public ActionResult<TokenPairDto> Refresh(TokenPairDto request) 
    {
        var account = _accessTokenService.GetPrincipalFromToken(request.AccessToken); // todo invalid principal check
        var sidClaim = account.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Sid);
        if (sidClaim is null) return NotFound();

        var accountId = ulong.Parse(sidClaim.Value);
        var authInfo = _authInfoContext.Info
            .SingleOrDefault(info => info.AccountId == accountId);
        if (authInfo is null) { return NotFound(); }

        var refreshTokenValid = _refreshTokenService.IsTokenValid(request.RefreshToken, authInfo.AccountId);
        if (!refreshTokenValid) { return Unauthorized(); }
        
        var (accessToken, refreshToken, refreshTokenExpirationTime) = GenerateTokens(authInfo.AccountId);

        authInfo.RefreshToken = refreshToken;
        authInfo.RefreshTokenExpirationTime = refreshTokenExpirationTime;
        _authInfoContext.SaveChanges();
        
        return Ok(new TokenPairDto(accessToken, refreshToken));
    }

    /// <summary>
    /// Регистрация пользователя в системе
    /// </summary>
    [HttpPost("register")]
    public ActionResult<AuthorizeResponseDto> Register(RegisterAccountDto account)
    {
        if (IsLoginInUse())  
            return BadRequest("login already in use"); 
        
        var passwordHash = HashPassword(account.Password);
        var newAccount = CreateNewAccount();
        newAccount = _accountContext.Add(newAccount).Entity;
        _accountContext.SaveChanges();
        
        var (accessToken, refreshToken, refreshTokenExpirationTime) = GenerateTokens(newAccount.Id);

        newAccount.AuthInfo.RefreshToken = refreshToken;
        newAccount.AuthInfo.RefreshTokenExpirationTime = refreshTokenExpirationTime;
        _accountContext.SaveChanges();

        var responseDto = new AuthorizeResponseDto(
            new AccountDto(newAccount),
            new TokenPairDto(accessToken, refreshToken)
        );
        return Ok(responseDto);



        bool IsLoginInUse() =>
            _authInfoContext
                .Info
                .Any(info => info.Login == account.Login);

        ModelAccount CreateNewAccount()
        {
            var newAuthInfo_ = new AuthInfo
            {
                Login = account.Login,
                PasswordHash = passwordHash
            };
            
            var newAccount_ = new ModelAccount
            {
                Name = account.Name,
                AuthInfo = newAuthInfo_
            };
/*
 * package Adeptik.Hosting.AspNet.Extensions contains bag, causes incorrect compilation error:
 * "Action method should return ActionResult explicitly" when local function doesnt return ActionResult
 */
#pragma warning disable ADWC0004 
            return newAccount_;
#pragma warning restore ADWC0004
        }
    }

    
    
    private (string accesToken, string refreshToken, DateTime refreshTokenExpirationTime) GenerateTokens(ulong ownerId)
    {
        var accessTokenClaims = CreateAccessTokenClaims();
        var accessToken = _accessTokenService.Generate(accessTokenClaims);
        var (refreshToken, expirationTime) = _refreshTokenService.Generate();

        return (accessToken, refreshToken, expirationTime);



        IEnumerable<Claim> CreateAccessTokenClaims() =>
            new List<Claim>
            {
                CreateClaim(ClaimTypes.Sid, ownerId.ToString())
            };
        
        Claim CreateClaim(string type, string value) =>
            new Claim(type, value, ClaimValueTypes.String);
    }

    private static string HashPassword(string password)
    {
        var passwordBytes = GetPasswordBytes();
        var passwordHash = SHA256.HashData(passwordBytes);

        var serializedHash = SerializePasswordHash();
        return serializedHash;



        byte[] GetPasswordBytes() =>
            Encoding.UTF8.GetBytes(password);

        string SerializePasswordHash() =>
            Convert.ToBase64String(passwordHash);
    }

    private readonly AccountContext _accountContext;
    private readonly AuthInfoContext _authInfoContext;
    private readonly IAccessTokenService _accessTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
}