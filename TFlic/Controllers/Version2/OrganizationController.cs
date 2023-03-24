using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TFlic.Controllers.Version2.DTO;
using TFlic.Controllers.Version2.Service;
using TFlic.Models.Domain.Authentication;
using TFlic.Models.Domain.Organization;
using TFlic.Models.Domain.Organization.Accounts;
using TFlic.Models.Services.Contexts;
using AccountDto = TFlic.Controllers.Version2.DTO.AccountDto;

namespace TFlic.Controllers.Version2;

using ModelAccount = Account;
using ModelOrganizationException = OrganizationException;
using ModelUserGroup = UserGroup;
using ModelOrganization = Organization;

[Authorize]
[ApiController]
[Route("api/v2")]
public class OrganizationController : ControllerBase
{
    public OrganizationController(
        OrganizationContext organizationContext,
        AccountContext accountContext,
        AuthInfoContext authInfoContext)
    {
        _organizationContext = organizationContext;
        _accountContext = accountContext;
        _authInfoContext = authInfoContext;
    }
    
    /// <summary>
    /// Получение сведений об организации с указанным Id
    /// </summary>
    [HttpGet("organizations/{organizationId}")]
    public ActionResult<OrganizationDto> GetOrganization(ulong organizationId)
    {
        var organization = DbValueRetriever.Retrieve(
            _organizationContext.Organizations.Include(org => org.Projects), 
            organizationId, 
            nameof(ModelOrganization.Id)
        );

        return organization is not null
            ? Ok(new OrganizationDto(organization))
            : NotFound();
    }

    /// <summary>
    /// Создание организации
    /// </summary>
    [HttpPost("organizations")]
    public ActionResult<OrganizationDto> CreateOrganization([FromBody] NewOrganizationDto registration)
    {
        var creator = DbValueRetriever.Retrieve(_accountContext.Accounts, registration.CreatorId, nameof(ModelAccount.Id));
        if (creator is null) { return BadRequest($"account with id = {registration.CreatorId} doesnt exist"); }
        
        var newOrganization = new ModelOrganization
        {
            Name = registration.Name,
            Description = registration.Description
        };
        
        _organizationContext.Add(newOrganization);
        _organizationContext.SaveChanges();
        /*
         * добавлять группы пользователей можно только после сохранения организации в бд,
         * так как до сохранения организация не имеет Id (его выдает база данных), вследствие
         * чего группам пользователей выдается некорректный LocalId 
         */
        newOrganization.CreateUserGroups();
        newOrganization.AddAccount(registration.CreatorId);
        newOrganization.AddAccountToGroup(registration.CreatorId, (short) ModelOrganization.PrimaryUserGroups.Admins);

        return Ok(new OrganizationDto(newOrganization));
    }

    /// <summary>
    /// Редактирование организации
    /// </summary>
    [HttpPatch("{organizationId}")]
    public ActionResult<OrganizationDto> EditOrganization(ulong organizationId, [FromBody] JsonPatchDocument<ModelOrganization> patch)
    {
        var organization = DbValueRetriever.Retrieve(
            _organizationContext.Organizations.Include(org => org.Projects), 
            organizationId, 
            nameof(ModelOrganization.Id)
        );
        if (organization is null) { return NotFound(); }
        
        patch.ApplyTo(organization);
        _organizationContext.SaveChanges();
        
        return Ok(new OrganizationDto(organization));
    }

    /// <summary>
    /// Получение списка участников организации
    /// </summary>
    [HttpGet("{organizationId}/members")]
    public ActionResult<IEnumerable<AccountDto>> GetOrganizationMembers(ulong organizationId)
    {
        var organization = DbValueRetriever.Retrieve(_organizationContext.Organizations, organizationId, nameof(ModelOrganization.Id));
        if (organization is null) { return NotFound(); }
        
        var members = new List<ModelAccount>();
        foreach (var userGroup in organization.GetUserGroups()) { members.AddRange(userGroup.Accounts); }

        var dtoMembers = members.Select(member => new AccountDto(member));
        return Ok(dtoMembers);
    }

