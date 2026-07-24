using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Domain;

/// <summary>
/// State Pattern guard for the Order lifecycle: every transition the Order aggregate performs
/// must be validated here first, so an invalid jump (e.g. AwaitingPayment straight to Completed)
/// throws instead of silently corrupting the order's status.
/// </summary>
public static class OrderStateMachine
{
    private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
    {
        [OrderStatus.Draft] = [OrderStatus.PendingDoctorApproval],
        [OrderStatus.PendingDoctorApproval] = [OrderStatus.AwaitingPayment, OrderStatus.Rejected],
        [OrderStatus.AwaitingPayment] = [OrderStatus.InProgress, OrderStatus.AwaitingTestResultUpload],
        [OrderStatus.InProgress] = [OrderStatus.Completed],
        [OrderStatus.AwaitingTestResultUpload] = [OrderStatus.AwaitingConsultationOpinion],
        [OrderStatus.AwaitingConsultationOpinion] = [OrderStatus.Completed],
        [OrderStatus.Completed] = [],
        [OrderStatus.Rejected] = [],
    };

    public static void EnsureCanTransition(OrderStatus current, OrderStatus target)
    {
        if (!AllowedTransitions.TryGetValue(current, out var allowed) || !allowed.Contains(target))
        {
            throw new ConflictException($"انتقال وضعیت سفارش از «{current}» به «{target}» مجاز نیست.");
        }
    }
}
