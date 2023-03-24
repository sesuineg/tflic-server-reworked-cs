using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TFlic.Models.Domain.Authentication;
using TFlic.Models.Domain.Organization.Accounts;
using TFlic.Models.Services.Contexts;
using AccountDto = TFlic.Controllers.Version2.DTO.AccountDto;

namespace TFlic.Controllers.Version2;

// todo документирование кодов возврата
[Authorize]
[ApiController]
[Route("api/v2/accounts")]
public class AccountsController : ControllerBase
{
    public AccountsController(
        AccountContext accountContext,
        AuthInfoContext authInfoContext)
    {
        _accountContext = accountContext;
        _authInfoContext = authInfoContext;
    }
    
    /// <summary>
    /// Получение аккаунта с указанным Login
    /// </summary>
    [HttpGet("{login}")]
    public ActionResult<AccountDto> GetAccountByLogin(string login)
    {
        var account = _authInfoContext.Info
            .Where(info => info.Login == login)
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
                Login = login,
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
        var account = _accountContext.Accounts
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
        var account = _accountContext.Accounts
            .Where(acc => acc.Id == accountId)
            .Include(acc => acc.UserGroups)
            .Include(acc => acc.AuthInfo)
            .SingleOrDefault();
        if (account is null) { return NotFound(); }
        
        
        patch.ApplyTo(account);
        _accountContext.SaveChanges(); 

        return Ok(new AccountDto(account)) ;
    }

    /// <summary>
    /// Получение списка организаций, в которых состоит указанный аккаунт
    /// </summary>
    [HttpGet("{accountId}/organizations")]
    public ActionResult<IEnumerable<ulong>> GetAccountsOrganizations(ulong accountId)
    {
        var account = _accountContext.Accounts.SingleOrDefault(acc => acc.Id == accountId);
        if (account is null) { return NotFound(); }

        var organizationIds = account.GetOrganizations().Select(org => org.Id);
        return Ok(organizationIds);
    }
    
    
    
    private readonly AccountContext _accountContext;
    private readonly AuthInfoContext _authInfoContext;
}
