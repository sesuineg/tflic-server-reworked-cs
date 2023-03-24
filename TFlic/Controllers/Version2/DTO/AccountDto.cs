﻿using TFlic.Models.Domain.Organization.Accounts;

namespace TFlic.Controllers.Version2.DTO;

using ModelAccount = Account;

public record AccountDto
{
    public AccountDto(ModelAccount account)
    {
        Id = account.Id;
        Name = account.Name;
        Login = account.AuthInfo.Login;

        var userGroups = account.UserGroups;
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (userGroups is null) { return; }
        foreach (var userGroup in userGroups) { UserGroups.Add(userGroup.GlobalId); }
    }
    
    public ulong Id { get; }
    public string Login { get; }
    public string Name { get; }
    public List<ulong> UserGroups { get; } = new();
}