    /// <summary>
    /// Добавление пользователя в организацию
    /// </summary>
    [HttpPost("organizations/{organizationId}/members")]
    // todo заменить логин на id
    public ActionResult<AccountDto> AddUserToOrganization(ulong organizationId, [FromBody] string login)
    {
        var authInfo = DbValueRetriever.Retrieve(_authInfoContext.Info, login, nameof(AuthInfo.Login));
        if (authInfo is null) { return NotFound(); }
        
        var account = DbValueRetriever.Retrieve(
            _accountContext.Accounts.Include(acc => acc.UserGroups), 
            authInfo.AccountId, 
            nameof(ModelAccount.Id)
        );
        if (account is null) { return NotFound(); }
        
        var organization = DbValueRetriever.Retrieve(_organizationContext.Organizations, organizationId, nameof(ModelOrganization.Id));
        if (organization is null) { return NotFound(); }

        organization.AddAccount(account);

        account.AuthInfo = authInfo;
        return Ok(new AccountDto(account));
    }

    /// <summary>
    /// Удаление пользователя из организации
    /// </summary>
    [HttpDelete("organizations/{organizationId}/members/{memberId}")]
    public ActionResult DeleteOrganizationsMember(ulong organizationId, ulong memberId)
    {
        var organization = DbValueRetriever.Retrieve(_organizationContext.Organizations, organizationId, nameof(ModelOrganization.Id));
        if (organization is null) { return NotFound(); }
        
        var removed = organization.RemoveAccount(memberId);
        
        return removed is not null
            ? Ok()
            : NotFound();
    }

    /// <summary>
    /// Получение групп пользователей организации
    /// </summary>
    [HttpGet("{organizationId}/userGroups")]
    public ActionResult<IEnumerable<UserGroupDto>> GetUserGroups(ulong organizationId)
    {
        var organization = DbValueRetriever.Retrieve(_organizationContext.Organizations, organizationId, nameof(ModelOrganization.Id));
        if (organization is null) { return NotFound(); }
        
        var dtoUserGroups = organization.GetUserGroups().Select(ug => new UserGroupDto(ug));
        return Ok(dtoUserGroups);
    }

    /// <summary>
    /// Получение участников указанной группы пользователей в организации
    /// </summary>
    [HttpGet("{organizationId}/userGroups/{userGroupLocalId}/members")]
    public ActionResult<IEnumerable<AccountDto>> GetUserGroupMembers(ulong organizationId, short userGroupLocalId)
    {
        var organization = DbValueRetriever.Retrieve(_organizationContext.Organizations, organizationId, nameof(ModelOrganization.Id));
        if (organization is null) { return NotFound(); }
        
        var userGroup = DbValueRetriever.Retrieve(organization.GetUserGroups(), userGroupLocalId, nameof(ModelUserGroup.LocalId));
        if (userGroup is null) { return NotFound(); }

        var accounts = userGroup.Accounts.Select(acc => new AccountDto(acc));
        return Ok(accounts);
    }
    
    /// <summary>
    /// Добавление аккаунта в указанную группу пользователей организации
    /// </summary>
    [HttpPost("{organizationId}/userGroups/{userGroupLocalId}/members/{memberId}")]
    public ActionResult AddMemberToUserGroup(ulong organizationId, short userGroupLocalId, ulong memberId)
    {
        var organization = DbValueRetriever.Retrieve(_organizationContext.Organizations, organizationId, nameof(ModelOrganization.Id));
        if (organization is null) { return NotFound(); }
        
        if (organization.Contains(memberId) is null)
        {
            return BadRequest(
                $"User with Id = {memberId} is not in organization with Id = {organizationId}. " +
                "The user must be added to organization before being added to any of its user groups"
            );
        }

        try { organization.AddAccountToGroup(memberId, userGroupLocalId); }
        catch (ModelOrganizationException) { return BadRequest($"Server with Id = {organizationId} doesnt contain a user group with local Id = {userGroupLocalId}"); }
        
        return Ok();
    }
    
    /// <summary>
    /// Удаление аккаунта из указанной группы пользователей организации
    /// </summary>
    [HttpDelete("{organizationId}/userGroups/{userGroupLocalId}/members/{memberId}")]
    public ActionResult DeleteMemberFromUserGroup(ulong organizationId, short userGroupLocalId, ulong memberId)
    {
        var organization = DbValueRetriever.Retrieve(_organizationContext.Organizations, organizationId, nameof(ModelOrganization.Id));
        if (organization is null) { return NotFound(); }

        organization.RemoveAccountFromGroup(memberId, userGroupLocalId);
        return Ok();
    }



    private readonly OrganizationContext _organizationContext;
    private readonly AccountContext _accountContext;
    private readonly AuthInfoContext _authInfoContext;
}