using Prescription.Modules.Orders.Domain;

namespace Prescription.Modules.Orders.Infrastructure.Notifications;

/// <summary>Turns an order's current status into the plain-language SMS a customer needs to understand
/// what just happened, without them having to open the app.</summary>
internal static class OrderStatusSmsComposer
{
    public static string Compose(Order order)
    {
        var reference = order.Id.ToString()[..8];

        return order.Status switch
        {
            OrderStatus.PendingDoctorApproval =>
                $"سامانه نسخه\nسفارش {reference} شما ثبت شد و در انتظار بررسی پزشک است.",

            OrderStatus.AwaitingPayment =>
                $"سامانه نسخه\nسفارش {reference} شما توسط پزشک تایید شد. برای ادامه، هزینه سفارش را پرداخت کنید.",

            OrderStatus.Rejected => string.IsNullOrWhiteSpace(order.RejectionReason)
                ? $"سامانه نسخه\nسفارش {reference} شما توسط پزشک رد شد."
                : $"سامانه نسخه\nسفارش {reference} شما توسط پزشک رد شد. دلیل: {order.RejectionReason}",

            OrderStatus.InProgress =>
                $"سامانه نسخه\nپرداخت سفارش {reference} شما با موفقیت انجام شد و سفارش در حال انجام است.",

            OrderStatus.AwaitingTestResultUpload =>
                $"سامانه نسخه\nپزشک سفارش {reference} شما را بررسی کرد. لطفاً جواب آزمایش خود را برای دریافت نظر پزشک بارگذاری کنید.",

            OrderStatus.AwaitingConsultationOpinion =>
                $"سامانه نسخه\nجواب آزمایش سفارش {reference} شما دریافت شد و در انتظار نظر پزشک است.",

            OrderStatus.Completed => string.IsNullOrWhiteSpace(order.PrescriptionReferenceNumber)
                ? $"سامانه نسخه\nسفارش {reference} شما تکمیل شد."
                : $"سامانه نسخه\nسفارش {reference} شما تکمیل شد. کد پیگیری نسخه: {order.PrescriptionReferenceNumber}",

            _ => $"سامانه نسخه\nوضعیت سفارش {reference} شما به‌روزرسانی شد.",
        };
    }
}
