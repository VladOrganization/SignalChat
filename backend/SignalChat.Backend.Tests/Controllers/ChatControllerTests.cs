using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SignalChat.Backend.Database;
using SignalChat.Backend.Database.Entities;
using SignalChat.Backend.Models;
using SignalChat.Backend.Tests.Infrastructure;

namespace SignalChat.Backend.Tests.Controllers;

public class ChatControllerTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    // ─── Auth guard ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMessages_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/chat/messages");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ─── Happy paths ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMessages_EmptyDatabase_Returns200WithEmptyList()
    {
        var token = await RegisterAndGetTokenAsync("Alice");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/chat/messages");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedResult<MessageDto>>();
        Assert.NotNull(body);
        Assert.Empty(body.Items);
        Assert.Equal(0, body.TotalCount);
    }

    [Fact]
    public async Task GetMessages_WithMessages_ReturnsPagedResult()
    {
        var token = await RegisterAndGetTokenAsync("Alice");

        await SeedMessagesAsync("Alice", 5);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/chat/messages?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedResult<MessageDto>>();
        Assert.NotNull(body);
        Assert.Equal(5, body.TotalCount);
        Assert.Equal(5, body.Items.Count);
        Assert.Equal(1, body.Page);
        Assert.Equal(10, body.PageSize);
    }

    [Fact]
    public async Task GetMessages_Pagination_ReturnsCorrectPage()
    {
        var token = await RegisterAndGetTokenAsync("Alice");

        await SeedMessagesAsync("Alice", 7);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/chat/messages?page=2&pageSize=3");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedResult<MessageDto>>();
        Assert.NotNull(body);
        Assert.Equal(7, body.TotalCount);
        Assert.Equal(3, body.Items.Count);
        Assert.Equal(2, body.Page);
        Assert.Equal(3, body.PageSize);
        Assert.Equal(3, body.TotalPages);
    }

    [Fact]
    public async Task GetMessages_OrderedByTimeDescending()
    {
        var token = await RegisterAndGetTokenAsync("Alice");

        await SeedMessagesAsync("Alice", 3);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/chat/messages");
        var body = await response.Content.ReadFromJsonAsync<PagedResult<MessageDto>>();

        Assert.NotNull(body);
        var times = body.Items.Select(m => m.Time).ToList();
        Assert.Equal(times.OrderByDescending(t => t).ToList(), times);
    }

    // ─── Validation ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMessages_PageZero_Returns400()
    {
        var token = await RegisterAndGetTokenAsync("Alice");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/chat/messages?page=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetMessages_PageSizeTooLarge_Returns400()
    {
        var token = await RegisterAndGetTokenAsync("Alice");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/chat/messages?pageSize=101");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    // ----ReactionMessage-----------------------------------------------------
    [Fact]
    public async Task ReactionMessage_WithoutAuth_Returns401()
    {
        var res = await _client.PostAsJsonAsync("/api/chat/reactions",new {reactions=1});
        Assert.Equal(HttpStatusCode.Unauthorized,res.StatusCode);
    }

    [Fact]
    public async Task ReactionMessage_Returns200()
    {
        var token = await RegisterAndGetTokenAsync("Alice");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var responce = await _client.PostAsJsonAsync("api/chat/rections", new { reaction = 1 });
        Assert.Equal(HttpStatusCode.OK, responce.StatusCode);

    }

    // ─── SendMessage ─────────────────────────────────────────────────────────

    [Fact]
    public async Task SendMessage_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/chat/messages", new { text = "Hello" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_ValidText_Returns200WithMessageDto()
    {
        var token = await RegisterAndGetTokenAsync("Alice");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/chat/messages", new { text = "Hello" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<MessageDto>();
        Assert.NotNull(body);
        Assert.Equal("Hello", body.Text);
        Assert.Equal("Alice", body.UserName);
        Assert.NotEqual(Guid.Empty, body.Id);
    }

    [Fact]
    public async Task SendMessage_ValidText_PersistsMessageInDatabase()
    {
        var token = await RegisterAndGetTokenAsync("Alice");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        await _client.PostAsJsonAsync("/api/chat/messages", new { text = "Hello" });

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        var message = await db.Messages.SingleAsync();

        Assert.Equal("Hello", message.Text);
    }

    [Fact]
    public async Task SendMessage_EmptyText_Returns400()
    {
        var token = await RegisterAndGetTokenAsync("Alice");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/chat/messages", new { text = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_TextTooLong_Returns400()
    {
        var token = await RegisterAndGetTokenAsync("Alice");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/chat/messages",
            new { text = new string('a', 2001) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private async Task<string> RegisterAndGetTokenAsync(string userName)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new { userName });
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return body!.AccessToken;
    }

    private async Task SeedMessagesAsync(string userName, int count)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();

        var user = await db.Users.FirstAsync(u => u.UserName == userName);

        var messages = Enumerable.Range(1, count).Select(i => new Message
        {
            Id = Guid.NewGuid(),
            Text = $"Message {i}",
            UserId = user.Id,
            Time = DateTime.UtcNow.AddMinutes(-i)
        });

        db.Messages.AddRange(messages);
        await db.SaveChangesAsync();
    }
}
