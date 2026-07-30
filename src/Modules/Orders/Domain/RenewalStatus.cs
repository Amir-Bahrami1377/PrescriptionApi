namespace Prescription.Modules.Orders.Domain;

public enum RenewalStatus
{
    PendingDoctorApproval = 0,
    AwaitingPayment = 1,

    /// <summary>Paid; the doctor is writing the renewed prescription and will register its new tracking number.</summary>
    InProgress = 2,

    Completed = 3,
    Rejected = 4,
}
