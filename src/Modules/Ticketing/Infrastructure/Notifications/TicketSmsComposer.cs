using Prescription.Modules.Ticketing.Domain;

namespace Prescription.Modules.Ticketing.Infrastructure.Notifications;

internal static class TicketSmsComposer
{
    public static string ComposeCreated(Ticket ticket) =>
        $"سامانه نسخه\nتیکت پشتیبانی {Reference(ticket)} ثبت شد و در حال بررسی است.";

    public static string ComposeAdminReply(Ticket ticket) =>
        $"سامانه نسخه\nبرای تیکت پشتیبانی {Reference(ticket)} پاسخ جدیدی ثبت شد. برای مشاهده پاسخ وارد سامانه شوید.";

    private static string Reference(Ticket ticket) => ticket.Id.ToString()[..8];
}
