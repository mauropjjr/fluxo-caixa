using Microsoft.AspNetCore.Authentication;
using System.Text.Encodings.Web;
using System.Text;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Auth;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.Fail("Missing Authorization Header");

        string authHeader = Request.Headers["Authorization"];
        if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.Fail("Invalid Authorization Header");

        var token = authHeader["Basic ".Length..].Trim();
        var credentialstring = Encoding.UTF8.GetString(Convert.FromBase64String(token));
        var credentials = credentialstring.Split(':');

        if (credentials.Length != 2)
            return AuthenticateResult.Fail("Invalid Authorization Header");

        var username = credentials[0];
        var password = credentials[1];

        var configUsername = Context.RequestServices.GetRequiredService<IConfiguration>()["BasicAuth:Username"];
        var configPassword = Context.RequestServices.GetRequiredService<IConfiguration>()["BasicAuth:Password"];

        if (username != configUsername || password != configPassword)
            return AuthenticateResult.Fail("Invalid Username or Password");

        var claims = new[] { new Claim(ClaimTypes.Name, username) };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}