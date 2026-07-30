using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Domain;

/// <summary>
/// Same guard pattern as OrderStateMachine, kept separate because a renewal has its own, shorter
/// lifecycle — there is no result upload or consultation branch, it ends when the doctor hands back
/// the new prescription tracking number.
/// </summary>
public static class RenewalStateMachine
{
    private static readonly Dictionary<RenewalStatus, RenewalStatus[]> AllowedTransitions = new()
    {
        [RenewalStatus.PendingDoctorApproval] = [RenewalStatus.AwaitingPayment, RenewalStatus.Rejected],
        [RenewalStatus.AwaitingPayment] = [RenewalStatus.InProgress],
        [RenewalStatus.InProgress] = [RenewalStatus.Completed],
        [RenewalStatus.Completed] = [],
        [RenewalStatus.Rejected] = [],
    };

    public static void EnsureCanTransition(RenewalStatus current, RenewalStatus target)
    {
        if (!AllowedTransitions.TryGetValue(current, out var allowed) || !allowed.Contains(target))
        {
            throw new ConflictException($"انتقال وضعیت درخواست تمدید از «{current}» به «{target}» مجاز نیست.");
        }
    }
}
