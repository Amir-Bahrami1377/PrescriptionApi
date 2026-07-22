using System.Net;

namespace Prescription.Modules.Payments.Tests;

internal sealed class FakeHttpMessageHandler(HttpStatusCode statusCode, string jsonBody) : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }
    public string? LastRequestBody { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        LastRequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json"),
        };
    }
}
