using Microsoft.EntityFrameworkCore;
using TFlic.Models.Domain.Authentication;
using TFlic.Models.Services.Contexts;

namespace TFlic.Models.Domain.Organization.Accounts;

public class Account
{
    #region Methods
    /// <summary>
    /// Метод проверяет, состоит ли пользователь в организации с указанным уникальным идентификатором
    /// </summary>
    /// <param name="id">Уникальный идентификатор проверяемой организации</param>
    /// <returns>Ссылка на организацию с указанным Id, если состоит, иначе - false</returns>
    public Organization? IsMemberOf(ulong id)
    {
        using var dbContext = DbContexts.Get<TFlicDbContext>();
        var organization = dbContext.Organizations.FirstOrDefault(org => org.Id == id);
        if (organization is null) { throw new OrganizationException($"Организация с Id = {id} не существует"); }
        
        return organization.Contains(Id) is not null ? organization : null;
    }

    public IEnumerable<Organization> GetOrganizations()
    {
        using var dbContext = DbContexts.Get<TFlicDbContext>();
        var account = dbContext.Accounts
            .Where(acc => acc.Id == Id)
            .Include(acc => acc.UserGroups)
            .ThenInclude(userGroup => userGroup.Accounts)
            .Single();

        var organizations = new List<Organization>();
        foreach (var userGroup in account.UserGroups)
        {
            var org = dbContext.Organizations.Single(org => org.Id == userGroup.OrganizationId);
            if (!organizations.Contains(org)) { organizations.Add(org); }
        }

        return organizations;
    }
    
    public ICollection<UserGroup> GetUserGroups()
    {
        using var dbContext = DbContexts.Get<TFlicDbContext>();
        var account = dbContext.Accounts
            .Where(acc => acc.Id == Id)
            .Include(acc => acc.UserGroups)
            .Single();
        
        return account.UserGroups;
    }
    #endregion
    
    #region Properties
    /// <summary>
    /// Уникальный идентификатор аккаунта
    /// </summary>
    public ulong Id { get; init;  }
    
    /// <summary>
    /// Имя аккаунта
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Организации, в которых состоит пользователь
    /// </summary>
    public ICollection<UserGroup> UserGroups { get; set; } = null!;

    /// <summary>
    /// Служебное поле, используется EF для настройки связи многие-ко-многим с сущностью Account
    /// </summary>
    public List<UserGroupsAccounts> UserGroupsAccounts { get; set; } = null!;

    public AuthInfo AuthInfo { get; set; } = null!;
    #endregion
}