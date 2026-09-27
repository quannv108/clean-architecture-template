using System.Net;
using Api.IntegrationTests.Infrastructure;
using Shouldly;
using Xunit.Abstractions;

namespace Api.IntegrationTests.Endpoints.Emails;

public class SendTestEmailTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiClient _client;

    public SendTestEmailTests(ApiTestFactory factory, ITestOutputHelper output)
    {
        factory.TestOutputHelper = output;
        _client = new ApiClient(factory.CreateClient(), output);
    }

    [Fact]
    public async Task SendTestEmail_ValidRequest_Returns200()
    {
        var request = new
        {
            to = "recipient@example.com",
            subject = "Test Subject",
            body = "<p>Hello</p>",
            isHtml = true
        };

        var response = await _client.PostAsync("emails/test-send", request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendTestEmail_MissingTo_Returns400()
    {
        var request = new
        {
            subject = "Test Subject",
            body = "<p>Hello</p>"
        };

        var response = await _client.PostAsync("emails/test-send", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SendTestEmail_EmptySubject_Returns400()
    {
        var request = new
        {
            to = "recipient@example.com",
            subject = "",
            body = "<p>Hello</p>"
        };

        var response = await _client.PostAsync("emails/test-send", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SendTestEmail_EmptyBody_Returns400()
    {
        var request = new
        {
            to = "recipient@example.com",
            subject = "Test Subject",
            body = "   "
        };

        var response = await _client.PostAsync("emails/test-send", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
