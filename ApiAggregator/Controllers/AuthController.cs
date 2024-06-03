using ApiAggregator.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtSettings _jwtSettings;

    public AuthController(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="login">The login model containing username and password.</param>
    /// <returns>A JWT token if authentication is successful.</returns>
    /// <response code="200">Returns the JWT token.</response>
    /// <response code="401">If the user credentials are invalid.</response>
    [AllowAnonymous]
    [HttpPost("token")]
    public IActionResult GetToken([FromBody] LoginModel login)
    {
        if (IsValidUser(login))
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.Name, login.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { Token = tokenString });
        }

        return Unauthorized();
    }

    /// <summary>
    /// Validates user credentials.
    /// </summary>
    /// <param name="login">The login model containing username and password.</param>
    /// <returns>True if the user credentials are valid; otherwise, false.</returns>
    private bool IsValidUser(LoginModel login)
    {
        // Implement your user validation logic here
        return login.Username == "test" && login.Password == "password";
    }
}

public class LoginModel
{
    public string Username { get; set; }
    public string Password { get; set; }
}
