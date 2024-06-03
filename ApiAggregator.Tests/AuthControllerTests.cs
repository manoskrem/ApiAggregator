using ApiAggregator.Configuration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

public class AuthControllerTests
{
    private readonly IOptions<JwtSettings> _jwtSettings;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _jwtSettings = Options.Create(new JwtSettings
        {
            Key = "ThisIsBadPractiseIKnowButItsForTestPurposesWeShouldUseKeyVault",
            Issuer = "yourIssuer",
            Audience = "yourAudience",
            DurationInMinutes = 60
        });

        _controller = new AuthController(_jwtSettings);
    }

    [Fact]
    public void GetToken_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var login = new LoginModel { Username = "test", Password = "password" };

        // Act
        var result = _controller.GetToken(login) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();

        var tokenObject = JObject.FromObject(result.Value);
        string tokenString = tokenObject["Token"]?.ToString();
        tokenString.Should().NotBeNullOrEmpty();

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Value.Key);
        tokenHandler.ValidateToken(tokenString, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Value.Issuer,
            ValidAudience = _jwtSettings.Value.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        }, out SecurityToken validatedToken);

        validatedToken.Should().NotBeNull();
    }

    [Fact]
    public void GetToken_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        // Arrange
        var login = new LoginModel { Username = "invalid", Password = "invalid" };

        // Act
        var result = _controller.GetToken(login);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }
}
