using Microsoft.EntityFrameworkCore;
using TFlic.Models.Services.Contexts;

namespace TFlic.Models.Domain.Organization.Accounts;

public class UserGroup
{
    #region Methods
    public bool AddAccount(Account account)
    {
        if (Contains(account.Id) is not null) { return false; }

        using var dbContext = DbContexts.Get<TFlicDbContext>();
        var accounts = dbContext.UserGroups
            .Where(ug => ug.GlobalId == GlobalId)
            .Include(ug => ug.Accounts)
            .Single()
            .Accounts;
        
        accounts.Add(account);
        dbContext.SaveChanges();
        
        return true;
    }

    public Account? RemoveAccount(ulong id)
    {
        using var dbContext = DbContexts.Get<TFlicDbContext>();
        var toRemove = dbContext.Accounts.FirstOrDefault(account => account.Id == id);
        if (toRemove is null) { return null; }
        
        var accounts = dbContext.UserGroups
            .Where(ug => ug.GlobalId == GlobalId)
            .Include(ug => ug.Accounts)
            .ToList()
            .Single()
            .Accounts;

        toRemove = accounts.FirstOrDefault(acc => acc.Id == id);
        if (toRemove is not null) accounts.Remove(toRemove);
        dbContext.SaveChanges();
        
        return toRemove;
    }

    public Account? Contains(ulong id)
    {
        using var dbContext = DbContexts.Get<TFlicDbContext>();
        Account? account = null;

        try
        {
            account = dbContext.UserGroups
                .Where(ug => ug.GlobalId == GlobalId)
                .Include(ug => ug.Accounts)
                .Single()
                .Accounts
                .FirstOrDefault(acc => acc.Id == id);
        }
        catch (NullReferenceException)
        {
            // если аккаунт в базе данных не найден, возвращается Null, дополнительных действий не требуется
        }

        return account;
    }
    #endregion

    #region Properties
    /// <summary>
    /// Уникальный идентификатор группы пользователей
    /// </summary>
    public ulong GlobalId { get; init; }
    
    /// <summary>
    /// Локальный идентификатор группы пользователей
    /// </summary>
    public required short LocalId { get; init; } // todo удалить локальный id
    
    /// <summary>
    /// Уникальный идентификатор организации, которая содержит текущую группу пользователей
    /// </summary>
    public required ulong OrganizationId { get; set; }
    
    /// <summary> 
    /// Название группы пользователей
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Участники группы пользователей
    /// </summary>
    public ICollection<Account> Accounts { get; set; } = null!;

    /// <summary>
    /// Служебное поле, используется EF для настройки связи многие-ко-многим с сущностью UserGroup
    /// </summary>
    public List<UserGroupsAccounts> UserGroupsAccounts { get; set; } = null!;
    #endregion
}