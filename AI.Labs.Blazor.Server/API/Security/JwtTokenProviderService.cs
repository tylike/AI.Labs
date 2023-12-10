using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.Authentication;
using DevExpress.ExpressApp.Security.Authentication.ClientServer;
using Microsoft.IdentityModel.Tokens;

namespace AI.Labs.WebApi.JWT;

public class JwtTokenProviderService : IAuthenticationTokenProvider {
    readonly IStandardAuthenticationService securityAuthenticationService;
    readonly IConfiguration configuration;

    public JwtTokenProviderService(IStandardAuthenticationService securityAuthenticationService, IConfiguration configuration) {
        this.securityAuthenticationService = securityAuthenticationService;
        this.configuration = configuration;
    }
    public string Authenticate(object logonParameters) {
        ClaimsPrincipal user = securityAuthenticationService.Authenticate(logonParameters);

        if(user != null) {
            var issuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Authentication:Jwt:IssuerSigningKey"]));
            var token = new JwtSecurityToken(
                //issuer: configuration["Authentication:Jwt:Issuer"],
                //audience: configuration["Authentication:Jwt:Audience"],
                claims: user.Claims,
                expires: DateTime.Now.AddDays(2),
                signingCredentials: new SigningCredentials(issuerSigningKey, SecurityAlgorithms.HmacSha256)
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        throw new AuthenticationException("User name or password is incorrect.");
    }
}
