using System.Net;
using System.Net.Http.Json;
using SignalChat.Backend.Models;
using SignalChat.Backend.Tests.Infrastructure;

namespace SignalChat.Backend.Tests.Controllers;

public class AuthControllerTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    // ─── Register ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ValidName_Returns200WithAuthResponse()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new { userName = "TestUser" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);
        Assert.Equal("TestUser", body.UserName);
        Assert.NotEmpty(body.Code);
        Assert.NotEmpty(body.AccessToken);
        Assert.NotEmpty(body.RefreshToken);
        Assert.NotEqual(Guid.Empty, body.Id);
    }

    [Fact]
    public async Task Register_DuplicateName_Returns409()
    {
        await _client.PostAsJsonAsync("/api/auth/register", new { userName = "TestUser" });

        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new { userName = "TestUser" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_NameTooShort_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new { userName = "Abc" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_NameWithDigits_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new { userName = "User123" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_EmptyName_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new { userName = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ─── Login ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCode_Returns200WithAuthResponse()
    {
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register",
            new { userName = "TestUser" });
        var registered = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { code = registered!.Code });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);
        Assert.Equal("TestUser", body.UserName);
        Assert.NotEmpty(body.AccessToken);
        Assert.NotEmpty(body.RefreshToken);
    }

    [Fact]
    public async Task Login_InvalidCode_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { code = "XXXXXX" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_CodeWrongLength_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { code = "ABC" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_EmptyCode_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { code = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ─── Refresh ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Refresh_ValidToken_Returns200WithNewTokens()
    {
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register",
            new { userName = "TestUser" });
        var registered = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        var response = await _client.PostAsJsonAsync("/api/auth/refresh",
            new { refreshToken = registered!.RefreshToken });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);
        Assert.NotEmpty(body.AccessToken);
        Assert.NotEmpty(body.RefreshToken);
        Assert.NotEqual(registered.RefreshToken, body.RefreshToken);
    }

    [Fact]
    public async Task Refresh_InvalidToken_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/refresh",
            new { refreshToken = Guid.NewGuid().ToString() });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_EmptyToken_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/refresh",
            new { refreshToken = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
