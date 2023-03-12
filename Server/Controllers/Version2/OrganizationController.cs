using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Controllers.Version2.DTO;
using Server.Controllers.Version2.Service;
using Server.Models.Authentication;
using Server.Models.Contexts;
using AccountDto = Server.Controllers.Version2.DTO.AccountDto;

namespace Server.Controllers.Version2;

using ModelAccount = Models.Organization.Accounts.Account;
using ModelOrganizationException = Models.Organization.OrganizationException;
using ModelUserGroup = Models.Organization.Accounts.UserGroup;
using ModelOrganization = Models.Organization.Organization;

#if AUTH
using Models.Authentication;
using Microsoft.AspNetCore.Authorization;
[Authorize]
#endif
[ApiController]
[Route("api/v2/organizations")]
public class OrganizationController : ControllerBase
{
    /// <summary>
    /// Получение сведений об организации с указанным Id
    /// </summary>
    [HttpGet("{organizationId}")]
    public ActionResult<OrganizationDto> GetOrganization(ulong organizationId)
    {
        using var orgContext = DbContexts.Get<OrganizationContext>();

        var organization = DbValueRetriever.Retrieve(
            orgContext.Organizations.Include(org => org.Projects), 
            organizationId, 
            nameof(ModelOrganization.Id)
        );

        return organization is not null
            ? Ok(new OrganizationDto(organization))
            : NotFound();
    }

    /// <summary>
    /// Регистрация организации в системе
    /// </summary>
    [HttpPost]
    public ActionResult<OrganizationDto> RegisterOrganization([FromBody] RegisterOrganizationRequestDto registrationRequest)
    {
        using var accountContext = DbContexts.Get<AccountContext>();
        var creator = DbValueRetriever.Retrieve(accountContext.Accounts, registrationRequest.CreatorId, nameof(ModelAccount.Id));
        if (creator is null) { return BadRequest($"account with id = {registrationRequest.CreatorId} doesnt exist"); }
        
        var newOrganization = new ModelOrganization
        {
            Name = registrationRequest.Name,
            Description = registrationRequest.Description
        };
        
        using var orgContext = DbContexts.Get<OrganizationContext>();

        orgContext.Add(newOrganization);
        orgContext.SaveChanges();
        /*
         * добавлять группы пользователей можно только после сохранения организации в бд,
         * так как до сохранения организация не имеет Id (его выдает база данных), вследствие
         * чего группам пользователей выдается некорректный LocalId 
         */
        newOrganization.CreateUserGroups();
        newOrganization.AddAccount(registrationRequest.CreatorId);
        newOrganization.AddAccountToGroup(registrationRequest.CreatorId, (short) ModelOrganization.PrimaryUserGroups.Admins);

        return Ok(new OrganizationDto(newOrganization));
    }

    /// <summary>
    /// Редактирование организации
    /// </summary>
    [HttpPatch("{organizationId}")]
    public ActionResult<OrganizationDto> EditOrganization(ulong organizationId, [FromBody] JsonPatchDocument<ModelOrganization> patch)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, adminRequired: true)) { return Forbid(); }
#endif
        
        using var orgContext = DbContexts.Get<OrganizationContext>();

        var organization = DbValueRetriever.Retrieve(
            orgContext.Organizations.Include(org => org.Projects), 
            organizationId, 
            nameof(ModelOrganization.Id)
        );
        if (organization is null) { return NotFound(); }
        
        patch.ApplyTo(organization);
        orgContext.SaveChanges();
        
        return Ok(new OrganizationDto(organization));
    }

    /// <summary>
    /// Получение списка участников организации
    /// </summary>
    [HttpGet("{organizationId}/members")]
    public ActionResult<IEnumerable<AccountDto>> GetOrganizationMembers(ulong organizationId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, allowNoRole: true)) { return Forbid(); }
#endif
        
        using var orgContext = DbContexts.Get<OrganizationContext>();

        var organization = DbValueRetriever.Retrieve(orgContext.Organizations, organizationId, nameof(ModelOrganization.Id));
        if (organization is null) { return NotFound(); }
        
        var members = new List<ModelAccount>();
        foreach (var userGroup in organization.GetUserGroups()) { members.AddRange(userGroup.Accounts); }

        var dtoMembers = members.Select(member => new AccountDto(member));
        return Ok(dtoMembers);
    }

    /// <summary>
    /// Добавление пользователя в организацию
    /// </summary>
    [HttpPost("{organizationId}/members")]
    public ActionResult<AccountDto> AddUserToOrganization(ulong organizationId, [FromBody] string login)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, adminRequired: true)) { return Forbid(); }
#endif

        using var authInfoContext = DbContexts.Get<AuthInfoContext>();
        var authInfo = DbValueRetriever.Retrieve(authInfoContext.Info, login, nameof(AuthInfo.Login));

        if (authInfo is null) { return NotFound(); }
        
        using var accountContext = DbContexts.Get<AccountContext>();
        var account = DbValueRetriever.Retrieve(
            accountContext.Accounts.Include(acc => acc.UserGroups), 
            authInfo.AccountId, 
            nameof(ModelAccount.Id)
        );
        if (account is null) { return NotFound(); }
        
        using var orgContext = DbContexts.Get<OrganizationContext>();
        var organization = DbValueRetriever.Retrieve(orgContext.Organizations, organizationId, nameof(ModelOrganization.Id));
        if (organization is null) { return NotFound(); }

        organization.AddAccount(account);

        account.AuthInfo = authInfo;
        return Ok(new AccountDto(account));
    }

    /// <summary>
    /// Удаление пользователя из организации
    /// </summary>
    [HttpDelete("{organizationId}/members/{memberId}")]
    public ActionResult DeleteOrganizationsMember(ulong organizationId, ulong memberId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, adminRequired: true)) { return Forbid(); }
#endif
        
        using var orgContext = DbContexts.Get<OrganizationContext>();
        
        var organization = DbValueRetriever.Retrieve(orgContext.Organizations, organizationId, nameof(ModelOrganization.Id));
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
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, allowNoRole: true)) { return Forbid(); }
#endif
        
        using var orgContext = DbContexts.Get<OrganizationContext>();

        var organization = DbValueRetriever.Retrieve(orgContext.Organizations, organizationId, nameof(ModelOrganization.Id));
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
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, allowNoRole: true)) { return Forbid(); }
#endif
        
        using var orgContext = DbContexts.Get<OrganizationContext>();
        
        var organization = DbValueRetriever.Retrieve(orgContext.Organizations, organizationId, nameof(ModelOrganization.Id));
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
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, adminRequired: true)) { return Forbid(); }
#endif
        
        using var orgContext = DbContexts.Get<OrganizationContext>();

        var organization = DbValueRetriever.Retrieve(orgContext.Organizations, organizationId, nameof(ModelOrganization.Id));
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
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, adminRequired: true)) { return Forbid(); }
#endif
        
        using var orgContext = DbContexts.Get<OrganizationContext>();

        var organization = DbValueRetriever.Retrieve(orgContext.Organizations, organizationId, nameof(ModelOrganization.Id));
        if (organization is null) { return NotFound(); }

        organization.RemoveAccountFromGroup(memberId, userGroupLocalId);
        return Ok();
    }
}