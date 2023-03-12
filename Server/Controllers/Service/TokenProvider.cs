namespace Server.Controllers.Service;

// todo реализовать нормальную авторизацию
public static class TokenProvider
{
    public static string GetToken(HttpRequest request) =>
        request.Headers.Authorization.ToString().Replace("Bearer ", "");
}