using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prescription.Modules.Identity.Features.VerifyOtpAndLogin;
using Prescription.Modules.Ticketing.Domain;
using Prescription.Modules.Ticketing.Features.CloseExpiredTickets;
using Prescription.Modules.Ticketing.Features.CreateTicket;
using Prescription.Modules.Ticketing.Features.GetTicket;
using Prescription.Modules.Ticketing.Features.ListMyTickets;
using Prescription.Modules.Ticketing.Features.ListTickets;
using Prescription.Modules.Ticketing.Infrastructure.Persistence;

namespace Prescription.IntegrationTests;

public sealed class TicketingFlowTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private const string CustomerPhoneNumber = "09124445566";
    private const string AdminPhoneNumber = "09351112233";

    [Fact]
    public async Task CustomerAndAdmin_CanCompleteTicketLifecycle()
    {
        using var customerClient = await LoginAsync(CustomerPhoneNumber);
        using var adminClient = await LoginAsync(AdminPhoneNumber);

        var createResponse = await customerClient.PostAsJsonAsync(
            "/api/tickets",
            new { Subject = "مشکل ورود", Message = "نمی‌توانم وارد پنل شوم." });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateTicketResponse>();
        created.Should().NotBeNull();

        factory.FakeNotificationQueue.Notifications.Should().ContainSingle(notification =>
            notification.PhoneNumber == CustomerPhoneNumber
            && notification.Message.Contains("ثبت شد"));

        var mine = await customerClient.GetFromJsonAsync<IReadOnlyList<TicketSummaryDto>>("/api/tickets/mine");
        mine.Should().ContainSingle(ticket => ticket.Id == created!.TicketId && ticket.Status == "Open");

        var customerDetail = await customerClient.GetFromJsonAsync<TicketDetailDto>(
            $"/api/tickets/{created!.TicketId}");
        customerDetail!.Messages.Should().ContainSingle();
        customerDetail.CustomerId.Should().NotBeEmpty();

        var adminTickets = await adminClient.GetFromJsonAsync<IReadOnlyList<AdminTicketSummaryDto>>(
            "/api/admin/tickets?status=Open");
        adminTickets.Should().ContainSingle(ticket => ticket.Id == created.TicketId);

        var adminReplyResponse = await adminClient.PostAsJsonAsync(
            $"/api/tickets/{created.TicketId}/reply",
            new { Message = "لطفاً دوباره تلاش کنید." });
        var adminReplyError = await adminReplyResponse.Content.ReadAsStringAsync();
        adminReplyResponse.StatusCode.Should().Be(HttpStatusCode.NoContent, adminReplyError);

        factory.FakeNotificationQueue.Notifications.Should().Contain(notification =>
            notification.PhoneNumber == CustomerPhoneNumber
            && notification.Message.Contains("پاسخ جدیدی"));

        var queueResponse = await adminClient.PostAsync(
            $"/api/admin/tickets/{created.TicketId}/queue-closure",
            content: null);
        queueResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var pendingDetail = await customerClient.GetFromJsonAsync<TicketDetailDto>(
            $"/api/tickets/{created.TicketId}");
        pendingDetail!.Status.Should().Be("PendingClosure");
        pendingDetail.AutoCloseAtUtc.Should().NotBeNull();
        pendingDetail.QueuedForClosureAtUtc.Should().NotBeNull();
        (pendingDetail.AutoCloseAtUtc - pendingDetail.QueuedForClosureAtUtc)
            .Should().Be(TimeSpan.FromHours(6));

        var customerReplyResponse = await customerClient.PostAsJsonAsync(
            $"/api/tickets/{created.TicketId}/reply",
            new { Message = "مشکل هنوز برطرف نشده است." });
        customerReplyResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var reopenedByReply = await customerClient.GetFromJsonAsync<TicketDetailDto>(
            $"/api/tickets/{created.TicketId}");
        reopenedByReply!.Status.Should().Be("Open");
        reopenedByReply.AutoCloseAtUtc.Should().BeNull();
        reopenedByReply.Messages.Should().HaveCount(3);

        var closeResponse = await customerClient.PostAsync(
            $"/api/tickets/{created.TicketId}/close",
            content: null);
        closeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var closedDetail = await customerClient.GetFromJsonAsync<TicketDetailDto>(
            $"/api/tickets/{created.TicketId}");
        closedDetail!.Status.Should().Be("Closed");

        var reopenResponse = await customerClient.PostAsync(
            $"/api/tickets/{created.TicketId}/reopen",
            content: null);
        reopenResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var finalDetail = await adminClient.GetFromJsonAsync<TicketDetailDto>(
            $"/api/tickets/{created.TicketId}");
        finalDetail!.Status.Should().Be("Open");
        finalDetail.ClosedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task CloseExpiredTicketsJob_ClosesPendingTicketAfterSixHours()
    {
        // Starting the server applies all module migrations to this factory's transient database.
        using var client = factory.CreateClient();
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();
        var job = scope.ServiceProvider.GetRequiredService<ICloseExpiredTicketsJob>();

        var ticket = Ticket.Create(Guid.NewGuid(), "تیکت منقضی", "پیام اولیه");
        ticket.QueueForClosure(DateTimeOffset.UtcNow.AddHours(-7));
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        await job.RunAsync(CancellationToken.None);

        dbContext.ChangeTracker.Clear();
        var persistedTicket = await dbContext.Tickets
            .AsNoTracking()
            .SingleAsync(t => t.Id == ticket.Id);
        persistedTicket.Status.Should().Be(TicketStatus.Closed);
        persistedTicket.ClosedAtUtc.Should().NotBeNull();
    }

    private async Task<HttpClient> LoginAsync(string phoneNumber)
    {
        var client = factory.CreateClient();

        var requestOtpResponse = await client.PostAsJsonAsync(
            "/api/auth/otp/request",
            new { PhoneNumber = phoneNumber });
        requestOtpResponse.EnsureSuccessStatusCode();

        var code = factory.FakeOtpProvider.GetLastCodeSentTo(phoneNumber);
        var verifyResponse = await client.PostAsJsonAsync(
            "/api/auth/otp/verify",
            new { PhoneNumber = phoneNumber, Code = code });
        verifyResponse.EnsureSuccessStatusCode();

        var token = await verifyResponse.Content.ReadFromJsonAsync<VerifyOtpAndLoginResponse>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token!.AccessToken);

        return client;
    }
}
