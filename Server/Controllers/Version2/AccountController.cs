using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Models.Authentication;
using Server.Models.Contexts;
using Server.Models.Organization.Accounts;
using AccountDto = Server.Controllers.Version2.DTO.AccountDto;

namespace Server.Controllers.Version2;

// todo документирование кодов возврата
#if AUTH
using Microsoft.AspNetCore.Authorization;
[Authorize]
#endif
[ApiController]
[Route("api/v2/accounts")]
public class AccountController : ControllerBase
{
    /// <summary>
    /// Получение аккаунта с указанным Login
    /// </summary>
    [HttpGet("{accountLogin}")]
    public ActionResult<AccountDto> GetAccountByLogin(string accountLogin)
    {
        using var authInfoContext = DbContexts.Get<AuthInfoContext>();
        var account = authInfoContext.Info
            .Where(info => info.Login == accountLogin)
            .Include(info => info.Account)
            .Select(info => info.Account)
            .SingleOrDefault();
        if (account is null) { return NotFound(); }

        var foundAccount = new Account
        {
            Id = account.Id,
            Name = account.Name,
            UserGroups = account.GetUserGroups(),
            AuthInfo = new AuthInfo
            {
                Login = accountLogin,
                PasswordHash = string.Empty
            }
        };
        return Ok(new AccountDto(foundAccount));
    }
    
    /// <summary>
    /// Получение аккаунта с указанным Id
    /// </summary>
    [HttpGet("{accountId:long}")]
    public ActionResult<AccountDto> GetAccountById(long accountId)
    {
        using var accountContext = DbContexts.Get<AccountContext>();
        
        var account = accountContext.Accounts
            .Where(acc => acc.Id == (ulong) accountId)
            .Include(acc => acc.UserGroups)
            .Include(acc => acc.AuthInfo)
            .SingleOrDefault();

        return account is not null
            ? Ok(new AccountDto(account))
            : NotFound();
    }

    /// <summary>
    /// Изменение данных аккаунта с указанным Id
    /// </summary>
    [HttpPatch("{accountId}")]
    public ActionResult<AccountDto> PatchAccount(ulong accountId, [FromBody] JsonPatchDocument<Account> patch)
    {
        using var accountContext = DbContexts.Get<AccountContext>();
        var account = accountContext.Accounts
            .Where(acc => acc.Id == accountId)
            .Include(acc => acc.UserGroups)
            .Include(acc => acc.AuthInfo)
            .SingleOrDefault();
        if (account is null) { return NotFound(); }
        
        
        patch.ApplyTo(account);
        accountContext.SaveChanges(); 

        return Ok(new AccountDto(account)) ;
    }

    /// <summary>
    /// Получение списка организаций, в которых состоит указанный аккаунт
    /// </summary>
    [HttpGet("{accountId}/Organizations")]
    public ActionResult<IEnumerable<ulong>> GetAccountsOrganizations(ulong accountId)
    {
        using var accountContext = DbContexts.Get<AccountContext>();
        
        var account = accountContext.Accounts.SingleOrDefault(acc => acc.Id == accountId);
        if (account is null) { return NotFound(); }

        var organizationIds = account.GetOrganizations().Select(org => org.Id);
        return Ok(organizationIds);
    }
}
