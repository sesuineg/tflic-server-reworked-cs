namespace Server.Controllers.Version2.DTO;

public record RegisterAccountRequestDto
{
    public RegisterAccountRequestDto(string login, string name, string passwordHash)
    {
        Login = login;
        Name = name;
        PasswordHash = passwordHash;
    }

    public string Login { get; }
    public string Name { get; }
    public string PasswordHash { get; }
}