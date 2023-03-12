﻿namespace Server.Controllers.Version1.DTO;

public record AuthorizeResponseDto
{
    public AuthorizeResponseDto(AccountDto accountDto, TokenPairDto tokens)
    {
        AccountDto = accountDto;
        Tokens = tokens;
    }

    public AccountDto AccountDto { get; }
    public TokenPairDto Tokens { get; }
}