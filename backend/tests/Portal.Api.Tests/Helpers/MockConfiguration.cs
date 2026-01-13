namespace Portal.Api.Tests.Helpers;

public static class MockConfiguration
{
    public static IConfiguration CreateWithJwtSettings(
        string secret = "TestSecretKeyThatIsAtLeast32CharactersLong!",
        string issuer = "test-issuer",
        int accessTokenMinutes = 15,
        int refreshTokenDays = 7)
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Jwt:Secret", secret},
            {"Jwt:Issuer", issuer},
            {"Jwt:AccessTokenExpiryMinutes", accessTokenMinutes.ToString()},
            {"Jwt:RefreshTokenExpiryDays", refreshTokenDays.ToString()}
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }
}
